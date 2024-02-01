using MauiChatApp.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class ChatMessageRequest <T> : MessageRequest
    {
        public T Data { get; set; }
    }
}
