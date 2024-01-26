using MauiChatApp.Core.Bases;
using MauiChatApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class MessagePingRequest: MessageRequest, IHopChain
    {
        public ChatHopChain HopChain { get; set; }
    }
}
