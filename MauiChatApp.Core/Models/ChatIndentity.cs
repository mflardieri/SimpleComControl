using SimpleComControl.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class ChatIndentity : IComIdentity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IndentityType { get; set; }
        public string Status { get; set; }

        public string Topic { get; set; }
        public List<ChatIndentity> SubIdentities { get; set; }
        public object GetIdentityId()
        {
            return Id;
        }
        public string GetIdentityName()
        {
            return Name;
        }
        public string GetIdentityStatus()
        {
            return Status;
        }
        public string GetIdentityType()
        {
            return IndentityType;
        }
        public List<IComIdentity> GetSubIdentities()
        {
            return SubIdentities.Cast<IComIdentity>().ToList();
        }
    }
}
