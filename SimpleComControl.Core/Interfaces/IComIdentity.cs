using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleComControl.Core.Interfaces
{
    public interface IComIdentity
    {
        public object GetIdentityId();
        public string GetIdentityName();
        public string GetIdentityStatus();
        public string GetIdentityType();

        public IComIdentity GetSubIdentity();
    }
}
