// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Happenstance.SE.DevConsole
{
    public class AutoComplete
    {
        private List<string> _commands;
        private readonly IHSLogger _logger;

        public AutoComplete(IEnumerable<string> commands, IHSLogger logger)
        {
            _commands = new List<string>(commands);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void UpdateCommands(IEnumerable<string> commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            _commands = new List<string>(commands);
            _logger.Debug("Auto-complete command list updated");
        }

        public IReadOnlyList<string> GetSuggestions(string partial)
        {
            if (string.IsNullOrWhiteSpace(partial)) return Array.Empty<string>();

            return _commands
                .Where(c => c.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();
        }
    }
}
