using SimpleComControl.Core.Bases;
using SimpleComControl.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Models
{
    public class SimpleTCPServer : ComTCPSocket, IDisposable, IComServer
    {
        public SimpleTCPServer() : base(new SimpleComMessageHandler())
        {
        }

        public bool isRunning { get; private set; }

        private void worker()
        {
            isRunning = true;
            while (isRunning)
            {
                Socket clientSocket = null;
                try
                {
                    clientSocket = _socket.Accept();
                }
                catch { }
                if (clientSocket != null)
                {
                    var th = new Thread(() =>
                    {
                        while (isRunning && clientSocket.Connected)
                        {
                            try
                            {
                                Receive(clientSocket);
                            }
                            catch (Exception ex)
                            {
                            }

                        }
                    });
                    th.Start();
                }
            }
        }
        public void Start(string address, int port)
        {
            if (isRunning) { throw new Exception("Server is already started"); }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //_socket.SetS ocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //_socket.Connect(IPAddress.Parse(address), port);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            _socket.Listen(10);

            if (_socket.IsBound && !isRunning)
            {
                Console.WriteLine($"Com TCP Server listening on port: {port}...");
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
