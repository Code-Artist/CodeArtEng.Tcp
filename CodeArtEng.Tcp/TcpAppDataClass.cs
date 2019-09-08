using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeArtEng.Tcp
{
    internal static class TcpAppCommon
    {
        public static readonly string Delimiter = "\r";

        public static string[] PrintCommandHelpContents(List<TcpAppCommand> commands)
        {
            List<string> lines = new List<string> { "==== COMMAND ====" };
            foreach (TcpAppCommand cmd in commands)
            {
                lines.Add(string.Format(" {0,-20}  {1}", cmd.Keyword, cmd.Description));
                foreach (TcpAppParameter p in cmd.Parameters)
                {
                    if (p.IsOptional)
                        lines.Add(string.Format(" {0,-5}{1,-15}   {2}", " ", "[-" + p.Name + "]", p.Description));
                    else
                        lines.Add(string.Format(" {0,-5}{1,-15}   {2}", " ", "<-" + p.Name + ">", p.Description));
                }
            }
            return lines.ToArray();
        }

        public static string[] ParseCommand(string command)
        {
            var regex = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+");
            string[] strings = regex.Matches(command).Cast<Match>().Select(m => m.Value).ToArray();
            return strings;
        }
    }

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
        /// BUSY - Execution started, command need longer time to complete.
        /// Use 'CheckStatus Command' to query execution status.
        /// </summary>
        BUSY,
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
        public string OutputMessage { get; set; } = string.Empty;
        /// <summary>
        /// Command execution status.
        /// </summary>
        public TcpAppCommandStatus Status { get; set; } = TcpAppCommandStatus.ERR;
        /// <summary>
        /// Command argument string.
        /// </summary>
        public string[] Arguments { get; set; }
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
}
