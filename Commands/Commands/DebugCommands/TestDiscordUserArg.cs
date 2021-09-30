#if DEBUG
using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.Commands.DebugCommands
{
    public class TestDiscordUserArg : Command
    {
        public override string GroupName => "Debug";
        public override string Description => "test";

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "User",
                Description = "yes"
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var user = ctx.GetArg<DiscordUser>("User");
            await ctx.ReplyAsync(user?.Id.ToString() ?? "null");
        }

        public override async Task Run(InteractionContext ctx)
        {
            var user = ctx.GetArg<DiscordUser>("User");
            await ctx.ReplyAsync(user?.Id.ToString() ?? "null");
        }

        public TestDiscordUserArg(DiscordClient client) : base(client)
        {
        }
    }
}
#endif