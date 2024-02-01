using MauiChatApp.Core.Enums;

namespace MauiChatApp.Core.Bases
{
    public abstract class MessageResponse<T>
    {
        public bool IsSuccess { get; set; }
        public MessageResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public T Result { get; set; }
    }
}
