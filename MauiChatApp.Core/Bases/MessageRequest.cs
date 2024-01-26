using MauiChatApp.Core.Models;

namespace MauiChatApp.Core.Bases
{
    public abstract class MessageRequest
    {
        public ChatIdentity Identity { get; set; }
    }
}
