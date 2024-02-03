using MauiChatApp.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class MessageDisconnectRequest : MessageRequest
    {
        public bool DisconnectAs { get; set; }
        public ChatIdentity RoomDisconnect { get; set; }
    }
}
