using System.Text.Json.Serialization;
using DSharpPlus;

namespace Commands
{
    public class CommandsExtensionBase
    {
        [JsonIgnore]
        public static DiscordClient Client { get; set; }
        [JsonIgnore]
        public static CommandsExtension Extension => _extension ??= Client.GetExtension<CommandsExtension>();
        [JsonIgnore]
        private static CommandsExtension _extension;
    }
}