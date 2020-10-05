using System;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Client Event Paremeters
    /// </summary>
    public class TcpAppClientEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public TcpAppClientEventArgs(string message) { Message = message; }
        /// <summary>
        /// TCP Command / Response
        /// </summary>
        public string Message { get; private set; }
    }

}
