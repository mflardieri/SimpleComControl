namespace MauiChatApp.Core.Models
{
    public class DisplayMessage
    {
        public ChatIdentity From { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        /// <summary>
        /// If unset this will be treated as a private message to a user.
        /// </summary>
        public string RoomId { get; set; }
        
        public DisplayMessage()
        { 
        }
        public DisplayMessage(ChatIdentity from, string message, string roomId = "")
        {
            From = from;
            RoomId = roomId;
            Message = message;
            EnsureMessageIsValid();
        }
        public void EnsureMessageIsValid()
        {
            if (From == null) { throw new ArgumentNullException(nameof(From)); }
            if (string.IsNullOrWhiteSpace(From.Id) || From.IdentityType <= 0) { new NullReferenceException("Invalid from identity"); }
            if (string.IsNullOrWhiteSpace(Message)) { throw new ArgumentNullException(nameof(Message)); }
        }
    }
}
