// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using System;

namespace Happenstance.SE.DevConsole
{
    /// <summary>
    /// A command is a single action/method that can be called from the console
    /// Description is called if the command is called with no arguments
    /// Command flag is what console will use to find correct command
    /// CommandArgs like main in console apps will pass the arguments to the method
    /// </summary>
    public class ConsoleCommand
    {
        public string CommandName { get; }
        public string CommandDescription { get; }
        public string CommandFlag { get; }
        public Action<string[]> CommandMethod { get; }

        public ConsoleCommand(string name, string description, string flag, Action<string[]> method)
        {
            CommandName = name;
            CommandDescription = description;
            CommandFlag = flag;
            CommandMethod = method;
        }
    }
}
