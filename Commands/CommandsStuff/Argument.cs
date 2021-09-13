using Commands.Types;

namespace Commands.CommandsStuff
{
    public class Argument
    {
        public string Key { get; init; }
        public string Description { get; init; }
        public bool Optional { get; init; }
        public string Default { get; init; }
        public string[] OneOf { get; init; }
        public bool Infinite { get; init; } 
    }

    public class Argument<T> : Argument where T : ArgumentType { }
}