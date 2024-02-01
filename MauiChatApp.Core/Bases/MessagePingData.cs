using MauiChatApp.Core.Interfaces;
using MauiChatApp.Core.Models;

namespace MauiChatApp.Core.Bases
{
    public class MessagePingData : IHopChain, IChatIdentityItem
    {
        public ChatIdentity Identity { get; set; }
        public ChatHopChain HopChain { get; set; }

        public ChatIdentity GetPingedIdentity()
        {
            if (!ChatHopChain.IdentityMatches(HopChain.Requestor, Identity))
            {
                return Identity;
            }
            else
            {
                if (HopChain.IdentityChain != null && HopChain.IdentityChain.Count >= 3)
                {
                    return HopChain.IdentityChain[2];
                }
            }
            return null;
        }
        public ChatIdentity GetRequestorIdentity()
        {
            if (HopChain.IdentityChain != null && HopChain.IdentityChain.Count > 0)
            {
                return HopChain.IdentityChain[0];
            }
            else
            {
                return Identity;

            }
        }
    }
}
