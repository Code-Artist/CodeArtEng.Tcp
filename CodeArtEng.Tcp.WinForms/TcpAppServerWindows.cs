using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeArtEng.Tcp
{
    public class TcpAppServerWindows : TcpAppServer
    {
        public TcpAppServerWindows() : base()
        {
            RegisterSystemCommand("ApplicationName?", "Get Application Name.", delegate (TcpAppInputCommand sender)
            {
                sender.OutputMessage = Application.ProductName;
                sender.Status = TcpAppCommandStatus.OK;
            });
            RegisterSystemCommand("ApplicationVersion?", "Get Application Version.", delegate (TcpAppInputCommand sender)
            {
                sender.OutputMessage = Application.ProductVersion;
                sender.Status = TcpAppCommandStatus.OK;
            });
        }

        protected override string OnShowHelpGetApplicationHeader()
        {
            return Application.ProductName + " V" + Application.ProductVersion.ToString();

        }
    }
}
