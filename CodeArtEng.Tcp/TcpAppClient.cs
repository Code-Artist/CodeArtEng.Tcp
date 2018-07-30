using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace CodeArtEng.Tcp
{
    public class TcpAppEventArgs : EventArgs
    {
        public TcpAppEventArgs(string message) { Message = message; }
        public string Message { get; private set; }
    }

    public class TcpAppClientException : Exception
    {
        public TcpAppCommandResult Result { get; private set; }

        public TcpAppClientException() : base() { }

        public TcpAppClientException(string message) : base(message) { }

        public TcpAppClientException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// TCP Application Client
    /// </summary>
    public class TcpAppClient : TcpClient
    {
        public event EventHandler<TcpAppEventArgs> CommandSend;
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
        private List<string> Commands { get; set; } = new List<string>();

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

        private void OnCommandSend(string command)
        {
            CommandSend?.Invoke(this, new TcpAppEventArgs(command));
        }

        private void OnResponseReceived(string message)
        {
            ResponseReceived?.Invoke(this, new TcpAppEventArgs(message));
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

                //Verify command registered in function list, only active after Connect() sequence completed.
                if (Initialized)
                {
                    if (!Commands.Contains(command, StringComparer.InvariantCultureIgnoreCase))
                    {
                        throw new TcpAppClientException("Invalid Command: " + command);
                    }
                }

                string tcpCommand = "#TCP# " + command + "\r\n";
                OnCommandSend(tcpCommand);
                Write(tcpCommand);

                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < timeout)
                {
                    string response = ReadString();
                    OnResponseReceived(response);
                    string[] resultParams = response.Split(' ');
                    if ((resultParams[0] == "#TCP#") &&
                        (string.Compare(resultParams[1], command, true) == 0))
                    {
                        result.Status = (TcpAppCommandStatus)Enum.Parse(typeof(TcpAppCommandStatus), resultParams[2]);
                        if (resultParams.Length > 3) result.ReturnMessage = string.Join(" ", resultParams.Skip(3));
                        return result;
                    }
                    Thread.Sleep(100); //Wait 100ms, retry.
                }//while
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
            throw new TcpAppClientException("TIMEOUT: No response received from server!");
        }

        /// <summary>
        /// Execute TCP Application Client Command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
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
            if (!Connected)
            {
                base.Connect();
                Thread.Sleep(50);
            }
            TcpAppCommandResult result = ExecuteTcpAppCommand("TcpAppInit", 3000);
            string[] param = result.ReturnMessage.Trim().Split(' ');
            ServerAppName = param[0];
            ServerAppVersion = param[1];

            result = ExecuteTcpAppCommand("TcpAppVersion?");
            if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
            TcpAppServerVersion = new Version(result.ReturnMessage);

            //Version Check - Always recommend user to use library from same major release.
            if (TcpAppServerVersion.Major > Version.Major)
            {
                Trace.WriteLine("WARNING: Server application created with newer library, version " +
                    TcpAppServerVersion.ToString() + ". Some feature might not be available.");
            }

            result = ExecuteTcpAppCommand("GetFunctionList");
            if (result.Status == TcpAppCommandStatus.ERR) throw new TcpAppClientException("Initialization failed! " + result.ReturnMessage);
            Commands.AddRange(result.ReturnMessage.Split(' '));

            Trace.WriteLine("TcpApp Initialized");
            Initialized = true;
        }

    }

}
