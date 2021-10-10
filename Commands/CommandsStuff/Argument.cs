using System;

namespace Commands.CommandsStuff
{
    public class Argument
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public Type[] Types { get; set; } 
        public bool Optional { get; set; }
        public string Default { get; set; }
        public string[] OneOf { get; set; }
        public bool Infinite { get; set; }

        public void Deconstruct(out string key, out string description, out Type[] types, out bool optional,
            out string @default, out string[] oneOf, out bool infinite)
        {
            key = Key;
            description = Description;
            types = Types;
            optional = Optional;
            @default = Default;
            oneOf = OneOf;
            infinite = Infinite;
        }
    }
}