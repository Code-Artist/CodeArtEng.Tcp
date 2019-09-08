using System;
using System.Collections.Generic;

namespace CodeArtEng.Tcp.Tests
{
    public class TcpAppServerSamplePlugin : ITcpAppServerPlugin
    {
        private readonly TcpAppServerPlugin TcpAppPlugin;

        public string Name { get; set; }

        public string Alias { get => Name; set => Name = value; }

        public void ExecutePluginCommand(TcpAppInputCommand sender)
        {
            TcpAppPlugin.ExecutePluginCommand(sender);
        }

        public TcpAppServerSamplePlugin()
        {
            TcpAppPlugin = new TcpAppServerPlugin();
            TcpAppPlugin.RegisterCommand("PluginCommand1", "Plugin Command 1", delegate (TcpAppInputCommand sender)
                {
                    sender.Status = TcpAppCommandStatus.OK;
                    sender.OutputMessage = "Command 1 Executed!";
                });

        }

        public bool DisposeRequest() { return true; }

        public void ShowHelp(TcpAppInputCommand sender)
        {
            TcpAppPlugin.ShowHelp(sender);
        }

    }
}
