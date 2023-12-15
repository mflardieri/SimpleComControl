using MauiChatApp.Core.Models;

namespace MauiChatApp.Core.Bases
{
    public abstract class MessageRequest
    {
        public ChatIndentity Indentity { get; set; }
    }
}
