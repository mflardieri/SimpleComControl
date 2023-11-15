using SimpleComControl.Core.Helpers;
using SimpleComControl.Core.Models;
using System.Diagnostics;

namespace SimpleComControl.Core.Tests
{
    [TestClass]
    public class ComSocketTests
    {
        [TestMethod]
        public void ComSocketServerConnectTest()
        {
            TextWriter textWriter = Console.Out;
            StringWriter sw = new StringWriter();
            Console.SetOut(sw);

            string messageToSend = "Hello world!";
            string ipAddress = "127.0.0.1";
            int serverPort = 0;
            int clientPort = 0;
            //Get Available Port for server;
            serverPort = ComSocketHelper.TcpOpenPort();

            SimpleTCPServer server = new SimpleTCPServer();
            SimpleTCPClient client = new SimpleTCPClient();
            try
            {
                //start the server
                server.Start(ipAddress, serverPort);
                
                //get the client port
                clientPort = ComSocketHelper.TcpOpenPort();
                //start the client
                client.Start(ipAddress, clientPort, ipAddress, serverPort);

                SimpleComMessage message = new SimpleComMessage();
                message.message = messageToSend;

                client.Send(message);
                //Wait for server to listen and process
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {

            }
           
            string? consoleout = sw.ToString();
            //Clean up client
            try {
                client.Dispose();
            }
            catch { }
            //Clean up server
            try
            {
                server.Dispose();
            }
            catch { }
            Console.SetOut(textWriter);
            Assert.IsTrue(consoleout != null);
            Assert.IsTrue(consoleout.Contains(messageToSend));
        }
    }
}
