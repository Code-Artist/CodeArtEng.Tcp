using System;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Event Paremeters
    /// </summary>
    public class TcpAppEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public TcpAppEventArgs(string message) { Message = message; }
        /// <summary>
        /// TCP Command / Response
        /// </summary>
        public string Message { get; private set; }
    }

}
