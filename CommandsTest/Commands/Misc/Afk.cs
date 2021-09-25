using System.Threading.Tasks;
using Commands.CommandsStuff;
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
            await ctx.ReplyAsync(ctx.GetArg<string>("SetOrClear") ?? "null");
            await ctx.ReplyAsync(ctx.GetArg<DiscordUser>("UserToClear")?.Username ?? "null");
            await ctx.ReplyAsync(ctx.GetArg<string>("Message") ?? "null");
        }

        public override async Task Run(InteractionContext ctx)
        {
            await ctx.FollowUpAsync(ctx.GetArg<string>("SetOrClear") ?? "null");
            await ctx.FollowUpAsync(ctx.GetArg<DiscordUser>("UserToClear")?.Username ?? "null");
            await ctx.FollowUpAsync(ctx.GetArg<string>("Message") ?? "null");
        }
    }
}