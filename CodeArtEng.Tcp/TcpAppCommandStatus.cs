namespace CodeArtEng.Tcp
{


    /// <summary>
    /// TCP Application Command Status
    /// </summary>
    public enum TcpAppCommandStatus
    {
        /// <summary>
        /// ERROR Raised
        /// </summary>
        ERR = -1,
        /// <summary>
        /// OK - No Error
        /// </summary>
        OK = 0,
        /// <summary>
        /// QUEUED - Command added to command queue.
        /// Use 'CheckStatus Command' to query execution status.
        /// </summary>
        QUEUED = 1,
        /// <summary>
        /// BUSY - Execution started, command need longer time to complete.
        /// Use 'CheckStatus Command' to query execution status.
        /// </summary>
        BUSY = 2
    };
}
