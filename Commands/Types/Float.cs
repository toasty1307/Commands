using System;
using DSharpPlus;

namespace Commands.Types
{
    [ArgumentType]
    public class FloatArgumentType : ArgumentType<float>
    {
        public override bool Validate(string argString)
        {
            try
            {
                _ = Convert.ToSingle(argString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override float Parse(string argString)
        {
            return Convert.ToSingle(argString);
        }

        public override bool IsEmpty(float arg) => false;

        public FloatArgumentType(DiscordClient client) : base(client)
        {
        }
    }
}