using DSharpPlus;

namespace Commands.Types
{
    [ArgumentType]
    public class StringArgumentType : ArgumentType<string>
    {
        public override bool Validate(string argString) =>
            !string.IsNullOrEmpty(argString);

        public override string Parse(string argString) =>
            argString;

        public override bool IsEmpty(string arg) =>
            string.IsNullOrEmpty(arg);

        public StringArgumentType(DiscordClient client) : base(client) { }
    }
}