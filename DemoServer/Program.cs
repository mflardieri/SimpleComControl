using SimpleComControl.Core.Models;

namespace DemoServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SimpleTCPServer comTs = new SimpleTCPServer();
            //int port = ComSocketHelper.TcpOpenPort();
            //Console.WriteLine(port);
            int serverPort = 65353;
            string serverIP = "127.0.0.1";
            try
            {
                comTs.Start(serverIP, serverPort);
                while (comTs.isRunning)
                {
                    Console.WriteLine("Enter Text or type '/E' to exit: ");
                    string? input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (input == "/E")
                        {
                            comTs.Stop();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            comTs._socket.Close();
            Console.WriteLine("Closed Server \n Press any key to exit");
            Console.ReadKey();
        }
    }
}