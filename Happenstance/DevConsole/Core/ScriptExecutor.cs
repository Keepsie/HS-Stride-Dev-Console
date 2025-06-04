// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Happenstance.SE.DevConsole
{
    public class ScriptExecutor
    {
        private readonly IHSLogger _logger;
        private readonly ConsoleManager _consoleManager;
        private bool _isExecuting;
        

        public ScriptExecutor(IHSLogger logger, ConsoleManager consoleManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consoleManager = consoleManager ?? throw new ArgumentNullException(nameof(consoleManager));
        }

        public async void ExecuteScript(string filePath)
        {
            if (_isExecuting)
            {
                _consoleManager.WriteToConsole("Another script is currently executing");
                return;
            }

            if (!File.Exists(filePath))
            {
                _consoleManager.WriteToConsole($"Script file not found: {filePath}", MessageType.Error);
                return;
            }

            try
            {
                _isExecuting = true;
                string[] lines = await File.ReadAllLinesAsync(filePath);
                await ExecuteScriptLinesAsync(lines);
                _consoleManager.WriteToConsole($"Script execution completed: {filePath}", MessageType.Success);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing script: {ex.Message}");
                _consoleManager.WriteToConsole($"Error executing script: {ex.Message}", MessageType.Error);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        private async Task ExecuteScriptLinesAsync(string[] lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                {
                    continue;
                }

                // Support both "pause" (Unity-like) and "wait" commands
                if (line.TrimStart().StartsWith("pause("))
                {
                    await HandlePauseCommand(line);
                }
                else
                {
                    _consoleManager.WriteToConsole($"> {line}", MessageType.Information);
                    _consoleManager.ExecuteCommand(line);
                    await Task.Delay(50); // Small delay between commands
                }
            }
        }

        private async Task HandlePauseCommand(string line)
        {
            try
            {
                float pauseTime = ParsePauseTime(line);
                _consoleManager.WriteToConsole($"Pausing for {pauseTime} seconds...", MessageType.Information);
                await Task.Delay(TimeSpan.FromSeconds(pauseTime));
            }
            catch (Exception ex)
            {
                _consoleManager.WriteToConsole($"Error parsing pause time: {ex.Message}. Using default 1 second delay.", 
                    MessageType.Warning);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private float ParsePauseTime(string line)
        {
            int startIndex = line.IndexOf("(") + 1;
            int endIndex = line.IndexOf(")");
            string timeString = line.Substring(startIndex, endIndex - startIndex);
            return float.Parse(timeString);
        }
    }
}