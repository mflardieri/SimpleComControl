using SimpleComControl.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Interfaces
{
    public interface IComMessage
    {
        public ComMessageType messageType { get; set; }
        public int GetConnectionId();
        public string GetMessageBody();
        public IComIdentity GetFromIdentity();
        public IComIdentity GetToIdentity();

        public bool IsValid();
    }
}
