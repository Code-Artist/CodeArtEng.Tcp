using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server implementation for WinForms Application.
    /// </summary>
    public class TcpAppServerWindows : TcpAppServer
    {
        /// <summary>
        /// Create TCP ApplicatioN Server for WinForms application and register WinForms specific commands.
        /// </summary>
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

        /// <summary>
        /// Show product name and application version
        /// </summary>
        /// <returns></returns>
        protected override string OnShowHelpGetApplicationHeader()
        {
            return Application.ProductName + " V" + Application.ProductVersion.ToString();

        }
    }
}
