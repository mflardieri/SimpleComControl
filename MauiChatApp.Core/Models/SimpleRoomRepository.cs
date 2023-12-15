using MauiChatApp.Core.Interfaces;

namespace MauiChatApp.Core.Models
{
    public class SimpleRoomRepository : IRoomRepository
    {
        public class SimpleRoom : IRoomDef
        {
            public string RoomId { get; set; }

            public string Name { get; set; }

            public string Topic { get; set; }

            public string TopicSetById { get; set; }

            public int MaxUsers { get; set; }
        }

        private static List<SimpleRoom> _rooms = new ();

        public List<IRoomDef> GetRooms(Func<IRoomDef, bool> exp = null)
        {
            SeedRooms();

            var q = _rooms.Cast<IRoomDef>().Where(x => 1 == 1);
            
            if (exp != null) { q= _rooms.Where(exp);  }

            return q.ToList();
        }

        private static void SeedRooms() 
        {
            _rooms ??= new List<SimpleRoom>();
            if (_rooms.Count == 0) 
            {
                _rooms.Add(new SimpleRoom() {  MaxUsers = 100, Name = "Main", RoomId = "0", Topic ="Welcome!!!" });
            }
        }
    }
}
