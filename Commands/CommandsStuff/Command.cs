using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Commands.CommandsStuff
{
    public abstract class Command : CommandsBase
    {
        public static readonly List<Command> Commands = new();

        public Group Group { get; set; }

        public virtual string Name => GetType().Name;

        public virtual bool RegisterSlashCommand => true;

        public virtual string[] Aliases => null;

        public abstract string GroupName { get; }
        
        public virtual string MemberName => GetType().Name;

        public virtual string Description => null;

        public virtual string DetailedDescription => null;

        public virtual string[] Examples => null;

        public virtual bool GuildOnly => false;

        public virtual bool OwnerOnly => false;

        public virtual Permissions ClientPermissions => Permissions.None;

        public virtual Permissions UserPermissions => Permissions.None;

        public virtual bool Nsfw => false;

        public virtual ThrottlingOptions ThrottlingOptions => null;

        public virtual Argument[] Arguments => null;

        public virtual bool Guarded => false;

        public virtual bool Hidden => false;

        public virtual bool Unknown => false;

        public abstract Task Run(MessageContext ctx);
        public abstract Task Run(InteractionContext ctx);

        public override string ToString() => $"{Group.Id}:{Name}";
        public static implicit operator Command(string s) => Commands.First(x => $"{x.Group.Id}:{x.Name}" == s || x.Name == s);

        public virtual async Task OnBlock(DiscordMessage message, string reason, Permissions missingUserPermissions = Permissions.None, Permissions missingClientPermissions = Permissions.None, uint seconds = 0)
        {
            switch (reason)
            {
                case "GUILD_ONLY":
                    await message.ReplyAsync($"The `{Name}` Command can only be used in a guild");
                    break;
                case "NSFW":
                    await message.ReplyAsync($"The `{Name}` Command can only be used in Nsfw channels");
                    break;
                case "USER_PERMISSIONS":
                    await message.ReplyAsync(
                        $"You are missing the following permissions for the `{Name}` Command to work:\n`{string.Join(", ", missingUserPermissions)}`");
                    break;
                case "CLIENT_PERMISSIONS":
                    await message.ReplyAsync($"I need the following permissions to run the `{Name}` Command:\n`{string.Join(", ", missingClientPermissions)}`");
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
                    await message.ReplyAsync($"You may not use the `{Name}` command for another {finalString}");
                    break;
                case "DISABLED":
                    await message.ReplyAsync($"The `{Name}` Command is Disabled!");
                    break;
                case "GROUP_DISABLED":
                    await message.ReplyAsync($"The `{Group.Name}` Group is Disabled!");
                    break;
            }
        }
        
        public virtual async Task OnBlock(DiscordInteraction interaction, string reason, Permissions missingUserPermissions = Permissions.None, Permissions missingClientPermissions = Permissions.None, uint seconds = 0)
        {
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
                case "GROUP_DISABLED":
                    await interaction.FollowUpAsync($"The `{Group.Name}` Group is Disabled!");
                    break;
            }
        }

#pragma warning disable 1998
        public virtual async Task<(bool, string)> HasPermission(DiscordMessage message, bool ownerOverride = false)
#pragma warning restore 1998
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
                if (!(Extension.Provider.Get(message.Channel.Guild)).Commands[this]) return (false, "DISABLED");
                if (!(Extension.Provider.Get(message.Channel.Guild)).Groups[Group]) return (false, "GROUP_DISABLED");
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
                if (!Extension.Provider.Get(message.Channel.Guild).Commands.ContainsKey(this))
                {
                    var guildSettings = Extension.Provider.Get(message.Channel.Guild);
                    guildSettings.Commands.Add(this, true);
                    Extension.Provider.Set(message.Channel.Guild, guildSettings, true);
                    Client.Logger.LogWarning("Command was not in DB, added it");
                }
                if (!Extension.Provider.Get(message.Channel.Guild).Groups.ContainsKey(Group))
                {
                    var guildSettings = Extension.Provider.Get(message.Channel.Guild);
                    guildSettings.Groups.Add(Group, true);
                    Extension.Provider.Set(message.Channel.Guild, guildSettings, true);
                    Client.Logger.LogWarning("Group was not in DB, added it");
                }
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

        public virtual async Task<(bool, string)> IsUsable(DiscordInteraction interaction)
        {
            if (GuildOnly && interaction.Channel.Guild is null) return (false, "GUILD_ONLY");
            var (hasPermission, response) = await HasPermission(interaction);
            if (!hasPermission) return (false, response);
            if (Nsfw && !interaction.Channel.IsNSFW) return (false, "NSFW");
            try
            {
                if (!(Extension.Provider.Get(interaction.Channel.Guild)).Commands[this]) return (false, "DISABLED");
                if (!(Extension.Provider.Get(interaction.Channel.Guild)).Groups[Group]) return (false, "GROUP_DISABLED");
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
                return (true, null);
            }
            if (GetThrottle(interaction.User) is not null) return (false, "THROTTLING");
            
            return (true, null);
        }

#pragma warning disable 1998
        public virtual async Task<(bool, string)> HasPermission(DiscordInteraction interaction, bool ownerOverride = true)
#pragma warning restore 1998
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

        protected Command(DiscordClient client) : base(client) { }
    }

    public class Throttle
    {
        private readonly DateTime _start;
        private int _usages;
        private Task _timeout;

        public DateTime Start
        {
            get => _start;
            init => _start = value;
        }

        public int Usages
        {
            get => _usages;
            init => _usages = value;
        }

        public Task Timeout
        {
            get => _timeout;
            init => _timeout = value;
        }
    }

    public class ThrottlingOptions
    {
        private int _usages;
        private float _duration;
        private string _id;

        public string Id
        {
            get => _id;
            init => _id = value;
        }

        public int Usages
        {
            get => _usages;
            init => _usages = value;
        }

        public float Duration
        {
            get => _duration;
            init => _duration = value;
        }

        public ThrottlingOptions()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}