using System;
using System.Linq;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.Types
{
    [ArgumentType]
    public class DiscordTextChannelArgumentType : ArgumentType<DiscordChannel>
    {
        public Regex ChannelRegex = new(@"\d+");
        public override bool Validate(string argString)
        {
            var match = ChannelRegex.Match(argString ?? "0");
            DiscordChannel channel;
            try
            {
                channel = Client.Guilds.Values.SelectMany(x => x.Channels.Values)
                    .First(x => x.Id == Convert.ToUInt64(match.Value));
                return channel.Type is ChannelType.News or ChannelType.Text;
            }
            catch
            {
                return false;
            }
        }

        public override DiscordChannel Parse(string argString)
        {
            var guilds = Client.Guilds.Values;
            var channels = guilds.SelectMany(x => x.Channels);
            var givenIdString = ChannelRegex.Match(argString).Value;
            var givenId = Convert.ToUInt64(givenIdString);
            try
            {
                var (_, value) = channels.First(x => x.Key == givenId);
                return value;
            }
            catch
            {
                return null;
            }
        }

        public override bool IsEmpty(DiscordChannel arg) => false;

        public DiscordTextChannelArgumentType(DiscordClient client) : base(client)
        {
        }
    }
    
    [ArgumentType]
    public class DiscordVoiceChannelArgumentType : ArgumentType<DiscordChannel>
    {
        public Regex ChannelRegex = new(@"\d+");
        public override bool Validate(string argString)
        {
            var match = ChannelRegex.Match(argString);
            DiscordChannel channel;
            try
            {
                channel = Client.Guilds.Values.SelectMany(x => x.Channels.Values)
                    .First(x => x.Id == Convert.ToUInt64(match.Value));
                return channel.Type is ChannelType.Voice;
            }
            catch
            {
                return false;
            }
        }

        public override DiscordChannel Parse(string argString)
        {
            var guilds = Client.Guilds.Values;
            var channels = guilds.SelectMany(x => x.Channels);
            var givenIdString = ChannelRegex.Match(argString).Value;
            var givenId = Convert.ToUInt64(givenIdString);
            try
            {
                var (_, value) = channels.First(x => x.Key == givenId);
                return value;
            }
            catch
            {
                return null;
            }
        }

        public override bool IsEmpty(DiscordChannel arg) => false;

        public DiscordVoiceChannelArgumentType(DiscordClient client) : base(client)
        {
        }
    }
    
    [ArgumentType]
    public class DiscordCategoryChannelArgumentType : ArgumentType<DiscordChannel>
    {
        public Regex ChannelRegex = new(@"\d+");
        public override bool Validate(string argString)
        {
            var match = ChannelRegex.Match(argString);
            DiscordChannel channel;
            try
            {
                channel = Client.Guilds.Values.SelectMany(x => x.Channels.Values)
                    .First(x => x.Id == Convert.ToUInt64(match.Value));
                return channel.Type is ChannelType.Category;
            }
            catch
            {
                return false;
            }
        }

        public override DiscordChannel Parse(string argString)
        {
            var guilds = Client.Guilds.Values;
            var channels = guilds.SelectMany(x => x.Channels);
            var givenIdString = ChannelRegex.Match(argString).Value;
            var givenId = Convert.ToUInt64(givenIdString);
            try
            {
                var (_, value) = channels.First(x => x.Key == givenId);
                return value;
            }
            catch
            {
                return null;
            }
        }

        public override bool IsEmpty(DiscordChannel arg) => false;

        public DiscordCategoryChannelArgumentType(DiscordClient client) : base(client)
        {
        }
    }
}