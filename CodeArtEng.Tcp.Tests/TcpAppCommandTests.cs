using System;
using System.Linq;
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
            Assert.That(cloned.Keyword,Is.EqualTo("CMD"));
            Assert.That(cloned.Description,Is.EqualTo("Dummy Command"));
            Assert.That(cloned.Parameters.Count(),Is.EqualTo(3));
            Assert.That(cloned.Parameters[2].Description,Is.EqualTo("Array 1"));
        }
    }
}
