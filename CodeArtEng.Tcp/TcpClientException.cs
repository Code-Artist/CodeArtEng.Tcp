﻿using System;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Client Exception
    /// </summary>
    public class TcpClientException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public TcpClientException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public TcpClientException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TcpClientException(string message, Exception innerException) : base(message, innerException) { }
    }

}
