using System;
using System.IO;
using System.Threading.Tasks;
using Commands;
using Commands.CommandsStuff;
using Commands.Providers;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CommandsTest
{
    public class CommandsTestBot
    {
        private DiscordClient Client { get; }
        private DiscordClient Client2 { get; }

        public CommandsTestBot()
        {
            var config = GetConfig("Config.json");
            var discordConfiguration = new DiscordConfiguration
            {
                Token = config.Token,
                LoggerFactory = new LoggerFactory(),
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Trace
            };
            var commandsConfig = new CommandsConfig
            {
                Prefix = config.Prefix,
                Owners = new ulong[]{ 742976057761726514 },
                Invite = "discord.gg/TCf7QexN5e"
            };
            Client = new DiscordClient(discordConfiguration);
            var commandsExtension = Client.UseCommands(new CommandsExtension(commandsConfig));
            commandsExtension.Registry.RegisterDefaults();
            commandsExtension.Registry.RegisterGroups(new Group[]
            {
                new()
                {
                    Name = "Misc",
                    Description = "misc stuff, nothing much"
                },
                new()
                {
                    Name = "Admin",
                    Description = "Owner only commands idk"
                },
                new()
                {
                    Name = "MusicStuff",
                    Description = "the title says it all"
                }
            });
            commandsExtension.Registry.RegisterCommands(GetType().Assembly);
            commandsExtension.SetProvider(new SqliteProvider(Client, "Data Source=Database.db"));
            Client.GuildMemberAdded += CheckForBadNick;
            Client.GuildMemberUpdated += CheckForBadNick;
            Client.UseLavalink();
            
            
            /*
            var config2 = GetConfig("Config.json");
            var discordConfiguration2 = new DiscordConfiguration
            {
                Token = "ODgzNzQ0NzU1MjYyMDUwMzg1.YTOZcA.KRtZ_csJOK4DUQDVfZvK_VEiJ7I",
                LoggerFactory = new LoggerFactory(),
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Trace
            };
            var commandsConfig2 = new CommandsConfig
            {
                Prefix = config2.Prefix,
                Owners = new ulong[]{ 742976057761726514 },
                Invite = "discord.gg/TCf7QexN5e"
            };
            Client2 = new DiscordClient(discordConfiguration2);
            var commandsExtension2 = Client2.UseCommands(new CommandsExtension(commandsConfig2));
            commandsExtension2.Registry.RegisterDefaults();
            commandsExtension2.Registry.RegisterGroups(new Group[]
            {
                new()
                {
                    Name = "Misc",
                    Description = "misc stuff, nothing much"
                },
                new()
                {
                    Name = "Admin",
                    Description = "Owner only commands idk"
                },
                new()
                {
                    Name = "MusicStuff",
                    Description = "the title says it all"
                }
            });
            commandsExtension2.Registry.RegisterCommands(GetType().Assembly);
            commandsExtension2.SetProvider(new SqliteProvider(Client2, "Data Source=Database.db"));
            Client2.GuildMemberAdded += CheckForBadNick;
            Client2.GuildMemberUpdated += CheckForBadNick;
            Client2.UseLavalink();
        */
        }

        private async Task CheckForBadNick(DiscordClient client, GuildMemberUpdateEventArgs args)
        {
            try
            {
                var nick = args.Member.Nickname ?? args.Member.Username;
                if (nick is null or "💩") return;
                if (!(nick[0] >= 48 && nick[0] <= 57 || nick[0] >= 65 && nick[0] <= 90 || nick[0] >= 97 && nick[0] <= 122))
                    await args.Member.ModifyAsync(x => x.Nickname = "💩");
            }
            catch (Exception e)
            {
                client.Logger.Error(e);
            }
        }
        private async Task CheckForBadNick(DiscordClient client, GuildMemberAddEventArgs args)
        {
            try
            {
                var nick = args.Member.Nickname ?? args.Member.Username;
                if (nick is null or "💩") return;
                if (!(nick[0] >= 48 && nick[0] <= 57 || nick[0] >= 65 && nick[0] <= 90 || nick[0] >= 97 && nick[0] <= 122))
                    await args.Member.ModifyAsync(x => x.Nickname = "💩");
            }
            catch (Exception e)
            {
                client.Logger.Error(e);
            }
        }

        public async Task Run()
        {
            await Client.ConnectAsync(new DiscordActivity{ActivityType = ActivityType.Playing, Name = "Milk"}, UserStatus.Idle, DateTimeOffset.Now);
            /*
            await Client2.ConnectAsync(new DiscordActivity{ActivityType = ActivityType.Playing, Name = "Milk"}, UserStatus.Idle, DateTimeOffset.Now);
        */
        }

        public Config GetConfig(string fileName)
        {
            var directory = Directory.GetCurrentDirectory();
            var fullPath = Path.Combine(directory, fileName);
            var reader = new StreamReader(fullPath);
            var jsonString = reader.ReadToEnd();
            var config = JsonConvert.DeserializeObject<Config>(jsonString);
            return config;
        }
    }
}