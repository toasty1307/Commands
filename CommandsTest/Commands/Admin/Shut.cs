using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace CommandsTest.Commands.Admin
{
    public class Shut : Command
    {
        public override string GroupName => "Admin";
        public override string Description => "close bot ig";
        public override bool OwnerOnly => true;

        public override async Task Run(MessageContext ctx)
        {
            await ctx.ReplyAsync("cya");
            Environment.Exit(0);
        }

        public override async Task Run(InteractionContext ctx)
        {
            await ctx.ReplyAsync("cya");
            Environment.Exit(0);
        }

        public Shut(DiscordClient client) : base(client)
        {
        }
    }
}