namespace MauiChatApp.Core.Interfaces
{
    public interface IUserRepository: IDisposable
    {
        public List<IUserDef> GetUsers(Func<IUserDef, bool> exp = null);
    }
}
