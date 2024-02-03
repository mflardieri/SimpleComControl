namespace MauiChatApp.Core.Models
{
    public class ChatEndpoint
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }
    }
}
