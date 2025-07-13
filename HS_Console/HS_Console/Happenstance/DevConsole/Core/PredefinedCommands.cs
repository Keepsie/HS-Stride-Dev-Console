// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Interfaces;
using System;

namespace Happenstance.SE.DevConsole
{
    public class PredefinedCommands
    {
        private readonly IHSLogger _logger;
        private readonly ConsoleManager _consoleManager;

        public PredefinedCommands(IHSLogger logger, ConsoleManager consoleManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consoleManager = consoleManager;
        }

        public void RegisterAll(ConsoleManager consoleManager)
        {
            // Help command
            var helpDescription = "Displays all available commands\n" +
                                "Usage: help";
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("help", helpDescription, "", HelpCommand));
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("help", "Shows help for a specific command", "-c", HelpCommandForSpecificCommand));

            // Clear command
            var clearDescription = "Clears the console\n" +
                                 "Usage: clear";
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("clear", clearDescription, "", ClearCommand));
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("clear", "Shows help for clear command", "-h", args => GetCommandDescription(consoleManager, "clear")));

            // Exit command
            var exitDescription = "Exits the application\n" +
                                "Usage: exit";
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("exit", exitDescription, "", ExitCommand));
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("exit", "Shows help for exit command", "-h", args => GetCommandDescription(consoleManager, "exit")));

            // Echo command
            var echoDescription = "Prints the given text to the console\n" +
                                "Usage: echo <text>";
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("echo", echoDescription, "", EchoCommand));
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("echo", "Shows help for echo command", "-h", args => GetCommandDescription(consoleManager, "echo")));

            // Version command
            var versionDescription = "Displays the current version of the application\n" +
                                   "Usage: version";
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("version", versionDescription, "", VersionCommand));
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("version", "Shows help for version command", "-h", args => GetCommandDescription(consoleManager, "version")));

            // Execute Script command
            var executeScriptDescription = "Executes a script file\n" +
                                         "Usage: execute_script <file_path>";
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("execute_script", executeScriptDescription, "", ExecuteScriptCommand));
            consoleManager.RegisterConsoleCommand(new ConsoleCommand("execute_script", "Shows help for execute_script command", "-h", args => GetCommandDescription(consoleManager, "execute_script")));
        }

        private void HelpCommand(string[] args)
        {
            var commandNames = _consoleManager.GetCommandNames();
   
            _consoleManager.WriteToConsole("Available commands:", MessageType.Success);
            foreach (var commandName in commandNames)
            {
                _consoleManager.WriteToConsole($"  {commandName}");
            }
            _consoleManager.WriteToConsole("Use '[command] -h' for more information on a specific command.", MessageType.Information);
        }

        private void HelpCommandForSpecificCommand(string[] args)
        {
            if (args.Length > 0)
            {
                GetCommandDescription(_consoleManager, args[0]);
            }
            else
            {
                _consoleManager.WriteToConsole("Usage: help -c [command]", MessageType.Information);
            }
        }

        private void ClearCommand(string[] args)
        {
            _consoleManager.ClearConsole();
        }

        private void ExitCommand(string[] args)
        {
            _consoleManager.WriteToConsole("Exiting application...", MessageType.Success);
            Environment.Exit(0);
        }

        private void EchoCommand(string[] args)
        {
            string message = string.Join(" ", args);
            _consoleManager.WriteToConsole(message);
        }

        private void VersionCommand(string[] args)
        {
            _consoleManager.DisplayConsoleInfo();
        }

        private void ExecuteScriptCommand(string[] args)
        {
            if (args.Length == 0)
            {
                _consoleManager.WriteToConsole("Usage: execute_script <file_path>");
                return;
            }

            string filePath = args[0];
            _consoleManager.ExecuteScript(filePath);
        }

        private void GetCommandDescription(ConsoleManager consoleManager, string commandName)
        {
            string description = consoleManager.GetCommandDescription(commandName);
            _consoleManager.WriteToConsole($"{commandName}: {description}", MessageType.Information);
        }
    }
}