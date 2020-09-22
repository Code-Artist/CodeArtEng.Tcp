namespace CodeArtEng.Tcp
{
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
