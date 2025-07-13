using System;
using System.Windows.Forms;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server Queue Visualizer for WinForms
    /// </summary>
    public partial class TcpAppServerQueue : UserControl
    {
        private TcpAppServer Server;

        /// <summary>
        /// Control refresh interval
        /// </summary>
        public int UpdateInterval { get => UpdateTimer.Interval; set => UpdateTimer.Interval = value; }

        /// <summary>
        /// 
        /// </summary>
        public TcpAppServerQueue()
        {
            InitializeComponent();
            LstQueuedCommands.Items.Clear();
            LstQueuedResults.Items.Clear();
        }

        /// <summary>
        /// Assign <see cref="TcpAppServer"/> object to control.
        /// </summary>
        /// <param name="server"></param>
        public void AssignObject(TcpAppServer server)
        {
            UpdateTimer.Stop();
            Server = server;
            LstQueuedCommands.Items.Clear();
            LstQueuedResults.Items.Clear();
            if (Server != null) UpdateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(UpdateQueue));
                return;
            }
            UpdateQueue();
        }

        private void UpdateQueue()
        {
            SuspendLayout();
            try
            {
                LstQueuedCommands.Items.Clear();
                LstQueuedCommands.Items.AddRange(Server.GetQueuedCommand());

                LstQueuedResults.Items.Clear();
                LstQueuedResults.Items.AddRange(Server.GetQueuedResults());
            }
            finally { ResumeLayout(); }
        }
    }
}
