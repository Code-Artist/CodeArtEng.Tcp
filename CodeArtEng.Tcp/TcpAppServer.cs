using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server
    /// </summary>
    public class TcpAppServer : TcpServer
    {
        public Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

        public TcpAppServer(string name):base(name)
        {
            MessageDelimiter = Convert.ToByte('\n');
            base.ClientConnected += TcpAppServer_ClientConnected;
            base.ClientDisconnected += TcpAppServer_ClientDisconnected;

            //Register System Commands
            //-- APP INFO --
            RegisterCommand("TcpAppInit");
            RegisterCommand("Name?");
            RegisterCommand("Version?");
            RegisterCommand("TcpAppClientVersion?");
            RegisterCommand("TcpAppServerVersion?");
            //-- GUI Control --
            RegisterCommand("MinimizeWindow");
            RegisterCommand("RestoreWindow");
            RegisterCommand("BringToFront");
            RegisterCommand("SetWindowPosition");
            //-- User Interaction --
            RegisterCommand("Help");
            RegisterCommand("CommandLins?");
        }

        private void TcpAppServer_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TcpAppServer_ClientConnected(object sender, TcpServerEventArgs e)
        {
            e.Client.MessageReceived -= Client_MessageReceived;
        }

        public void RegisterCommand(string command, params string [] arguments)
        {

        }

        public void ExecuteCommand()
        {

        }
    }
}
