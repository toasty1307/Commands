using System;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace CommandsTest.Commands.MusicStuff
{
    public class Play : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "adwd";

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "Uri",
                Types = new []{typeof(string)}
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var uri = new Uri(ctx.GetArg<string>("Uri"));
            if (Connect.LavaLinkVoice is null) return;

            Connect.ContextChannel = ctx.Channel;

            var trackLoad = await Connect.LavaLink.Rest.GetTracksAsync(uri);
            var track = trackLoad.Tracks.First();
            await Connect.LavaLinkVoice.PlayAsync(track);

            await ctx.ReplyAsync(
                $"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.");
        }

        public override async Task Run(InteractionContext ctx)
        {
            var uri = new Uri(ctx.GetArg<string>("Uri"));
            if (Connect.LavaLinkVoice is null) return;

            Connect.ContextChannel = ctx.Channel;

            var trackLoad = await Connect.LavaLink.Rest.GetTracksAsync(uri);
            var track = trackLoad.Tracks.First();
            await Connect.LavaLinkVoice.PlayAsync(track);

            await ctx.FollowUpAsync(
                $"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.");
        }

        public Play(DiscordClient client) : base(client)
        {
        }
    }

    public class Pause : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "adwd";

        public override async Task Run(MessageContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.PauseAsync();
            await ctx.ReplyAsync("ok");
        }

        public override async Task Run(InteractionContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.PauseAsync();
            await ctx.FollowUpAsync("ok");
        }

        public Pause(DiscordClient client) : base(client)
        {
        }
    }

    public class Resume : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "adwd";

        public override async Task Run(MessageContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.ResumeAsync();
            await ctx.ReplyAsync("ok");
        }

        public override async Task Run(InteractionContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.ResumeAsync();
            await ctx.FollowUpAsync("ok");
        }

        public Resume(DiscordClient client) : base(client)
        {
        }
    }

    public class Stop : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "adwd";

        public override async Task Run(MessageContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.StopAsync();
            await ctx.ReplyAsync("ok");
        }

        public override async Task Run(InteractionContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.StopAsync();
            await ctx.FollowUpAsync("ok");
        }

        public Stop(DiscordClient client) : base(client)
        {
        }
    }
}