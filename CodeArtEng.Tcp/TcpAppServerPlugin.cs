using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server Plugin Helper Class.
    /// Implement in class which implement <see cref="ITcpAppServerPlugin"/>
    /// </summary>
    public class TcpAppServerPlugin
    {
        private readonly List<TcpAppCommand> Commands = new List<TcpAppCommand>();

        /// <summary>
        /// Get Command by Keyword
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private TcpAppCommand GetCommand(string command)
        {
            return Commands.FirstOrDefault(x => x.Keyword.Equals(command, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Register Command and Callback
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="executeCallback"></param>
        /// <param name="parameters"></param>
        public void RegisterCommand(string command, string description, TcpAppServerExecuteDelegate executeCallback, params TcpAppParameter[] parameters)
        {
            if (GetCommand(command) != null) throw new ArgumentException("Failed to register command [" + command + "], already exist!");
            if (command.Contains(" ")) throw new ArgumentException("Invalid Character in command name, space ' ' is not allowed!");

            TcpAppCommand tCmd = new TcpAppCommand(command, description, executeCallback);
            foreach (TcpAppParameter a in parameters)
            {
                tCmd.AddParameter(a);
            }
            Commands.Add(tCmd);
        }

        /// <summary>
        /// Print help screen for selected plugin components.
        /// </summary>
        /// <param name="sender"></param>
        public void ShowHelp(TcpAppInputCommand sender)
        {
            sender.OutputMessage = string.Join("\r\n", TcpAppCommon.PrintCommandHelpContents(Commands));
            sender.Status = TcpAppCommandStatus.OK;
        }

        /// <summary>
        /// Execute Plugin Callback. Call by ITcpAppServerPlugin
        /// </summary>
        /// <param name="commandArguments">Command keyword and arguments in string array.</param>
        public TcpAppInputCommand ExecutePluginCommand(string [] commandArguments)
        {
            TcpAppInputCommand command = TcpAppCommon.CreateInputCommand(Commands, commandArguments);
            command.ExecuteCallback();
            return command;
        }
    }
}
