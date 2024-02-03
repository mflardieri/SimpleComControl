using MauiChatApp.Core.Enums;
using MauiChatApp.Core.Interfaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace MauiChatApp.Core.Models
{
    public class ChatServer
    {
        public class ChatServerStateControl
        {
            public HashSet<string> RoomsAvailable { get; set; }
            public HashSet<string> UsersConnected { get; set; }
            public Dictionary<string, List<string>> RoomUsers { get; set; }
            public Dictionary<string, int> UserConnections { get; set; }
            public int LastConnectionId { get; set; }

            public Dictionary<string, List<ChatEndpoint>> UserEndPoints { get; set; }
            public Dictionary<string, Socket> EndPointSockets { get; set; }
            public Dictionary<string, ChatIdentity> UserIdentities { get; set; }
            public Dictionary<string, ChatIdentity> RoomIdentities { get; set; }

            public void EnsureLookups()
            {
                RoomsAvailable ??= new();
                StateControl.UsersConnected ??= new();
                RoomUsers ??= new();
                UserConnections ??= new();
                EndPointSockets ??= new();
                UserEndPoints ??= new();
                UserIdentities ??= new();
                RoomIdentities ??= new();
            }
        }
        private static ChatServerStateControl StateControl { get; set; }

        private static readonly object LockOps = new();
        public ChatServer(bool resetState = false)
        {
            if (resetState)
            {
                StateControl = null;
            }
            EnsureLookups();
            EnsureServerRooms();
        }
        public static void EnsureLookups()
        {
            lock (LockOps)
            {
                StateControl ??= new();
                StateControl.EnsureLookups();
            }
        }
        public static void EnsureServerRooms()
        {
            var rooms = GetRooms();
            lock (LockOps)
            {
                foreach (var room in rooms)
                {
                    EnsureRoom(room);
                }
            }
        }
        #region [ Helpers ]
        public static int ConnectSession(ChatEndpoint endpoint, Socket socket)
        {
            if (endpoint == null || string.IsNullOrWhiteSpace(endpoint.IpAddress)) { throw new ArgumentNullException(nameof(endpoint), "You must supply an endpoint"); }
            lock (LockOps)
            {
                if (!StateControl.UserConnections.TryGetValue(endpoint.ToString(), out int connectionId))
                {
                    StateControl.LastConnectionId++;
                    connectionId = StateControl.LastConnectionId;
                    StateControl.UserConnections.Add(endpoint.ToString(), connectionId);
                }
                //Add User Socket
                if (socket != null)
                {
                    StateControl.EndPointSockets[endpoint.ToString()] = socket;
                }
                return connectionId;
            }
        }
        public static int? GetConnectionId(ChatEndpoint endpoint)
        {
            string Id = endpoint.ToString();
            lock (LockOps)
            {
                if (StateControl.UserConnections.TryGetValue(Id, out int value))
                {
                    return value;
                }
            }
            return null;
        }
        public static void ConnectAsUser(ChatIdentity identity)
        {
            if (identity == null) { throw new ArgumentNullException(nameof(identity), "You must supply an identity"); }
            if (string.IsNullOrWhiteSpace(identity.Id)) { throw new NullReferenceException("Id is blank."); }
            if (identity.IdentityType != ChatIdentity.UserType) { throw new Exception("Only Users can connect to a server"); }
            lock (LockOps)
            {
                if (!StateControl.UsersConnected.Contains(identity.Id))
                {
                    //Check is Id is for a User
                    var check = GetUser(identity.Id);
                    if (check != null && check.Id == identity.Id)
                    {
                        StateControl.UsersConnected.Add(identity.Id);
                    }
                    else
                    {
                        throw new Exception("User does not exist.");
                    }
                }
                else
                {
                    throw new Exception("User already connected.");
                }
            }
        }
        public static int ConnectUserIdentity(ChatIdentity chatIdentity, ChatEndpoint endpoint)
        {

            if (chatIdentity != null)
            {
                if (!StateControl.UserEndPoints.TryGetValue(chatIdentity.Id, out List<ChatEndpoint> ueps))
                {
                    ueps = new List<ChatEndpoint>();
                }
                if (ueps.FirstOrDefault(x => x.ToString() == endpoint.ToString()) == null)
                {
                    ueps.Add(endpoint);
                    StateControl.UserEndPoints[chatIdentity.Id] = ueps;
                }

                return GetConnectionId(endpoint) ?? 0;
            }
            return 0;
        }
        public static int DisconnectAsUser(ChatIdentity identity)
        {
            if (identity == null) { throw new ArgumentNullException(nameof(identity), "You must supply an identity"); }
            if (string.IsNullOrWhiteSpace(identity.Id)) { throw new NullReferenceException("Id is blank."); }
            if (identity.IdentityType != ChatIdentity.UserType) { throw new Exception("Only Users can disconnect from a server"); }
            lock (LockOps)
            {
                int connectionId = 0;
                //Get Endpoints
                var userEndPoints = GetUserEndPoints(identity.Id);
                //Disconnect and Remove Sockets
                if (userEndPoints != null)
                {
                    foreach (var endpoint in userEndPoints)
                    {
                        try
                        {
                            var socket = GetSocket(endpoint);
                            if (socket != null)
                            {
                                try
                                {
                                    if (socket.Connected)
                                    {
                                        socket.Close();
                                    }
                                    Console.WriteLine("Socket Disconnected");
                                }
                                catch { }
                                try
                                {
                                    socket.Dispose();
                                    Console.WriteLine("Socket Disposed");
                                }
                                catch { }
                                //Remove User Sockets
                                StateControl.EndPointSockets.Remove(endpoint.ToString());
                            }
                            if (connectionId == 0 && StateControl.UserConnections.TryGetValue(endpoint.ToString(), out connectionId)) 
                            { 
                                StateControl.UserConnections.Remove(endpoint.ToString());
                            }
                        }
                        catch { }
                    }
                }

                //Remove User Endpoints
                StateControl.UserEndPoints.Remove(identity.Id);

                //Remove User from Rooms
                foreach (var roomKV in StateControl.RoomUsers)
                {
                    roomKV.Value.Remove(identity.Id);
                }
                //Remove User Connection registry
                StateControl.UsersConnected.Remove(identity.Id);
                //Remove User Identity
                StateControl.UserIdentities.Remove(identity.Id);
                //Remove User Connections Defs
                //if (StateControl.UsersConnected.Contains(identity.Id))
                //{                    
                //}
                return connectionId;
            }
        }
        public static ChatIdentity GetUserIdentity(string Id)
        {
            lock (LockOps)
            {
                if (StateControl.UserIdentities.TryGetValue(Id, out ChatIdentity value))
                {
                    return value;
                }
            }
            return null;
        }
        public static ChatIdentity GetRoomIdentity(string Id)
        {
            lock (LockOps)
            {
                if (StateControl.RoomIdentities.TryGetValue(Id, out ChatIdentity value))
                {
                    //Rebuild Users in Room
                    value.SubIdentities = new();
                    if (StateControl.RoomUsers != null && StateControl.RoomUsers.TryGetValue(Id, out List<string> users))
                    {
                        foreach (var u in users)
                        {
                            var user = GetUserIdentity(u);
                            if (user != null) { value.SubIdentities.Add(user); }
                        }
                    }
                    return value;
                }
            }
            return null;
        }
        public static Dictionary<string, List<ChatEndpoint>> GetRoomUserEndPoints(string roomId)
        {
            var rtnVal = new Dictionary<string, List<ChatEndpoint>>();
            lock (LockOps)
            {

                var room = GetRoomIdentity(roomId);
                if (room != null)
                {
                    if (room.SubIdentities != null)
                    {
                        foreach (var u in room.SubIdentities)
                        {
                            if (StateControl.UserEndPoints != null && StateControl.UserEndPoints.TryGetValue(u.Id, out List<ChatEndpoint> value))
                            {
                                rtnVal.Add(u.Id, value);
                            }
                        }
                    }
                }
            }
            return rtnVal;
        }
        public static Dictionary<string, Socket> GetRoomUserSockets(string roomId)
        {
            var rtnVal = new Dictionary<string, Socket>();
            lock (LockOps)
            {
                Dictionary<string, List<ChatEndpoint>> endpoints = GetRoomUserEndPoints(roomId);
                if (endpoints != null)
                {
                    foreach (var kv in endpoints)
                    {
                        string ep = kv.Value.ToString();

                        if (StateControl.EndPointSockets != null && StateControl.EndPointSockets.TryGetValue(ep, out Socket value))
                        {
                            rtnVal.Add(kv.Key, value);
                        }
                    }
                }

            }
            return rtnVal;
        }
        public static List<ChatEndpoint> GetUserEndPoints(string userId)
        {
            lock (LockOps)
            {
                if (StateControl.UserEndPoints != null && StateControl.UserEndPoints.TryGetValue(userId, out List<ChatEndpoint> value))
                {
                    return value;
                }
                else
                {
                    throw new Exception("User does not exist!!!");
                }
            }

        }
        public static Dictionary<string,List<ChatEndpoint>> GetServerEndpoints()
        {
            lock (LockOps)
            {
                if (StateControl != null)
                {
                    return StateControl.UserEndPoints;
                }
                return null;
            }
        }
        public static List<Socket> GetUserSockets(string userId)
        {
            lock (LockOps)
            {
                List<ChatEndpoint> endpoints = GetUserEndPoints(userId);
                if (endpoints != null && endpoints.Count > 0)
                {
                    List<Socket> result = new();
                    foreach (var endpoint in endpoints)
                    {
                        result.Add(GetSocket(endpoint));
                    }
                    return result;
                }
                else
                {
                    throw new Exception("User End Point does not exist!!!");
                }
            }

        }
        public static Socket GetSocket(ChatEndpoint endpoint)
        {
            lock (LockOps)
            {
                if (endpoint != null)
                {
                    if (StateControl.EndPointSockets != null && StateControl.EndPointSockets.TryGetValue(endpoint.ToString(), out Socket value))
                    {
                        return value;
                    }
                    else
                    {
                        throw new Exception("User Socket does not exist!!!");
                    }
                }
                else
                {
                    throw new Exception("User End Point does not exist!!!");
                }
            }

        }
        public static void AddUserToRoom(string roomId, string userId)
        {
            lock (LockOps)
            {
                if (StateControl.RoomsAvailable == null || !StateControl.RoomsAvailable.Contains(roomId)) { throw new Exception("Room doesn't exist"); }
                StateControl.RoomUsers.TryGetValue(roomId, out List<string> users);
                users ??= new();
                var user = GetUserIdentity(userId);
                if (user != null)
                {
                    if (!users.Contains(userId))
                    {
                        users.Add(userId);
                        StateControl.RoomUsers[roomId] = users;
                    }
                }
                else
                {
                    throw new Exception("User does not exist!!!");
                }

            }
        }

        public static void RemoveUserFromRoom(string roomId, string userId)
        {
            lock (LockOps)
            {
                if (StateControl.RoomUsers.TryGetValue(roomId, out List<string> members))
                {
                    members.Remove(userId);
                    StateControl.RoomUsers[roomId] = members;
                }
            }
        }
        public static ChatIdentity UserToChatIdentity(IUserDef userDef, string status = "Connected")
        {
            return new ChatIdentity()
            {
                Id = userDef.UserId,
                IdentityType = ChatIdentity.UserType,
                Name = userDef.Name,
                Status = status
            };
        }
        public static ChatIdentity RoomToChatIdentity(IRoomDef roomDef, string status = "Active")
        {
            return new ChatIdentity()
            {
                Id = roomDef.RoomId,
                IdentityType = ChatIdentity.RoomType,
                Name = roomDef.Name,
                Status = status,
                Topic = roomDef.Topic
            };
        }
        public static List<ChatIdentity> GetIdentities(MessageIdentityRequest identityRequest)
        {
            lock (LockOps)
            {
                List<ChatIdentity> list = new();
                if (identityRequest != null)
                {
                    switch (identityRequest.InquiryType)
                    {
                        case MessageIdentityInquiryType.AvailableUsers: //Only For Demo
                            IUserRepository userRepository = new SimpleUserRepository();
                            var users = userRepository.GetUsers();
                            if (users != null)
                            {
                                List<ChatIdentity> availableUsers = new();
                                foreach (var u in users)
                                {
                                    if (StateControl.UsersConnected == null || !StateControl.UserConnections.ContainsKey(u.UserId))
                                    {
                                        availableUsers.Add(UserToChatIdentity(u, "Available"));
                                    }
                                }
                                list.AddRange(availableUsers);
                            }
                            break;
                        case MessageIdentityInquiryType.CurrentRoom:
                            if (identityRequest.Current != null)
                            {
                                var currentRoom = GetRoomIdentity(identityRequest.Current.Id);
                                if (currentRoom != null)
                                {
                                    list.Add(currentRoom);
                                }
                            }
                            break;
                        case MessageIdentityInquiryType.CurrentUser:
                            if (identityRequest.Current != null)
                            {
                                var currentUser = GetUserIdentity(identityRequest.Current.Id);
                                if (currentUser != null)
                                {
                                    list.Add(currentUser);
                                }
                            }
                            break;
                        case MessageIdentityInquiryType.Rooms:
                            list.AddRange(StateControl.RoomIdentities.Values);
                            break;
                        case MessageIdentityInquiryType.Users:
                            list.AddRange(StateControl.UserIdentities.Values);
                            break;
                        case MessageIdentityInquiryType.All:
                        default:
                            list.AddRange(StateControl.UserIdentities.Values);
                            list.AddRange(StateControl.RoomIdentities.Values);
                            break;
                    }
                }
                return list;
            }
        }
        public static void EnsureRoom(ChatIdentity room)
        {
            lock (LockOps)
            {
                if (!StateControl.RoomIdentities.ContainsKey(room.Id))
                {
                    StateControl.RoomIdentities.Add(room.Id, room);
                }
                if (!StateControl.RoomsAvailable.Contains(room.Id))
                {
                    StateControl.RoomsAvailable.Add(room.Id);
                }
            }
        }
        #endregion [ Helpers ]
        #region [ Cache ]
        private static ChatIdentity GetUser(string Id)
        {
            if (StateControl.UserIdentities.TryGetValue(Id, out ChatIdentity value))
            {
                return value;
            }

            //Change to Service lookup
            IUserRepository rep = new SimpleUserRepository();

            var users = rep.GetUsers(x => x.UserId == Id);
            if (users != null && users.Count == 1)
            {
                var user = users[0];
                ChatIdentity identity = UserToChatIdentity(user);
                StateControl.UserIdentities[Id] = identity;
                return identity;
            }
            return null;
        }

        private static List<ChatIdentity> GetRooms()
        {
            List<ChatIdentity> rtnVal = new();
            //Change to Service lookup
            IRoomRepository roomRepository = new SimpleRoomRepository();
            foreach (var item in roomRepository.GetRooms(x => !string.IsNullOrEmpty(x.RoomId)))
            {
                rtnVal.Add(RoomToChatIdentity(item));
            }
            return rtnVal;
        }
        public static bool IsUserConnected(string userId)
        {
            lock (LockOps)
            {
                return StateControl.UsersConnected.Contains(userId);
            }
        }
        #endregion [ Cache ]
    }
}
