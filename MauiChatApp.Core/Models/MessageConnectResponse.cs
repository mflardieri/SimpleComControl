using MauiChatApp.Core.Bases;

namespace MauiChatApp.Core.Models
{
    public class MessageConnectResponse : MessageResponse<int>
    {
        public ChatIdentity RoomRef { get; set; }
    }
}
