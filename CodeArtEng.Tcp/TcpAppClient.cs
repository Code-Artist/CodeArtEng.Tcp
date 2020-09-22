using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace CodeArtEng.Tcp
{

    /// <summary>
    /// TCP Application Client
    /// </summary>
    public class TcpAppClient : TcpClient
    {
        /// <summary>
        /// Occurs when Command sent to TCP Server
        /// </summary>
        public event EventHandler<TcpAppEventArgs> CommandSend;
        /// <summary>
        /// Occurs when Command received from TCP Server
        /// </summary>
        public event EventHandler<TcpAppEventArgs> ResponseReceived;

        private bool Initialized = false;

        /// <summary>
        /// Return Server Application Name
        /// </summary>
        public string ServerAppName { get; private set; }
        /// <summary>
        /// Return Server Application Version
        /// </summary>
        public string ServerAppVersion { get; private set; }

        private Version TcpAppServerVersion { get; set; }
        /// <summary>
        /// List of registered commands read from TcpAppServer
        /// </summary>
        public List<string> Commands { get; private set; } = new List<string>();

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
            if (!Connected) Initialized = false;
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

                //ToDo: TcpAppClient - Disable local command verification?
                //Verify command registered in function list, only active after Connect() sequence completed.
                //if (Initialized)
                //{
                //    if (!Commands.Contains(commandKeyword, StringComparer.InvariantCultureIgnoreCase))
                //    {
                //        throw new TcpAppClientException("Invalid Command: " + commandKeyword);
                //    }
                //}

                string tcpCommand = command + TcpAppCommon.Delimiter;
                CommandSend?.Invoke(this, new TcpAppEventArgs(command));
                Write(tcpCommand);

                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < timeout)
                //while(true)
                {
                    string response = ReadString();
                    ResponseReceived?.Invoke(this, new TcpAppEventArgs(response));
                    if (!string.IsNullOrEmpty(response))
                    {
                        string[] resultParams = response.Split('\r');

                        //ToDo: Handle Busy Status?
                        result.Status = (TcpAppCommandStatus)Enum.Parse(typeof(TcpAppCommandStatus), resultParams[0]);
                        if (resultParams.Length > 1) result.ReturnMessage = string.Join(TcpAppCommon.NewLine, resultParams.Skip(1)).Trim(); //Remove trailing CRLF
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
        /// <param name="command"></param>
        /// <param name="timeout">Command timeout in ms</param>
        /// <returns></returns>
        public TcpAppCommandResult ExecuteCommand(string command, int timeout = 1000)
        {
            if (!Connected) throw new TcpAppClientException("Connection with server not established!");
            if (!Initialized) throw new TcpAppClientException("TcpApp not initialized, execute Connect() first!");
            return ExecuteTcpAppCommand(command, timeout);
        }

        /// <summary>
        /// Connect to TCP server and initialize TCP Application.
        /// </summary>
        public override void Connect()
        {
            Initialized = false;
            base.Connect();
            Thread.Sleep(50);
            TcpAppCommandResult result = ExecuteTcpAppCommand("TcpAppInit", 3000);
            string[] param = result.ReturnMessage.Trim().Split(' ');
            ServerAppName = param[0];
            ServerAppVersion = param[1];

            result = ExecuteTcpAppCommand("Version?");
            if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
            TcpAppServerVersion = new Version(result.ReturnMessage);

            //Version Check - Always recommend user to use library from same major release.
            if (TcpAppServerVersion.Major > Version.Major)
            {
                Trace.WriteLine("WARNING: Server application created with newer library, version " +
                    TcpAppServerVersion.ToString() + ". Some feature might not be available.");
            }

            result = ExecuteTcpAppCommand("FunctionList?");
            if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
            Commands.AddRange(result.ReturnMessage.Split(' '));

            Trace.WriteLine("TCP Application Connection Ready.");
            Initialized = true;
        }

    }

}
