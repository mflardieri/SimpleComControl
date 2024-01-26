using MauiChatApp.Core.Enums;
using MauiChatApp.Core.Interfaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
namespace MauiChatApp.Core.Models
{
    public class ChatServer
    {
        private static HashSet<string> RoomsAvailable { get; set; }
        private static HashSet<string> UsersConnected { get; set; }
        private static Dictionary<string, List<string>> RoomUsers { get; set; }
        private static Dictionary<string, int> UserConnections { get; set; }
        private static int LastConnectionId { get; set; }

        private static Dictionary<string, List<ChatEndpoint>> UserEndPoints { get; set; }
        private static Dictionary<string, Socket> EndPointSockets { get; set; }
        private static Dictionary<string, ChatIndentity> UserIndentities { get; set; }
        private static Dictionary<string, ChatIndentity> RoomIndentities { get; set; }

        private static readonly object LockOps = new();
        public ChatServer()
        {
            EnsureLookups();
        }
        public static void EnsureLookups()
        {
            lock (LockOps)
            {
                RoomsAvailable ??= new();
                UsersConnected ??= new();
                RoomUsers ??= new();
                UserConnections ??= new();
                EndPointSockets ??= new();
                UserEndPoints ??= new();
                UserIndentities ??= new();
                RoomIndentities ??= new();
            }
        }

        #region [ Helpers ]
        public static int ConnectSession(ChatEndpoint endpoint, Socket socket)
        {
            if (endpoint == null || string.IsNullOrWhiteSpace(endpoint.IpAddress)) { throw new ArgumentNullException(nameof(endpoint), "You must supply an endpoint"); }
            lock (LockOps)
            {
                if (!UserConnections.TryGetValue(endpoint.ToString(), out int connectionId))
                {
                    LastConnectionId++;
                    connectionId = LastConnectionId;
                    UserConnections.Add(endpoint.ToString(), connectionId);
                }
                //Add User Socket
                if (socket != null) 
                {
                    EndPointSockets[endpoint.ToString()] = socket;
                }
                return connectionId;
            }
        }
        public static int? GetConnectionId(ChatEndpoint endpoint)
        {
            string Id = endpoint.ToString();
            lock (LockOps)
            {
                if (UserConnections.TryGetValue(Id, out int value))
                {
                    return value;
                }
            }
            return null;
        }
        public static void ConnectAsUser(ChatIndentity indentity)
        {
            if (indentity == null) { throw new ArgumentNullException(nameof(indentity), "You must supply an indentity"); }
            if (string.IsNullOrWhiteSpace(indentity.Id)) { throw new NullReferenceException("Id is blank."); }
            if (string.IsNullOrEmpty(indentity.IndentityType) || indentity.IndentityType != ChatIndentity.UserType) { throw new Exception("Only Users can connect to a server"); }
            lock (LockOps)
            {
                if (!UsersConnected.Contains(indentity.Id))
                {
                    //Check is Id is for a User
                    var check = GetUser(indentity.Id);
                    if (check != null && check.Id == indentity.Id)
                    {
                        UsersConnected.Add(indentity.Id);
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
        public static int ConnectUserIndentity(ChatIndentity chatIndentity, ChatEndpoint endpoint)
        {

            if (chatIndentity != null)
            {
                if (!UserEndPoints.TryGetValue(endpoint.ToString(), out List<ChatEndpoint> ueps))
                {
                    ueps = new List<ChatEndpoint>();
                }
                if (ueps.FirstOrDefault(x => x.ToString() == endpoint.ToString()) == null)
                {
                    ueps.Add(endpoint);
                    UserEndPoints[chatIndentity.Id] = ueps;
                }

                return GetConnectionId(endpoint) ?? 0;
            }
            return 0;
        }

        public static ChatIndentity GetUserIndentity(string Id)
        {
            lock (LockOps)
            {
                if (UserIndentities.TryGetValue(Id, out ChatIndentity value))
                {
                    return value;
                }
            }
            return null;
        }

        public static ChatIndentity GetRoomIndentity(string Id)
        {
            lock (LockOps)
            {
                if (RoomIndentities.TryGetValue(Id, out ChatIndentity value))
                {
                    //Rebuild Users in Room
                    value.SubIdentities = new();
                    if (RoomUsers != null && RoomUsers.TryGetValue(Id, out List<string> users))
                    {
                        foreach (var u in users)
                        {
                            var user = GetUserIndentity(u);
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

                var room = GetRoomIndentity(roomId);
                if(room != null)
                {
                    if(room.SubIdentities != null)
                    {
                        foreach (var  u in room.SubIdentities)
                        {
                            if (UserEndPoints != null && UserEndPoints.TryGetValue(u.Id, out List<ChatEndpoint> value))
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

                        if (EndPointSockets != null && EndPointSockets.TryGetValue(ep, out Socket value)) 
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
                if (UserEndPoints != null && UserEndPoints.TryGetValue(userId, out List<ChatEndpoint> value))
                {
                    return value;
                }
                else
                {
                    throw new Exception("User does not exist!!!");
                }
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
                    if (EndPointSockets != null && EndPointSockets.TryGetValue(endpoint.ToString(), out Socket value))
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
                if (RoomsAvailable == null || !RoomsAvailable.Contains(roomId)) { throw new Exception("Room doesn't exist"); }
                if (RoomUsers.TryGetValue(roomId, out List<string> users))
                {

                    var user = GetUserIndentity(userId);
                    if (user != null)
                    {
                        users.Add(userId);
                        RoomUsers[roomId] = users;
                    }
                    else
                    {
                        throw new Exception("User does not exist!!!");
                    }
                }
            }
        }
        public static ChatIndentity UserToChatIndentity(IUserDef userDef, string status = "Connected")
        {
            return new ChatIndentity()
            {
                Id = userDef.UserId,
                IndentityType = ChatIndentity.UserType,
                Name = userDef.Name,
                Status = status
            };
        }

        public static ChatIndentity RoomToChatIndentity(IRoomDef roomDef, string status = "Active")
        {
            return new ChatIndentity()
            {
                Id = roomDef.RoomId,
                IndentityType = ChatIndentity.UserType,
                Name = roomDef.Name,
                Status = status,
                Topic = roomDef.Topic
            };
        }
        public static List<ChatIndentity> GetIndentities(MessageIndentityRequest indentityRequest)
        {
            lock (LockOps)
            {
                List<ChatIndentity> list = new();
                if (indentityRequest != null)
                {
                    switch (indentityRequest.InquiryType)
                    {
                        case MessageIndentityInquiryType.AvailableUsers: //Only For Demo
                            IUserRepository userRepository = new SimpleUserRepository();
                            var users = userRepository.GetUsers();
                            if (users != null)
                            {
                                List<ChatIndentity> availableUsers = new();
                                foreach (var u in users)
                                {
                                    if (UsersConnected == null || !UserConnections.ContainsKey(u.UserId))
                                    {
                                        availableUsers.Add(UserToChatIndentity(u, "Available"));
                                    }
                                }
                                list.AddRange(availableUsers);
                            }
                            break;
                        case MessageIndentityInquiryType.CurrentRoom:
                            if (indentityRequest.Current != null)
                            {
                                var currentRoom = GetRoomIndentity(indentityRequest.Current.Id);
                                if (currentRoom != null)
                                {
                                    list.Add(currentRoom);
                                }
                            }
                            break;
                        case MessageIndentityInquiryType.CurrentUser:
                            if (indentityRequest.Current != null)
                            {
                                var currentUser = GetUserIndentity(indentityRequest.Current.Id);
                                if (currentUser != null)
                                {
                                    list.Add(currentUser);
                                }
                            }
                            break;
                        case MessageIndentityInquiryType.Rooms:
                            list.AddRange(RoomIndentities.Values);
                            break;
                        case MessageIndentityInquiryType.Users:
                            list.AddRange(UserIndentities.Values);
                            break;
                        case MessageIndentityInquiryType.All:
                        default:
                            list.AddRange(UserIndentities.Values);
                            list.AddRange(RoomIndentities.Values);
                            break;
                    }
                }
                return list;
            }
        }
        #endregion [ Helpers ]
        #region [ Cache ]
        private static ChatIndentity GetUser(string Id)
        {
            if (UserIndentities.TryGetValue(Id, out ChatIndentity value))
            {
                return value;
            }

            //Change to Service lookup
            IUserRepository rep = new SimpleUserRepository();

            var users = rep.GetUsers(x => x.UserId == Id);
            if (users != null && users.Count == 1)
            {
                var user = users[0];
                ChatIndentity indentity = UserToChatIndentity(user);
                UserIndentities[Id] = indentity;
                return indentity;
            }
            return null;
        }
        #endregion [ Cache ]
    }
}
