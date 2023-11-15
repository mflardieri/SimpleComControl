using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Enums
{
    public enum ComMessageType
    {
        Connnect = 1,
        ConnectedMessage,
        Ping,
        Pinged,
        Disconnect,
        DisconnectedMessage,
        IndentityInfo,
        SentMessage,
        ReceivedMessage,
        ResendMessage,
        TestMessage,
        HelpMessage,
    }
}
