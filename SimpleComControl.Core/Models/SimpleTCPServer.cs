using SimpleComControl.Core.Bases;
using SimpleComControl.Core.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace SimpleComControl.Core.Models
{
    public class SimpleTCPServer : ComTCPSocket, IDisposable, IComServer
    {
        public SimpleTCPServer() : base(new SimpleComMessageHandler())
        {
        }

        public bool IsRunning { get; private set; }

        private void Worker()
        {
            IsRunning = true;
            while (IsRunning)
            {
                Socket? clientSocket = null;
                try
                {
                    if (_socket != null)
                    {
                        clientSocket = _socket.Accept();
                    }
                }
                catch { }
                if (clientSocket != null)
                {
                    var th = new Thread(() =>
                    {
                        while (IsRunning && clientSocket.Connected)
                        {
                            try
                            {
                                Receive(clientSocket);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                        }
                    });
                    th.Start();
                }
            }
        }
        public void Start(string address, int port)
        {
            if (IsRunning) { throw new Exception("Server is already started"); }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //_socket.SetS ocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //_socket.Connect(IPAddress.Parse(address), port);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            _socket.Listen(10);

            if (_socket.IsBound && !IsRunning)
            {
                Console.WriteLine($"Com TCP Server listening on port: {port}...");
                var workerTh = new Thread(() => { Worker(); });
                workerTh.Start();
                IsRunning = true;
            }
        }
        public void Stop()
        {
            IsRunning = false;
            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch { }
                try { _socket.Close(); } catch { }
            }
        }
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
