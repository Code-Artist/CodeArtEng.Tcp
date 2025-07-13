namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Plugin Interface for Plugin Implementation
    /// </summary>
    public interface ITcpAppServerPlugin
    {
        /// <summary>
        /// Short Name
        /// </summary>
        /// <remarks>Single word name without space</remarks>
        string PluginName { get; }

        /// <summary>
        /// Short Description for this plugin type
        /// </summary>
        string PluginDescription { get; }

        /// <summary>
        /// Unique alias name for plugin object
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Execute TCP Application Command for Plugin.
        /// </summary>
        /// <param name="commandArguments">Command and arguments in string array form</param>
        TcpAppInputCommand GetPluginCommand(string [] commandArguments);

        /// <summary>
        /// TcpAppServer request to dispose object. 
        /// </summary>
        /// <returns>true = proceed to dispose object; false = request rejected, object not disposed.</returns>
        bool DisposeRequest();

        /// <summary>
        /// Print help for plugin.
        /// </summary>
        /// <param name="sender"></param>
        void ShowHelp(TcpAppInputCommand sender);
    }
}
