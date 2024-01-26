using SimpleComControl.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class ChatIdentity : IComIdentity
    {
        public const string UserType = "U";
        public const string RoomType = "R";
        public string Id { get; set; }
        public string Name { get; set; }
        public string IdentityType { get; set; }
        public string Status { get; set; }

        public string Topic { get; set; }
        public List<ChatIdentity> SubIdentities { get; set; }
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
            return IdentityType;
        }
        public List<IComIdentity> GetSubIdentities()
        {
            return SubIdentities.Cast<IComIdentity>().ToList();
        }
    }
}
