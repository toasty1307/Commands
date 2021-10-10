using System;
using System.IO;
using System.Threading.Tasks;
using Commands;
using Commands.CommandsStuff;
using Commands.Utils;
using CommandsTest.Commands.Misc;
using CommandsTest.Modules;
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
        private DiscordClient Client { get; }
        private BlacklistModule BlacklistModule { get; }
        
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
                Owners = new ulong[]{ 742976057761726514, 519673297693048832 },
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
                },
                new()
                {
                    Name = "Moderation",
                    Description = "for people who like milk"
                }
            });
            commandsExtension.Registry.RegisterCommands(GetType().Assembly);
            Client.GuildMemberAdded += (_, args) => CheckForBadNick(args.Member, args.Member.Nickname ?? args.Member.Username);
            Client.GuildMemberUpdated += (_, args) => CheckForBadNick(args.Member, args.NicknameAfter);
            Client.GuildDownloadCompleted += (client, _) =>
            {
                client.Logger.LogInformation($"Client ready, logged in as ({client.CurrentUser.Username}#{client.CurrentUser.Discriminator}), you can run your command now senpai");
                return Task.CompletedTask;
            };
            Client.UseLavalink();
            Client.Logger.LogInformation("Initializing blacklist module");
            BlacklistModule = Client.AddBlacklistModule();
            Client.Logger.LogInformation("Initialized blacklist module");
            commandsExtension.Dispatcher.AddInhibitor((DiscordMessage message, Command t2) => BlacklistModule.Check(message, t2));
            commandsExtension.Dispatcher.AddInhibitor((DiscordInteraction interaction, Command t2) => BlacklistModule.Check(interaction, t2));
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
                Client.Logger.Error(e);
            }
        }

        public async Task Run()
        {
            await Client.ConnectAsync(new DiscordActivity{ActivityType = ActivityType.Playing, Name = "Milk"}, UserStatus.Idle, DateTimeOffset.Now);
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