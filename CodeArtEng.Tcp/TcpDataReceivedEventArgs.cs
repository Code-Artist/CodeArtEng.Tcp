using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    public class TcpDataReceivedEventArgs : EventArgs
    {
        public byte [] Data { get; internal set; }
        public string GetString() { return Encoding.ASCII.GetString(Data); }
    }
}
