using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp.Tests
{
    class TcpAppSimpleString : TcpAppServerPlugin, ITcpAppServerPlugin
    {
        public string PluginName { get; } = "SimpleString";
        public string PluginDescription { get; } = "Simple String Manipulation plugin";
        public string Alias { get; set; }

        public bool DisposeRequest()
        {
            return true;
        }

        public TcpAppSimpleString()
        {
            RegisterCommands();
        }

        public void RegisterCommands()
        {
            RegisterCommand("Replace", "Replace string", SplitString,
                TcpAppParameter.CreateParameter("Input", "Input String"),
                TcpAppParameter.CreateParameter("Search", "String to search"),
                TcpAppParameter.CreateParameter("Replace", "String to replace"));
        }

        private void SplitString(TcpAppInputCommand sender)
        {
            string inputString = sender.Command.Parameter("Input").Value;
            string searchString = sender.Command.Parameter("Search").Value;
            string replaceString = sender.Command.Parameter("Replace").Value;

            sender.OutputMessage = inputString.Replace(searchString, replaceString);
            sender.Status = TcpAppCommandStatus.OK;
        }
    }
}
