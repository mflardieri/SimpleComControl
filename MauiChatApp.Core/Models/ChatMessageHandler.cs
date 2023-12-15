using SimpleComControl.Core.Bases;
using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            cMessage.Message = pc.ReadString(encoding);

            return cMessage;
        }


        public static object GetSubMessageType(ChatMessage message)
        {
            object rtnVal = null;
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
                    case ComMessageType.IndentityInfo:
                        var indentityRequest = message.Message.FromJson<MessageIndentityRequest>();
                        if (indentityRequest != null)
                        {
                            var indentities = ChatServer.GetIndentities(indentityRequest);
                            var indentityResponse = new MessageIndentityResponse()
                            {
                                IsSuccess = true,
                                Result = indentities,
                                Status = Enums.MessageResponseStatus.Success
                            };
                            rtnVal = indentityResponse;
                        }
                        break;
                }
            }
            return rtnVal;
        }
        public ChatMessage GetReturnMessage(ChatMessage message, bool IsServer)
        {
            ChatMessage rtnVal = new();
            if (message != null)
            {
                //Add Corresponding outbound message.
                object returnMessage = GetSubMessageType(message);
                rtnVal.FromEntityId = message.FromEntityId;
                rtnVal.ConnectionId = message.ConnectionId;
                rtnVal.Message = returnMessage.ToJson(false);
                switch (message.MessageType)
                {
                    #region [ Ignore ]
                    case ComMessageType.TestResponse:
                    case ComMessageType.IndentityInfoResponse:
                        break;
                    #endregion [ Ignore ] 
                    #region [Requests to the Server ]
                    case ComMessageType.TestMessage:
                        rtnVal.FromEntityId = "Server";
                        rtnVal.MessageType = ComMessageType.TestResponse;
                        break;
                    case ComMessageType.IndentityInfo:
                        rtnVal.FromEntityId = "Server";
                        rtnVal.MessageType = ComMessageType.IndentityInfoResponse;
                        break;
                    //case ComMessageType.Connnect:
                    //    //Get Information about Connection
                    //    MessageConnectRequest connectRequest 
                    //    break;
                    //case ComMessageType.TestMessage:
                    //    //Once Connection is made.
                    //    rtnVal = message;
                    //    rtnVal.Message = "Test Completed";
                    //    break;
                    //case ComMessageType.IndentityInfo:
                    //body messsage will have request info.
                    //    break;
                    //case ComMessageType.Ping:
                    //    //Once Connection is made.
                    //    break;
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
                    ChatMessage returnMessage = GetReturnMessage(chatMessage, IsServer);

                    IPEndPoint fromEP = targetSocket.RemoteEndPoint as IPEndPoint;
                    if (chatMessage.FromEntityId == null) { chatMessage.FromEntityId = ""; }
                    //Send Return Messages
                    if (returnMessage != null)
                    {

                        Dictionary<string, ChatEndpoint> endpoints = new();
                        switch (chatMessage.MessageType)
                        {
                            case ComMessageType.IndentityInfo:
                            case ComMessageType.TestMessage:
                                ChatEndpoint fromCEP = ChatSocket.ConvertToEndPoint(fromEP);
                                endpoints.Add(chatMessage.FromEntityId, fromCEP);
                                break;
                        }

                        //try each endpoint.

                        foreach (var kv in endpoints)
                        {
                            try
                            {
                                returnMessage.ToEntityId = kv.Key;
                                //Assign message
                                chatSocket.SendServerMessage(returnMessage, kv.Value);
                            }
                            catch { }
                        }
                    }
                    List<ChatMessage> messages = new List<ChatMessage>();
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
