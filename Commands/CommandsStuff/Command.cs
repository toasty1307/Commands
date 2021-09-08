using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Commands.CommandsStuff
{
    public abstract class Command : CommandsExtensionBase
    {
        public Group Group { get; set; }
        public ArgumentCollector Collector { get; set; }

        public virtual string Name => GetType().Name;
        public virtual string[] Aliases { get; }
        public virtual bool AutoAliases { get; } = true;
        public abstract string GroupName { get; }
        public virtual string MemberName => GetType().Name;
        public abstract string Description { get; }
        public virtual string Format { get; }
        public virtual string DetailedDescription { get; }
        public virtual string[] Examples { get; }
        public virtual bool GuildOnly { get; }
        public virtual bool OwnerOnly { get; }
        public virtual Permissions ClientPermissions { get; } = Permissions.None;
        public virtual Permissions UserPermissions { get; } = Permissions.None;
        public virtual bool Nsfw { get; }
        public virtual ThrottlingOptions ThrottlingOptions { get; }
        public virtual bool DefaultHandling { get; }
        public virtual Argument[] Arguments { get; }
        public virtual int ArgumentPromptLimit { get; } = int.MaxValue;
        public virtual ArgsType ArgsType { get; } = ArgsType.Single;
        public virtual int ArgsCount { get; } = 0;
        public virtual bool ArgsSingleQuotes { get; } = true;
        public virtual Regex[] Patterns { get; }
        public virtual bool Guarded { get; } = false;
        public virtual bool Hidden { get; } = false;
        public virtual bool Unknown { get; } = false;


        public abstract Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector);

        public override string ToString() => $"{Group.Id}:{Name}";
        public static implicit operator Command(string s) => Extension.Registry.Commands.First(x => $"{x.Group.Id}:{x.Name}" == s);

        public virtual async Task<DiscordMessage[]> OnBlock(DiscordMessage message, string reason,
            Permissions missingUserPermissions = Permissions.None,
            Permissions missingClientPermissions = Permissions.None, uint seconds = 0)
        {
            var messages = new List<DiscordMessage>();
            switch (reason)
            {
                case "GUILD_ONLY":
                    messages.Add(await message.ReplyAsync($"The `{Name}` Command can only be used in a guild"));
                    break;
                case "NSFW":
                    messages.Add(await message.ReplyAsync($"The `{Name}` Command can only be used in Nsfw channels"));
                    break;
                case "USER_PERMISSIONS":
                    messages.Add(await message.ReplyAsync(
                        $"You are missing the following permissions for the `{Name}` Command to work:\n`{string.Join(", ", missingUserPermissions)}`"));
                    break;
                case "CLIENT_PERMISSIONS":
                    messages.Add(await message.ReplyAsync($"I need the following permissions to run the `{Name}` Command:\n`{string.Join(", ", missingClientPermissions)}`"));
                    break;
                case "THROTTLING":
                    var minutes = seconds / 60;
                    seconds %= 60;
                    var hours = minutes / 60;
                    minutes %= 60;
                    var days = hours / 24;
                    hours %= 24;
                    var months = days / 30;
                    days %= 30;
                    var years = months / 12;
                    months %= 12;
                    var decades = years / 10;
                    years %= 10;
                    var stuff = new List<string>
                    {
                        decades    > 0 ? $"{decades} decades" : string.Empty,
                        years      > 0 ? $"{years} years"     : string.Empty,
                        months     > 0 ? $"{months} months"   : string.Empty,
                        days       > 0 ? $"{days} days"       : string.Empty,
                        hours      > 0 ? $"{hours} hours"     : string.Empty,
                        minutes    > 0 ? $"{minutes} minutes" : string.Empty,
                        seconds    > 0 ? $"{seconds} seconds" : string.Empty,
                    };
                    stuff = stuff.Where(x => !string.IsNullOrEmpty(x)).ToList();
                    var finalString = string.Join(", ", stuff);
                    messages.Add(await message.ReplyAsync($"You may not use the `{Name}` command for another {finalString}"));
                    break;
                case "DISABLED":
                    messages.Add(await message.ReplyAsync($"The `{Name}` Command is Disabled!"));
                    break;
            }
            return messages.ToArray();
        }
        
        public virtual async Task OnBlock(DiscordInteraction interaction, string reason,
            Permissions missingUserPermissions = Permissions.None,
            Permissions missingClientPermissions = Permissions.None, uint seconds = 0)
        {
            var messages = new List<DiscordMessage>();
            switch (reason)
            {
                case "GUILD_ONLY":
                    await interaction.FollowUpAsync($"The `{Name}` Command can only be used in a guild");
                    break;
                case "NSFW":
                    await interaction.FollowUpAsync($"The `{Name}` Command can only be used in Nsfw channels");
                    break;
                case "USER_PERMISSIONS":
                    await interaction.FollowUpAsync(
                        $"You are missing the following permissions for the `{Name}` Command to work:\n`{string.Join(", ", missingUserPermissions)}`");
                    break;
                case "CLIENT_PERMISSIONS":
                    await interaction.FollowUpAsync($"I need the following permissions to run the `{Name}` Command:\n`{string.Join(", ", missingClientPermissions)}`");
                    break;
                case "THROTTLING":
                    var minutes = seconds / 60;
                    seconds %= 60;
                    var hours = minutes / 60;
                    minutes %= 60;
                    var days = hours / 24;
                    hours %= 24;
                    var months = days / 30;
                    days %= 30;
                    var years = months / 12;
                    months %= 12;
                    var decades = years / 10;
                    years %= 10;
                    var stuff = new List<string>
                    {
                        decades    > 0 ? $"{decades} decades" : string.Empty,
                        years      > 0 ? $"{years} years"     : string.Empty,
                        months     > 0 ? $"{months} months"   : string.Empty,
                        days       > 0 ? $"{days} days"       : string.Empty,
                        hours      > 0 ? $"{hours} hours"     : string.Empty,
                        minutes    > 0 ? $"{minutes} minutes" : string.Empty,
                        seconds    > 0 ? $"{seconds} seconds" : string.Empty,
                    };
                    stuff = stuff.Where(x => !string.IsNullOrEmpty(x)).ToList();
                    var finalString = string.Join(", ", stuff);
                    await interaction.FollowUpAsync($"You may not use the `{Name}` command for another {finalString}");
                    break;
                case "DISABLED":
                    await interaction.FollowUpAsync($"The `{Name}` Command is Disabled!");
                    break;
            }
        }

        public virtual async Task<(bool, string)> HasPermission(DiscordMessage message, bool ownerOverride = false)
        {
            if (!OwnerOnly && UserPermissions is Permissions.None && ClientPermissions is Permissions.None) return (true, null);
            if (ownerOverride && Extension.Owners.Contains(message.Author)) return (true, null);
            if (OwnerOnly && (ownerOverride || !Extension.Owners.Contains(message.Author))) return (false, $"OWNER_ONLY");
            var missingClientPermissions =
                (message.Channel.Guild.Members[Client.CurrentUser.Id].PermissionsIn(message.Channel) ^
                 ClientPermissions) & ClientPermissions;
            var missingUserPermissions =
                (message.Channel.Guild.Members[Client.CurrentUser.Id].PermissionsIn(message.Channel) ^
                 UserPermissions) & UserPermissions;
            if (missingClientPermissions is not Permissions.None) return (false, "CLIENT_PERMISSIONS");
            if (missingUserPermissions is not Permissions.None) return (false, "USER_PERMISSIONS");
            return (true, null);
        }

        public Throttle GetThrottle(DiscordUser user) => _throttling.ContainsKey(user.Id) ? _throttling[user.Id] : null;

        public virtual async Task<(bool, string)> IsUsable(DiscordMessage message)
        {
            if (GuildOnly && message.Channel.Guild is null) return (false, "GUILD_ONLY");
            var (hasPermission, response) = await HasPermission(message);
            if (!hasPermission) return (false, response);
            if (Nsfw && !message.Channel.IsNSFW) return (false, "NSFW");
            try
            {
                if (!(await Extension.Provider.Get(message.Channel.Guild)).CommandStatuses[this]) return (false, "DISABLED");
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
                await Extension.Provider?.Init()!;
                return (true, null);
            }
            if (GetThrottle(message.Author) is not null) return (false, "THROTTLING");
            
            return (true, null);
        }

        public void Throttle(DiscordUser user)
        {
            if (ThrottlingOptions is null || Extension.Owners.Contains(user)) return;
            var throttleExists = _throttling.ContainsKey(user.Id);
            Throttle throttle;
            if (throttleExists)
                throttle = _throttling[user.Id];
            else
                throttle = new Throttle
                {
                    Start = DateTime.Now,
                    Usages = 0,
                    Timeout = Task.Delay((int) (ThrottlingOptions.Duration * 1000)).ContinueWith(_ => _throttling.Remove(user.Id))
                };
            if (!_throttling.ContainsValue(throttle)) _throttling.Add(user.Id, throttle);
        }

        private Dictionary<ulong, Throttle> _throttling = new();
        public abstract Task Run(DiscordInteraction interaction, ArgumentCollector collector);

        public virtual async Task<(bool, string)> IsUsable(DiscordInteraction interaction)
        {
            if (GuildOnly && interaction.Channel.Guild is null) return (false, "GUILD_ONLY");
            var (hasPermission, response) = await HasPermission(interaction);
            if (!hasPermission) return (false, response);
            if (Nsfw && !interaction.Channel.IsNSFW) return (false, "NSFW");
            try
            {
                if (!(await Extension.Provider.Get(interaction.Channel.Guild)).CommandStatuses[this]) return (false, "DISABLED");
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
                await Extension.Provider?.Init()!;
                return (true, null);
            }
            if (GetThrottle(interaction.User) is not null) return (false, "THROTTLING");
            
            return (true, null);
        }

        public virtual async Task<(bool, string)> HasPermission(DiscordInteraction interaction, bool ownerOverride = true)
        {
            if (!OwnerOnly && UserPermissions is Permissions.None && ClientPermissions is Permissions.None) return (true, null);
            if (ownerOverride && Extension.Owners.Contains(interaction.User)) return (true, null);
            if (OwnerOnly && (ownerOverride || !Extension.Owners.Contains(interaction.User))) return (false, $"OWNER_ONLY");
            var missingClientPermissions =
                (interaction.Channel.Guild.Members[Client.CurrentUser.Id].PermissionsIn(interaction.Channel) ^
                 ClientPermissions) & ClientPermissions;
            var missingUserPermissions =
                (interaction.Channel.Guild.Members[Client.CurrentUser.Id].PermissionsIn(interaction.Channel) ^
                 UserPermissions) & UserPermissions;
            if (missingClientPermissions is not Permissions.None) return (false, "CLIENT_PERMISSIONS");
            if (missingUserPermissions is not Permissions.None) return (false, "USER_PERMISSIONS");
            return (true, null);
        }
    }

    public partial class Throttle
    {
        public DateTime Start { get; set; }
        public int Usages { get; set; }
        public Task Timeout { get; set; }
    }

    public enum ArgsType
    {
        Single,
        Multiple
    }

    public class ThrottlingOptions
    {
        public int Usages { get; set; }
        public float Duration { get; set; }
    }
}