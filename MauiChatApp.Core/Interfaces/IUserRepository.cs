namespace MauiChatApp.Core.Interfaces
{
    public interface IUserRepository
    {
        public List<IUserDef> GetUsers(Func<IUserDef, bool> exp = null);
    }
}
