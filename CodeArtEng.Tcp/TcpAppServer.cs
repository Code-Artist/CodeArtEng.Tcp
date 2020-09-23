using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server execution callback
    /// </summary>
    /// <param name="sender"></param>
    public delegate void TcpAppServerExecuteDelegate(TcpAppInputCommand sender);

    /// <summary>
    /// TCP Application Server
    /// </summary>
    public class TcpAppServer : TcpServer
    {
        private long Counter = 0;

        private readonly List<TcpAppServerConnection> Clients = new List<TcpAppServerConnection>();
        private readonly List<TcpAppCommand> Commands = new List<TcpAppCommand>();
        private readonly List<TcpAppServerPluginType> PluginTypes = new List<TcpAppServerPluginType>();
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
        public TcpAppServer() : base()
        {
            MessageDelimiter = Convert.ToByte(Convert.ToChar(TcpAppCommon.Delimiter));
            base.ClientConnected += TcpAppServer_ClientConnected;
            base.ClientDisconnected += TcpAppServer_ClientDisconnected;
            base.ServerStopped += TcpAppServer_ServerStopped;

            //TcpAppServer Format: 
            // TX: TCP: <Command> [-Param0] [-Param1] ... [-ParamN]
            // RX: TCP: <Command> <Status> [Return Message]
            // Source - Appname@192.168.0.1:23
            // Command - Registered Command.

            //Register System Commands
            //--- INIT (Commands used by TcpAppClient) ---
            RegisterSystemCommand("TcpAppInit", "Initialize TCP Application.", delegate (TcpAppInputCommand sender)
                {
                    //Assign Name
                    string name = sender.Command.Parameter("Name").Value;
                    if (string.IsNullOrEmpty(name)) name = "(" + (++Counter).ToString() + ")" + sender.Client.Connection.ClientIPAddress.ToString();
                    sender.Client.Name = name;

                    sender.OutputMessage = string.IsNullOrEmpty(WelcomeMessage) ? Application.ProductName + " " + Application.ProductVersion : WelcomeMessage;
                    sender.Status = TcpAppCommandStatus.OK;
                },
                TcpAppParameter.CreateOptionalParameter("Name", "Client Name. Auto assign by server if not defined.", ""));
            RegisterSystemCommand("Name?", "Get Connection Name", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = sender.Client.Name;
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("Version?", "Get TcpAppServer Library Version", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = Version.ToString();
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("FunctionList?", "Get list of registered functions.", delegate (TcpAppInputCommand sender)
                {
                    foreach (TcpAppCommand x in Commands)
                    {
                        sender.OutputMessage += x.Keyword;
                        sender.OutputMessage += " ";
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                });

            //-- APP INFO --
            RegisterSystemCommand("ApplicationName?", "Get Application Name.", delegate (TcpAppInputCommand sender)
            {
                sender.OutputMessage = Application.ProductName;
                sender.Status = TcpAppCommandStatus.OK;
            });
            RegisterSystemCommand("ApplicationVersion?", "Get Application Version.", delegate (TcpAppInputCommand sender)
            {
                sender.OutputMessage = Application.ProductVersion;
                sender.Status = TcpAppCommandStatus.OK;
            });

            //--- PLUGIN ---
            RegisterSystemCommand("PluginTypes?", "Get list of plugin class type. Use CreateObject command to instantiate type.", delegate (TcpAppInputCommand sender)
                {
                    if (PluginTypes.Count == 0) sender.OutputMessage = "-NONE-";
                    else sender.OutputMessage = string.Join(" ", PluginTypes.Select(x => x.Name).ToArray());
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("CreateObject", "Create an object from listed plugin types.", delegate (TcpAppInputCommand sender)
                {
                    string typeName = sender.Command.Parameter("TypeName").Value;

                    //Sanity Check - Type Name
                    if (PluginTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)) == null)
                        throw new ArgumentException("Unknown plugin type: " + typeName);

                    Type pluginType = PluginTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)).Type;

                    string aliasName;
                    sender.OutputMessage = "Object created:";
                    foreach (string value in sender.Command.Parameter("Alias").Values)
                    {
                        aliasName = value;
                        if (Commands.FirstOrDefault(x => string.Compare(x.Keyword, aliasName, true) == 0) != null)
                            throw new ArgumentException("Unable to create object with alias '" + aliasName + "'. Name already registered as command!");

                        //Sanity Check - Verify alias name is not plugin type name
                        if (PluginTypes.FirstOrDefault(x => string.Compare(x.Name, aliasName, true) == 0) != null)
                            throw new ArgumentException("Unable to create object with alias '" + aliasName + "'. Name already registered plugin type!");

                        //Sanity Check - Alias Name
                        if (Plugins.FirstOrDefault(x => string.Compare(x.Alias, aliasName, true) == 0) != null)
                            throw new ArgumentException("Unable to create object with alias '" + aliasName + "'. Object already exists!");

                        ITcpAppServerPlugin pluginInstance = Activator.CreateInstance(pluginType) as ITcpAppServerPlugin;
                        pluginInstance.Alias = aliasName;
                        Plugins.Add(pluginInstance);
                        sender.OutputMessage += " " + aliasName;
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                },
                TcpAppParameter.CreateParameter("TypeName", "Plugin type name."),
                TcpAppParameter.CreateParameterArray("Alias", "Plugin object name, case insensitive.", false));
            RegisterSystemCommand("Objects?", "Return object list by alias name.", delegate (TcpAppInputCommand sender)
                {
                    if (Plugins.Count == 0) sender.OutputMessage = "-NONE-";
                    else sender.OutputMessage = string.Join(TcpAppCommon.NewLine, Plugins.Select(x => x.Alias + "(" + PluginTypes.FirstOrDefault(n => n.Type == x.GetType())?.Name + ")").ToArray());
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("Execute", "Execute plugin's command.", delegate (TcpAppInputCommand sender)
                {
                    ITcpAppServerPlugin plugin = Plugins.FirstOrDefault(x => string.Compare(x.Alias, sender.Command.Parameter("Alias").Value, true) == 0);
                    if (plugin == null) throw new ArgumentNullException("Plugin not exists!");
                    TcpAppInputCommand pluginCommand = plugin.ExecutePluginCommand(sender.Arguments.Skip(1).ToArray());
                    sender.Status = pluginCommand.Status;
                    sender.OutputMessage = pluginCommand.OutputMessage;
                },
                TcpAppParameter.CreateParameter("Alias", "Plugin object Alias Name."));
            RegisterSystemCommand("DisposeObject", "Delete object by alias name.", delegate (TcpAppInputCommand sender)
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
                TcpAppParameter.CreateParameter("Alias", "Object alias name."));

            RegisterSystemCommand("Terminate", "Terminate Application. Default Exit Code = -99", delegate (TcpAppInputCommand sender)
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
                TcpAppParameter.CreateOptionalParameter("ExitCode", "Assign Exit Code for application termination.", "-99"));

            //-- User Interaction --
            RegisterSystemCommand("Help", "Show help screen. Include plugin type or object alias name to show commands for selected plugin.", ShowHelp,
                TcpAppParameter.CreateOptionalParameter("Plugin", "Plugin type or Alias", "-"));
        }

        private void TcpAppServer_ServerStopped(object sender, EventArgs e)
        {
            Clients.Clear();
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
            RegisterCommandInt(command, description, executeCallback, parameters);
        }

        private void RegisterSystemCommand(string command, string description, TcpAppServerExecuteDelegate executeCallback, params TcpAppParameter[] parameters)
        {
            RegisterCommandInt(command, description, executeCallback, parameters).IsSystemCommand = true;
        }

        private TcpAppCommand RegisterCommandInt(string command, string description, TcpAppServerExecuteDelegate executeCallback, params TcpAppParameter[] parameters)
        {
            if (GetCommand(command) != null) throw new ArgumentException("Failed to register command [" + command + "], already exist!");
            if (command.Contains(" ")) throw new ArgumentException("Invalid Character in command name, space ' ' is not allowed!");

            TcpAppCommand tCmd = new TcpAppCommand(command, description, executeCallback);
            foreach (TcpAppParameter a in parameters)
            {
                tCmd.AddParameter(a);
            }
            Commands.Add(tCmd);

            //Sanity Check - Parameter array is only allowed as last parameters
            for (int x = 0; x < tCmd.Parameters.Count - 1; x++)
            {
                if (tCmd.Parameters[x].IsArray) throw new ArgumentException("Failed to register command [" + command +
                     "], parameter array [" + tCmd.Parameters[x].Name + "] must be last parameter!");
            }
            return tCmd;
        }

        /// <summary>
        /// Register type as plugin.
        /// </summary>
        /// <param name="pluginType"></param>
        public void RegisterPluginType(Type pluginType)
        {
            if (PluginTypes.FirstOrDefault(x => x.Type == pluginType) != null)
                throw new ArgumentException("Unable to register plugin " + pluginType.ToString() + ", type already registered!");

            ITcpAppServerPlugin ptrPlugin = Activator.CreateInstance(pluginType) as ITcpAppServerPlugin;
            try
            {
                if (PluginTypes.FirstOrDefault(x => x.Name.Equals(ptrPlugin.PluginName, StringComparison.InvariantCultureIgnoreCase)) != null)
                    throw new ArgumentException("Unable to register plugin, name already exist!");

                if (pluginType.GetInterfaces().Contains(typeof(ITcpAppServerPlugin)))
                    PluginTypes.Add(new TcpAppServerPluginType() { Name = ptrPlugin.PluginName, Description = ptrPlugin.PluginDescription, Type = pluginType });

                else throw new ArgumentException("Unable to register plugin, input type does not implement ITcpAppServerPlugin interface!");
            }
            finally { ptrPlugin.DisposeRequest(); }
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
            string aliasName = sender.Command.Parameter("Plugin").Value;
            List<string> lines = null;
            if (aliasName == "-")
            {
                //Print Help Screen - Write to sender.OutputMessage
                lines = new List<string>
                    {
                        "TCP Aplication Server Version " + Version.ToString(),
                        " ",
                        "==== USAGE ====",
                        "  Execute Tcp App Server Command:",
                        "  SEND: <Command> [Param0] ... [ParamN]",
                        "  RECV: <Status> [Return Message]",
                        " ",
                        "  Execute plugin Command:",
                        "  SEND: <Alias> <Command> [Param0] ... [ParamN]",
                        "  RECV: <Status> [Return Message]",
                        " ",
                        "  Notes:",
                        "   Command are not case sensitive.",
                        "   <> = Required parameters",
                        "   [] = Optional parameters",
                        " ",
                    };

                lines.AddRange(TcpAppCommon.PrintCommandHelpContents(Commands));

                //Print registered plugins
                if (PluginTypes.Count > 0)
                {
                    lines.Add("[ PLUGINS ]");
                    lines.AddRange(PluginTypes.Select(x => string.Format(" {0, -20}  {1}", x.Name, x.Description)).ToArray());
                    lines.Add(" ");
                }

                if (Plugins.Count > 0)
                {
                    lines.Add("[ OBJECTS ]");
                    lines.AddRange(Plugins.Select(x => string.Format(" {0, -20}  {1}", x.Alias, "(" + PluginTypes.FirstOrDefault(n => n.Type == x.GetType())?.Name + ")")).ToArray());
                }

                sender.OutputMessage = string.Join(TcpAppCommon.NewLine, lines.ToArray());
                sender.Status = TcpAppCommandStatus.OK;
            }
            else
            {
                //Get Help Content for selected object.
                string pluginName = sender.Command.Parameter("Plugin").Value;
                ITcpAppServerPlugin plugin = null;
                TcpAppServerPluginType ptrType = PluginTypes.FirstOrDefault(x => x.Name.Equals(pluginName, StringComparison.InvariantCultureIgnoreCase));
                if (ptrType != null)
                {
                    //Get help by type
                    Type pluginType = ptrType.Type;
                    plugin = Plugins.FirstOrDefault(x => x.GetType() == pluginType);
                    if (plugin != null)
                    {
                        //Show Help using existing object
                        plugin.ShowHelp(sender);
                        return;
                    }

                    //Create instance and show help, dispose after use
                    plugin = Activator.CreateInstance(pluginType) as ITcpAppServerPlugin;
                    plugin.ShowHelp(sender);
                    plugin.DisposeRequest();
                    plugin = null;
                    return;
                }
                else
                {
                    //Get help by alias name
                    plugin = Plugins.FirstOrDefault(x => string.Compare(x.Alias, pluginName, true) == 0);
                    if (plugin != null)
                    {
                        plugin.ShowHelp(sender);
                        return;
                    }
                }
                sender.Status = TcpAppCommandStatus.ERR;
                sender.OutputMessage = "Object [" + aliasName + "] not exist!";

            }
        }

        private void TcpAppServer_ClientConnected(object sender, TcpServerEventArgs e)
        {
            Clients.Add(new TcpAppServerConnection() { Connection = e.Client });
            e.Client.ProcessReceivedMessageCallback = Client_ProcessReceivedMessage;
        }

        private void TcpAppServer_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            TcpAppServerConnection ptrClient = Clients.FirstOrDefault(x => x.Connection == e.Client);
            if (ptrClient != null) Clients.Remove(ptrClient);
        }

        private void Client_ProcessReceivedMessage(TcpServerConnection client, string message, byte[] messageBytes)
        {
            //Parse and Execute Commands
            string[] cmdArg = TcpAppCommon.ParseCommand(message.Trim());
            try
            {
                TcpAppInputCommand inputCommand = TcpAppCommon.CreateInputCommand(Commands, cmdArg);

                if (inputCommand == null)
                {
                    //Check if command keyword is alias name
                    ITcpAppServerPlugin plugin = Plugins.FirstOrDefault(x => string.Compare(x.Alias, cmdArg[0], true) == 0);
                    if (plugin != null)
                    {
                        //Execute plugin command
                        inputCommand = plugin.ExecutePluginCommand(cmdArg.Skip(1).ToArray());
                        inputCommand.Client = Clients.FirstOrDefault(x => x.Connection == client);
                    }
                    else
                    {
                        //Error - Unrecognized command.
                        client.WriteLineToClient(string.Format("{0} {1} Invalid Command!",
                            cmdArg[0], TcpAppCommandStatus.ERR.ToString()));
                        return;
                    }
                }
                else
                {
                    inputCommand.Client = Clients.FirstOrDefault(x => x.Connection == client);
                    inputCommand.ExecuteCallback(); //Execute registered command
                }


                WriteResultToClient(client, inputCommand); //Send result back to client.
            }
            catch (Exception ex)
            {
                WriteExceptionErrorToClient(client, ex);
            }
        }

        private void WriteResultToClient(TcpServerConnection client, TcpAppInputCommand input)
        {
            string returnMsg = input.Status.ToString() + " ";
            if (!string.IsNullOrEmpty(input.OutputMessage)) returnMsg += input.OutputMessage;
            System.Diagnostics.Trace.WriteLine("Write to Client : " + returnMsg);
            client.WriteLineToClient(returnMsg);
        }

        private void WriteExceptionErrorToClient(TcpServerConnection client, Exception ex)
        {
            string returnMsg = TcpAppCommandStatus.ERR + " " + ex.Message;
            System.Diagnostics.Trace.WriteLine("Write to Client: " + returnMsg);
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
