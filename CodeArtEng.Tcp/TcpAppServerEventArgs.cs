using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server Event Parameters
    /// </summary>
    public class TcpAppServerEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender"></param>
        public TcpAppServerEventArgs(TcpAppServerConnection sender) { Client = sender; }
        /// <summary>
        /// Client connection which triggered this event
        /// </summary>
        public TcpAppServerConnection Client { get; private set; }
        /// <summary>
        /// Object associated with this event.
        /// </summary>
        public object Object { get; set; }
    }
}
