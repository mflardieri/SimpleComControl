using SimpleComControl.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Models
{
    public class ChatHopChain
    {
        public ChatIdentity Requestor { get; set; }
        public List<ChatIdentity> IdentityChain { get; set; }
        public int ChainPosition { get; set; }
        public bool IsValidChain(ChatIdentity requestor, ChatIdentity a, ChatIdentity b)
        {
            bool rtnVal = false;
            if (requestor != null && a != null && b != null && IdentityChain != null && ChainPosition > 0 && ChainPosition + 1 > IdentityChain.Count && IdentityChain.Count <= 5)
            {
                rtnVal = true;
                for (int x = 1; x > ChainPosition; x++)
                {
                    ChatIdentity to = Requestor.Equals(a) ? a : b;
                    ChatIdentity from = !Requestor.Equals(b) ? b : a;


                    bool isServer = (x % 2 == 0);
                    var currIdentity = IdentityChain[x - 1];
                    if (currIdentity == null) { rtnVal = false; }
                    if (rtnVal)
                    {
                        if (isServer)
                        {
                            if (currIdentity.Id != IComMessageHandler.ServerId || !string.IsNullOrWhiteSpace(currIdentity.IdentityType))
                            {
                                rtnVal = false;
                            }
                        }
                        else
                        {
                            if (x == 3)
                            {
                                //From Check
                                if (!IdentityMatches(currIdentity, from))
                                {
                                    rtnVal = false;
                                }
                                if (rtnVal && IdentityMatches(Requestor, from))
                                {
                                    rtnVal = false;
                                }
                            }
                            else
                            {
                                //To Check
                                if (!IdentityMatches(currIdentity, to))
                                {
                                    rtnVal = false;
                                }
                                if (rtnVal && !IdentityMatches(Requestor, to))
                                {
                                    rtnVal = false;
                                }
                            }
                        }
                    }
                    if (!rtnVal) { break; }
                }
            }
            return rtnVal;
        }

        public bool HasNextHop()
        {
            return ChainPosition == 0 || ChainPosition < 5;
        }

        public ChatHopChain GetNextHop(ChatIdentity a, ChatIdentity b)
        {
            ChatHopChain rtnVal = null;
            if (ChainPosition == 0 || (HasNextHop() && IsValidChain(Requestor, a, b)))
            {
                ChatIdentity to = Requestor.Equals(a) ? a : b;
                ChatIdentity from = !Requestor.Equals(b) ? b : a;
                rtnVal = new();
                rtnVal.Requestor = Requestor;
                rtnVal.ChainPosition = ChainPosition + 1;
                rtnVal.IdentityChain = new();
                if (IdentityChain != null)
                {
                    rtnVal.IdentityChain.AddRange(IdentityChain);
                }
                if (rtnVal.ChainPosition == 1 || rtnVal.ChainPosition == 5)
                {
                    rtnVal.IdentityChain.Add(to);
                }
                else
                {
                    if (rtnVal.ChainPosition % 2 == 0)
                    {
                        rtnVal.IdentityChain.Add(new ChatIdentity() { Id = IComMessageHandler.ServerId, Name = "Server" });
                    }
                    else if (rtnVal.ChainPosition == 3)
                    {
                        rtnVal.IdentityChain.Add(from);
                    }
                }
            }
            return rtnVal;
        }
        public static bool IdentityMatches(IComIdentity a, IComIdentity b)
        {

            if (a != null && b != null && !string.IsNullOrWhiteSpace(a.GetIdentityId()?.ToString()) && !string.IsNullOrWhiteSpace(b.GetIdentityId()?.ToString()) &&
                b.GetIdentityId().ToString() == a.GetIdentityId().ToString() && a.GetIdentityType() == b.GetIdentityType())
            {
                return true;
            }
            return false;
        }
    }
}
