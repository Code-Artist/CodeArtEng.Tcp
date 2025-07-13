using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace CodeArtEng.Tcp
{

    /// <summary>
    /// TCP Application Client
    /// </summary>
    public class TcpAppClient : TcpClient
    {
        private bool Initialized = false;
        private Version TcpAppServerVersion { get; set; }

        /// <summary>
        /// Occurs when Command sent to TCP Server
        /// </summary>
        public event EventHandler<TcpAppClientEventArgs> CommandSend;
        /// <summary>
        /// Occurs when Command received from TCP Server
        /// </summary>
        public event EventHandler<TcpAppClientEventArgs> ResponseReceived;

        /// <summary>
        /// Unique Connection ID returned from <see cref="TcpAppServer"/> in <see cref="Connect"/> methods.
        /// </summary>
        public string ConnectionID { get; private set; }
        /// <summary>
        /// Return Server Application Name
        /// </summary>
        public string ServerAppName { get; private set; }
        /// <summary>
        /// Return Server Application Version
        /// </summary>
        public string ServerAppVersion { get; private set; }

        /// <summary>
        /// List of registered commands read from TcpAppServer
        /// </summary>
        public List<string> Commands { get; private set; } = new List<string>();
        /// <summary>
        /// List of plugin objects created in server application.
        /// </summary>
        public List<string> PluginObjects { get; private set; } = new List<string>();

        /// <summary>
        /// Return version of CodeArtEng.Tcp Assembly
        /// </summary>
        public Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public TcpAppClient() : base() { InitInstance(); }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        public TcpAppClient(string hostName, int port) : base(hostName, port) { InitInstance(); }

        private void InitInstance()
        {
            ConnectionStatusChanged += TcpAppClient_ConnectionStatusChanged;
        }

        private void TcpAppClient_ConnectionStatusChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Execute TCP Application Client Command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private TcpAppCommandResult ExecuteTcpAppCommand(string command, int timeout = 1000)
        {
            TcpAppCommandResult result = new TcpAppCommandResult();
            try
            {
                SuspendDataReceivedEvent = true;
                string commandKeyword = command.Split(' ').First();

                //Verify command registered in function list, only active after Connect() sequence completed.
                if (Initialized)
                {
                    //Compare command keyword
                    if (!Commands.Contains(commandKeyword, StringComparer.InvariantCultureIgnoreCase))
                    {
                        //Compare plugin object list
                        if (!PluginObjects.Contains(commandKeyword, StringComparer.InvariantCultureIgnoreCase))
                        {
                            RefreshPlugins(); //Get latest plugin objects from server
                            if (!PluginObjects.Contains(commandKeyword, StringComparer.InvariantCultureIgnoreCase))
                                throw new TcpAppClientException("Invalid Command: " + commandKeyword); //Still no match, FAILED!
                        }
                    }
                }

                string tcpCommand = command + TcpAppCommon.Delimiter;
                CommandSend?.Invoke(this, new TcpAppClientEventArgs(command));
                FlushInputBuffer();
                Write(tcpCommand);

                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < timeout)
                {
                    string response = ReadString();
                    ResponseReceived?.Invoke(this, new TcpAppClientEventArgs(response));
                    if (!string.IsNullOrEmpty(response))
                    {
                        string[] resultParams = response.Split(' ');

                        //BUSY and Queue statue handle by application
                        result.Status = (TcpAppCommandStatus)Enum.Parse(typeof(TcpAppCommandStatus), resultParams[0]);
                        if (resultParams.Length > 1) result.ReturnMessage = string.Join(" ", resultParams.Skip(1)).Trim(); //Remove trailing CRLF
                        return result;
                    }
                    Thread.Sleep(100); //Wait 100ms, retry.
                }//while
                throw new TcpAppClientException("TIMEOUT: No response received from server!");
            }
            catch (TcpAppClientException) { throw; }
            catch (Exception ex)
            {
                throw new TcpAppClientException("Exception raised!", ex);
            }
            finally
            {
                SuspendDataReceivedEvent = false;
            }
        }

        /// <summary>
        /// Execute TCP Application Client Command.
        /// </summary>
        /// <param name="command">Command keyword and arguments in space delimited format, add double quote for argument with spaces.</param>
        /// <param name="timeout">Command timeout in ms</param>
        /// <param name="arguments">Command arguments, optional.</param>
        /// <returns></returns>
        public TcpAppCommandResult ExecuteCommand(string command, int timeout = 1000, string[] arguments = null)
        {
            if (!Initialized) throw new TcpAppClientException("TcpApp not initialized, execute Connect() first!");
            if (!Connected) Connect();

            // Compile arguments into space delimited string, add double quote when argument contains space
            if (arguments != null && arguments.Length > 0)
            {
                command += " " + string.Join(" ", arguments.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg));
            }
            return ExecuteTcpAppCommand(command, timeout);
        }


        /// <summary>
        /// Connect and signin to <see cref="TcpAppServer"/>
        /// </summary>
        public override void Connect()
        {
            base.Connect();
            Thread.Sleep(50);

            TcpAppCommandResult result = ExecuteTcpAppCommand("SignIn " + (Initialized ? ConnectionID : Environment.MachineName), 3000); ;
            if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
            ConnectionID = result.ReturnMessage;

            if (!Initialized)
            {
                //First time login
                //TcpAppServer API Version Check - Always recommend user to use library from same major release.
                result = ExecuteTcpAppCommand("Version?");
                if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
                TcpAppServerVersion = new Version(result.ReturnMessage);
                if (TcpAppServerVersion.Major > Version.Major)
                {
                    Trace.WriteLine("WARNING: Server application created with newer library, version " +
                        TcpAppServerVersion.ToString() + ". Some feature might not be available.");
                }

                result = ExecuteTcpAppCommand("FunctionList?");
                if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
                Commands.AddRange(result.ReturnMessage.Split(' '));

                RefreshPlugins();
                Trace.WriteLine("TCP Application Connection Ready.");
                Initialized = true;
            }
        }

        /// <summary>
        /// Signout and disconnect client.
        /// </summary>
        public override void Disconnect()
        {
            if (base.Connected)
            {
                TcpAppCommandResult result = ExecuteTcpAppCommand("SignOut");
                if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Failed to SignOut client! " + result.ReturnMessage);
            }
            ConnectionID = string.Empty;
            Initialized = false;
            base.Disconnect();
        }

        /// <summary>
        /// Read plugin objects created by Server's application
        /// </summary>
        public void RefreshPlugins()
        {
            PluginObjects.Clear();
            TcpAppCommandResult result = ExecuteTcpAppCommand("Plugins?");
            if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Failed to get plugins list from server! " + result.ReturnMessage);
            if (result.ReturnMessage.Equals("-NONE-")) return;
            PluginObjects.AddRange(result.ReturnMessage.Split('\r').Select(x => x.Split('(')[0].Trim()).ToArray());
        }
    }

}
