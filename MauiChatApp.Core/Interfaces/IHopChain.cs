using MauiChatApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Interfaces
{
    public interface IHopChain
    {
        public ChatHopChain HopChain { get; set; }
    }
}
