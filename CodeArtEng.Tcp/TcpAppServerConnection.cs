using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Incoming connection for <see cref="TcpAppServer"/>
    /// </summary>
    public class TcpAppServerConnection
    {
        /// <summary>
        /// Name of <see cref="TcpAppServerConnection"/>.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Connection Object
        /// </summary>
        public TcpServerConnection Connection { get; set; }
        /// <summary>
        /// Client Connection Signed in state
        /// </summary>
        public bool SignedIn { get; set; } = false;
        /// <summary>
        /// Last command added to queue. Each client can only add one command to queue at a time.
        /// </summary>
        internal TcpAppInputCommand NextQueuedCommand { get; set; } = null;

    }
}
