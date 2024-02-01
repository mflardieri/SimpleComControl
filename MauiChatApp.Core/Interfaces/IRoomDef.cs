namespace MauiChatApp.Core.Interfaces
{
    public interface IRoomDef
    {
        public string RoomId { get;}
        public string Name { get;}
        public string Topic { get; }
        public string TopicSetById { get; }
        public int MaxUsers { get; }
    }
}
