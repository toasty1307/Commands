using Commands.Types;

namespace Commands.CommandsStuff
{
    public class Argument
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public bool Optional { get; set; }
        public string Default { get; set; }
        public string[] OneOf { get; set; }
        public bool Infinite { get; set; }
    }

    public class Argument<T> : Argument where T : ArgumentType
    {
    }
}