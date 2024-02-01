using MauiChatApp.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class SimpleUserRepository : IUserRepository
    {
        public class SimpleUser : IUserDef
        {
            public string Name { get; set; }
            public string UserId { get; set; }
        }
        private static List<SimpleUser> _users = new();
        private bool disposedValue;

        public List<IUserDef> GetUsers(Func<IUserDef, bool> exp = null)
        {
            SeedUsers();
            var q = _users.Cast<IUserDef>().Where(x=> 1==1);

            if (exp != null) { q = _users.Where(exp); }

            return q.ToList();
        }


        private static void SeedUsers()
        {
            _users ??= new List<SimpleUser>();

            if (_users.Count == 0) 
            {
                _users.Add(new SimpleUser() { Name = "Kirk", UserId = "1" });
                _users.Add(new SimpleUser() { Name = "Spock", UserId = "2" });
                _users.Add(new SimpleUser() { Name = "Bones", UserId = "3" });
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _users = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SimpleUserRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
