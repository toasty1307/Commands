using System;
using System.IO;
using System.Threading.Tasks;
using Commands;
using Commands.CommandsStuff;
using Commands.Providers;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CommandsTest
{
    public class CommandsTestBot
    {
        public DiscordClient Client { get; internal set; }

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
                Owners = new ulong[]{ 849939032410030080, 742976057761726514 },
                Invite = "https://discord.gg/TCf7QexN5e"
            };
            Client = new DiscordClient(discordConfiguration);
            var commandsExtension = Client.UseCommands(new CommandsExtension(commandsConfig));
            commandsExtension.Registry.RegisterDefaults();
            commandsExtension.Registry.RegisterGroups(new []{new Group{Name = "Misc", Description = "misc stuff, nothing much"}, new Group{Name = "Admin", Description = "Owner only commands idk"}});
            commandsExtension.Registry.RegisterCommands(GetType().Assembly);
            commandsExtension.SetProvider(new SqliteProvider("Data Source=Database.db"));
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