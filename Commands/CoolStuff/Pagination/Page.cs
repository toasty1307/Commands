using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Commands.CoolStuff.Pagination
{
    public class Page
    {
        public string Content { get; init; }
        public DiscordEmbed Embed { get; init; }

        public override string ToString() => string.Join(':', Content, JsonConvert.SerializeObject(Embed));
    }
}