using SimpleComControl.Core.Enums;

namespace SimpleComControl.Core.Interfaces
{
    public interface IComMessage
    {
        public ComMessageType MessageType { get; set; }
        public int GetConnectionId();
        public string GetMessageBody();
        public IComIdentity GetFromIdentity();
        public IComIdentity GetToIdentity();
        public string GetToMessageType();

        public bool IsValid();
    }
}
