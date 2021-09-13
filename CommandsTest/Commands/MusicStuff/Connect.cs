using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;

namespace CommandsTest.Commands.MusicStuff
{
    public class Connect : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "join vc";
        public static LavalinkNodeConnection LavaLink { get; set; }
        public static LavalinkGuildConnection LavaLinkVoice { get; set; }
        public static DiscordChannel ContextChannel { get; set; }

        public override async Task Run(CommandContext ctx)
        {
            if (LavaLink is not null) return;

            var lava = ctx.Client.GetLavalink();
            if (lava is null)
            {
                await ctx.ReplyAsync("Lavalink not enabled");
                return;
            }

            LavaLink = await lava.ConnectAsync(new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint("127.0.0.1", 2333),
                SocketEndpoint = new ConnectionEndpoint("127.0.0.1", 2333),
                Password = "youshallnotpass"
            });
            LavaLink.Disconnected += LavaLinkDisconnected;
            await ctx.ReplyAsync("Connected to lavalink node.");
        }

        private Task LavaLinkDisconnected(LavalinkNodeConnection ll, NodeDisconnectedEventArgs e)
        {
            if (!e.IsCleanClose) return Task.CompletedTask;
            
            LavaLink = null;
            LavaLinkVoice = null;

            return Task.CompletedTask;
        }
        
        public override async Task Run(InteractionContext ctx)
        {
            if (LavaLink != null) return;

            var lava = ctx.Client.GetLavalink();
            if (lava is null)
            {
                await ctx.FollowUpAsync("LavaLink not enabled");
                return;
            }

            LavaLink = await lava.ConnectAsync(new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint("127.0.0.1", 2333),
                SocketEndpoint = new ConnectionEndpoint("127.0.0.1", 2333),
                Password = "youshallnotpass"
            });
            
            LavaLink.Disconnected += LavaLinkDisconnected;
            await ctx.FollowUpAsync("yes");
        }

        public Connect(DiscordClient client) : base(client)
        {
        }
    }
    
    public class Disconnect : Command
    {
        public override string GroupName => "MusicStuff";
        public override string Description => "a";
        public override async Task Run(CommandContext ctx)
        {
            if (Connect.LavaLink is null)
            {
                await ctx.ReplyAsync("what if no");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            if (lava is null)
            {
                await ctx.ReplyAsync("no what you trying to do bruh");
                return;
            }
            await Connect.LavaLink.StopAsync();
            Connect.LavaLink = null;
            await ctx.ReplyAsync("disconnected");
        }

        public override async Task Run(InteractionContext ctx)
        {
            if (Connect.LavaLink is null) return;

            var lava = ctx.Client.GetLavalink();
            if (lava is null)
            {
                await ctx.FollowUpAsync("no what you trying to do bruh");
                return;
            }
            await Connect.LavaLink.StopAsync();
            Connect.LavaLink = null;
            await ctx.FollowUpAsync("disconnected ");
        }

        public Disconnect(DiscordClient client) : base(client)
        {
        }
    }
}