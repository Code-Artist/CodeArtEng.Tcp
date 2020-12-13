using System;
using System.Linq;
using System.Collections.Generic;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Registered Command
    /// </summary>
    public class TcpAppCommand : ICloneable
    {
        internal TcpAppServerExecuteDelegate ExecuteCallback { get; set; }

        /// <summary>
        /// Command which executed by message queues
        /// </summary>
        public bool UseMessageQueue { get; set; }

        /// <summary>
        /// Command description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Unique keyword
        /// </summary>
        public string Keyword { get; internal set; }

        /// <summary>
        /// Return true if command is <see cref="TcpAppServer"/> built in Command.
        /// </summary>
        public bool IsSystemCommand { get; internal set; }

        private readonly List<TcpAppParameter> Params = new List<TcpAppParameter>();
        /// <summary>
        /// Parameter list.
        /// </summary>
        public IList<TcpAppParameter> Parameters { get { return Params.AsReadOnly(); } }

        private TcpAppCommand() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyword">Command unique keyword</param>
        /// <param name="description">Description / Short Help.</param>
        /// <param name="executeCallback">Callback function</param>
        public TcpAppCommand(string keyword, string description, TcpAppServerExecuteDelegate executeCallback)
        {
            Keyword = keyword;
            Description = description;
            ExecuteCallback = executeCallback;
        }

        /// <summary>
        /// Add parameter to command.
        /// </summary>
        /// <param name="parameter"></param>
        public void AddParameter(TcpAppParameter parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            //Sanity Check
            if (Params.FirstOrDefault(x => x.Name.Equals(parameter.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                //Duplicated parameter, throw exception.
                throw new ArgumentException("Unable to add parameter " + parameter.Name + ", alredy exist!");
            }
            Params.Add(parameter);
        }

        /// <summary>
        /// Get Parameter by Name, non case sensitive.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TcpAppParameter Parameter(string name)
        {
            return Parameters.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Reset the value for each parameter to default value.
        /// </summary>
        public void ResetParametersValue()
        {
            foreach (TcpAppParameter item in Params)
            {
                item.Value = item.DefaultValue;
                item.Values?.Clear();
            }
        }

        /// <summary>
        /// Clone current object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            TcpAppCommand result = new TcpAppCommand();
            result.Description = Description;
            result.Keyword = Keyword;
            result.ExecuteCallback = ExecuteCallback;
            result.IsSystemCommand = IsSystemCommand;
            result.UseMessageQueue = UseMessageQueue;
            foreach (TcpAppParameter p in Parameters) result.AddParameter(p);
            return result;
        }
    }
}
