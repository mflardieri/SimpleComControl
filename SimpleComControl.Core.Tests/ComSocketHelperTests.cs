using SimpleComControl.Core.Helpers;

namespace SimpleComControl.Core.Tests
{
    [TestClass]
    public class ComSocketHelperTests
    {
        [TestMethod]
        public void TestOpenPort()
        {
            int port = 0;
            port = ComSocketHelper.TcpOpenPort();
            Assert.IsTrue(port > 0);
        }
        [TestMethod]
        public void TestTcpNetWorkInfoAvailablity()
        {
            int port = -1;
            bool available = ComSocketHelper.TcpNetWorkInfoAvailablity(port);
            Assert.IsTrue(!available);
            port = 0;
            available = ComSocketHelper.TcpNetWorkInfoAvailablity(port);
            Assert.IsTrue(available);
        }

    }
}