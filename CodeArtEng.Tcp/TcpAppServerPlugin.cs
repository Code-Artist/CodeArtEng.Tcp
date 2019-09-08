using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server Plugin Helper Class
    /// </summary>
    public class TcpAppServerPlugin
    {
        private readonly List<TcpAppCommand> Commands = new List<TcpAppCommand>();

        /// <summary>
        /// Get Command by Keyword
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public TcpAppCommand GetCommand(string command)
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
        /// <param name="sender"></param>
        public void ExecutePluginCommand(TcpAppInputCommand sender)
        {
            sender.Status = TcpAppCommandStatus.ERR;
            string[] cmdArg = sender.Arguments.Skip(1).ToArray();

            //Process Command Keyword
            TcpAppCommand cmdHandler = GetCommand(cmdArg[0]);
            if (cmdHandler == null)
            {
                //Error - Unrecognized command.
                sender.OutputMessage = string.Format("Invalid Command: {0}!", cmdArg[0]);
                return;
            }
            sender.Command = cmdHandler;

            //Process Parameters
            cmdHandler.ResetParametersValue();
            int argID = 1; //First Parameter
            foreach (TcpAppParameter item in cmdHandler.Parameters)
            {
                if (argID >= cmdArg.Length)
                {
                    //Argument with no input
                    if (!item.IsOptional)
                    {
                        //Error - Missing required parameter
                        sender.OutputMessage = "Missing required parameter: " + item.Name + "!";
                        return;
                    }
                }
                else
                {
                    item.Value = cmdArg[argID]; //Assign parameter value
                }
                argID++;
            }

            //Execute Command.
            //Note: Error handling not required. Will handle by TcpAppServer class.
            sender.Command.ExecuteCallback(sender);
        }
    }
}
