using MauiChatApp.Core.Interfaces;
using MauiChatApp.Core.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Interfaces;
using System.Text;

namespace MauiChatApp.Core.Tests
{
    [TestClass]
    public class ChatUnitTests
    {

        private ChatSocket? Server { get; set; }
        private const string IpAddress = "127.0.0.1";
        private int ServerPort { get; set; }

        private ChatServer MyChatServer { get; set; }
        private int ClientPort { get; set; }
        private ChatSocket? Client { get; set; }

        private int Client2Port { get; set; }
        private ChatSocket? Client2 { get; set; }
        private IUserRepository? UserRepository { get; set; }
        private List<IUserDef>? Users { get; set; }

        [TestMethod]
        public void TestPacketConverter()
        {
            Encoding encoding = Encoding.UTF8;
            PacketConverter pc = new();

            int numIn = 100;
            bool? blnIn = true;
            //string strIn = "";
            string strIn = "Hi There!!!";
            bool? blnIn2 = false;
            pc.Write(numIn);//Int 
            if (blnIn.HasValue)
            {
                pc.Write(true);//Header for boolean
                pc.Write(blnIn.Value);
            }
            pc.Write(strIn, encoding); //String
            if (blnIn2.HasValue)
            {
                pc.Write(true);//Header for boolean
                pc.Write(blnIn2.Value);
            }
            byte[] data = pc.ToArray();
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Length > 0);


            //Read Headers and Data
            pc = new PacketConverter(data);

            int numOut = pc.ReadInt();
            bool? blnOut = null;
            string strOut = "";
            bool? blnOut2 = null;

            Assert.AreEqual(numIn, numOut);
            bool header = pc.ReadBool();
            if (header) { blnOut = pc.ReadBool(); }
            Assert.AreEqual(blnIn, blnOut);
            int intHeader = pc.ReadInt(false);
            if (intHeader > -1)
            {
                strOut = pc.ReadString(encoding);
            }
            else
            {
                pc.ReadInt(); //Skip Blank header
            }
            Assert.AreEqual(strIn, strOut);
            header = pc.ReadBool();
            if (header) { blnOut2 = pc.ReadBool(); }
            Assert.AreEqual(blnIn2, blnOut2);

        }
        [TestMethod]
        public void TestChatMessageConversion()
        {
            ChatMessageHandler messageHandler = new();
            ChatMessage inMessage = new()
            {
                MessageType = SimpleComControl.Core.Enums.ComMessageType.TestMessage,
                ConnectionId = 1,
                FromEntityId = "1",
                ToEntityId = "2",
                Message = "Hello World!!!"
            };
            byte[] data = messageHandler.Convert(inMessage);
            ChatMessage? outMessage = messageHandler.Convert(data, data.Length) as ChatMessage;

            Assert.IsNotNull(outMessage);
            Assert.AreEqual(inMessage.MessageType, outMessage.MessageType);
            Assert.AreEqual(inMessage.ConnectionId, outMessage.ConnectionId);
            Assert.AreEqual(inMessage.ToEntityId, outMessage.ToEntityId);
            Assert.AreEqual(inMessage.FromEntityId, outMessage.FromEntityId);
            Assert.AreEqual(inMessage.Message, outMessage.Message);
        }
        [TestMethod]
        public void TestServerInternalCommands()
        {
            EnsureUserRepo();
            Assert.IsNotNull(Users);

            //Bare bones setup for chat server.
            MyChatServer = new(true);

            #region [ ConnectAsUser ]
            string userId = Users.First().UserId;
            ChatIdentity identity = new() { Id = userId, IdentityType = ChatIdentity.UserType };
            ChatServer.ConnectAsUser(identity);
            var outIdentity = ChatServer.GetUserIdentity(userId);
            Assert.AreEqual(identity.Id, outIdentity.Id);
            userId = "TestThisShouldNotExistId";
            identity.Id = userId;
            Assert.ThrowsException<Exception>(() => ChatServer.ConnectAsUser(identity));
            outIdentity = ChatServer.GetUserIdentity(userId);
            Assert.IsNull(outIdentity);
            #endregion [ ConnectAsUser ]

            #region [ ConnectSession ]
            ChatEndpoint endpoint = new() { IpAddress = "127.0.0.1", Port = 0 };
            int? connnectionId = ChatServer.GetConnectionId(endpoint);
            Assert.IsNull(connnectionId);
            ChatServer.ConnectSession(endpoint, null);
            connnectionId = ChatServer.GetConnectionId(endpoint);
            Assert.IsNotNull(connnectionId);
            Assert.IsTrue(connnectionId.Value > 0);
            #endregion [ ConnectSession ]
        }
        [TestMethod]
        public void TestHopChain()
        {
            ChatIdentity a = new() { Id = "1", IdentityType = ChatIdentity.UserType, Name = "Tester", Status = "Connected" };
            ChatIdentity b = new() { Id = "1", IdentityType = ChatIdentity.RoomType, Name = "Test Room", Status = "Connected" };

            //Hop chain from User A to Room B
            ChatHopChain chatHopChain = new();
            chatHopChain.Requestor = a;

            //Hops: A
            Assert.IsTrue(chatHopChain.HasNextHop());
            chatHopChain = chatHopChain.GetNextHop(a, b);
            Assert.IsNotNull(chatHopChain);
            Assert.AreEqual(1, chatHopChain.IdentityChain.Count);
            Assert.AreEqual(a.Id, chatHopChain.IdentityChain[chatHopChain.IdentityChain.Count - 1].Id);


            //Hops: A -> Server
            Assert.IsTrue(chatHopChain.HasNextHop());
            chatHopChain = chatHopChain.GetNextHop(a, b);
            Assert.IsNotNull(chatHopChain);
            Assert.AreEqual(2, chatHopChain.IdentityChain.Count);
            Assert.AreEqual(IComMessageHandler.ServerId, chatHopChain.IdentityChain[chatHopChain.IdentityChain.Count - 1].Id);

            //Hops: A -> Server -> B
            Assert.IsTrue(chatHopChain.HasNextHop());
            chatHopChain = chatHopChain.GetNextHop(a, b);
            Assert.IsNotNull(chatHopChain);
            Assert.AreEqual(3, chatHopChain.IdentityChain.Count);
            Assert.AreEqual(b.Id, chatHopChain.IdentityChain[chatHopChain.IdentityChain.Count - 1].Id);

            //Hops: A -> Server -> B -> Server
            Assert.IsTrue(chatHopChain.HasNextHop());
            chatHopChain = chatHopChain.GetNextHop(a, b);
            Assert.IsNotNull(chatHopChain);
            Assert.AreEqual(4, chatHopChain.IdentityChain.Count);
            Assert.AreEqual(IComMessageHandler.ServerId, chatHopChain.IdentityChain[chatHopChain.IdentityChain.Count - 1].Id);

            //Hops: A -> Server -> B -> Server -> A
            Assert.IsTrue(chatHopChain.HasNextHop());
            chatHopChain = chatHopChain.GetNextHop(a, b);
            Assert.IsNotNull(chatHopChain);
            Assert.AreEqual(5, chatHopChain.IdentityChain.Count);
            Assert.AreEqual(a.Id, chatHopChain.IdentityChain[chatHopChain.IdentityChain.Count - 1].Id);


            //No More Hops
            Assert.IsTrue(!chatHopChain.HasNextHop());
        }
        [TestMethod]
        public void TestSimpleChatMessage()
        {
            EnsureServerClientSetup();
            Assert.IsNotNull(Client);
            Assert.IsTrue(Client.IsRunning);
            //Simulate Message Sent
            var testMessage = ChatMessageService.CreateNewTestRequest();
            Assert.IsTrue(!string.IsNullOrEmpty(testMessage.TagId));
            Client.Send(testMessage);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            ChatMessageService chatMessageService = new();
            chatMessageService.ProcessIncomingMessages();

        }
        [TestMethod]
        public void TestIdentityInfoResponseMessage()
        {
            EnsureServerClientSetup();
            Assert.IsNotNull(Client);
            Assert.IsTrue(Client.IsRunning);
            //Simulate Message Sent
            var identityInfoRequest = ChatMessageService.CreateNewIdentityRequest(Enums.MessageIdentityInquiryType.AvailableUsers);
            Assert.IsTrue(!string.IsNullOrEmpty(identityInfoRequest.TagId));
            Client.Send(identityInfoRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            ChatMessageService chatMessageService = new();
            chatMessageService.ProcessIncomingMessages();
            Assert.IsNotNull(ChatMessageService.HistoricalDisplayMessages);

            //Assert Available UserInfo As been given.
            var identityInfoResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.IdentityInfoResponse);
            Assert.IsNotNull(identityInfoResponseMessage);
            var identityInfoResponse = identityInfoResponseMessage.Message.FromJson<MessageIdentityResponse>();
            Assert.IsNotNull(identityInfoResponse);
            Assert.IsTrue(identityInfoResponse.Result != null && identityInfoResponse.Result.Count > 0);
        }
        [TestMethod]
        public void TestConnectResponseMessage()
        {
            EnsureServerClientSetup(resetChatServer: true);
            Assert.IsNotNull(Client);
            Assert.IsTrue(Client.IsRunning);
            Assert.IsNotNull(Client._socket);

            EnsureClient2Setup();
            Assert.IsNotNull(Client2);
            Assert.IsTrue(Client2.IsRunning);
            Assert.IsNotNull(Client2._socket);

            EnsureUserRepo();
            Assert.IsNotNull(Users);

            var userExist = ChatServer.UserToChatIdentity(Users.First());

            //Register Existing User as Client #2
            ChatServer.ConnectAsUser(userExist);
            int cIdUserExist = ChatServer.ConnectSession(ChatSocket.ConvertToEndPoint(Client2._socket.LocalEndPoint as System.Net.IPEndPoint), Client2._socket);
            Assert.IsTrue(cIdUserExist > 0);

            int cid = ChatServer.ConnectUserIdentity(userExist, ChatSocket.ConvertToEndPoint(Client2._socket.LocalEndPoint as System.Net.IPEndPoint));
            Assert.IsTrue(cIdUserExist == cid);



            string userId = Users[1].UserId;
            ChatIdentity identity = new() { Id = userId, IdentityType = ChatIdentity.UserType };

            var connectRequest = ChatMessageService.CreateNewConnectRequest(true, current: identity);
            Assert.IsTrue(!string.IsNullOrEmpty(connectRequest.TagId));
            //Connect as User initially
            Client.Send(connectRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            ChatMessageService chatMessageService = new();
            chatMessageService.ProcessIncomingMessages();
            Assert.IsNotNull(ChatMessageService.HistoricalDisplayMessages);

            var connectResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.ConnectedMessage);
            Assert.IsNotNull(connectResponseMessage);
            var connectResponse = connectResponseMessage.Message.FromJson<MessageConnectResponse>();
            Assert.IsNotNull(connectResponse);
            Assert.IsTrue(connectResponse.Result > 0);
            int connectionId = connectResponse.Result;
            Assert.IsTrue(connectResponse.IsSuccess);
            ChatMessageService.HistoricalDisplayMessages.Clear();

            //Try to connect as user again. This should fail.
            Client.Send(connectRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            chatMessageService.ProcessIncomingMessages();

            connectResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.ConnectedMessage);
            Assert.IsNotNull(connectResponseMessage);
            connectResponse = connectResponseMessage.Message.FromJson<MessageConnectResponse>();
            Assert.IsNotNull(connectResponse);
            Assert.IsTrue(connectResponse.Result == 0);
            Assert.IsTrue(!connectResponse.IsSuccess);
            ChatMessageService.HistoricalDisplayMessages.Clear();


            //Connect with connection id as Identity
            connectRequest = ChatMessageService.CreateNewConnectRequest(false, current: identity);
            connectRequest.ConnectionId = connectionId;
            Assert.IsTrue(!string.IsNullOrEmpty(connectRequest.TagId));
            Client.Send(connectRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            chatMessageService.ProcessIncomingMessages();

            connectResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.ConnectedMessage);
            Assert.IsNotNull(connectResponseMessage);
            connectResponse = connectResponseMessage.Message.FromJson<MessageConnectResponse>();
            Assert.IsNotNull(connectResponse);
            Console.WriteLine(connectResponse.ErrorMessage);
            Assert.IsTrue(connectResponse.Result == connectionId);
            Assert.IsTrue(connectResponse.IsSuccess);
            ChatMessageService.HistoricalDisplayMessages.Clear();

            //Connect to a room
            ChatMessageService.SetUserIdentity(identity);
            ChatMessageService.SetConnectionId(connectionId);
            IRoomRepository roomRepository = new SimpleRoomRepository();
            var room = ChatServer.RoomToChatIdentity(roomRepository.GetRooms(x=>!string.IsNullOrEmpty(x.RoomId))[0]);
            connectRequest = ChatMessageService.CreateConnectToRoomRequest(room,identity);
            connectRequest.ConnectionId = connectionId;
            Assert.IsTrue(!string.IsNullOrEmpty(connectRequest.TagId));
            Client.Send(connectRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            chatMessageService.ProcessIncomingMessages();

            connectResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.ConnectedMessage);
            Assert.IsNotNull(connectResponseMessage);
            connectResponse = connectResponseMessage.Message.FromJson<MessageConnectResponse>();
            Assert.IsNotNull(connectResponse);
            Console.WriteLine(connectResponse.ErrorMessage);
            Assert.IsTrue(connectResponse.Result == connectionId);
            Assert.IsNotNull(connectResponse.RoomRef);
            Assert.AreEqual("0", connectResponse.RoomRef.Id);
            Assert.IsTrue(connectResponse.RoomRef.SubIdentities.FirstOrDefault(x => x.Id == identity.Id) != null);
            Assert.IsTrue(connectResponse.IsSuccess);
            ChatMessageService.HistoricalDisplayMessages.Clear();


            //Disconnect from Room
            var disConnectRequest = ChatMessageService.CreateDisconnectFromRoomRequest(room, identity);
            disConnectRequest.ConnectionId = connectionId;
            Assert.IsTrue(!string.IsNullOrEmpty(connectRequest.TagId));
            Client.Send(disConnectRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            chatMessageService.ProcessIncomingMessages();

            connectResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.DisconnectedMessage);
            Assert.IsNotNull(connectResponseMessage);
            connectResponse = connectResponseMessage.Message.FromJson<MessageConnectResponse>();
            Assert.IsNotNull(connectResponse);
            Console.WriteLine(connectResponse.ErrorMessage);
            Assert.IsTrue(connectResponse.Result == connectionId);
            Assert.IsNotNull(connectResponse.RoomRef);
            Assert.AreEqual("0", connectResponse.RoomRef.Id);
            Assert.IsTrue(connectResponse.RoomRef.SubIdentities.FirstOrDefault(x => x.Id == identity.Id) == null);
            Assert.IsTrue(connectResponse.IsSuccess);
            ChatMessageService.HistoricalDisplayMessages.Clear();

            //Disconnect from Server
            disConnectRequest = ChatMessageService.CreateDisconnectRequest(identity);
            disConnectRequest.ConnectionId = connectionId;
            Assert.IsTrue(!string.IsNullOrEmpty(connectRequest.TagId));
            Client.Send(disConnectRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());
            chatMessageService.ProcessIncomingMessages();

            connectResponseMessage = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.MessageType == SimpleComControl.Core.Enums.ComMessageType.DisconnectedMessage);
            Assert.IsNotNull(connectResponseMessage);
            connectResponse = connectResponseMessage.Message.FromJson<MessageConnectResponse>();
            Assert.IsNotNull(connectResponse);
            Console.WriteLine(connectResponse.ErrorMessage);
            Assert.IsTrue(connectResponse.Result == connectionId);
            Assert.IsTrue(connectResponse.IsSuccess);
            Assert.IsFalse(ChatServer.IsUserConnected(userId));
            ChatMessageService.HistoricalDisplayMessages.Clear();
        }

        [TestMethod]
        public void TestPingResponseMessage()
        {
            //Ping: Client #1  -> To Server -> To Client #2 -> To Server -> To Client #1
            //Custom Message type to control server direction and hops.
            EnsureServerClientSetup(resetChatServer: true);
            Assert.IsNotNull(Client);
            Assert.IsTrue(Client.IsRunning);
            Assert.IsNotNull(Client._socket);
            EnsureClient2Setup();
            Assert.IsNotNull(Client2);
            Assert.IsTrue(Client2.IsRunning);
            Assert.IsNotNull(Client2._socket);


            EnsureUserRepo();
            Assert.IsNotNull(Users);
            Assert.IsTrue(Users.Count > 1);
            IUserDef currentUser = Users[0];
            IUserDef pingUser = Users[1];
            ChatIdentity iCurrentUser = ChatServer.UserToChatIdentity(currentUser);
            ChatIdentity iPingUser = ChatServer.UserToChatIdentity(pingUser);

            //Register Current User as Client #1
            ChatServer.ConnectAsUser(iCurrentUser);
            int cIdCurrentUser = ChatServer.ConnectSession(ChatSocket.ConvertToEndPoint(Client._socket.LocalEndPoint as System.Net.IPEndPoint), Client._socket);
            Assert.IsTrue(cIdCurrentUser > 0);

            //Register Ping User as Client #2
            ChatServer.ConnectAsUser(iPingUser);
            int cIdPingUser = ChatServer.ConnectSession(ChatSocket.ConvertToEndPoint(Client2._socket.LocalEndPoint as System.Net.IPEndPoint), Client2._socket);
            Assert.IsTrue(cIdPingUser > 0);

            int cid = ChatServer.ConnectUserIdentity(iCurrentUser, ChatSocket.ConvertToEndPoint(Client._socket.LocalEndPoint as System.Net.IPEndPoint));
            Assert.IsTrue(cIdCurrentUser == cid);

            cid = ChatServer.ConnectUserIdentity(iPingUser, ChatSocket.ConvertToEndPoint(Client2._socket.LocalEndPoint as System.Net.IPEndPoint));
            Assert.IsTrue(cIdPingUser == cid);



            ChatMessageService.SetConnectionId(cIdCurrentUser);
            ChatMessageService.SetUserIdentity(iCurrentUser);
            var pingRequest = ChatMessageService.CreatePingMessage(iPingUser, null, iCurrentUser);
            Assert.IsTrue(!string.IsNullOrEmpty(pingRequest.TagId));
            //Send Request to Server
            Client.Send(pingRequest);
            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());

            var chatMessageService = new ChatMessageService();
            chatMessageService.ChatWindowId = iCurrentUser.Id;
            chatMessageService.ProcessIncomingMessages();
            var q = ChatMessageService.HistoricalDisplayMessages.Where(x => x.MessageType == ComMessageType.Ping || x.MessageType == ComMessageType.PingResponse);
            var pingMessages = q.ToList();
            if (pingMessages.Count != 3)
            {
                var dt = DateTime.Now;
                int pingMessageCount = 0;
                while ((DateTime.Now - dt).TotalSeconds < 2 && pingMessageCount < 3)
                {
                    try
                    {
                        if (ChatMessageService.HasMessagesToProcess())
                        {
                            chatMessageService.ProcessIncomingMessages();
                        }
                        pingMessages = q.ToList();
                        Thread.Sleep(100);
                    }
                    catch { }
                }
            }
            Assert.IsTrue(pingMessages.Count == 3);
            Assert.IsTrue(chatMessageService.DisplayMessages.Count == 0);

            var pingMessageResponse = pingMessages.FirstOrDefault(x => x.ToEntityId == iCurrentUser.Id && x.MessageType == ComMessageType.Ping);
            Assert.IsNotNull(pingMessageResponse);
            var pingMessageRequest = pingMessageResponse.Message.FromJson<MessagePingRequest>();
            Assert.IsNotNull(pingMessageRequest);
            Assert.IsNotNull(pingMessageRequest.HopChain);
            Assert.IsNotNull(pingMessageRequest.HopChain.IdentityChain);
            Assert.IsNotNull(pingMessageRequest.HopChain.Requestor);
            Assert.AreEqual(5, pingMessageRequest.HopChain.ChainPosition);
            Assert.AreEqual(iCurrentUser.Id, pingMessageRequest.HopChain.Requestor.Id);
            Assert.IsTrue(pingMessageRequest.HopChain.IsValidChain(pingMessageRequest.HopChain.Requestor, iCurrentUser, iPingUser));

            //Assert.IsTrue(false);
        }
        [TestMethod]
        public void TestClientResponseMessage()
        {
            //Ping: Client #1  -> To Server -> To Client #2 -> To Server -> To Client #1
            //Custom Message type to control server direction and hops.
            EnsureServerClientSetup(resetChatServer: true);
            Assert.IsNotNull(Client);
            Assert.IsTrue(Client.IsRunning);
            Assert.IsNotNull(Client._socket);
            EnsureClient2Setup();
            Assert.IsNotNull(Client2);
            Assert.IsTrue(Client2.IsRunning);
            Assert.IsNotNull(Client2._socket);


            EnsureUserRepo();
            Assert.IsNotNull(Users);
            Assert.IsTrue(Users.Count > 1);
            IUserDef currentUser = Users[0];
            IUserDef toUser = Users[1];
            ChatIdentity iFromUser = ChatServer.UserToChatIdentity(currentUser);
            ChatIdentity iToUser = ChatServer.UserToChatIdentity(toUser);

            //Register Current User as Client #1
            ChatServer.ConnectAsUser(iFromUser);
            int cIdFromUser = ChatServer.ConnectSession(ChatSocket.ConvertToEndPoint(Client._socket.LocalEndPoint as System.Net.IPEndPoint), Client._socket);
            Assert.IsTrue(cIdFromUser > 0);

            //Register Ping User as Client #2
            ChatServer.ConnectAsUser(iToUser);
            int cIdToUser = ChatServer.ConnectSession(ChatSocket.ConvertToEndPoint(Client2._socket.LocalEndPoint as System.Net.IPEndPoint), Client2._socket);
            Assert.IsTrue(cIdToUser > 0);

            int cid = ChatServer.ConnectUserIdentity(iFromUser, ChatSocket.ConvertToEndPoint(Client._socket.LocalEndPoint as System.Net.IPEndPoint));
            Assert.IsTrue(cIdFromUser == cid);

            cid = ChatServer.ConnectUserIdentity(iToUser, ChatSocket.ConvertToEndPoint(Client2._socket.LocalEndPoint as System.Net.IPEndPoint));
            Assert.IsTrue(cIdToUser == cid);

            ChatMessageService.SetConnectionId(cIdFromUser);
            ChatMessageService.SetUserIdentity(iFromUser);
            var clientRequest = ChatMessageService.CreateClientMessage(iToUser, "Hi, Testing!!!");
            Assert.IsTrue(!string.IsNullOrEmpty(clientRequest.TagId));
            //Send Request to Server
            Client.Send(clientRequest);


            WaitForMessagesToProcess();
            Assert.IsTrue(ChatMessageService.HasMessagesToProcess());

            var chatMessageService = new ChatMessageService();
            chatMessageService.ChatWindowId = iFromUser.Id;
            chatMessageService.ProcessIncomingMessages();

            var fromResponse = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.ToEntityId == iFromUser.Id && x.MessageType == ComMessageType.ReceivedMessage);
            Assert.IsNotNull(fromResponse);

            var toResponse = ChatMessageService.HistoricalDisplayMessages.FirstOrDefault(x => x.ToEntityId == iToUser.Id && x.MessageType == ComMessageType.ReceivedMessage);
            Assert.IsNotNull(toResponse);


        }
        #region [ Test Helpers ]
        public void EnsureServerClientSetup(bool forceServer = false, bool resetChatServer = false, bool forceClientSetup = false)
        {
            EnsureServerSetup(forceServer, resetChatServer);
            EnsureClientSetup(forceClientSetup);
        }
        public void EnsureServerSetup(bool force = false, bool resetChatServer = false)
        {
            if (Server == null || force)
            {
                Server?.Dispose();
            }
            Server = new();
            ServerPort = ComSocketHelper.TcpOpenPort();
            Server.StartServer(IpAddress, ServerPort);
            MyChatServer = new(resetChatServer);
        }

        public void EnsureClientSetup(bool force = false)
        {
            if (Client == null || force)
            {
                Client?.Dispose();
                Client = new();
                ClientPort = ComSocketHelper.TcpOpenPort();
                Client.StartClient(IpAddress, ClientPort, IpAddress, ServerPort);
            }
        }

        public void EnsureClient2Setup(bool force = false)
        {
            if (Client2 == null || force)
            {
                Client2?.Dispose();
                Client2 = new();
                Client2Port = ComSocketHelper.TcpOpenPort();
                Client2.StartClient(IpAddress, Client2Port, IpAddress, ServerPort);
            }
        }

        public void EnsureUserRepo(bool force = false)
        {
            if (force || Users == null || Users.Count == 0)
            {
                if (UserRepository != null)
                {
                    try
                    {
                        UserRepository.Dispose();
                    }
                    catch { }
                }

                UserRepository = new SimpleUserRepository();
                Users = UserRepository.GetUsers();
            }
        }
        public void WaitForMessagesToProcess()
        {
            int MaxTimeThreshold = 1000;
            int incValue = 0;
            while (!ChatMessageService.HasMessagesToProcess() && incValue < MaxTimeThreshold)
            {
                incValue += 10;
                Thread.Sleep(10);
            }
        }
        #endregion [ Test Helpers ]
    }
}
