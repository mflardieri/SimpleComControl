using MauiChatApp.Core.Models;

namespace MauiChatApp.Core.Tests
{
    [TestClass]
    public class RoomUnitTests
    {
        [TestMethod]
        public void TestGetRooms() 
        {
            SimpleRoomRepository repo = new();
            var rooms = repo.GetRooms(x => x.Name !="");
            Assert.IsNotNull(rooms);
            Assert.IsTrue(rooms.Count > 0);
        }
    }
}
