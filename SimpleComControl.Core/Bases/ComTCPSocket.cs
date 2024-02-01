using SimpleComControl.Core.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace SimpleComControl.Core.Bases
{
    public abstract class ComTCPSocket
    {
        public Socket? _socket;
        private const int bufSize = 8 * 1024;
        private readonly State state = new();
        //private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private IComMessageHandler ComMessageHandler { get; set; }

        public ComTCPSocket(IComMessageHandler comMessageHandler)
        {
            //_encoding = Encoding.ASCII;
            ComMessageHandler = comMessageHandler;
        }

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public bool Send(IComMessage comMessage)
        {
            bool rtnVal = false;
            if (_socket != null)
            {
                rtnVal = Send(comMessage, _socket);
            }
            return rtnVal;

        }


        public bool Send(IComMessage comMessage, Socket targetSocket)
        {
            bool rtnVal = false;

            byte[] data = ComMessageHandler.Convert(comMessage);

            if (targetSocket != null && targetSocket.RemoteEndPoint != null && targetSocket.RemoteEndPoint is IPEndPoint ep)
            {
                try
                {
                    if (ep != null)
                    {
                        targetSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, ep, (ar) =>
                        {
                            int bytes = targetSocket.EndSend(ar);
                            rtnVal = true;
                        }, state);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send {ex.Message}");
                }
            }
            return rtnVal;
        }

        public void Receive(Socket? targetSocket = null)
        {
            targetSocket ??= _socket;
            if (targetSocket != null)
            {
                int bytes = targetSocket.Receive(state.buffer, SocketFlags.None);
                if (bytes > 0)
                {
                    var msg = ComMessageHandler.Convert(state.buffer, bytes);
                    if (msg != null && msg.IsValid())
                    {
                        ComMessageHandler.ProcessMessage(this, targetSocket, msg);
                    }
                }
            }
        }
    }
}
