using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server execution callback
    /// </summary>
    /// <param name="sender"></param>
    public delegate void TcpAppServerExecuteDelegate(TcpAppInputCommand sender);

    /// <summary>
    /// TCP Application Command Status
    /// </summary>
    public enum TcpAppCommandStatus
    {
        /// <summary>
        /// OK - No Error
        /// </summary>
        OK,
        /// <summary>
        /// ERROR Raised
        /// </summary>
        ERR
    };

    /// <summary>
    /// TCP Application Server Received Command
    /// </summary>
    /// <remarks>Data structure to handle incoming command received by TCP Application Server.</remarks>
    public class TcpAppInputCommand
    {
        /// <summary>
        /// Command Handler
        /// </summary>
        public TcpAppCommand Command { get; set; }
        /// <summary>
        /// Output message to be send to client.
        /// </summary>
        public string OutputMessage { get; set; }
        /// <summary>
        /// Command execution status.
        /// </summary>
        public TcpAppCommandStatus Status { get; set; } = TcpAppCommandStatus.ERR;
    }

    /// <summary>
    /// TCP Application Command Execution Result
    /// </summary>
    /// <remarks>Used by TcpAppClient</remarks>
    public class TcpAppCommandResult
    {
        /// <summary>
        /// Message returned from TCP Application Server
        /// </summary>
        public string ReturnMessage { get; set; }
        /// <summary>
        /// Command execution status.
        /// </summary>
        public TcpAppCommandStatus Status { get; set; } = TcpAppCommandStatus.ERR;

    }

    /// <summary>
    /// TCP Application Registered Command
    /// </summary>
    public class TcpAppCommand
    {
        internal TcpAppServerExecuteDelegate ExecuteCallback;
        public string Description { get; private set; }
        private List<TcpAppArgument> Arg = new List<TcpAppArgument>();

        /// <summary>
        /// Unique keyword
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// Argument list.
        /// </summary>
        public IList<TcpAppArgument> Arguments { get { return Arg.AsReadOnly(); } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyword">Unique keyword</param>
        /// <param name="executeCallback">Callback function</param>
        public TcpAppCommand(string keyword, string description, TcpAppServerExecuteDelegate executeCallback)
        {
            Keyword = keyword;
            Description = description;
            ExecuteCallback = executeCallback;
        }

        /// <summary>
        /// Add argument to command.
        /// </summary>
        /// <param name="argument"></param>
        public void AddArgument(TcpAppArgument argument)
        {
            //Sanity Check
            if (Arg.FirstOrDefault(x => x.Name.Equals(argument.Name)) != null)
            {
                //Duplicated argument, throw exception.
                throw new ArgumentException("Unable to add argument " + argument.Name + ", alredy exist!");
            }
            Arg.Add(argument);
        }

        /// <summary>
        /// Get Argument by Name, non case sensitive.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TcpAppArgument Argument(string name)
        {
            return Arguments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Reset the value for each argument to default value.
        /// </summary>
        public void ResetArgumentsValue() { foreach (TcpAppArgument item in Arg) item.Value = item.DefaultValue; }
    }

    /// <summary>
    /// TCP Application Command Argument
    /// </summary>
    /// <seealso cref="TcpAppCommand"/>
    public class TcpAppArgument
    {
        /// <summary>
        /// Argument Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Default value stored as String.
        /// </summary>
        public string DefaultValue { get; private set; }
        /// <summary>
        /// Defined if argument is optional
        /// </summary>
        public bool IsOptional { get; private set; }
        /// <summary>
        /// Argument's value for command execution. 
        /// </summary>
        public string Value { get; set; } = string.Empty;
        /// <summary>
        /// Description about this argument
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="isOptional"></param>
        public TcpAppArgument(string name, string description, object defaultValue, bool isOptional = true)
        {
            Name = name;
            DefaultValue = defaultValue.ToString();
            IsOptional = isOptional;
            Description = description;
        }
    }
}
