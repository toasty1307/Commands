using Commands.CommandsStuff;

namespace Commands.Types
{
    [ArgumentType]
    public class CommandArgumentType : ArgumentType<Command>
    {
        private static CommandRegistry Registry => Extension.Registry;

        public override bool Validate(string argString) =>
            Registry.FindCommands(argString).Length != 0;

        public override Command Parse(string argString) =>
            Registry.FindCommands(argString)[0];

        public override bool IsEmpty(Command arg) => 
            false;
    }
}