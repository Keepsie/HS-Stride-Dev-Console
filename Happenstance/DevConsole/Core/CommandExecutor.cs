// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Happenstance.SE.DevConsole
{
    public class CommandExecutor
    {
        private readonly List<ConsoleCommand> _registeredCommands;
        private readonly HashSet<string> _commandNames;
        private ConsoleManager _consoleManager;
        private readonly IHSLogger _logger;

        public CommandExecutor(IHSLogger logger)
        {
            _registeredCommands = new List<ConsoleCommand>();
            _commandNames = new HashSet<string>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void RegisterCommandHandler(ConsoleManager consoleManager, ConsoleCommand consoleCommand)
        {
            if(consoleManager == null) throw new ArgumentNullException(nameof(consoleManager));
            if(consoleCommand == null) throw new ArgumentNullException(nameof(consoleCommand));

            _registeredCommands.RemoveAll(c =>
                c.CommandName == consoleCommand.CommandName &&
                c.CommandFlag == consoleCommand.CommandFlag);

            _registeredCommands.Add(consoleCommand);
            _commandNames.Add(consoleCommand.CommandName);
            _consoleManager = consoleManager;
        }

        public void ExecuteCommand(DevCommand devCommand)
        {
            if (devCommand == null) throw new ArgumentNullException(nameof(devCommand));

            var matchedCommand = FindMatchingCommand(devCommand);

            if (matchedCommand != null)
            {
                ExecuteMatchedCommand(matchedCommand, devCommand);
            }
            else
            {
                HandleCommandNotFound(devCommand);
            }
        }

        private ConsoleCommand FindMatchingCommand(DevCommand devCommand)
        {
            if (devCommand.Flags.Count > 0)
            {
                return FindCommandWithFlag(devCommand);
            }

            return FindCommandWithoutFlag(devCommand);
        }

        private ConsoleCommand FindCommandWithFlag(DevCommand devCommand)
        {
            return _registeredCommands.FirstOrDefault(c =>
                 c.CommandName == devCommand.Name &&
                 c.CommandFlag == devCommand.Flags.Values.FirstOrDefault());
        }

        private ConsoleCommand FindCommandWithoutFlag(DevCommand devCommand)
        {
            return _registeredCommands.FirstOrDefault(c =>
                c.CommandName == devCommand.Name &&
                string.IsNullOrEmpty(c.CommandFlag));
        }

        private void ExecuteMatchedCommand(ConsoleCommand matchedCommand, DevCommand devCommand)
        {
            try
            {
                if (devCommand.Flags.Count > 0)
                {
                    foreach (var flag in devCommand.Flags)
                    {
                        var index = flag.Key;
                        string[] args = CommandParser.GetArgsForFlag(index, devCommand.Arguments);
                        matchedCommand.CommandMethod?.Invoke(args);
                    }
                }
                else
                {
                    string[] args = CommandParser.GetArgsForFlag(0, devCommand.Arguments);
                    matchedCommand.CommandMethod?.Invoke(args);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing command '{devCommand.Name}': {ex.Message}");
            }
        }

        private void HandleCommandNotFound(DevCommand devCommand)
        {
            if (devCommand.Flags.Count > 0)
            {
                _consoleManager.WriteToConsole($"Command '{devCommand.Name}' does not support the provided flag.",MessageType.Warning);
            }
            else
            {
                _consoleManager.WriteToConsole($"Command '{devCommand.Name}' not found.", MessageType.Error);
            }
        }

        public string[] GetCommandNames()
        {
            return _commandNames.ToArray();
        }

        public string GetCommandDescription(string name)
        {
            var command = _registeredCommands.FirstOrDefault(c =>
                c.CommandName == name &&
                string.IsNullOrEmpty(c.CommandFlag));

            return command?.CommandDescription ?? "Command not found";
        }
    }
}
