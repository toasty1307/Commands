using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace CommandsTest.Commands.MusicStuff
{
    public class Join : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "aadw";

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<DiscordVoiceChannelArgumentType>
            {
                Key = "Channel",
                Optional = true
            }
        };

        public override async Task Run(CommandContext ctx)
        {
            if (Connect.LavaLink is null)
            {
                await ctx.ReplyAsync("lavalink no");
                return;
            }

            var channel = ctx.GetArg<DiscordChannel>("Channel") ??
                          ((DiscordMember) ctx.Author).VoiceState?.Channel;
            if (channel is null)
            {
                await ctx.ReplyAsync("no");
                return;
            }

            try
            {
                _ = Task.Run(async () =>
                {
                    Connect.LavaLinkVoice = await Connect.LavaLink.ConnectAsync(channel);
                    Connect.LavaLinkVoice.PlaybackFinished += LavaLinkVoicePlaybackFinished;
                    Connect.LavaLinkVoice.DiscordWebSocketClosed += (_, _) => ctx.ReplyAsync("discord websocket closed ig");
                    await ctx.ReplyAsync("pog");
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private async Task LavaLinkVoicePlaybackFinished(LavalinkGuildConnection ll, TrackFinishEventArgs e)
        {
            if (Connect.ContextChannel is null) return;

            await Connect.ContextChannel.SendMessageAsync($"what next");
        }

        public override async Task Run(InteractionContext ctx)
        {
            if (Connect.LavaLink is null)
            {
                await ctx.FollowUpAsync("not connected :|");
                return;
            }

            var channel = ctx.GetArg<DiscordChannel>("Channel") ??
                          ((DiscordMember) ctx.Author).VoiceState.Channel;
            if (channel is null)
            {
                await ctx.FollowUpAsync("no");
                return;
            }

            try
            {
                _ = Task.Run(async () =>
                {
                    Connect.LavaLinkVoice = await Connect.LavaLink.ConnectAsync(channel);
                    Connect.LavaLinkVoice.PlaybackFinished += LavaLinkVoicePlaybackFinished;
                    Connect.LavaLinkVoice.DiscordWebSocketClosed += (_, _) => ctx.FollowUpAsync("discord websocket closed ig");
                    await ctx.FollowUpAsync("pog");
                });
            }
            catch (Exception e)
            {
                ctx.Client.Logger.Error(e);
                throw;
            }
        }

        public Join(DiscordClient client) : base(client)
        {
        }
    }
    
    public class Leave : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "aa";
        public override async Task Run(CommandContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.DisconnectAsync();
            Connect.LavaLinkVoice = null;
            await Connect.ContextChannel.SendMessageAsync("ok");
        }

        public override async Task Run(InteractionContext ctx)
        {
            if (Connect.LavaLinkVoice is null) return;
            await Connect.LavaLinkVoice.DisconnectAsync();
            Connect.LavaLinkVoice = null;
            await ctx.FollowUpAsync("ok");
        }

        public Leave(DiscordClient client) : base(client)
        {
        }
    }
}