namespace SimpleComControl.Core.Interfaces
{
    public interface IComIdentity
    {
        public object GetIdentityId();
        public string GetIdentityName();
        public string GetIdentityStatus();
        public int GetIdentityType();

        public List<IComIdentity> GetSubIdentities();
    }
}
