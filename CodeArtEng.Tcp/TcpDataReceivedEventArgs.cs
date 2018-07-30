using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Data received event arguments, used in <see cref="TcpAppClient"/>
    /// </summary>
    public class TcpDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Recevied data in byte array
        /// </summary>
        public byte [] Data { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetString() { return Encoding.ASCII.GetString(Data); }
    }
}
