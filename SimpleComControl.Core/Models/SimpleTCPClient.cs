using SimpleComControl.Core.Bases;
using System.Net;
using System.Net.Sockets;

namespace SimpleComControl.Core.Models
{
    public class SimpleTCPClient : ComTCPSocket
    {
        public SimpleTCPClient() : base(new SimpleComMessageHandler())
        {
        }
        public bool IsRunning { get; private set; }
        private void Worker()
        {
            IsRunning = true;
            while (IsRunning)
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

            if (_socket.Connected && !IsRunning)
            {
                Console.WriteLine($"Client running on port: {port}...");
                var workerTh = new Thread(() => { Worker(); });
                workerTh.Start();
                IsRunning = true;
            }
        }
        public void Stop()
        {
            IsRunning = false;
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
