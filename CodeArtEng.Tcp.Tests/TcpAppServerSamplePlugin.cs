using System;

namespace CodeArtEng.Tcp.Tests
{
    public class TcpAppServerSamplePlugin : ITcpAppServerPlugin, IDisposable
    {
        private readonly TcpAppServerPlugin TcpAppPlugin;
        private bool disposedValue;

        public string PluginName { get; private set; } = "SamplePlugin";
        public string PluginDescription { get; private set; } = "Example Plugin Implementation";

        public string Alias { get; set; }

        public TcpAppInputCommand GetPluginCommand(string[] commandArguments)
        {
            return TcpAppPlugin.GetPluginCommand(commandArguments);
        }

        public TcpAppServerSamplePlugin()
        {
            TcpAppPlugin = new TcpAppServerPlugin();
            TcpAppPlugin.RegisterCommand("PluginCommand1", "Plugin Command 1", delegate (TcpAppInputCommand sender)
                {
                    sender.Status = TcpAppCommandStatus.OK;
                    sender.OutputMessage = "Command 1 Executed!";
                });
            TcpAppPlugin.RegisterQueuedCommand("LongDelay", "10s Delay", delegate (TcpAppInputCommand sender)
            {
                System.Threading.Thread.Sleep(10000);
                sender.Status = TcpAppCommandStatus.OK;
            });
            TcpAppPlugin.RegisterCommand("InfiniteLoop", "Infinite Loop Command", delegate (TcpAppInputCommand sender)
            {
                while (true) { System.Threading.Thread.Sleep(1000); }
            });
            TcpAppPlugin.RegisterQueuedCommand("InfiniteLoopQueue", "Infinite Loop Queue", delegate (TcpAppInputCommand sender)
            {
                while (true) { System.Threading.Thread.Sleep(1000); }
            });
        }

        public bool DisposeRequest()
        {
            Dispose(); //Execute Dispose method in IDisposable
            return true;
        }

        public void ShowHelp(TcpAppInputCommand sender)
        {
            TcpAppPlugin.ShowHelp(sender);
        }

        #region [ Dispose Pattern ]

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        // //  override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TcpAppServerSamplePlugin()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
