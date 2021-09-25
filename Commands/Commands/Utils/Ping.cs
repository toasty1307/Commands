using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace Commands.Commands.Utils
{
    public class Ping : Command
    {
        public Ping(DiscordClient client) : base(client)
        {
        }

        public override string GroupName => "Utils";
        public override string[] Aliases => new[] {"latency"};

        public override ThrottlingOptions ThrottlingOptions => new()
        {
            Usages = 2,
            Duration = 20
        };
        public override string Description => "Ping, thats about it, yeah";

        public override async Task Run(MessageContext ctx)
        {
            await ctx.ReplyAsync($"The bot's ping is {ctx.Client.Ping}ms");
        }

        public override async Task Run(InteractionContext ctx)
        {
            await ctx.FollowUpAsync($"The bot's ping is {ctx.Client.Ping}ms");
        }
    }
}