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
        private readonly List<TcpAppCommand> Commands = new List<TcpAppCommand>();
        private readonly Dictionary<string, Type> PluginTypes = new Dictionary<string, Type>();
        private readonly List<ITcpAppServerPlugin> Plugins = new List<ITcpAppServerPlugin>();

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
            MessageDelimiter = Convert.ToByte(Convert.ToChar(TcpAppCommon.Delimiter));
            base.ClientConnected += TcpAppServer_ClientConnected;
            base.ClientDisconnected += TcpAppServer_ClientDisconnected;

            //TcpAppServer Format: 
            // TX: TCP: <Command> [-Param0] [-Param1] ... [-ParamN]
            // RX: TCP: <Command> <Status> [Return Message]
            // Source - Appname@192.168.0.1:23
            // Command - Registered Command.

            //Register System Commands
            //--- INIT (Commands used by TcpAppClient) ---
            RegisterCommand("TcpAppInit", "Initialize TCP Application.", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = string.IsNullOrEmpty(WelcomeMessage) ? Application.ProductName + " " + Application.ProductVersion : WelcomeMessage;
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("TcpAppVersion?", "Get TcpAppServer Library Version", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = Version.ToString();
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("FunctionList?", "Get list of registered functions.", delegate (TcpAppInputCommand sender)
                {
                    foreach (TcpAppCommand x in Commands)
                    {
                        sender.OutputMessage += x.Keyword;
                        sender.OutputMessage += " ";
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                });

            //-- APP INFO --
            RegisterCommand("ApplicationName?", "Get Application Name.", delegate (TcpAppInputCommand sender)
            {
                sender.OutputMessage = Application.ProductName;
                sender.Status = TcpAppCommandStatus.OK;
            });
            RegisterCommand("ApplicationVersion?", "Get Application Version.", delegate (TcpAppInputCommand sender)
            {
                sender.OutputMessage = Application.ProductVersion;
                sender.Status = TcpAppCommandStatus.OK;
            });

            //--- PLUGIN ---
            RegisterCommand("PluginTypes?", "Get list of plugin class type. Use CreateObject command to instantiate type.", delegate (TcpAppInputCommand sender)
                {
                    if (PluginTypes.Count == 0) sender.OutputMessage = "-NONE-";
                    else sender.OutputMessage = string.Join(" ", PluginTypes.Keys.ToArray());
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("CreateObject", "Create an object from listed plugin types.", delegate (TcpAppInputCommand sender)
                {
                    string typeName = sender.Command.Parameter("TypeName").Value;
                    string aliasName = sender.Command.Parameter("Alias").Value;

                    //Sanity Check - Type Name
                    Type pluginType = PluginTypes[PluginTypes.Keys.FirstOrDefault(x => string.Compare(x, typeName, true) == 0)];

                    if (!PluginTypes.Keys.Contains(typeName, StringComparer.InvariantCultureIgnoreCase))
                        throw new ArgumentException("Unknown plugin type: " + typeName);

                    //Sanity Check - Alias Name
                    if (Plugins.FirstOrDefault(x => string.Compare(x.Alias, aliasName, true) == 0) != null)
                        throw new ArgumentException("Unable to create object with alias '" + aliasName + "'. Object already exists!");

                    ITcpAppServerPlugin pluginInstance = Activator.CreateInstance(pluginType) as ITcpAppServerPlugin;
                    pluginInstance.Alias = aliasName;
                    Plugins.Add(pluginInstance);

                    sender.OutputMessage = "Object created " + aliasName;
                    sender.Status = TcpAppCommandStatus.OK;
                },
                new TcpAppParameter("TypeName", "Plugin type name."),
                new TcpAppParameter("Alias", "Plugin object Alias Name. Alias name is case insensitive"));
            RegisterCommand("Objects?", "Return object list by alias name.", delegate (TcpAppInputCommand sender)
                {
                    if (Plugins.Count == 0) sender.OutputMessage = "-NONE-";
                    else sender.OutputMessage = string.Join(" ", Plugins.Select(x => x.Alias).ToArray());
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterCommand("Execute", "Exceute plugin's command.", delegate (TcpAppInputCommand sender)
                {
                    ITcpAppServerPlugin plugin = Plugins.FirstOrDefault(x => string.Compare(x.Alias, sender.Command.Parameter("Alias").Value, true) == 0);
                    if (plugin == null) throw new ArgumentNullException("Plugin not exists!");
                    TcpAppInputCommand pluginCommand = new TcpAppInputCommand() { Arguments = sender.Arguments };
                    plugin.ExecutePluginCommand(pluginCommand);

                    sender.Status = pluginCommand.Status;
                    sender.OutputMessage = pluginCommand.OutputMessage;
                },
                new TcpAppParameter("Alias", "Plugin object Alias Name."));
            RegisterCommand("DisposeObject", "Delete object by alias name.", delegate (TcpAppInputCommand sender)
                {
                    string alias = sender.Command.Parameter("Alias").Value;
                    ITcpAppServerPlugin item = Plugins.FirstOrDefault(x => string.Compare(x.Alias, alias, true) == 0);
                    if (item == null)
                    {
                        sender.OutputMessage = "Object [" + alias + "] not found / disposed.";
                        sender.Status = TcpAppCommandStatus.OK; return;
                    }
                    else
                    {
                        if (item.DisposeRequest() == true)
                        {
                            Plugins.Remove(item);
                            sender.OutputMessage = alias + " disposed.";
                            sender.Status = TcpAppCommandStatus.OK;
                        }
                        else
                        {
                            sender.Status = TcpAppCommandStatus.ERR;
                            sender.OutputMessage = "Unable to dispose " + alias;
                        }
                    }
                },
                new TcpAppParameter("Alias", "Object alias name."));

            //-- GUI Control --
            string ErrMainFormNull = "Main Form not assigned!";
            RegisterCommand("MaximizeWindow", "Maximize Application Window.", delegate (TcpAppInputCommand sender)
                {
                    if (MainForm != null)
                    {
                        MainForm.WindowState = FormWindowState.Maximized;
                        sender.Status = TcpAppCommandStatus.OK;
                    }
                    else sender.OutputMessage = ErrMainFormNull;
                });
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
            RegisterCommand("SetWindowPosition", "Set Window Position.", delegate (TcpAppInputCommand sender)
                {
                    if (MainForm != null)
                    {
                        MainForm.Location = new System.Drawing.Point(
                            Convert.ToInt16(sender.Command.Parameter("X").Value),
                            Convert.ToInt16(sender.Command.Parameter("Y").Value));
                        sender.Status = TcpAppCommandStatus.OK;
                    }
                    else sender.OutputMessage = ErrMainFormNull;
                },
                new TcpAppParameter("X", "Upper left X coordinate of main form."),
                new TcpAppParameter("Y", "Upper left Y coordinate of main form."));

            RegisterCommand("Terminate", "Terminate Application. Default Exit Code = -99", delegate (TcpAppInputCommand sender)
                {
                    int exitCode = -99;
                    try
                    {
                        exitCode = Convert.ToInt32(sender.Command.Parameter("ExitCode").Value);
                    }
                    catch { }
                    sender.Status = TcpAppCommandStatus.OK;

                    Environment.ExitCode = exitCode;
                    System.Threading.Thread ptrThread = new System.Threading.Thread(TerminateApplication);
                    ptrThread.Start();
                },
                new TcpAppParameter("ExitCode", "Assign Exit Code for application termination.", "-99"));

            //-- User Interaction --
            RegisterCommand("Help", "Show help screen.", ShowHelp,
                new TcpAppParameter("Alias", "Object Alias Name", "-"));
        }

        private void TerminateApplication()
        {
            System.Threading.Thread.Sleep(1000);
            Environment.Exit(Environment.ExitCode);
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
        /// <param name="parameters">Command Parameters</param>
        /// <returns></returns>
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
        /// Register type as plugin.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pluginType"></param>
        public void RegisterPluginType(string name, Type pluginType)
        {
            if (PluginTypes.Keys.Contains(name)) throw new ArgumentException("Unable to register plugin, name already exist!");
            if (pluginType.GetInterfaces().Contains(typeof(ITcpAppServerPlugin)))
                PluginTypes.Add(name, pluginType);
            else throw new ArgumentException("Unable to register plugin, input type does not implement ITcpAppServerPlugin interface!");
        }

        /// <summary>
        /// Add plugin object directly from TcpAppServer.
        /// </summary>
        /// <param name="plugin"></param>
        public void AddPlugin(ITcpAppServerPlugin plugin)
        {
            if (Plugins.FirstOrDefault(x => string.Compare(x.Alias, plugin.Alias, true) == 0) != null)
                throw new ArgumentException("Unable to add object with alias '" + plugin.Alias + "'. Object already exists!");
            Plugins.Add(plugin);
        }

        private void ShowHelp(TcpAppInputCommand sender)
        {
            string aliasName = sender.Command.Parameter("Alias").Value;
            List<string> lines = null;
            if (aliasName == "-")
            {
                //Print Help Screen - Write to sender.OutputMessage
                lines = new List<string>
                    {
                        "TCP Aplication Server Version " + Version.ToString(),
                        " ",
                        "==== USAGE ====",
                        "  SEND: <Command> [Param0] ... [ParamN]",
                        "  RECV: <Command> <Status> [Return Message]",
                        " ",
                        " Notes:",
                        "  <> = Required parameters",
                        "  [] = Optional parameters",
                        " ",
                    };

                lines.AddRange(TcpAppCommon.PrintCommandHelpContents(Commands));
                sender.OutputMessage = string.Join("\r\n", lines.ToArray());
                sender.Status = TcpAppCommandStatus.OK;
            }
            else
            {
                //Get Help Content for selected object.
                ITcpAppServerPlugin plugin = Plugins.FirstOrDefault(x => string.Compare(x.Alias, sender.Command.Parameter("Alias").Value, true) == 0);
                if(plugin == null)
                {
                    sender.Status = TcpAppCommandStatus.ERR;
                    sender.OutputMessage = "Object [" + aliasName + "] not exist!";
                }
                else
                {
                    plugin.ShowHelp(sender);
                }
            }
        }

        private void TcpAppServer_ClientConnected(object sender, TcpServerEventArgs e)
        {
            //e.Client.MessageReceived += Client_MessageReceived;
            e.Client.ProcessReceivedMessageCallback = Client_ProcessReceivedMessage;
        }

        private void TcpAppServer_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            //e.Client.MessageReceived -= Client_MessageReceived;
        }

        private void Client_ProcessReceivedMessage(TcpServerConnection client, string message, byte[] messageBytes)
        {
            //Parse and Execute Commands
            string[] cmdArg = TcpAppCommon.ParseCommand(message.Trim());

            //Process Command Keyword
            TcpAppCommand cmdHandler = GetCommand(cmdArg[0]);
            if (cmdHandler == null)
            {
                //Error - Unrecognized command.
                client.WriteLineToClient(string.Format("{0} {1} Invalid Command!",
                    cmdArg[0], TcpAppCommandStatus.ERR.ToString()));
                return;
            }
            TcpAppInputCommand cmdInput = new TcpAppInputCommand() { Command = cmdHandler };
            cmdInput.Arguments = cmdArg.Skip(1).ToArray(); //Move to TcpAppInputCommand

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
                        cmdInput.OutputMessage = "Missing required parameter: " + item.Name + "!";
                        WriteResultToClient(client, cmdInput);
                        return;
                    }
                }
                else
                {
                    item.Value = cmdArg[argID]; //Assign parameter value
                }
                argID++;
            }

            //Execute Commands
            try
            {
                cmdInput.Command.ExecuteCallback(cmdInput);
            }
            catch (Exception ex)
            {
                //Catch and report all execution error
                cmdInput.OutputMessage = "Exception Raised! " + ex.Message;
                cmdInput.Status = TcpAppCommandStatus.ERR; //Force status to error, make sure no surprise.
            }
            finally
            {
                WriteResultToClient(client, cmdInput); //Send result back to client.
            }
        }

        private void WriteResultToClient(TcpServerConnection client, TcpAppInputCommand input)
        {
            string returnMsg = input.Command.Keyword + " " + input.Status.ToString();
            if (!string.IsNullOrEmpty(input.OutputMessage)) returnMsg += " " + input.OutputMessage;
            System.Diagnostics.Trace.WriteLine("Write To Client: " + returnMsg);
            client.WriteLineToClient(returnMsg);
        }

        private TcpAppCommand GetCommand(string command)
        {
            return Commands.FirstOrDefault(x => x.Keyword.Equals(command, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Return list of plugin objects.
        /// </summary>
        public IList<ITcpAppServerPlugin> PluginList { get => Plugins.AsReadOnly(); }
    }
}
