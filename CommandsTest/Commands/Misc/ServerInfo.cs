using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Misc
{
    public class ServerInfo : Command
    {
        public ServerInfo(DiscordClient client) : base(client) { }

        public override string GroupName => "Misc";
        public override Task Run(MessageContext ctx)
        {
            return ctx.ReplyAsync(new DiscordEmbedBuilder
            {
                Title = $"Info about {ctx.Guild.Name}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = ctx.Guild.IconUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Requested by {ctx.Author.Username}#{ctx.Author.Discriminator}",
                    IconUrl = ctx.Author.AvatarUrl
                }
            }
                .AddField("Owner:", $"<@{ctx.Guild.OwnerId}>")
                .AddField("Members:", ctx.Guild.MemberCount.ToString()));
        }

        public override Task Run(InteractionContext ctx)
        {
            return ctx.ReplyAsync(new DiscordEmbedBuilder
                {
                    Title = $"Info about {ctx.Guild.Name}",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = ctx.Guild.IconUrl
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Requested by {ctx.Author.Username}#{ctx.Author.Discriminator}",
                        IconUrl = ctx.Author.AvatarUrl
                    }
                }
                .AddField("Owner:", $"<@{ctx.Guild.OwnerId}>")
                .AddField("Members:", ctx.Guild.MemberCount.ToString()));
        }
    }
}