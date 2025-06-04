// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Interfaces;
using System.Collections.Generic;

namespace Happenstance.SE.DevConsole
{
    public class CommandHistory
    {
        private readonly List<string> _history;
        private int _currentIndex;
        private readonly IHSLogger _logger;
        private readonly int _maxHistorySize;

        public CommandHistory(IHSLogger logger, int maxHistorySize = 100)
        {
            _history = new List<string>();
            _currentIndex = -1;
            _logger = logger;
            _maxHistorySize = maxHistorySize;
        }

        public void AddCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            // Remove duplicate if exists
            _history.Remove(command);

            _history.Add(command);

            // Trim history if it exceeds max size
            while (_history.Count > _maxHistorySize)
            {
                _history.RemoveAt(0);
            }

            _currentIndex = _history.Count;

            _logger.Debug($"Command added to history: {command}");
        }

        public string GetPreviousCommand()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                return _history[_currentIndex];
            }
            return string.Empty;
        }

        public string GetNextCommand()
        {
            if (_currentIndex < _history.Count - 1)
            {
                _currentIndex++;
                return _history[_currentIndex];
            }
            return string.Empty;
        }

        public void Clear()
        {
            _history.Clear();
            _currentIndex = -1;
            _logger.Info("Command history cleared");
        }

        public IReadOnlyList<string> GetHistory()
        {
            return _history.AsReadOnly();
        }
    }
}
