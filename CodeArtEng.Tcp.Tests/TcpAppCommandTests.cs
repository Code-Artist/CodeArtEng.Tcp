using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CodeArtEng.Tcp.Tests
{
    [TestFixture]
    class TcpAppCommandTests
    {
        [Test]
        public void TestAddDuplicatedParameter_ArgumentException()
        {
            TcpAppCommand cmd = new TcpAppCommand("CMD", "Dummy Command", null);
            cmd.AddParameter(TcpAppParameter.CreateParameter("P1", "Parameter 1"));
            Assert.Throws<ArgumentException>(() => { cmd.AddParameter(TcpAppParameter.CreateParameter("P1", "Parameter 1")); });
        }

        [Test]
        public void TestCommand_Clone()
        {
            TcpAppCommand cmd = new TcpAppCommand("CMD", "Dummy Command", null);
            cmd.AddParameter(TcpAppParameter.CreateParameter("P1", "Parameter 1"));
            cmd.AddParameter(TcpAppParameter.CreateParameter("P2", "Parameter 2"));
            cmd.AddParameter(TcpAppParameter.CreateParameterArray("Array1", "Array 1", false));

            TcpAppCommand cloned = cmd.Clone() as TcpAppCommand;
            Assert.AreEqual("CMD", cloned.Keyword);
            Assert.AreEqual("Dummy Command", cloned.Description);
            Assert.AreEqual(3, cloned.Parameters.Count());
            Assert.AreEqual("Array 1", cloned.Parameters[2].Description);
        }
    }
}
