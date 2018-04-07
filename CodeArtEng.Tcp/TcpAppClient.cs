using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Client
    /// </summary>
    public class TcpAppClient
    {
        public string Name { get; private set; }

        public Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

        public TcpAppClient(string name)
        {
            Name = name;
        }
    }

}
