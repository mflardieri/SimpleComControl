using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Interfaces;

namespace SimpleComControl.Core.Models
{
    public class SimpleComMessage : IComMessage
    {
        public ComMessageType MessageType { get; set; }

        public string Message { get; set; }

        public int GetConnectionId()
        {
            throw new NotImplementedException();
        }

        public IComIdentity GetFromIdentity()
        {
            throw new NotImplementedException();
        }

        public string GetMessageBody()
        {
            return Message;
        }

        public IComIdentity GetToIdentity()
        {
            throw new NotImplementedException();
        }

        public int GetToMessageType()
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            //
            return true;
        }
    }
}
