using System;
using DSharpPlus;

namespace Commands.Types
{
    [ArgumentType]
    public class IntArgumentType : ArgumentType<int>
    {
        public override bool Validate(string argString)
        {
            try
            {
                _ = Convert.ToInt32(argString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override int Parse(string argString)
        {
            return Convert.ToInt32(argString);
        }

        public override bool IsEmpty(int arg) => false;

        public IntArgumentType(DiscordClient client) : base(client)
        {
        }
    }
}