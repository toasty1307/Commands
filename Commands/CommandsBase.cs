using Commands.Utils;
using DSharpPlus;
using Newtonsoft.Json;

namespace Commands
{
    public class CommandsBase
    {
        [JsonIgnore] protected DiscordClient Client { get; }
        [JsonIgnore] protected CommandsExtension Extension => _extension ??= Client.GetCommandsExtension();
        [JsonIgnore] private CommandsExtension _extension;
        protected CommandsBase(DiscordClient client) => Client = client;
    }
}