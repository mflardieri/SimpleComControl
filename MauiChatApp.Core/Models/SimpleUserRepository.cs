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
        private static List<SimpleUser> _users = new List<SimpleUser>();
        public List<IUserDef> GetUsers(Func<IUserDef, bool> exp = null)
        {
            SeedUsers();
            var q = _users.Cast<IUserDef>().Where(x=> 1==1);

            if (exp != null) { q = _users.Where(exp); }

            return q.ToList();
        }


        public void SeedUsers()
        {
            if (_users == null) { _users = new List<SimpleUser>();  }
            if (_users.Count == 0) 
            {
                _users.Add(new SimpleUser() { Name = "Kirk", UserId = "1" });
                _users.Add(new SimpleUser() { Name = "Spock", UserId = "2" });
                _users.Add(new SimpleUser() { Name = "Bones", UserId = "3" });
            }
        }
    }
}
