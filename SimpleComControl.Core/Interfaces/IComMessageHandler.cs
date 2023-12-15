using SimpleComControl.Core.Bases;
using System.Net.Sockets;

namespace SimpleComControl.Core.Interfaces
{
    public interface IComMessageHandler
    {

        public bool ProcessMessage(ComTCPSocket source, Socket targetSocket, IComMessage comMessage);
        public byte[] Convert(IComMessage comMessage);
        public IComMessage Convert(byte[] buffer, int bytes);

        //public void WriteToLog(string message, Microsoft.Extensions.Logging.LogLevel logLevel);
    }
}
