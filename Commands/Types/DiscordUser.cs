using System;
using System.Linq;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.Types
{
    [ArgumentType]
    public class DiscordUserArgumentType : ArgumentType<DiscordUser>
    {
        public Regex UserRegex = new(@"\d+");
        public override bool Validate(string argString)
        {
            try
            {
                var match = UserRegex.Match(argString);
                try
                {
                    _ = Client.Guilds.Values.SelectMany(x => x.Members.Values)
                        .First(x => x.Id == Convert.ToUInt64(match.Value));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        public override DiscordUser Parse(string argString)
        {
            var guilds = Client.Guilds.Values;
            var channels = guilds.SelectMany(x => x.Members.Values);
            var givenIdString = UserRegex.Match(argString).Value;
            var givenId = Convert.ToUInt64(givenIdString);
            try
            {
                var value = channels.First(x => x.Id == givenId);
                return value;
            }
            catch
            {
                return null;
            }
        }

        public override bool IsEmpty(DiscordUser arg) => arg is null;

        public DiscordUserArgumentType(DiscordClient client) : base(client)
        {
        }
    }
}