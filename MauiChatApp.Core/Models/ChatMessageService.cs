using MauiChatApp.Core.Enums;
using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class ChatMessageService
    {
        private static int UserConnectionId { get; set; }
        private static ChatIdentity UserIdentity { get; set; }
        public static List<ChatMessage> HistoricalDisplayMessages { get; set; }
        public static Dictionary<string, ChatIdentity> CachedIdentities { get; set; }
        public bool IsInitialized { get; set; }

        //Room or Private Chat
        public string ChatWindowId { get; set; }
        public List<DisplayMessage> DisplayMessages { get; set; }

        private static readonly object lockMessages = new();

        public static void SetConnectionId(int connnectionId)
        {
            UserConnectionId = connnectionId;
        }
        public static void SetUserIdentity(ChatIdentity userIdentity)
        {
            UserIdentity = userIdentity;
        }
        public static ChatMessage CreateNewIdentityRequest(MessageIdentityInquiryType inquiryType, ChatIdentity current = null)
        {
            var request = new MessageIdentityRequest() { Identity = UserIdentity, Current = current, InquiryType = inquiryType };
            return new ChatMessage()
            {
                ConnectionId = UserConnectionId,
                FromEntityId = UserIdentity == null ? "" : UserIdentity.Id,
                ToEntityId = IComMessageHandler.ServerId,
                MessageType = SimpleComControl.Core.Enums.ComMessageType.IdentityInfo,
                Message = request.ToJson(false)
            };
        }
        public static ChatMessage CreateNewTestRequest()
        {
            var request = new ChatMessageRequest<string>() { Identity = UserIdentity, Data = "Testing" };
            return new ChatMessage()
            {
                ConnectionId = UserConnectionId,
                FromEntityId = UserIdentity == null ? "" : UserIdentity.Id,
                ToEntityId = IComMessageHandler.ServerId,
                MessageType = SimpleComControl.Core.Enums.ComMessageType.TestMessage,
                Message = request.ToJson(false)
            };
        }
        public static ChatMessage CreateNewConnectRequest(bool connectAs = false, ChatIdentity current = null)
        {
            current ??= UserIdentity;

            var request = new MessageConnectRequest() { ConnectAs = connectAs, Identity = current };

            return new ChatMessage()
            {
                ConnectionId = UserConnectionId,
                FromEntityId = UserIdentity == null ? "" : UserIdentity.Id,
                ToEntityId = IComMessageHandler.ServerId,
                MessageType = SimpleComControl.Core.Enums.ComMessageType.Connnect,
                Message = request.ToJson(false)
            };
        }

        public static ChatMessage CreatePingMessage(ChatIdentity ping, ChatHopChain startHop = null, ChatIdentity current = null)
        {
            current ??= UserIdentity;
            if (current == null) { throw new ArgumentNullException(nameof(current), "You must supply a current identity."); }
            if (ping == null) { throw new ArgumentNullException(nameof(ping), "You must supply a identity to ping."); }


            startHop ??= new();
            startHop.Requestor = current;
            if (startHop.ChainPosition > 0 && !startHop.HasNextHop()) { throw new Exception(""); }
            startHop = startHop.GetNextHop(current, ping);
            var request = new MessagePingRequest() { Identity = ping, HopChain = startHop };

            return new ChatMessage()
            {
                ConnectionId = UserConnectionId,
                FromEntityId = UserIdentity == null ? "" : UserIdentity.Id,
                ToEntityId = ping.Id,
                MessageType = SimpleComControl.Core.Enums.ComMessageType.Ping,
                Message = request.ToJson(false)
            };
        }


        public static bool HasMessagesToProcess()
        {
            return IncomingMessages?.Count > 0;
        }
        private static List<ChatMessage> IncomingMessages { get; set; }
        public static void AddIncomingMessages(List<ChatMessage> messages)
        {
            lock (lockMessages)
            {
                IncomingMessages ??= new();
                IncomingMessages.AddRange(messages);
                NewMessagesAddedChanged();
            }
        }
        public int ProcessIncomingMessages()
        {
            int processed = 0;
            lock (lockMessages)
            {
                IncomingMessages ??= new();
                DisplayMessages ??= new();
                HistoricalDisplayMessages ??= new();

                if (!IsInitialized)
                {
                    IsInitialized = true;
                    if (DisplayMessages.Count == 0 && HistoricalDisplayMessages.Count > 0)
                    {
                        IncomingMessages.InsertRange(0, HistoricalDisplayMessages);
                    }
                }

                foreach (ChatMessage msg in IncomingMessages)
                {
                    if (msg != null && !string.IsNullOrWhiteSpace(msg.Message) && msg.IsValid())
                    {
                        HistoricalDisplayMessages.Add(msg);
                        if (msg.ToEntityId == ChatWindowId && msg.MessageType == SimpleComControl.Core.Enums.ComMessageType.SentMessage)
                        {
                            //Filter, Convert, Apply to Output as needed
                            //DisplayMessages.Add(msg.message);
                        }
                        else
                        {

                        }
                    }
                }
                IncomingMessages = new List<ChatMessage>();
                while (DisplayMessages.Count > 25)
                {
                    DisplayMessages.RemoveAt(0);
                }
                while (HistoricalDisplayMessages.Count > 500)
                {
                    HistoricalDisplayMessages.RemoveAt(0);
                }
            }

            return processed;
        }

        public static event Action OnNewMessagesAdded;
        private static void NewMessagesAddedChanged()
        {
            OnNewMessagesAdded?.Invoke();
        }

    }
}
