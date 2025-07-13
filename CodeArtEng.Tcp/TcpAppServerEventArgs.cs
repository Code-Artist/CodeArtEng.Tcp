using System;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Extended TCP Application Server Event Parameters with <see cref="Cancel"/> option.
    /// </summary>
    public class TcpAppServerExEventArgs : TcpAppServerEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender"></param>
        public TcpAppServerExEventArgs(TcpAppServerConnection sender) : base(sender) { }

        /// <summary>
        /// Default is false, set to true to cancel command execution.
        /// </summary>
        public bool Cancel { get; set; } = false;
        /// <summary>
        /// Cancellation reason
        /// </summary>
        public string Reason { get; set; } = "Command rejected by server!";
    }

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
        /// Plugin 
        /// </summary>
        public ITcpAppServerPlugin Plugin { get; set; }
        /// <summary>
        /// Misc properties to store additional information of event
        /// </summary>
        public object Value { get; set; }
    }
}
