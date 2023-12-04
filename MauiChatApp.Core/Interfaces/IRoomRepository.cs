namespace MauiChatApp.Core.Interfaces
{

    public interface IRoomRepository
    {
        public List<IRoomDef> GetRooms(Func<IRoomDef, bool> exp);
    }
}
