using MauiChatApp.Core.Models;
namespace MauiChatApp.Core.Tests
{
    [TestClass]
    public class UserUnitTests
    {
        [TestMethod]
        public void TestGetUsers()
        {
            SimpleUserRepository repo = new();
            var users = repo.GetUsers(x => x.Name != "");
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count > 0);
        }
    }
}