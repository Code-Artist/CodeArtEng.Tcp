using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server
    /// </summary>
    public class TcpAppServer : TcpServer
    {
        private Form MainForm { get; set; }
        private List<TcpAppCommand> Commands = new List<TcpAppCommand>();

        /// <summary>
        /// Optional user defined Welcome message sent to client in "TcpAppInit" command.
        /// Return system defined message if leave blank.
        /// </summary>
        public string WelcomeMessage { get; set; } = string.Empty;

        /// <summary>
        /// Return version of CodeArtEng.Tcp Assembly
        /// </summary>
        public Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainForm">Main Form Handle</param>
        public TcpAppServer(Form mainForm) : base()
        {
            MainForm = mainForm;
            MessageDelimiter = Convert.ToByte('\n');
            base.ClientConnected += TcpAppServer_ClientConnected;
            base.ClientDisconnected += TcpAppServer_ClientDisconnected;

            //TcpAppServer Format: 
            // TX: #TCP# <Command> [Param0] ... [ParamN]
            // RX: #TCP# <Command> <Status> [Return Message]
            // Source - Appname@192.168.0.1:23
            // Command - Registered Command.

            //Register System Commands
            //-- APP INFO --
            RegisterCommand("TcpAppInit", "Initialize TCP Application.", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = string.IsNullOrEmpty(WelcomeMessage) ? Application.ProductName + " " + Application.ProductVersion : WelcomeMessage;
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("ProgramName?", "Get Application Name.", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = Application.ProductName;
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("ProgramVersion?", "Get Application Version.", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = Application.ProductVersion;
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("TcpAppVersion?", "Get TcpAppServer Library Version", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = Version.ToString();
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("GetFunctionList", "Get list of registered functions.", delegate (TcpAppInputCommand sender)
                {
                    foreach (TcpAppCommand x in Commands)
                    {
                        sender.OutputMessage += x.Keyword;
                        //foreach(TcpAppArgument a in x.Arguments)
                        //{
                        //    sender.OutputMessage += "/" + a.Name;
                        //}
                        sender.OutputMessage += " ";
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                });

            //-- GUI Control --
            string ErrMainFormNull = "Main Form not assigned!";
            RegisterCommand("MinimizeWindow", "Minimize Application Window.", delegate (TcpAppInputCommand sender)
                {
                    if (MainForm != null)
                    {
                        MainForm.WindowState = FormWindowState.Minimized;
                        sender.Status = TcpAppCommandStatus.OK;
                    }
                    else sender.OutputMessage = ErrMainFormNull;
                });
            RegisterCommand("RestoreWindow", "Restore Application Window.", delegate (TcpAppInputCommand sender)
                {
                    if (MainForm != null)
                    {
                        MainForm.WindowState = FormWindowState.Normal;
                        sender.Status = TcpAppCommandStatus.OK;
                    }
                    else sender.OutputMessage = ErrMainFormNull;
                });
            RegisterCommand("BringToFront", "Set Application Window as Top Most.", delegate (TcpAppInputCommand sender)
                {
                    if (MainForm != null)
                    {
                        MainForm.BringToFront();
                        sender.Status = TcpAppCommandStatus.OK;
                    }
                    else sender.OutputMessage = ErrMainFormNull;
                });
            TcpAppCommand cmd = RegisterCommand("SetWindowPosition", "Set Window Position.", delegate (TcpAppInputCommand sender)
                {
                    if (MainForm != null)
                    {
                        MainForm.Location = new System.Drawing.Point(
                            Convert.ToInt16(sender.Command.Argument("XPos").Value),
                            Convert.ToInt16(sender.Command.Argument("YPos").Value));
                        sender.Status = TcpAppCommandStatus.OK;
                    }
                    else sender.OutputMessage = ErrMainFormNull;
                });
            cmd.AddArgument(new TcpAppArgument("X", "Upper left X coordinate of main form.", 0, false));
            cmd.AddArgument(new TcpAppArgument("Y", "Upper left Y coordinate of main form.", 0, false));

            //-- User Interaction --
            RegisterCommand("Help", "Show help screen.", ShowHelp);
        }

        /// <summary>
        /// Perform user defined Initialization sequence when received "TcpAppInit" command from client.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Register new command.
        /// </summary>
        /// <param name="command">Command keyword, single word.</param>
        /// <param name="description">Short description of command.</param>
        /// <param name="executeCallback">Command execution callback.</param>
        /// <returns></returns>
        public TcpAppCommand RegisterCommand(string command, string description, TcpAppServerExecuteDelegate executeCallback)
        {
            if (GetCommand(command) != null) throw new ArgumentException("Failed to register command [" + command + "], already exist!");
            if (command.Contains(" ")) throw new ArgumentException("Invalid Character in command name, space ' ' is not allowed!");

            TcpAppCommand tCmd = new TcpAppCommand(command, description, executeCallback);
            Commands.Add(tCmd);
            return tCmd;
        }

        private void ShowHelp(TcpAppInputCommand sender)
        {
            //Print Help Screen - Write to sender.OutputMessage
            List<string> lines = new List<string>();
            lines.Add("TCP Aplication Server Version " + Version.ToString());
            lines.Add(" ");
            lines.Add("== USAGE ==");
            lines.Add("  SEND: #TCP# <Command> [Param0] ... [ParamN]");
            lines.Add("  RECV: #TCP# <Command> <Status> [Return Message]");
            lines.Add(" ");
            lines.Add("== COMMAND ==");
            foreach (TcpAppCommand cmd in Commands)
            {
                lines.Add(String.Format(" {0,-20}  {1}", cmd.Keyword, cmd.Description));
                foreach (TcpAppArgument arg in cmd.Arguments)
                {
                    lines.Add(String.Format(" {0,-10} {1,-10}  {2}", " ", "<" + arg.Name + ">", arg.Description));
                }
            }

            sender.OutputMessage = string.Join("\r\n", lines.ToArray());
            sender.Status = TcpAppCommandStatus.OK;
        }

        private void TcpAppServer_ClientConnected(object sender, TcpServerEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void TcpAppServer_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            e.Client.MessageReceived -= Client_MessageReceived;
        }

        internal void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Process incoming message from Client
            if (!e.ReceivedMessage.StartsWith("#TCP#")) return; //Drop message which is not using defined format.

            //Parse and Execute Commands
            string[] cmdArg = e.ReceivedMessage.Trim().Split(' ');
            TcpAppCommand cmdHandler = GetCommand(cmdArg[1]);
            if (cmdHandler == null)
            {
                //Error - Unrecognized command.
                e.Client.WriteLineToClient(string.Format("#TCP# {0} {1} Invalid Command!",
                    cmdArg[1], TcpAppCommandStatus.ERR.ToString()));
                return;
            }

            TcpAppInputCommand cmdInput = new TcpAppInputCommand() { Command = cmdHandler };
            cmdHandler.ResetArgumentsValue();
            int argID = 2; //First Argument
            foreach (TcpAppArgument item in cmdHandler.Arguments)
            {
                if (argID >= cmdArg.Length)
                {
                    //Argument with no input
                    if (!item.IsOptional)
                    {
                        //Error - Missing required argument
                        cmdInput.OutputMessage = "Missing required argument: " + item.Name + "!";
                        WriteResultToClient(e.Client, cmdInput);
                        return;
                    }
                }
                else
                {
                    item.Value = cmdArg[argID]; //Assign argument value
                }
                argID++;
            }
            cmdInput.Command.ExecuteCallback(cmdInput); //Send result back to client.
            WriteResultToClient(e.Client, cmdInput);
        }

        private void WriteResultToClient(TcpServerConnection client, TcpAppInputCommand input)
        {
            string returnMsg = "#TCP# " + input.Command.Keyword + " " + input.Status.ToString();
            if (!string.IsNullOrEmpty(input.OutputMessage)) returnMsg += " " + input.OutputMessage;
            client.WriteLineToClient(returnMsg);
        }

        private TcpAppCommand GetCommand(string command)
        {
            return Commands.FirstOrDefault(x => x.Keyword.Equals(command, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
