using System;
using System.IO;
using System.Threading.Tasks;
using Commands;
using Commands.CommandsStuff;
using Commands.Utils;
using CommandsTest.Commands.Misc;
using CommandsTest.Data;
using CommandsTest.Extensions;
using CommandsTest.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LoggerFactory = Commands.Utils.LoggerFactory;

namespace CommandsTest
{
    public class CommandsTestBot
    {
        private readonly DiscordShardedClient _client;
        private readonly Config _config;
        
        public CommandsTestBot()
        {
            _config = GetConfig("Config.json");
            var discordConfiguration = new DiscordConfiguration
            {
                Token = _config.Token,
                LoggerFactory = new LoggerFactory(),
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Trace
            };
            _client = new DiscordShardedClient(discordConfiguration);
            _client.GuildMemberAdded += (_, args) => CheckForBadNick(args.Member, args.Member.Nickname ?? args.Member.Username);
            _client.GuildMemberUpdated += (_, args) => CheckForBadNick(args.Member, args.NicknameAfter);
            _client.GuildDownloadCompleted += (client, _) =>
            {
                client.Logger.LogInformation($"Client ready, logged in as ({client.CurrentUser.Username}#{client.CurrentUser.Discriminator}), you can run your command now senpai");
                return Task.CompletedTask;
            };
            _client.UseLavalinkAsync().GetAwaiter().GetResult();
            _client.Logger.LogInformation("Initializing blacklist module");
            _client.Logger.LogInformation("Initialized blacklist module");
            Tag.RegisterTags(GetType().Assembly);
        }

        private async Task CheckForBadNick(DiscordMember member, string nick)
        {
            try
            {
                if (nick is null or "💩") return;
                if (!(nick[0] >= 48 && nick[0] <= 57 || nick[0] >= 65 && nick[0] <= 90 || nick[0] >= 97 && nick[0] <= 122))
                    await member.ModifyAsync(x => x.Nickname = "💩");
            }
            catch (Exception e)
            {
                _client.Logger.Error(e);
            }
        }

        public async Task Run()
        {
            await _client.StartAsync();
            var commandsConfig = new CommandsConfig
            {
                Prefix = _config.Prefix,
                Owners = new ulong[]{ 742976057761726514 },
                Invite = "discord.gg/TCf7QexN5e",
                Provider = new GuildContext()
            };

            foreach (var (_, client) in _client.ShardClients)
            {
                var bl = client.AddBlacklistModule();
                var afk = client.AddAfkModule();
                client.MessageCreated += afk.OnMessage;
                client.AddExtension(new Nqn());
                var commandsExtension = client.UseCommands(new CommandsExtension(commandsConfig));
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
                    },
                    new()
                    {
                        Name = "Moderation",
                        Description = "for people who like milk"
                    }
                });
                commandsExtension.Registry.RegisterCommands(GetType().Assembly);
                commandsExtension.Dispatcher.AddInhibitor((DiscordMessage message, Command t2) => bl.Check(message, t2));
                commandsExtension.Dispatcher.AddInhibitor((DiscordInteraction interaction, Command t2) => bl.Check(interaction, t2));
            }

            await _client.UpdateStatusAsync(new DiscordActivity("nothing", ActivityType.Playing), UserStatus.Idle,
                    DateTimeOffset.Now);
        }

        private Config GetConfig(string fileName)
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