// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using Happenstance.SE.Logger.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Happenstance.SE.DevConsole
{
    public class CommandParser
    {
        private readonly List<string> _tempArgs;
        private readonly IHSLogger _logger;

        public CommandParser(IHSLogger logger)
        {
            _tempArgs = new List<string>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DevCommand Parse(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    _logger.Warning("Empty command input");
                    return null;
                }

                // Use regex to split the input, keeping quoted strings together
                var parts = Regex.Matches(input, @"[^\s""]+|""([^""]*)""")
                    .Cast<Match>()
                    .Select(m => m.Value.Trim('"'))
                    .ToList();

                _tempArgs.Clear();
                _tempArgs.AddRange(parts);

                if (_tempArgs.Count <= 0) return null;

                // First argument is the command name
                var commandName = _tempArgs[0].ToLower();
                _tempArgs.RemoveAt(0);

                // Separate flags from arguments
                var flags = new Dictionary<int, string>();
                var arguments = new List<string>();
                var flagIndex = 0;
                var usingFlags = false;

                foreach (var arg in _tempArgs)
                {
                    if (arg.StartsWith("-"))
                    {
                        flagIndex++;
                        usingFlags = true;
                        flags.Add(flagIndex, arg);
                    }
                    else
                    {
                        if (usingFlags)
                        {
                            arguments.Add($"{flagIndex} {arg}");
                        }
                        else
                        {
                            arguments.Add(arg);
                        }
                    }
                }

                return new DevCommand(commandName, flags, arguments);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error parsing command: {ex.Message}");
                return null;
            }
        }

        public static string[] GetArgsForFlag(int flagIndex, List<string> arguments)
        {
            if (arguments == null) return Array.Empty<string>();

            var argsForFlag = new List<string>();

            if (flagIndex <= 0)
            {
                return arguments.ToArray();
            }

            foreach (var argument in arguments)
            {
                var splitArg = argument.Split(' ');
                if (splitArg.Length > 1 && splitArg[0] == flagIndex.ToString())
                {
                    argsForFlag.Add(splitArg[1]);
                }
            }

            return argsForFlag.ToArray();
        }
    }
}
