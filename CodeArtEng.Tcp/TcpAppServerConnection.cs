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
    }
}
