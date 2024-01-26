using MauiChatApp.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class MessagePingResponse : MessageResponse<ChatHopChain>
    {
        public bool IsComplete { get; set; }
    }
}
