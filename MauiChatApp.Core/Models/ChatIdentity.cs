using SimpleComControl.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class ChatIdentity : IComIdentity
    {
        public const int UserType = 1;
        public const int RoomType = 9;
        public string Id { get; set; }
        public string Name { get; set; }
        public int IdentityType { get; set; }
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
        public int GetIdentityType()
        {
            return IdentityType;
        }
        public List<IComIdentity> GetSubIdentities()
        {
            return SubIdentities.Cast<IComIdentity>().ToList();
        }
    }
}
