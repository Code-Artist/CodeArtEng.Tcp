using System;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Client Exception
    /// </summary>
    public class TcpAppClientException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public TcpAppClientException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public TcpAppClientException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TcpAppClientException(string message, Exception innerException) : base(message, innerException) { }
    }

}
