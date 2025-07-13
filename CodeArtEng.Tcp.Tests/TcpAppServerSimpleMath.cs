using System;
using System.Linq;

namespace CodeArtEng.Tcp.Tests
{
    class TcpAppServerSimpleMath : ITcpAppServerPlugin
    {
        public string PluginName { get; } = "SimpleMath";
        public string PluginDescription { get; } = "Simple Mathematic plugin";
        public string Alias { get; set; }

        private readonly TcpAppServerPlugin TcpAppPlugin;

        public TcpAppServerSimpleMath()
        {
            TcpAppPlugin = new TcpAppServerPlugin();
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            TcpAppPlugin.RegisterCommand("Sum", "Add arguments", Sum,
                TcpAppParameter.CreateParameterArray("Values", "value to sum", false));
            TcpAppPlugin.RegisterQueuedCommand("SumQ", "Add arguments", SumQ,
                TcpAppParameter.CreateParameterArray("Values", "value to sum", false));
            TcpAppPlugin.RegisterCommand("TimeoutSim", "Simulate Timeout", TimeoutSim);
        }

        private void TimeoutSim(TcpAppInputCommand sender)
        {
            System.Threading.Thread.Sleep(5000);
        }

        public void Sum(TcpAppInputCommand sender)
        {
            TcpAppParameter param = sender.Command.Parameter("Values");
            double result = param.Values.Select(x => Convert.ToDouble(x)).Sum();
            sender.OutputMessage = result.ToString();
            sender.Status = TcpAppCommandStatus.OK;
        }
        public void SumQ(TcpAppInputCommand sender)
        {
            TcpAppParameter param = sender.Command.Parameter("Values");
            double result = param.Values.Select(x => Convert.ToDouble(x)).Sum();
            System.Threading.Thread.Sleep((int)(result * 2));
            sender.OutputMessage = result.ToString();
            sender.Status = TcpAppCommandStatus.OK;
        }

        public bool DisposeRequest()
        {
            return true;
        }

        public TcpAppInputCommand GetPluginCommand(string[] commandArguments)
        {
            return TcpAppPlugin.GetPluginCommand(commandArguments);
        }

        public void ShowHelp(TcpAppInputCommand sender)
        {
            TcpAppPlugin.ShowHelp(sender);
        }

    }
}
