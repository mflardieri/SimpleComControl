using Microsoft.Maui.Platform;
using SimpleComControl.Core.Bases;
using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks.Sources;
using System.Xml;

namespace MauiChatApp.Core.Models
{
    public class ChatMessageHandler : IComMessageHandler
    {
        private readonly Encoding encoding = Encoding.UTF8;

        public byte[] Convert(IComMessage comMessage)
        {
            if (comMessage != null && comMessage is ChatMessage)
            {
                ChatMessage cMessage = comMessage as ChatMessage;
                PacketConverter pc = new();
                pc.Write((int)cMessage.MessageType);
                pc.Write(cMessage.ConnectionId);
                pc.Write(cMessage.FromEntityId, encoding);
                pc.Write(cMessage.ToEntityId, encoding);
                pc.Write(cMessage.ToMessageType);
                pc.Write(cMessage.Message, encoding);

                //Optional Add Encrption to btye array
                return pc.ToArray();
            }
            else
            {
                throw new Exception("Invalid base messasge type");
            }
        }

        public IComMessage Convert(byte[] buffer, int bytes)
        {
            if (bytes == 0) { return null; }
            if (buffer == null) { return null; }
            if (buffer.Length == 0) { return null; }

            //Optional Add Decryption and post checks to incoming byte array.
            ChatMessage cMessage = new();
            PacketConverter pc = new(buffer);
            cMessage.MessageType = (ComMessageType)pc.ReadInt();
            cMessage.ConnectionId = pc.ReadInt();
            cMessage.FromEntityId = pc.ReadString(encoding);
            cMessage.ToEntityId = pc.ReadString(encoding);
            cMessage.ToMessageType = pc.ReadInt();
            cMessage.Message = pc.ReadString(encoding);

            return cMessage;
        }


        public static object GetSubMessageType(Socket socket, ChatMessage message, bool IsServer)
        {
            object rtnVal = null;
            IPEndPoint fromEP = socket.RemoteEndPoint as IPEndPoint;
            ChatEndpoint fromCEP = ChatSocket.ConvertToEndPoint(fromEP);

            if (message != null)
            {
                switch (message.MessageType)
                {
                    case ComMessageType.TestMessage:
                        rtnVal = new ChatMessageResponse<string>()
                        {
                            IsSuccess = true,
                            Result = $"Server Responded at: {DateTime.Now}",
                            Status = Enums.MessageResponseStatus.Success
                        };
                        break;
                    case ComMessageType.IdentityInfo:
                        var identityRequest = message.Message.FromJson<MessageIdentityRequest>();
                        if (identityRequest != null)
                        {

                            if (identityRequest.InquiryType != Enums.MessageIdentityInquiryType.AvailableUsers)
                            {
                                EnsureIsValidConnection(message, IsServer, fromCEP);
                            }

                            var identities = ChatServer.GetIdentities(identityRequest);
                            var identityResponse = new MessageIdentityResponse()
                            {
                                IsSuccess = true,
                                Result = identities,
                                Status = Enums.MessageResponseStatus.Success
                            };
                            rtnVal = identityResponse;
                        }
                        break;
                    case ComMessageType.Connnect:
                        var connectRequest = message.Message.FromJson<MessageConnectRequest>();
                        var connectedResponse = new MessageConnectResponse();
                        try
                        {
                            if (connectRequest != null)
                            {
                                if (connectRequest.ConnectAs)
                                {
                                    ChatServer.ConnectAsUser(connectRequest.Identity);
                                }
                                var connectedIdentity = ChatServer.GetUserIdentity(connectRequest.Identity.Id);
                                if (connectedIdentity != null)
                                {
                                    int connectedId = message.ConnectionId;
                                    if (connectedId == 0)
                                    {
                                        connectedId = ChatServer.ConnectSession(fromCEP, socket);
                                    }
                                    if (connectedId > 0)
                                    {
                                        //Connect the user
                                        int connectedUserCID = ChatServer.ConnectUserIdentity(connectedIdentity, fromCEP);
                                        if (connectedUserCID != connectedId)
                                        {
                                            throw new Exception("Connnection is bad.");
                                        }

                                        //Formulate response
                                        connectedResponse.IsSuccess = true;
                                        connectedResponse.Status = Enums.MessageResponseStatus.Success;
                                        connectedResponse.Result = connectedUserCID;
                                    }
                                    else
                                    {
                                        throw new Exception("Connection failed to obtain an connection Id.");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            connectedResponse.ErrorMessage = ex.Message;
                            connectedResponse.IsSuccess = false;
                            connectedResponse.Status = Enums.MessageResponseStatus.Error;
                        }
                        rtnVal = connectedResponse;
                        break;
                    case ComMessageType.Ping:
                        var pingRequest = message.Message.FromJson<MessagePingRequest>();
                        var pingedResponse = new MessagePingResponse();
                        try
                        {
                            EnsureIsValidConnection(message, IsServer, fromCEP);

                            //Check if the Hop is complete
                            pingedResponse.IsComplete = !pingRequest.HopChain.HasNextHop();
                            pingedResponse.Status = Enums.MessageResponseStatus.Success;
                            //Force Server Hop 
                            if (!pingedResponse.IsComplete)
                            {
                                ChatIdentity pingedIdentity = pingRequest.GetPingedIdentity();
                                if (pingedIdentity == null) { throw new NullReferenceException("Unable to obtain the pinged or destination identitiy"); }
                                //Get Next Hop
                                var nextHop = GetNextServerHop(pingRequest.HopChain, IsServer, pingedIdentity);

                                //If pinging the server then auto complete the request.
                                if (!pingedResponse.IsComplete && pingedIdentity.Id == IComMessageHandler.ServerId)
                                {
                                    pingedResponse.IsComplete = true;
                                }
                                //Add Next Hop to response
                                pingedResponse.Result = new();
                                pingedResponse.Result.HopChain = nextHop;
                                pingedResponse.Result.Identity = pingedIdentity;
                                if (IsServer)
                                {
                                    //Is User - Check if user is not connected
                                    if (pingedIdentity.IdentityType == ChatIdentity.UserType && !ChatServer.IsUserConnected(pingedIdentity.Id))
                                    {
                                        pingedResponse.Status = Enums.MessageResponseStatus.Error;
                                        pingedResponse.ErrorMessage = "User is not connected.";
                                    }
                                    //Is Room - Check if room does not exist
                                    if (pingedIdentity.IdentityType == ChatIdentity.RoomType)
                                    {
                                        //Auto complete the request
                                        pingedResponse.IsComplete = true;

                                        if (ChatServer.GetRoomIdentity(pingedIdentity.Id) == null)
                                        {
                                            pingedResponse.Status = Enums.MessageResponseStatus.Error;
                                            pingedResponse.ErrorMessage = "Room is not available.";
                                        }
                                    }

                                }
                            }
                            pingedResponse.IsSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            pingedResponse.ErrorMessage = ex.Message;
                            pingedResponse.IsSuccess = false;
                            pingedResponse.Status = Enums.MessageResponseStatus.Error;
                        }
                        rtnVal = pingedResponse;
                        break;
                    case ComMessageType.PingResponse:
                        var pingResponse = message.Message.FromJson<MessagePingResponse>();
                        var pingedRequest = new MessagePingRequest();
                        try
                        {
                            if (pingResponse.IsSuccess && pingResponse.Status == Enums.MessageResponseStatus.Success)
                            {

                                var requestor = pingResponse.Result.GetRequestorIdentity();
                                if (pingResponse.Result.HopChain.IsValidChain(requestor, pingResponse.Result.Identity, requestor))
                                {
                                    var nextHop = GetNextServerHop(pingResponse.Result.HopChain, IsServer, pingResponse.Result.Identity);

                                    pingedRequest.HopChain = nextHop;
                                    pingedRequest.Identity = requestor;
                                }
                                else
                                {
                                    throw new Exception("Response: Invalid Hop Chain.");
                                }
                            }
                        }
                        catch
                        { }
                        rtnVal = pingedRequest;
                        break;
                }
            }
            return rtnVal;
        }
        private static void EnsureIsValidConnection(ChatMessage message, bool IsServer, ChatEndpoint chatEndpoint)
        {
            if (!MessageShowsAsConnected(message)) { throw new Exception("You do not have an active connection"); }
            if (IsServer)
            {
                if (message.ConnectionId == 0 || message.ConnectionId != ChatServer.GetConnectionId(chatEndpoint))
                {
                    throw new Exception("You have an invalid connection");
                }
            }
        }
        private static bool MessageShowsAsConnected(ChatMessage message)
        {
            return message != null && message.ConnectionId > 0;
        }
        private static ChatHopChain GetNextServerHop(ChatHopChain startHop, bool IsServer, ChatIdentity pingedIdentity)
        {
            var nextHop = startHop.GetNextHop(startHop.Requestor, pingedIdentity);
            if (nextHop == null) { throw new NullReferenceException("Unable to obtain the next hop in chain."); }
            if (nextHop.IdentityChain == null || nextHop.ChainPosition == 1 || nextHop.ChainPosition == 3) { throw new Exception("Unable to find server chain hop."); }
            //Check to see if the Next Hop is Server

            bool isSeverHop = nextHop.IsServerHop();
            //Ensure the request is to not ping the server. Otherwise return the server hop.
            if (isSeverHop && IsServer && pingedIdentity.Id != IComMessageHandler.ServerId)
            {
                nextHop = nextHop.GetNextHop(startHop.Requestor, pingedIdentity);
                if (nextHop == null) { throw new NullReferenceException("Unable to obtain the next hop in server chain."); }
                isSeverHop = nextHop.IsServerHop();
                if (isSeverHop) { throw new Exception("Major hop chain issue: Unable to validate next hop."); }
            }
            return nextHop;
        }
        public ChatMessage GetReturnMessage(Socket socket, ChatMessage message, bool IsServer)
        {
            ChatMessage rtnVal = new();
            if (message != null)
            {
                //Add Corresponding outbound message.
                object returnMessage = GetSubMessageType(socket, message, IsServer);
                rtnVal.FromEntityId = message.FromEntityId;
                rtnVal.ConnectionId = message.ConnectionId;
                rtnVal.MessageAsObject = returnMessage;
                rtnVal.Message = returnMessage.ToJson(false);
                switch (message.MessageType)
                {
                    #region [ Ignore ]
                    case ComMessageType.TestResponse:
                    case ComMessageType.IdentityInfoResponse:
                    case ComMessageType.ConnectedMessage:
                    case ComMessageType.DisconnectedMessage:
                    case ComMessageType.ReceivedMessage:
                    case ComMessageType.HelpResponse:
                        break;
                    #endregion [ Ignore ] 
                    #region [Requests to the Server ]
                    case ComMessageType.TestMessage:
                        rtnVal.FromEntityId = IComMessageHandler.ServerId;
                        rtnVal.MessageType = ComMessageType.TestResponse;
                        break;
                    case ComMessageType.IdentityInfo:
                        rtnVal.FromEntityId = IComMessageHandler.ServerId;
                        rtnVal.MessageType = ComMessageType.IdentityInfoResponse;
                        break;
                    case ComMessageType.Connnect:
                        //Get Information about Connection
                        rtnVal.FromEntityId = IComMessageHandler.ServerId;
                        rtnVal.MessageType = ComMessageType.ConnectedMessage;
                        break;
                    case ComMessageType.Ping:
                        //Once Connection is made.
                        rtnVal.MessageType = ComMessageType.PingResponse;
                        break;
                    case ComMessageType.PingResponse:
                        //Once Connection is made.
                        rtnVal.MessageType = ComMessageType.Ping;
                        break;
                    //case ComMessageType.Disconnect:
                    //case ComMessageType.SentMessage:
                    #endregion [Requests to the Server ]
                    default:
                        throw new NotImplementedException("Message Type not Implemented");
                }
            }

            return rtnVal;
        }
        public bool ProcessMessage(ComTCPSocket source, Socket targetSocket, IComMessage comMessage)
        {

            //TODO Process by type and routing
            //Console.WriteLine("TCP RECV: {0}: {1}", targetSocket.RemoteEndPoint.ToString(), comMessage.GetMessageBody());
            try
            {
                if (source is ChatSocket chatSocket)
                {
                    bool IsServer = chatSocket.IsServer;
                    var chatMessage = comMessage as ChatMessage;
                    IPEndPoint fromEP = targetSocket.RemoteEndPoint as IPEndPoint;
                    ChatEndpoint fromCEP = ChatSocket.ConvertToEndPoint(fromEP);
                    ChatMessage returnMessage = GetReturnMessage(targetSocket, chatMessage, IsServer);

                    if (chatMessage.FromEntityId == null) { chatMessage.FromEntityId = ""; }
                    //Send Return Messages
                    if (returnMessage != null)
                    {
                        returnMessage.ToMessageType = chatMessage.ToMessageType;
                        Dictionary<string, List<ChatEndpoint>> endpoints = new();
                        switch (chatMessage.MessageType)
                        {
                            case ComMessageType.IdentityInfo:
                            case ComMessageType.TestMessage:
                            case ComMessageType.Connnect:
                                endpoints.Add(chatMessage.FromEntityId, new List<ChatEndpoint>() { fromCEP });
                                break;
                            case ComMessageType.Ping:
                                //Lookup Endpoints
                                if (IsServer)
                                {
                                    //Not Server Ping
                                    if (chatMessage.ToMessageType > 0)
                                    {
                                        if (chatMessage.ToMessageType == ChatIdentity.UserType)
                                        {
                                            //Unwrap sub message
                                            if (returnMessage.MessageAsObject is MessagePingResponse pingResponse && pingResponse.IsSuccess)
                                            {
                                                returnMessage.ToEntityId = pingResponse.Result.HopChain.Requestor.Id;
                                                returnMessage.FromEntityId = pingResponse.Result.GetPingedIdentity().Id;
                                                var pingEndpoints = ChatServer.GetUserEndPoints(chatMessage.ToEntityId);
                                                if (pingEndpoints != null && pingEndpoints.Count > 0)
                                                {
                                                    endpoints.Add(pingResponse.Result.GetPingedIdentity().Id, pingEndpoints);
                                                }
                                                else
                                                {
                                                    pingResponse.IsComplete = false;
                                                    pingResponse.Status = Enums.MessageResponseStatus.Error;
                                                    pingResponse.ErrorMessage = "Unable to User endpoint to forward ping request";
                                                    //Write response to message
                                                    returnMessage.Message = pingResponse.ToJson(false);
                                                }
                                            }
                                            //Console.WriteLine(pingResponse.IsSuccess);
                                        }
                                        else if (chatMessage.ToMessageType == ChatIdentity.RoomType)
                                        {
                                            //Nothing to do here the request is auto completed
                                        }
                                        else
                                        {
                                            //Dead end
                                        }
                                    }
                                    else
                                    {
                                        //You are pinging the server and the response is auto completed.
                                        //Force Ping back to requestor hop done
                                        endpoints.Add(chatMessage.FromEntityId, new List<ChatEndpoint>() { fromCEP });
                                    }
                                }
                                break;
                            case ComMessageType.PingResponse:
                                if (IsServer)
                                {
                                    if (returnMessage.MessageAsObject is MessagePingRequest pingRequest)
                                    {
                                        if (chatMessage.ToMessageType > 0)
                                        {
                                            if (chatMessage.ToMessageType == ChatIdentity.UserType)
                                            {
                                                returnMessage.ToEntityId = pingRequest.HopChain.Requestor.Id;
                                                returnMessage.FromEntityId = pingRequest.GetPingedIdentity().Id;

                                                var pingEndpoints = ChatServer.GetUserEndPoints(chatMessage.ToEntityId);
                                                if (pingEndpoints != null && pingEndpoints.Count > 0)
                                                {
                                                    endpoints.Add(returnMessage.ToEntityId, pingEndpoints);
                                                }
                                                else
                                                {
                                                    //Write response to message
                                                    returnMessage.Message = pingRequest.ToJson(false);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }

                        //try each endpoint.
                        foreach (var kv in endpoints)
                        {
                            try
                            {
                                returnMessage.ToEntityId = kv.Key;
                                //Assign message
                                foreach (var ep in kv.Value)
                                {
                                    chatSocket.SendServerMessage(returnMessage, ep);
                                }
                            }
                            catch { }
                        }
                    }
                    List<ChatMessage> messages = new();
                    messages.Add(chatMessage);
                    ChatMessageService.AddIncomingMessages(messages);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return true;
        }
    }
}
