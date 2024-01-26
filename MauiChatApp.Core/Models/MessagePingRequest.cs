using MauiChatApp.Core.Bases;
using MauiChatApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class MessagePingRequest : MessageRequest, IHopChain
    {
        public ChatHopChain HopChain { get; set; }

        public ChatIdentity GetPingedIdentity()
        {
            ChatIdentity rtnVal = null;

            if (!ChatHopChain.IdentityMatches(HopChain.Requestor, Identity))
            {
                rtnVal = Identity;
            }
            else
            {
                if (HopChain.IdentityChain != null && HopChain.IdentityChain.Count >= 3)
                {
                    rtnVal = HopChain.IdentityChain[2];
                }
            }
            return rtnVal;
        }
    }
}
