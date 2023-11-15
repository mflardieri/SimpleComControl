using SimpleComControl.Core.Enums;
using SimpleComControl.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Models
{
    public class SimpleComMessage : IComMessage
    {
        public ComMessageType messageType { get; set; }

        public string message { get; set; }

        public int GetConnectionId()
        {
            throw new NotImplementedException();
        }

        public IComIdentity GetFromIdentity()
        {
            throw new NotImplementedException();
        }

        public string GetMessageBody()
        {
            return message;
        }

        public IComIdentity GetToIdentity()
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            //
            return true;
        }
    }
}
