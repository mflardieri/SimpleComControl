using SimpleComControl.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Models
{
    public class SimpleTCPClient : ComTCPSocket
    {
        public SimpleTCPClient() : base(new SimpleComMessageHandler())
        {
        }
        public bool isRunning { get; private set; }
        private void worker()
        {
            isRunning = true;
            while (isRunning)
            {
                if (_socket.Connected)
                {
                    try
                    {
                        Receive();
                    }
                    catch { }

                }
            }
        }
        public void Start(string address, int port, string serverAddress, int serverPort)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            _socket.Connect(IPAddress.Parse(serverAddress), serverPort);

            if (_socket.Connected && !isRunning)
            {
                Console.WriteLine($"Client running on port: {port}...");
                var workerTh = new Thread(() => { worker(); });
                workerTh.Start();
                isRunning = true;
            }
        }
        public void Stop()
        {
            isRunning = false;
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try { _socket.Close(); } catch { }
        }
        public void Dispose()
        {
            Stop();
        }
    }
}
