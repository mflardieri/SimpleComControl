using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class DisplayMessage
    {
        public ChatIdentity From {  get; set; }
        public string Message { get; set; }
    }
}
