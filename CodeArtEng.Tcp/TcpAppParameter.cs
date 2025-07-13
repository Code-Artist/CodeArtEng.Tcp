using System.Collections.Generic;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Command Parameter
    /// </summary>
    /// <seealso cref="TcpAppCommand"/>
    public class TcpAppParameter
    {
        /// <summary>
        /// Parameter Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Default value stored as String.
        /// </summary>
        /// <summary>
        /// Description about this Parameter
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Define default value for parameter.
        /// </summary>
        public string DefaultValue { get; private set; }
        /// <summary>
        /// Defined if Parameter is optional
        /// </summary>
        public bool IsOptional { get; private set; }

        /// <summary>
        /// Parameter's value for command execution. 
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Values for Array type parameter
        /// </summary>
        public List<string> Values { get; private set; }

        /// <summary>
        /// Defined if Parameter is an array
        /// </summary>
        public bool IsArray { get; private set; } = false;

        private TcpAppParameter() { }

        /// <summary>
        /// Factory - Create Optional Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TcpAppParameter CreateOptionalParameter(string name, string description, string defaultValue)
        {
            return new TcpAppParameter() { Name = name, Description = description, DefaultValue = defaultValue?.ToString(), IsOptional = true };
        }

        /// <summary>
        /// Factory - Create Mandatory Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static TcpAppParameter CreateParameter(string name, string description)
        {
            return new TcpAppParameter() { Name = name, Description = description };
        }

        /// <summary>
        /// Factory - Create Parameter array which take 1 or more arguments.
        /// Parameter array can only be declared as last paramter in a command.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="optional"></param>
        /// <returns></returns>
        public static TcpAppParameter CreateParameterArray(string name, string description, bool optional)
        {
            return new TcpAppParameter()
            {
                Name = name,
                Description = description,
                IsArray = true,
                IsOptional = optional,
                Values = new List<string>()
            };
        }
    }
}
