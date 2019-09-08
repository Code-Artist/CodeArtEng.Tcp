using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Plugin Interface for Plugin Implementation
    /// </summary>
    public interface ITcpAppServerPlugin
    {
        /// <summary>
        /// Unique alias name for plugin object
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Execute TCP Application Command for Plugin.
        /// </summary>
        /// <param name="sender"></param>
        void ExecutePluginCommand(TcpAppInputCommand sender);

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
