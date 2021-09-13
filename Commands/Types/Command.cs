using Commands.CommandsStuff;
using DSharpPlus;

namespace Commands.Types
{
    [ArgumentType]
    public class CommandArgumentType : ArgumentType<Command>
    {
        public override bool Validate(string argString) =>
            Extension.Registry.FindCommands(argString).Length != 0;

        public override Command Parse(string argString) =>
            Extension.Registry.FindCommands(argString)[0];

        public override bool IsEmpty(Command arg) => 
            false;

        public CommandArgumentType(DiscordClient client) : base(client)
        {
        }
    }
}