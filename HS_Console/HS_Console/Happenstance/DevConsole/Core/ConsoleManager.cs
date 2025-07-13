// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Core;
using Stride.Engine;
using Stride.Input;
using System;
using System.Collections.Generic;

namespace Happenstance.SE.DevConsole
{
    public class ConsoleManager : HSSyncScript
    {
        private Entity _consoleUIEntity;
        private bool _isConsoleActive;


        private CommandParser _commandParser;
        private CommandExecutor _commandExecutor;
        private CommandHistory _commandHistory;
        private AutoComplete _autoComplete;
        private ScriptExecutor _scriptExecutor;
        private PredefinedCommands _predefinedCommands;

        public string ConsoleVersion { get; } = "1.0.0";
        public bool IsDebugBuild { get; private set; }

        public event Action<string, MessageType> OnConsoleOutput;
        public event Action OnClearConsole;

        protected override void OnAwake()
        {
            // Only enable in debug builds or editor
#if DEBUG
            IsDebugBuild = true;
#endif

            if (!IsDebugBuild)
            {
                Logger.Info("Console disabled in release build");
                Entity.EnableAll(false, true);
                return;
            }
        }

        protected override void OnStart()
        {
            _consoleUIEntity = Entity.FindChildByName_HS("DevConsoleUI");

            if (_consoleUIEntity == null)
            {
                Logger.Warning("Console UI entity not found. UI features will not be available.");
            }

            InitializeComponents();
            DisplayConsoleInfo();
        }

        protected override void OnUpdate()
        {
            if (Input.IsKeyPressed(Keys.OemTilde)) // Backtick key
            {
                ToggleConsole();
            }
        }

        private void InitializeComponents()
        {
            _commandExecutor = new CommandExecutor(Logger);
            _commandParser = new CommandParser(Logger);
            _commandHistory = new CommandHistory(Logger);
            _scriptExecutor = new ScriptExecutor(Logger, this);
            _predefinedCommands = new PredefinedCommands(Logger, this);
            _autoComplete = new AutoComplete(_commandExecutor.GetCommandNames(), Logger);
            _predefinedCommands.RegisterAll(this);
        }

        // Output handling
        public void WriteToConsole(string message, MessageType messageType = MessageType.Normal)
        {
            OnConsoleOutput?.Invoke(message, messageType);
        }

        public void ExecuteCommand(string input)
        {
            try
            {
                var devCommand = _commandParser.Parse(input);
                if (devCommand != null)
                {
                    _commandExecutor.ExecuteCommand(devCommand);
                    _commandHistory.AddCommand(input);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error executing command: {ex.Message}");
            }
        }

        public void ExecuteScript(string filePath)
        {
            _scriptExecutor.ExecuteScript(filePath);
        }

        public void RegisterConsoleCommand(ConsoleCommand consoleCommand)
        {
            _commandExecutor.RegisterCommandHandler(this,consoleCommand);
            _autoComplete.UpdateCommands(_commandExecutor.GetCommandNames());
        }

        public void DisplayConsoleInfo()
        {
            WriteToConsole($"HS - Dev Console v{ConsoleVersion}", MessageType.Success);
            WriteToConsole("Type 'help' for a list of commands.", MessageType.Normal);
        }

        public void ClearConsole()
        {
            OnClearConsole?.Invoke();
        }

        public void ToggleConsole()
        {
            bool newState = !_isConsoleActive;

            if (_consoleUIEntity != null)
            {
                _consoleUIEntity.EnableAll(newState, true);
            }

            _isConsoleActive = newState;

            Logger.Info($"Console {(newState ? "enabled" : "disabled")}");
        }

        // Command history navigation
        public string GetPreviousCommand() => _commandHistory.GetPreviousCommand();
        public string GetNextCommand() => _commandHistory.GetNextCommand();

        // Auto-completion
        public IReadOnlyList<string> GetSuggestions(string partial) => _autoComplete.GetSuggestions(partial);

        // Help and information
        public string[] GetCommandNames() => _commandExecutor.GetCommandNames();
        public string GetCommandDescription(string commandName) => _commandExecutor.GetCommandDescription(commandName);

    }
}
