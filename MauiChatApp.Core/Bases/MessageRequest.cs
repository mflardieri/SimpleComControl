using MauiChatApp.Core.Interfaces;
using MauiChatApp.Core.Models;

namespace MauiChatApp.Core.Bases
{
    public abstract class MessageRequest : IChatIdentityItem
    {
        public ChatIdentity Identity { get; set; }
    }
}
