using SimpleComControl.Core.Bases;
using SimpleComControl.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Models
{
    public class SimpleComMessageHandler : IComMessageHandler
    {
        private Encoding encoding = Encoding.ASCII;
        //private const int bufSize = 8 * 1024;

        public byte[] Convert(IComMessage comMessage)
        {
            byte[] bytes = encoding.GetBytes(comMessage.GetMessageBody());
            return bytes;
        }

        public IComMessage Convert(byte[] buffer, int bytes)
        {
            SimpleComMessage comMessage = new SimpleComMessage();
            comMessage.message = encoding.GetString(buffer, 0, bytes);
            //targetSocket.RemoteEndPoint.ToString(), bytes, );
            return comMessage;
        }

        public bool ProcessMessage(ComTCPSocket source, Socket targetSocket, IComMessage comMessage)
        {
            //TODO Process by type and routing
            Console.WriteLine("TCP RECV: {0}: {1}", targetSocket.RemoteEndPoint.ToString(), comMessage.GetMessageBody());

            //if (source is IComServer)
            //{
            //    //SimpleComMessage rtnVal = new SimpleComMessage();
            //    //rtnVal.message = "Received Message";
            //    //let the client know the result of the message
            //    //targetSocket.Send(Convert(rtnVal));
            //}
            return true;
            //
            //
        }
       
        /*
        public void WriteToLog(string message, LogLevel logLevel)
        {
            throw new NotImplementedException();
        }
        */
    }
}
