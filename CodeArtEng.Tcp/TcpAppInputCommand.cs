﻿using System;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Application Server Received Command
    /// </summary>
    /// <remarks>Data structure to handle incoming command received by TCP Application Server.</remarks>
    public class TcpAppInputCommand
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Command Handler
        /// </summary>
        public TcpAppCommand Command { get; set; }
        /// <summary>
        /// Handle to Client object
        /// </summary>
        public TcpAppServerConnection AppClient { get; set; }
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
        /// <summary>
        /// Execute command callback
        /// </summary>
        public void ExecuteCallback()
        {
            try
            {
                Status = TcpAppCommandStatus.BUSY; //Start Execution
                Command?.ExecuteCallback(this);
                //if (Status == TcpAppCommandStatus.BUSY)
                //{
                //    //Default status as Error is not updated.
                //    Status = TcpAppCommandStatus.ERR;
                //    OutputMessage = "Unknown Error!";
                //}
            }
            catch (Exception ex)
            {
                OutputMessage = "ERROR: Exception Raised! " + ex.Message;
                Status = TcpAppCommandStatus.ERR; //Force status to error, make sure no surprise.
            }
        }
    }
}
