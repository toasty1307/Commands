using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace Commands.Commands.Utils
{
    public class UnknownCommand : Command
    {
        public override string GroupName => "Utils";
        public override bool Unknown => true;
        public override bool Hidden => true;
        public override string Description => "Command for unknown idk";
        public override async Task Run(MessageContext ctx)
        {
            var privateChannel = ctx.Guild is null;
            var extension = ctx.Extension;
            var prefix = (extension.Provider.Get(ctx.Guild)).Prefix;
            var command = privateChannel ? "`help`" : $"`{prefix}help` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator} help`";
            var replyString = $"Unknown Command. Use {command} to get the command list";
            await ctx.ReplyAsync(replyString);
        }

        public override Task Run(InteractionContext ctx) => Task.CompletedTask;

        public UnknownCommand(DiscordClient client) : base(client)
        {
        }
    }
}