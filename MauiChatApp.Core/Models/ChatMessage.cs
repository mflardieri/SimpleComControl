using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class ChatMessage : IComMessage
    {
        public ComMessageType MessageType { get; set; }

        public string Message { get; set; }

        public int ConnectionId { get; set; }

        public string FromEntityId { get; set; }

        public string ToEntityId { get; set; }


        public int GetConnectionId()
        {
            return ConnectionId;
        }

        public IComIdentity GetFromIdentity()
        {
            //Cache Repo
            throw new NotImplementedException();
        }

        public string GetMessageBody()
        {
            return Message;
        }

        public IComIdentity GetToIdentity()
        {
            //Cache Repo
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            //TODO Add Validation

            return true;
        }
    }
}
