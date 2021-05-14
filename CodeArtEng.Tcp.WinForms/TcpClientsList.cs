using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// WinForms control to display list of TCP Clients connected to assigned server.
    /// </summary>
    public partial class TcpClientsList : UserControl
    {
        private bool IsTcpAppServer { get; set; } = false;
        private TcpServer Server { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TcpClientsList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Assign <see cref="TcpServer"/> object to control.
        /// </summary>
        /// <param name="server"></param>
        public void AssignObject(TcpServer server)
        {
            if (Server != null)
            {
                Server.ClientConnected -= Server_ClientConnected;
                Server.ClientDisconnected -= Server_ClientDisconnected;
            }

            Server = server;
            TcpAppServer appServer = Server as TcpAppServer;
            IsTcpAppServer = (appServer != null);
            if (IsTcpAppServer)
            {
                appServer.ClientSignedIn += AppServer_ClientSignedIn;
                appServer.ClientSigningOut += AppServer_ClientSigningOut;
                appServer.ClientDisconnected += Server_ClientDisconnected;

            }
            else
            {
                Server.ClientConnected += Server_ClientConnected;
                Server.ClientDisconnected += Server_ClientDisconnected;
            }
        }

        private void AppServer_ClientSigningOut(object sender, TcpAppServerEventArgs e)
        {
            RefreshControl();
        }

        private void AppServer_ClientSignedIn(object sender, TcpAppServerEventArgs e)
        {
            RefreshControl();
        }

        private void Server_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            RefreshControl();
        }

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            RefreshControl();
        }

        private void RefreshControl()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(RefreshControl));
                return;
            }

            ClientTable.SuspendLayout();
            ClientTable.Rows.Clear();
            try
            {
                if (IsTcpAppServer)
                {
                    TcpAppServer appServer = Server as TcpAppServer;
                    //Thread lock to prevent race condition.
                    lock (appServer.AppClients)
                    {
                        foreach (TcpAppServerConnection c in appServer.AppClients)
                        {
                            ClientTable.Rows.Add(
                                c.Name + " | " + c.Connection.ClientIPAddress.ToString(),
                                c.Connection.Connected ? "Connected" : "-"
                                );
                        }
                    }
                }
                else
                {
                    foreach (TcpServerConnection c in Server.Clients)
                    {
                        ClientTable.Rows.Add(
                            c.ClientIPAddress.ToString(),
                            c.Connected ? "Connected" : "-"
                            );
                    }
                }
            }
            finally { ClientTable.ResumeLayout(); }
        }

        private void ClientTable_SelectionChanged(object sender, EventArgs e)
        {
            ClientTable.ClearSelection();
        }
    }
}
