using System;
using System.Threading;
using System.Diagnostics;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Incoming connection for <see cref="TcpAppServer"/>
    /// </summary>
    public class TcpAppServerConnection : IDisposable
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
        private EventWaitHandle ExecutionWatchDog = new ManualResetEvent(false);
        internal Thread ExecutionThread = null;
        private bool disposedValue;
        private CancellationTokenSource CancellationToken = null;

        /// <summary>
        /// Constructor - Create new instance. Initiate execution thread
        /// </summary>
        public TcpAppServerConnection()
        {
            CreateAndStartExecutionThread();
        }

        private void StopExecutionThread()
        {
            //Cancel Thread - Implement using CancellationTokenSource?
            //https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads

            //AbortExecutionThread = true;
            Trace.WriteLine("AppServer-StopThread");
            CancellationToken.Cancel();
            Trace.WriteLine("AppServer: Cancellation token signaled.");
            ExecutionSignal.Set();
            Thread.Sleep(500);
            CancellationToken.Dispose();
            CancellationToken = null;
        }

        private void CreateAndStartExecutionThread()
        {
            if (CancellationToken != null) StopExecutionThread();
            CancellationToken = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(new WaitCallback(ExecutionThread_DoWork), CancellationToken.Token);
        }

        /// <summary>
        /// Execute command with timeout handling
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout_ms"></param>
        internal void ExecuteCommandAsync(TcpAppInputCommand command, int timeout_ms)
        {
            Command = command;
            Debug.WriteLine("ExecuteCommandAsync: Start");
            ExecutionWatchDog.Reset();
            ExecutionSignal.Set();
            bool done = ExecutionWatchDog.WaitOne(timeout_ms);

            if (!done)
            {
                Debug.WriteLine("[ERROR]AppServer-ExecuteCommandAsync: Timeout! Recreate execution thread!");
                StopExecutionThread();
                Command.OutputMessage = "Server Timeout";
                Command.Status = TcpAppCommandStatus.ERR;
                Command = null;
                CreateAndStartExecutionThread();
            }
            else
                Debug.WriteLine("ExecuteCommandAsync: Completed");
        }

        private void ExecutionThread_DoWork(object obj)
        {
            try
            {
                CancellationToken token = (CancellationToken)obj;
                while (!token.IsCancellationRequested)
                {
                    if (Command == null)
                    {
                        Debug.WriteLine("EecutionThread " + Thread.CurrentThread.GetHashCode() + ": Execution Signal, wait for next...");
                        ExecutionSignal.WaitOne();
                        Debug.WriteLine("EecutionThread " + Thread.CurrentThread.GetHashCode() + ": Execution Signal received.");

                    }
                    else
                    {
                        Debug.WriteLine("EecutionThread " + Thread.CurrentThread.GetHashCode() + ": Processing command...");
                        Command?.ExecuteCallback();
                        Command = null;
                        ExecutionWatchDog.Set(); //Notify Watch dog execution completed.
                        Debug.WriteLine("EecutionThread " + Thread.CurrentThread.GetHashCode() + ": Completed.");
                    }
                }
                if (token.IsCancellationRequested) Trace.WriteLine("EecutionThread " + Thread.CurrentThread.GetHashCode() + ": Thread Cancel and Terminated!");
            }
            catch(Exception ex) { Trace.WriteLine("[ERROR] ExecutionThread Crahsed: " + ex.Message); }
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
                    StopExecutionThread();
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
