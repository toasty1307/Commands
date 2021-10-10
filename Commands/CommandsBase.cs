using Commands.Utils;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LoggerFactory = Commands.Utils.LoggerFactory;

namespace Commands
{
    public class CommandsBase
    {
        [JsonIgnore] protected ILogger<CommandsBase> Logger { get; }
        [JsonIgnore] protected DiscordClient Client { get; }
        [JsonIgnore] protected CommandsExtension Extension => _extension ??= Client.GetCommandsExtension();
        [JsonIgnore] private CommandsExtension _extension;
        protected CommandsBase(DiscordClient client)
        {
            Logger = new Logger<CommandsBase>(new LoggerFactory());
            Client = client;
        }
    }
}