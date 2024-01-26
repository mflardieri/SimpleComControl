﻿using SimpleComControl.Core.Bases;
using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Interfaces;
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
                pc.Write(cMessage.ToMessageType, encoding);
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
            cMessage.ToMessageType = pc.ReadString(encoding);
            cMessage.Message = pc.ReadString(encoding);

            return cMessage;
        }


        public static object GetSubMessageType(Socket socket, ChatMessage message)
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
                            //Check if the Hop is complete
                            pingedResponse.IsComplete = !pingRequest.HopChain.HasNextHop();
                            if (!pingedResponse.IsComplete)
                            {
                                ChatIdentity pingedIdentity = pingRequest.GetPingedIdentity();

                                if (pingedIdentity == null) { throw new NullReferenceException("Unable to obtain the pinged or destination identitiy"); }
                                //Get Next Hop
                                var nextHop = pingRequest.HopChain.GetNextHop(pingRequest.HopChain.Requestor, pingedIdentity);
                                if (nextHop == null) { throw new NullReferenceException("Unable to obtain the next hop in chain."); }
                                //Check to see if the Next Hop is Server
                                //Add Next Hop to response
                                //pingedResponse.Result = pingRequest.HopChain.GetNextHop(pingRequest.Identity);
                            }
                        }
                        catch (Exception ex) 
                        {
                            pingedResponse.ErrorMessage = ex.Message;
                            pingedResponse.IsSuccess = false;
                            pingedResponse.Status = Enums.MessageResponseStatus.Error;
                        }
                        break;
                }
            }
            return rtnVal;
        }
        public ChatMessage GetReturnMessage(Socket socket, ChatMessage message, bool IsServer)
        {
            ChatMessage rtnVal = new();
            if (message != null)
            {
                //Add Corresponding outbound message.
                object returnMessage = GetSubMessageType(socket, message);
                rtnVal.FromEntityId = message.FromEntityId;
                rtnVal.ConnectionId = message.ConnectionId;
                rtnVal.Message = returnMessage.ToJson(false);
                switch (message.MessageType)
                {
                    #region [ Ignore ]
                    case ComMessageType.TestResponse:
                    case ComMessageType.IdentityInfoResponse:
                    case ComMessageType.ConnectedMessage:
                    case ComMessageType.PingResponse:
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

                        Dictionary<string, List<ChatEndpoint>> endpoints = new();
                        switch (chatMessage.MessageType)
                        {
                            case ComMessageType.IdentityInfo:
                            case ComMessageType.TestMessage:
                            case ComMessageType.Connnect:
                                endpoints.Add(chatMessage.FromEntityId, new List<ChatEndpoint>() { fromCEP });
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
