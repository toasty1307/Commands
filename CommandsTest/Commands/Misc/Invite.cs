using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace CommandsTest.Commands.Misc
{
    public class Invite : Command
    {
        public Invite(DiscordClient client) : base(client) { }
        public override string GroupName => "Misc";
        public override async Task Run(MessageContext ctx)
        {
            await ctx.ReplyAsync(
                "https://discord.com/api/oauth2/authorize?client_id=874898292771807282&permissions=8&scope=bot%20applications.commands");
        }

        public override async Task Run(InteractionContext ctx)
        {
            await ctx.FollowUpAsync(
                "https://discord.com/api/oauth2/authorize?client_id=874898292771807282&permissions=8&scope=bot%20applications.commands");
        }
    }
}