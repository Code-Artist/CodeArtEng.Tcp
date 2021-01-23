using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Incoming connection for <see cref="TcpAppServer"/>
    /// </summary>
    public class TcpAppServerConnection: IDisposable
    {
        /// <summary>
        /// Name of <see cref="TcpAppServerConnection"/>.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Connection Object
        /// </summary>
        public TcpServerConnection Connection { get; set; }
        /// <summary>
        /// Client Connection Signed in state
        /// </summary>
        public bool SignedIn { get; set; } = false;
        /// <summary>
        /// Last command added to queue. Each client can only add one command to queue at a time.
        /// </summary>
        internal TcpAppInputCommand NextQueuedCommand { get; set; } = null;

        private TcpAppInputCommand Command;

        private EventWaitHandle ExecutionSignal = new AutoResetEvent(false);
        private EventWaitHandle ExecutionWatchDog = new AutoResetEvent(false);
        internal Thread ExecutionThread = null;
        private bool disposedValue;

        /// <summary>
        /// Constructor - Create new instance. Initiate execution thread
        /// </summary>
        public TcpAppServerConnection()
        {
            if (ExecutionThread == null)
            {
                ExecutionThread = new Thread(ExecuteCommand);
                ExecutionThread.Name = "TcpApp-" + Name;
                ExecutionThread.Start();
            }
        }

        internal void ExecuteCommandAsync(TcpAppInputCommand command, int timeout_ms)
        {
            Command = command;
            Debug.WriteLine("ExecuteCommandAsync - Start");
            ExecutionSignal.Set();
            bool done = ExecutionWatchDog.WaitOne(timeout_ms);

            if (!done)
            {
                Debug.WriteLine("ExecuteCommandAsync Timeout! Recrete ExecutionThread!");
                ExecutionThread.Abort();
                Command.OutputMessage = "Timeout";
                Command.Status = TcpAppCommandStatus.ERR;
                Command = null;
                ExecutionThread = new Thread(ExecuteCommand);
                ExecutionThread.Name = "TcpApp-" + Name;
                ExecutionThread.Start();
            }
            else
                Debug.WriteLine("ExecuteCommandAsync - Completed");
        }

        private void ExecuteCommand()
        {
            while (true)
            {
                if (Command == null)
                {
                    Debug.WriteLine("ExecuteCommand - WAIT");
                    ExecutionSignal.WaitOne();

                }
                Debug.WriteLine("ExecuteCommand - Processing");
                Command?.ExecuteCallback();
                Command = null;
                ExecutionWatchDog.Set(); //Notify Watch dog execution completed.
                Debug.WriteLine("ExecuteCommand - Completed.");
            }
        }

        #region [ IDisposable ]

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Command = null;
                    ExecutionThread.Abort();
                    ExecutionSignal.Dispose();
                    ExecutionWatchDog.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TcpAppServerConnection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
