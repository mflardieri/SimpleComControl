using SimpleComControl.Core.Bases;
using System.Net;
using System.Net.Sockets;

namespace MauiChatApp.Core.Models
{
    public class ChatSocket : ComTCPSocket
    {
        public ChatSocket() : base(new ChatMessageHandler())
        {
        }
        private static object LockOps = new(); 
        public bool IsRunning { get; private set; }
        public bool IsServer { get; private set; }
        private void WorkerClient()
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
        public void StartClient(string address, int port, string serverAddress, int serverPort)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            _socket.Connect(IPAddress.Parse(serverAddress), serverPort);

            if (_socket.Connected && !IsRunning)
            {
                IsServer = false;
                Console.WriteLine($"Client running on port: {port}...");
                var workerTh = new Thread(() => { WorkerClient(); });
                workerTh.Start();
                IsRunning = true;
            }
        }
        private void WorkerServer()
        {
            IsRunning = true;
            while (IsRunning)
            {
                Socket clientSocket = null;
                try
                {
                    clientSocket = _socket.Accept();
                }
                catch { }
                if (clientSocket != null)
                {
                    lock (LockOps)
                    {
                        var endpoint = ConvertToEndPoint(clientSocket.RemoteEndPoint as IPEndPoint);

                        ChatServer.ConnectSession(endpoint, clientSocket);
                    }   
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
        public void StartServer(string address, int port)
        {
            if (IsRunning) { throw new Exception("Server is already started"); }
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            _socket.Listen(10);

            if (_socket.IsBound && !IsRunning)
            {
                IsServer = true;
                Console.WriteLine($"Com TCP Server listening on port: {port}...");
                var workerTh = new Thread(() => { WorkerServer(); });
                workerTh.Start();
                IsRunning = true;
            }
        }

        public void SendServerMessage(ChatMessage message, ChatEndpoint endPoint)
        {
            lock (LockOps)
            {
                if (IsRunning)
                {
                    var socket = ChatServer.GetSocket(endPoint);
                    this.Send(message, socket);
                }
            }
        }
        public static ChatEndpoint ConvertToEndPoint(IPEndPoint endPoint)
        {
            return new ChatEndpoint() { IpAddress = endPoint.Address.ToString(), Port = endPoint.Port };
        }
        public static IPEndPoint ConvertToEndPoint(ChatEndpoint endPoint)
        {
            return new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.Port);
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
