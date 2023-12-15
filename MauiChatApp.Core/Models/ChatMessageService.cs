using MauiChatApp.Core.Enums;
using SimpleComControl.Core.Helpers;

namespace MauiChatApp.Core.Models
{
    public class ChatMessageService
    {
        private static int UserConnectionId { get; set; }
        private static ChatIndentity UserIndentity { get; set; }
        public static List<ChatMessage> HistoricalDisplayMessages { get; set; }
        public static Dictionary<string, ChatIndentity> CachedIndentities { get; set; }
        public bool IsInitialized { get; set; }

        //Room or Private Chat
        public string ChatWindowId { get; set; }
        public List<DisplayMessage> DisplayMessages { get; set; }

        private static readonly object lockMessages = new();

        public static void SetConnectionId(int connnectionId) 
        {
            UserConnectionId = connnectionId;
        }
        public static void SetUserIndentity(ChatIndentity userIndentity) 
        {
            UserIndentity = userIndentity;
        }
        public static ChatMessage CreateNewIndentityRequest(MessageIndentityInquiryType inquiryType, ChatIndentity current = null)
        {
            var request = new MessageIndentityRequest() {  Indentity = UserIndentity, Current = current, InquiryType = inquiryType  };
            return new ChatMessage() {
                ConnectionId = UserConnectionId,
                FromEntityId = UserIndentity == null ? "" : UserIndentity.Id,
                ToEntityId = "Server",
                MessageType = SimpleComControl.Core.Enums.ComMessageType.IndentityInfo,
                Message = request.ToJson(false)
            };
        }
        public static ChatMessage CreateNewTestRequest()
        {
            var request = new ChatMessageRequest<string>() { Indentity = UserIndentity, Data ="Testing" };
            return new ChatMessage()
            {
                ConnectionId = UserConnectionId,
                FromEntityId = UserIndentity == null ? "" : UserIndentity.Id,
                ToEntityId = "Server",
                MessageType = SimpleComControl.Core.Enums.ComMessageType.TestMessage,
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
