// HS Dev Console Core (c) 2025 Happenstance Games LLC - MIT License

using System.Collections.Generic;


namespace Happenstance.SE.DevConsole
{
    public class DevCommand
    {
        public string Name { get; set; }
        public Dictionary<int, string> Flags { get; set; }
        public List<string> Arguments { get; set; }

        public DevCommand(string name, Dictionary<int, string> flags, List<string> arguments)
        {
            Name = name;
            Flags = flags;
            Arguments = arguments;
        }
    }
}
