using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

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
        //ToDo: Option for TcpAppServer to execute with / without thread.

        private long Counter = 0;
        /// <summary>
        /// Occur when TcpAppClient is about to sign in. Subscribe this event to decide if a client is allowed to connect.
        /// </summary>
        public event EventHandler<TcpAppServerExEventArgs> ClientSigningIn;
        /// <summary>
        /// Occur when <see cref="TcpAppClient"/> signed in successfully.
        /// </summary>
        public event EventHandler<TcpAppServerEventArgs> ClientSignedIn;
        /// <summary>
        /// Occur when <see cref="TcpAppClient"/> disconnected from server.
        /// </summary>
        public event EventHandler<TcpAppServerEventArgs> ClientSigningOut;
        /// <summary>
        /// Occure when plugin object is created. This event only triggered by TCP App Command.
        /// </summary>
        public event EventHandler<TcpAppServerEventArgs> PluginCreated;
        /// <summary>
        /// Occur when plugin object is disposed. This event only triggered by TCP App Command.
        /// </summary>
        public event EventHandler<TcpAppServerEventArgs> PluginDisposed;
        /// <summary>
        /// Occur before plugin command is executed. Override this event to control if a plugin command is allowed to be executed.
        /// </summary>
        public event EventHandler<TcpAppServerExEventArgs> BeforeExecutePluginCommand;

        /// <summary>
        /// <see cref="TcpAppClient"/> connected to server.
        /// </summary>
        public List<TcpAppServerConnection> AppClients { get; private set; } = new List<TcpAppServerConnection>();
        
        
        private readonly List<TcpAppCommand> Commands = new List<TcpAppCommand>();
        private readonly List<TcpAppServerPluginType> PluginTypes = new List<TcpAppServerPluginType>();
        private readonly List<ITcpAppServerPlugin> _Plugins = new List<ITcpAppServerPlugin>();

        private readonly List<TcpAppInputCommand> ResultQueue = new List<TcpAppInputCommand>();
        private readonly List<TcpAppInputCommand> CommandQueue = new List<TcpAppInputCommand>();
        private readonly EventWaitHandle CommandQueueWaitSignal = new ManualResetEvent(false);
        private readonly Thread CommandQueueThread;

        /// <summary>
        /// Return list of queued commands
        /// </summary>
        /// <returns></returns>
        public string[] GetQueuedCommand()
        {
            lock (CommandQueue)
            {
                return CommandQueue.Select(x => x.AppClient.Name + ": " + string.Join(" ", x.Command, x.Arguments)).ToArray();
            }
        }

        /// <summary>
        /// Return list of queued results
        /// </summary>
        /// <returns></returns>
        public string[] GetQueuedResults()
        {
            lock (ResultQueue)
            {
                return ResultQueue.Select(x => x.AppClient.Name + ": " + string.Join(" ", x.Command, x.Arguments) + " [" + x.Status.ToString() + "]").ToArray();
            }
        }

        /// <summary>
        /// Duration in ms to terminate command execution and return timeout error.
        /// Default is 0, wait forever.
        /// </summary>
        /// <remarks>Not applicable for system commands and queued commands</remarks>
        public int ExecutionTimeout { get; set; } = 0;

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
            CommandQueueThread = new Thread(ExecuteQueuedCommandsAsync);
            CommandQueueThread.Start();

            //TcpAppServer Format: 
            // TX: TCP: <Command> [-Param0] [-Param1] ... [-ParamN]
            // RX: TCP: <Status> [Return Message]
            // Source - Appname@192.168.0.1:23
            // Command - Registered Command.

            //Register System Commands
            //--- INIT (Commands used by TcpAppClient) ---
            RegisterSystemCommand("Help", "Show help screen. Include plugin type or object alias name to show commands for selected plugin.", ShowHelp,
                TcpAppParameter.CreateOptionalParameter("Plugin", "Plugin type or Alias", "-"));
            RegisterSystemCommand("SignIn", "Sign in to TcpAppServer. Server will verify connection id and return unique ID.", delegate (TcpAppInputCommand sender)
                {
                    //Assign Name
                    string machineName = sender.Command.Parameter("ConnectionID").Value?.Replace(" ", "_");
                    if (string.IsNullOrEmpty(machineName)) machineName = sender.AppClient.Connection.ClientIPAddress.ToString();

                    if (sender.AppClient.SignedIn)
                    {
                        //Client already signed in, verify connection ID.
                        if (sender.AppClient.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            sender.OutputMessage = sender.AppClient.Name;
                            sender.Status = TcpAppCommandStatus.OK;
                            return;
                        }
                        else sender.AppClient.SignedIn = false;
                    }

                    TcpAppServerExEventArgs signInArg = new TcpAppServerExEventArgs(sender.AppClient) { Value = machineName };
                    ClientSigningIn?.Invoke(this, signInArg);
                    if (signInArg.Cancel == true)
                    {
                        sender.OutputMessage = signInArg.Reason;
                        if (string.IsNullOrEmpty(sender.OutputMessage)) sender.OutputMessage = "Access Denied!";
                        sender.Status = TcpAppCommandStatus.ERR;
                        return;
                    }

                    string uniqueName = machineName;
                    lock (AppClients)
                    {
                        //Cleanup instance with same name but already disconnected without signout
                        if (AppClients.FirstOrDefault(x => x.Name == uniqueName) != null)
                        {
                            for (int x = 0; x < AppClients.Count;)
                            {
                                if (!AppClients[x].Connection.Connected && AppClients[x].Name.StartsWith(uniqueName))
                                {
                                    AppClients[x].Dispose();
                                    AppClients.RemoveAt(x);
                                }
                                else x++;
                            }
                        }
                        while (AppClients.FirstOrDefault(x => x.Name == uniqueName) != null)
                        {
                            uniqueName = machineName + "_" + (++Counter).ToString();
                        }
                        sender.AppClient.Name = uniqueName;
                        sender.OutputMessage = uniqueName;
                        sender.Status = TcpAppCommandStatus.OK;
                        sender.AppClient.SignedIn = true;
                        ClientSignedIn?.Invoke(this, new TcpAppServerEventArgs(sender.AppClient)); //Event
                    }

                },
                TcpAppParameter.CreateParameter("ConnectionID", "Connection ID. If already exist, server will return an updated unique ID."));
            RegisterSystemCommand("SignOut", "Signout TcpAppClient.", delegate (TcpAppInputCommand sender)
                {
                    ClientSigningOut?.Invoke(this, new TcpAppServerEventArgs(sender.AppClient));
                    sender.AppClient.Dispose();
                    lock (AppClients) { AppClients.Remove(sender.AppClient); }
                    sender.AppClient.SignedIn = false;
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("Version?", "Get TcpAppServer Library Version", delegate (TcpAppInputCommand sender)
                {
                    sender.OutputMessage = Version.ToString();
                    sender.Status = TcpAppCommandStatus.OK;
                });
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
            RegisterSystemCommand("Terminate", "Terminate Application. Command only valid after client Signin. Default Exit Code = -99", delegate (TcpAppInputCommand sender)
            {
                VerifyUserSignedIn(sender);
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

            //--- Execution ---
            RegisterSystemCommand("FunctionList?", "Get list of registered functions.", delegate (TcpAppInputCommand sender)
            {
                foreach (TcpAppCommand x in Commands)
                {
                    sender.OutputMessage += x.Keyword;
                    sender.OutputMessage += " ";
                }
                sender.Status = TcpAppCommandStatus.OK;
            });
            RegisterSystemCommand("Execute", "Execute plugin's command. Command only valid after client Signin.", delegate (TcpAppInputCommand sender)
            {
                VerifyUserSignedIn(sender);
                ITcpAppServerPlugin plugin = _Plugins.FirstOrDefault(x => string.Compare(x.Alias, sender.Command.Parameter("Alias").Value, true) == 0);
                if (plugin == null) throw new ArgumentException("Plugin not exists!");

                TcpAppInputCommand pluginCommand = plugin.GetPluginCommand(sender.Arguments.Skip(1).ToArray());
                TcpAppServerExEventArgs pluginExecuteEventArgs = new TcpAppServerExEventArgs(sender.AppClient) { Plugin = plugin };
                BeforeExecutePluginCommand?.Invoke(this, pluginExecuteEventArgs);
                if (pluginExecuteEventArgs.Cancel)
                {
                    //Command execution cancelled by server, return error with reason.
                    sender.Status = TcpAppCommandStatus.ERR;
                    sender.OutputMessage = pluginExecuteEventArgs.Reason;
                }
                else
                {
                    //Proceed with execution.
                    pluginCommand.ExecuteCallback();
                    sender.Status = pluginCommand.Status;
                    sender.OutputMessage = pluginCommand.OutputMessage;
                }
            },
                TcpAppParameter.CreateParameter("Alias", "Plugin Alias Name."));
            RegisterSystemCommand("CheckStatus", "Check execution status for queued command. RETURN: Command status if executed, else BUSY. ERR if no queued message.",
                delegate (TcpAppInputCommand sender)
                {
                    int queueID = Convert.ToInt32(sender.Command.Parameter("QueueID").Value);
                    if (queueID == 0)
                    {
                        //ID not specified, get status for next queued command.
                        if (sender.AppClient.NextQueuedCommand == null)
                        {
                            sender.Status = TcpAppCommandStatus.ERR;
                            sender.OutputMessage = "No queued message!";
                            return;
                        }

                        sender.Status = sender.AppClient.NextQueuedCommand.Status;
                        switch (sender.AppClient.NextQueuedCommand.Status)
                        {
                            case TcpAppCommandStatus.BUSY:
                            case TcpAppCommandStatus.QUEUED:
                                sender.OutputMessage = string.Empty;
                                break;

                            default:
                                lock (CommandQueue)
                                {
                                    //Return result for all queued message except executing one.
                                    TcpAppInputCommand[] results = ResultQueue.Where(x => x.AppClient == sender.AppClient).ToArray();
                                    TcpAppInputCommand nextQueue = null;
                                    foreach (TcpAppInputCommand cmd in results)
                                    {
                                        if (cmd.Status == TcpAppCommandStatus.BUSY || cmd.Status == TcpAppCommandStatus.QUEUED)
                                        {
                                            nextQueue = cmd;
                                            break;
                                        }
                                        if (cmd.Status == TcpAppCommandStatus.ERR)
                                        {
                                            cmd.Status = sender.Status;
                                            cmd.OutputMessage += "! "; //Prefix for command with error status.
                                        }
                                        ResultQueue.Remove(cmd);
                                        sender.OutputMessage += cmd.OutputMessage + "\n";
                                    }

                                    //Return number of remaining queued commands
                                    sender.OutputMessage += CommandQueue.Where(x => x.AppClient == sender.AppClient).Count().ToString();
                                    sender.AppClient.NextQueuedCommand = nextQueue;
                                }
                                break;
                        }
                    }
                    else
                    {
                        //Return status of specific message.
                        TcpAppInputCommand ptrCmd = CommandQueue.FirstOrDefault(x => x.AppClient == sender.AppClient && x.ID == queueID);
                        if (ptrCmd == null)
                        {
                            ptrCmd = ResultQueue.FirstOrDefault(x => x.AppClient == sender.AppClient);
                            if (ptrCmd != null) ResultQueue.Remove(ptrCmd);
                            else
                            {
                                sender.Status = TcpAppCommandStatus.ERR;
                                sender.OutputMessage = "Invalid ID: " + queueID.ToString();
                                return;
                            }
                        }

                        sender.OutputMessage = ptrCmd.OutputMessage;
                        sender.Status = ptrCmd.Status;
                    }
                },
                TcpAppParameter.CreateOptionalParameter("QueueID", "Get status of specific message.", "0"));
            RegisterSystemCommand("Abort", "Abort last queued command.", delegate (TcpAppInputCommand sender)
            {
                VerifyUserSignedIn(sender);
                if (sender.AppClient.NextQueuedCommand != null)
                {
                    lock (CommandQueue)
                    {
                        CommandQueue.Remove(sender.AppClient.NextQueuedCommand);
                        ResultQueue.Remove(sender.AppClient.NextQueuedCommand);
                        sender.AppClient.NextQueuedCommand = null;
                    }
                }
                sender.Status = TcpAppCommandStatus.OK;
            });
            RegisterSystemCommand("FlushQueue", "Flush message queue for calling client.", delegate (TcpAppInputCommand sender)
                {
                    VerifyUserSignedIn(sender);
                    lock (CommandQueue)
                    {
                        CommandQueue.RemoveAll(x => x.AppClient == sender.AppClient);
                        ResultQueue.RemoveAll(x => x.AppClient == sender.AppClient);
                        sender.AppClient.NextQueuedCommand = null;
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("FlushAllQueue", "Flush message queue for all clients.", delegate (TcpAppInputCommand sender)
                {
                    VerifyUserSignedIn(sender);
                    lock (CommandQueue)
                    {
                        CommandQueue.Clear();
                        ResultQueue.Clear();
                        lock (AppClients)
                        {
                            foreach (TcpAppServerConnection client in AppClients) client.NextQueuedCommand = null;
                        }
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                });

            //--- PLUGIN ---
            RegisterSystemCommand("PluginTypes?", "Get list of plugin class type. Use CreatePlugins command to instantiate type.", delegate (TcpAppInputCommand sender)
                {
                    if (PluginTypes.Count == 0) sender.OutputMessage = "-NONE-";
                    else sender.OutputMessage = string.Join(" ", PluginTypes.Select(x => x.Name).ToArray());
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("CreatePlugin", "Create an plugin object from listed plugin types.  Command only valid after client Signin.", delegate (TcpAppInputCommand sender)
                {
                    VerifyUserSignedIn(sender);
                    string typeName = sender.Command.Parameter("TypeName").Value;

                    //Sanity Check - Type Name
                    if (PluginTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)) == null)
                        throw new ArgumentException("Unknown plugin type: " + typeName);

                    Type pluginType = PluginTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)).Type;

                    string aliasName;
                    sender.OutputMessage = "Plugin created:";
                    foreach (string value in sender.Command.Parameter("Alias").Values)
                    {
                        aliasName = value;
                        if (Commands.FirstOrDefault(x => string.Compare(x.Keyword, aliasName, true) == 0) != null)
                            throw new ArgumentException("Unable to create plugin with alias '" + aliasName + "'. Name already registered as command!");

                        //Sanity Check - Verify alias name is not plugin type name
                        if (PluginTypes.FirstOrDefault(x => string.Compare(x.Name, aliasName, true) == 0) != null)
                            throw new ArgumentException("Unable to create plugin with alias '" + aliasName + "'. Name already registered plugin type!");

                        //Sanity Check - Alias Name
                        if (_Plugins.FirstOrDefault(x => string.Compare(x.Alias, aliasName, true) == 0) != null)
                            throw new ArgumentException("Unable to create plugin with alias '" + aliasName + "'. Plugin already exists!");

                        ITcpAppServerPlugin pluginInstance = Activator.CreateInstance(pluginType) as ITcpAppServerPlugin;
                        pluginInstance.Alias = aliasName;
                        _Plugins.Add(pluginInstance);
                        PluginCreated?.Invoke(this, new TcpAppServerEventArgs(sender.AppClient) { Plugin = pluginInstance });
                        sender.OutputMessage += " " + aliasName;
                    }
                    sender.Status = TcpAppCommandStatus.OK;
                },
                TcpAppParameter.CreateParameter("TypeName", "Plugin type name."),
                TcpAppParameter.CreateParameterArray("Alias", "Plugin object name, case insensitive.", false));
            RegisterSystemCommand("Plugins?", "Return plugins list by alias name.", delegate (TcpAppInputCommand sender)
                {
                    if (_Plugins.Count == 0) sender.OutputMessage = "-NONE-";
                    else sender.OutputMessage = string.Join(TcpAppCommon.NewLine, _Plugins.Select(x => x.Alias + "(" + PluginTypes.FirstOrDefault(n => n.Type == x.GetType())?.Name + ")").ToArray());
                    sender.Status = TcpAppCommandStatus.OK;
                });
            RegisterSystemCommand("DisposePlugin", "Delete plugin by alias name. Command only valid after client Signin.", delegate (TcpAppInputCommand sender)
                {
                    VerifyUserSignedIn(sender);
                    string alias = sender.Command.Parameter("Alias").Value;
                    ITcpAppServerPlugin item = _Plugins.FirstOrDefault(x => string.Compare(x.Alias, alias, true) == 0);
                    if (item == null)
                    {
                        sender.OutputMessage = "Plugin [" + alias + "] not found / disposed.";
                        sender.Status = TcpAppCommandStatus.OK; return;
                    }
                    else
                    {
                        if (item.DisposeRequest() == true)
                        {
                            _Plugins.Remove(item);
                            PluginDisposed?.Invoke(this, new TcpAppServerEventArgs(sender.AppClient) { Plugin = item });
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
                TcpAppParameter.CreateParameter("Alias", "Plugin alias name."));


        }

        private void VerifyUserSignedIn(TcpAppInputCommand command)
        {
            if (command.AppClient.SignedIn) return;
            throw new InvalidOperationException("Client not signed in!");
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
        /// Register new command which execute using command queue
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        /// <param name="executeCallback"></param>
        /// <param name="parameters"></param>
        public void RegisterQueuedCommand(string command, string description, TcpAppServerExecuteDelegate executeCallback, params TcpAppParameter[] parameters)
        {
            RegisterCommandInt(command, description, executeCallback, parameters).UseMessageQueue = true;
        }

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
            if (string.IsNullOrEmpty(command)) throw new ArgumentNullException(nameof(command));
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
            if (pluginType == null) throw new ArgumentNullException(nameof(pluginType));
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
        /// Add plugin object directly from TcpAppServer by code.
        /// </summary>
        /// <param name="plugin"></param>
        public void AddPlugin(ITcpAppServerPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (_Plugins.FirstOrDefault(x => string.Compare(x.Alias, plugin.Alias, true) == 0) != null)
                throw new ArgumentException("Unable to add object with alias '" + plugin.Alias + "'. Object already exists!");

            //Verify input object type is registered
            if (PluginTypes.FirstOrDefault(x => x.Type == plugin.GetType()) == null)
                throw new ArgumentException("Plugin type not registered! " + plugin.GetType().ToString());

            _Plugins.Add(plugin);
        }

        /// <summary>
        /// Remove plugin by code.
        /// </summary>
        /// <param name="plugin"></param>
        public void DisposePlugin(ITcpAppServerPlugin plugin)
        {
            _Plugins.Remove(plugin);
        }

        private void ShowHelp(TcpAppInputCommand sender)
        {
            string aliasName = sender.Command.Parameter("Plugin").Value;
            List<string> lines = null;
            if (aliasName == "-")
            {
                lines = new List<string>
                    {
                        Application.ProductName +  " V" + Application.ProductVersion.ToString(),
                        "[ TCP Aplication Server V" + Version.ToString() + " ]",
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

                if (_Plugins.Count > 0)
                {
                    lines.Add("[ OBJECTS ]");
                    lines.AddRange(_Plugins.Select(x => string.Format(" {0, -20}  {1}", x.Alias, "(" + PluginTypes.FirstOrDefault(n => n.Type == x.GetType())?.Name + ")")).ToArray());
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
                    plugin = _Plugins.FirstOrDefault(x => x.GetType() == pluginType);
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
                    plugin = _Plugins.FirstOrDefault(x => string.Compare(x.Alias, pluginName, true) == 0);
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
            AddClientToAppClientsList(e.Client);
        }

        private TcpAppServerConnection AddClientToAppClientsList(TcpServerConnection client)
        {
            lock (AppClients)
            {
                TcpAppServerConnection result = new TcpAppServerConnection() { Connection = client, Name = "#" + client.ClientIPAddress.ToString() };
                AppClients.Add(result);
                client.ProcessReceivedMessageCallback = Client_ProcessReceivedMessage;
                return result;
            }
        }

        private void TcpAppServer_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            //Client Disconnected - clean up queued message
            lock (CommandQueue) { CommandQueue.RemoveAll(x => x.AppClient.Connection == e.Client); }

        }

        private void Client_ProcessReceivedMessage(TcpServerConnection client, string message, byte[] messageBytes)
        {
            //Parse and Execute Commands
            string[] cmdArg = TcpAppCommon.ParseCommand(message.Trim());
            try
            {
                //Register Client Connection
                TcpAppServerConnection ptrClient = AppClients.FirstOrDefault(x => x.Connection == client);
                if (ptrClient == null)
                {
                    //Reconstruct device which had already signed out
                    ptrClient = AddClientToAppClientsList(client);
                }

                TcpAppInputCommand inputCommand = TcpAppCommon.CreateInputCommand(Commands, cmdArg);
                if (inputCommand != null)
                {
                    inputCommand.AppClient = ptrClient;

                }
                else//Command keyword not exist
                {
                    //Check if command keyword is alias name
                    ITcpAppServerPlugin plugin = _Plugins.FirstOrDefault(x => string.Compare(x.Alias, cmdArg[0], true) == 0);
                    if (plugin != null)
                    {
                        //Execute plugin command
                        inputCommand = plugin.GetPluginCommand(cmdArg.Skip(1).ToArray());
                        inputCommand.AppClient = ptrClient;
                        BeforeExecutePluginCommand?.Invoke(this, new TcpAppServerExEventArgs(ptrClient) { Plugin = plugin });
                    }
                    else
                    {
                        //Error - Unrecognized command.
                        inputCommand = new TcpAppInputCommand()
                        {
                            Status = TcpAppCommandStatus.ERR,
                            OutputMessage = "Invalid Command " + cmdArg[0]
                        };
                        WriteResultToClient(ptrClient, inputCommand);
                        return;
                    }
                }

                //Verify Client had signed in.
                if (!inputCommand.AppClient.SignedIn && !inputCommand.Command.IsSystemCommand)
                    throw new Exception("Client not signed in! Execute SignIn first.");

                if (inputCommand.Command.UseMessageQueue && !inputCommand.Command.IsSystemCommand)
                {
                    //Single thread execution, post message to message queue.
                    lock (CommandQueue)
                    {
                        inputCommand.ID = inputCommand.GetHashCode();
                        if (ptrClient.NextQueuedCommand == null) ptrClient.NextQueuedCommand = inputCommand; //Set pointer to next queued command.

                        //Add Command to Queue
                        CommandQueue.Add(inputCommand);
                        CommandQueueWaitSignal.Set();
                        inputCommand.OutputMessage = inputCommand.ID.ToString();
                        inputCommand.Status = TcpAppCommandStatus.QUEUED;
                    }
                }
                else if (inputCommand.Command.IsSystemCommand) inputCommand.ExecuteCallback();
                else
                {
                    //Execute command, wait until return
                    if (ExecutionTimeout == 0) inputCommand.ExecuteCallback();
                    //Execute command, terminate on timeout
                    else ptrClient.ExecuteCommandAsync(inputCommand, ExecutionTimeout);
                }

                WriteResultToClient(ptrClient, inputCommand); //Send result back to client.
            }
            catch (Exception ex)
            {
                WriteExceptionErrorToClient(client, ex);
            }
        }

        private void ExecuteQueuedCommandsAsync()
        {
            while (true)
            {
                if (CommandQueue.Count == 0)
                {
                    //Queued empty, go to sleep
                    CommandQueueWaitSignal.Reset();
                    CommandQueueWaitSignal.WaitOne();
                }
                else
                {
                    //Process Queued Commands
                    TcpAppInputCommand ptrCommand;
                    lock (CommandQueue)
                    {
                        ptrCommand = CommandQueue.First();
                        CommandQueue.RemoveAt(0);
                    }

                    System.Diagnostics.Trace.WriteLine(ptrCommand.AppClient.Name + " QCommand: " + string.Join(" ", ptrCommand.Command.Keyword));
                    ResultQueue.Add(ptrCommand);
                    ptrCommand.ExecuteCallback();
                }
            }
        }

        /// <summary>
        /// /Dispose object - Abort Command Queue Execution Thread.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            CommandQueueThread.Abort();
            CommandQueueWaitSignal.Dispose();
            lock (AppClients)
            {
                foreach (TcpAppServerConnection c in AppClients)
                {
                    c.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void WriteResultToClient(TcpAppServerConnection client, TcpAppInputCommand input)
        {
            string returnMsg = input.Status.ToString() + " ";
            if (!string.IsNullOrEmpty(input.OutputMessage)) returnMsg += input.OutputMessage;
            System.Diagnostics.Trace.WriteLine(client.Name + " Reply: " + returnMsg);
            client.Connection.WriteLineToClient(returnMsg);
        }

        private void WriteExceptionErrorToClient(TcpServerConnection client, Exception ex)
        {
            string returnMsg = TcpAppCommandStatus.ERR + " " + ex.Message;
            TcpAppServerConnection appServerClient = AppClients.FirstOrDefault(x => x.Connection == client);
            string clientName = appServerClient == null ? client.ClientIPAddress.ToString() : appServerClient.Name;

            System.Diagnostics.Trace.WriteLine(clientName + " ERR: " + returnMsg);
            client.WriteLineToClient(returnMsg);
        }

        private TcpAppCommand GetCommand(string command)
        {
            return Commands.FirstOrDefault(x => x.Keyword.Equals(command, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Return list of plugin objects.
        /// </summary>
        public IList<ITcpAppServerPlugin> Plugins { get => _Plugins.AsReadOnly(); }

    }
}
