using System.Text;
using System.Threading.Tasks;

namespace CodeArtEng.Tcp
{


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
}
