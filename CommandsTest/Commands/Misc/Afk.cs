using System.Threading.Tasks;
using Commands.CommandsStuff;
using CommandsTest.Utils;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Misc
{
    public class Afk : Command
    {
        
        public Afk(DiscordClient client) : base(client) { }
        public override string GroupName => "Misc";

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Types = new []{typeof(string)},
                Key = "SetOrClear",
                Default = "set",
                Optional = true,
                OneOf = new []{"set", "clear"}
            },
            new()
            {
                Key = "UserToClear",
                Types = new []{typeof(DiscordUser)},
                Optional = true
            },
            new()
            {
                Key = "Message",
                Optional = true,
                Default = "Afk",
                Infinite = true,
                Types = new []{typeof(string)}
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var afkModule = ctx.Client.GetAfkModule();
            var setOrClear = ctx.GetArg<string>("SetOrClear");
            var message = ctx.GetArg<string>("Message");
            var userToRemove = ctx.GetArg<DiscordUser>("UserToClear");
            switch (setOrClear)
            {
                case "set":
                    afkModule.SetAfk(ctx.Author, message);
                    await ctx.ReplyAsync($"<@{ctx.Author.Id}> i set your afk: {message}");
                    break;
                case "clear":
                    afkModule.RemoveAfk(userToRemove);
                    await ctx.ReplyAsync($"removed afk for {userToRemove.Id}");
                    break;
            }
        }

        public override async Task Run(InteractionContext ctx)
        {
            var afkModule = ctx.Client.GetAfkModule();
            var setOrClear = ctx.GetArg<string>("SetOrClear");
            var message = ctx.GetArg<string>("Message");
            var userToRemove = ctx.GetArg<DiscordUser>("UserToClear");
            switch (setOrClear)
            {
                case "set":
                    afkModule.SetAfk(ctx.Author, message);
                    await ctx.ReplyAsync($"<@{ctx.Author.Id}> i set your afk: {message}");
                    break;
                case "clear":
                    afkModule.RemoveAfk(userToRemove);
                    await ctx.ReplyAsync($"removed afk for {userToRemove.Username}");
                    break;
            }
        }
    }
}