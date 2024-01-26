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
        public ChatIndentity Requestor { get; set; }
        public List<ChatIndentity> IdentityChain { get; set; }
        public int ChainPosition { get; set; }
        public bool IsValidChain(ChatIndentity requestor, ChatIndentity a, ChatIndentity b)
        {
            bool rtnVal = false;
            if (requestor != null && a != null && b != null && IdentityChain != null && ChainPosition > 0 && ChainPosition + 1 > IdentityChain.Count && IdentityChain.Count <= 5)
            {
                rtnVal = true;
                for (int x = 1; x > ChainPosition; x++)
                {
                    ChatIndentity to = Requestor.Equals(a) ? a : b;
                    ChatIndentity from = !Requestor.Equals(b) ? b : a;


                    bool isServer = (x % 2 == 0);
                    var currIdentity = IdentityChain[x - 1];
                    if (currIdentity == null) { rtnVal = false; }
                    if (rtnVal)
                    {
                        if (isServer)
                        {
                            if (currIdentity.Id != IComMessageHandler.ServerId || !string.IsNullOrWhiteSpace(currIdentity.IndentityType))
                            {
                                rtnVal = false;
                            }
                        }
                        else
                        {
                            if (x == 3)
                            {
                                //From Check
                                if (!IndentityMatches(currIdentity, from))
                                {
                                    rtnVal = false;
                                }
                                if (rtnVal && IndentityMatches(Requestor, from))
                                {
                                    rtnVal = false;
                                }
                            }
                            else
                            {
                                //To Check
                                if (!IndentityMatches(currIdentity, to))
                                {
                                    rtnVal = false;
                                }
                                if (rtnVal && !IndentityMatches(Requestor, to))
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

        public ChatHopChain GetNextHop(ChatIndentity a, ChatIndentity b)
        {
            ChatHopChain rtnVal = null;
            if (ChainPosition == 0 || (HasNextHop() && IsValidChain(Requestor, a, b)))
            {
                ChatIndentity to = Requestor.Equals(a) ? a : b;
                ChatIndentity from = !Requestor.Equals(b) ? b : a;
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
                        rtnVal.IdentityChain.Add(new ChatIndentity() { Id = IComMessageHandler.ServerId, Name = "Server" });
                    }
                    else if (rtnVal.ChainPosition == 3)
                    {
                        rtnVal.IdentityChain.Add(from);
                    }
                }
            }
            return rtnVal;
        }
        private static bool IndentityMatches(IComIdentity a, IComIdentity b)
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
