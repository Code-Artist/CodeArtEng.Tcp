using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeArtEng.Tcp
{
    internal static class TcpAppCommon
    {
        /// <summary>
        /// Termination character for command received by <see cref="TcpAppServer"/>.
        /// Default value is "\r"
        /// </summary>
        public static string Delimiter { get; private set; } = "\r";

        /// <summary>
        /// New Line delimiter for multiline messages. Default value is "\r\n"
        /// </summary>
        public static string NewLine { get; private set; } = "\r\n";

        public static string[] PrintCommandHelpContents(List<TcpAppCommand> commands)
        {
            List<string> lines = new List<string> { "==== COMMANDS ====" };
            TcpAppCommand[] systemCommands = commands.Where(x => x.IsSystemCommand).ToArray();
            if (systemCommands.Count() > 0)
            {
                lines.Add("[ SYSTEM COMMANDS ]");
                PrintCommandDetails(lines, systemCommands);
                lines.Add(" ");
            }

            TcpAppCommand[] appCommands = commands.Where(x => !x.IsSystemCommand).ToArray();
            if (appCommands.Count() > 0)
            {
                lines.Add("[ APPLICATION COMMANDS ]");
                PrintCommandDetails(lines, appCommands);
                lines.Add(" ");
            }

            return lines.ToArray();
        }

        private static void PrintCommandDetails(List<string> lines, TcpAppCommand[] Commands)
        {
            foreach (TcpAppCommand cmd in Commands)
            {
                lines.Add(string.Format(" {0,-20}  {1}", cmd.Keyword.ToUpper(), cmd.Description));
                foreach (TcpAppParameter p in cmd.Parameters)
                {
                    if (p.IsOptional)
                        lines.Add(string.Format("  {0,-19}  ~ {1}", "[" + p.Name + (p.IsArray ? "[]" : "") + "]", p.Description));
                    else
                        lines.Add(string.Format("  {0,-19}  ~ {1}", "<" + p.Name + (p.IsArray ? "[]" : "") + ">", p.Description));
                }
            }
        }

        public static string[] ParseCommand(string command)
        {
            var regex = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+"); //Parse space delimited string with double quote
            string[] strings = regex.Matches(command).Cast<Match>().Select(m => m.Value.Trim()).Where(n => n.Length > 0).ToArray();
            return strings;
        }

        public static TcpAppInputCommand CreateInputCommand(List<TcpAppCommand> commandList, string[] commandArguments)
        {
            TcpAppInputCommand result = null;

            //Process Command Keyword
            TcpAppCommand cmdHandler = commandList.FirstOrDefault(x => x.Keyword.Equals(commandArguments[0], StringComparison.InvariantCultureIgnoreCase));
            if (cmdHandler == null) return null;

            result = new TcpAppInputCommand() { Command = cmdHandler.Clone() as TcpAppCommand };
            result.Arguments = commandArguments.Skip(1).ToArray(); //Arguments exclude command keyword

            //Process Parameters
            cmdHandler.ResetParametersValue();
            int argID = 0; //First Parameter
            foreach (TcpAppParameter item in cmdHandler.Parameters)
            {
                if (argID >= result.Arguments.Length)
                {
                    //Argument with no input
                    if (!item.IsOptional)
                    {
                        //Error - Missing required parameter
                        throw new ArgumentException("Missing required parameter: " + item.Name + "!");
                    }
                }
                else if (item.IsArray)
                {
                    item.Values.Clear();
                    //Parameter Array is last parameters consume all arguments in command
                    for (int m = argID; m < result.Arguments.Length; m++)
                    {
                        item.Values.Add(result.Arguments[m]);
                    }
                    break;
                }
                else
                {
                    item.Value = result.Arguments[argID]; //Assign parameter value
                }
                argID++;
            }
            return result;
        }
    }
}
