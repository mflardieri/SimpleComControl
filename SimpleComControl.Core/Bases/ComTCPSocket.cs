using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SimpleComControl.Core.Interfaces;

namespace SimpleComControl.Core.Bases
{
    public abstract class ComTCPSocket
    {
        public Socket _socket;
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private IComMessageHandler _comMessageHandler { get; set; }
     
        public ComTCPSocket(IComMessageHandler comMessageHandler)
        {
            //_encoding = Encoding.ASCII;
            _comMessageHandler = comMessageHandler;
        }
     
        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }
        public bool Send(IComMessage comMessage
                         )
        {

            bool rtnVal = false;

            byte[] data = _comMessageHandler.Convert(comMessage);

     
            IPEndPoint ep = (IPEndPoint)_socket.RemoteEndPoint;
            try
            {

                _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, ep, (ar) =>
                {
                    State so = (State)ar.AsyncState;
                    int bytes = _socket.EndSend(ar);
                    rtnVal = true;
                }, state);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send {ex.Message}");
            }
            return false;
        }

        public void Receive(Socket? targetSocket = null)
        {
            if (targetSocket == null) { targetSocket = _socket; }
            int bytes = 0;
            bytes = targetSocket.Receive(state.buffer, SocketFlags.None);
            if (bytes > 0)
            {
                var msg = _comMessageHandler.Convert(state.buffer, bytes);
                if (msg != null && msg.IsValid())
                {
                    _comMessageHandler.ProcessMessage(this, targetSocket, msg);
                }
             }
        }
    }
}
