using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Models;

namespace DemoClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SimpleTCPClient client = new SimpleTCPClient();
            int port = ComSocketHelper.TcpOpenPort();
            string IP = "127.0.0.1";
            int serverPort = 65353;
            string serverIP = "127.0.0.1";
            try
            {
                client.Start(IP, port, serverIP, serverPort);
                while (true)
                {
                    Console.WriteLine("Enter Text or type '/E' to exit: ");
                    string? input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (input == "/E")
                        {
                            client.Stop();
                            break;
                        }
                        else
                        {
                            SimpleComMessage comMessage = new SimpleComMessage();
                            comMessage.Message = input;
                            client.Send(comMessage); // Send Message to Server
                            //client.Receive(); //Receive Message from Service
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            client._socket.Close();
            Console.WriteLine("Closed Client \n Press any key to exit");
            Console.ReadKey();
        }
    }
}