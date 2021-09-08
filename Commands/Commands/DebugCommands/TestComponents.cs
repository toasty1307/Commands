using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using DSharpPlus.Entities;

namespace Commands.Commands.DebugCommands
{
    public class TestComponents : Command
    {
        public override string GroupName => "Debug";
        public override string Description => "hehe";

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<StringArgumentType>
            {
                Key = "Component",
                OneOf = new []{"Button", "Select", "LinkButton"}
            }
        };

        public override Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            throw new NotImplementedException("no");
        }

        public override Task Run(DiscordInteraction interaction, ArgumentCollector collector)
        {
            throw new System.NotImplementedException();
        }
    }
}