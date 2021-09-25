using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace CommandsTest.Commands.Misc
{
    public class SetUp : Command
    {
        public override string GroupName => "Misc"; 
        public override string Description => "setup stuff idk";
        public override bool RegisterSlashCommand => false;
        public override bool GuildOnly => true;

        public override async Task Run(MessageContext ctx)
        {
            var msg = await ctx.ReplyAsync("i'll try");
            await ctx.Extension.Registry.RegisterSlashCommands(ctx.Extension.Registry.Commands.ToArray(), ctx.Guild);
            await msg.ModifyAsync("done ig");
        }

        public override Task Run(InteractionContext _) => Task.CompletedTask;

        public SetUp(DiscordClient client) : base(client)
        {
        }
    }
}