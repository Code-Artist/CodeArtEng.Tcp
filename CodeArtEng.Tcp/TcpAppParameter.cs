using System;

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
        /// Constructor - Optional Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        public TcpAppParameter(string name, string description, string defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue?.ToString();
            IsOptional = true;
            Description = description;
        }

        /// <summary>
        /// Constructor - Mandatory Parameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public TcpAppParameter(string name, string description)
        {
            Name = name;
            Description = description;
            IsOptional = false;
        }
    }
}
