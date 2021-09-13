using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace Commands.Providers
{
    public class SqliteProvider : SettingProvider
    {
        public SqliteConnection Connection { get; set; }
        public Dictionary<ulong, GuildSettingHelper> SettingsMap { get; set; } = new();

        public SqliteProvider(DiscordClient client, string sqlConnectionString) : base(client)
        {
            Connection = new SqliteConnection(sqlConnectionString);
        }

        public override async Task Init()
        {
            // Make sure Table Exist
            await Connection.OpenAsync();
            var createTableCommand = Connection.CreateCommand();
            createTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS Settings(Guild TEXT, Settings TEXT)";
            await createTableCommand.ExecuteNonQueryAsync();

            // Sync Memory to DB
            var getSettingsCommand = Connection.CreateCommand();
            getSettingsCommand.CommandText = "SELECT * FROM Settings";
            await using var reader = await getSettingsCommand.ExecuteReaderAsync();
            SettingsMap = new Dictionary<ulong, GuildSettingHelper>();
            while (await reader.ReadAsync())
            {
                try
                {
                    var guild = Convert.ToUInt64(reader.GetString(0));
                    var guildSettingHelper = JsonConvert.DeserializeObject<GuildSettingHelper>(reader.GetString(1));
                    SettingsMap.Add(guild, guildSettingHelper);
                }
                catch (Exception e)
                {
                    Client.Logger.Error(e);
                    throw;
                }
            }

            foreach (var discordGuild in Client.Guilds.Values)
            {
                foreach (var command in Extension.Registry.Commands.Where(x => !SettingsMap[discordGuild.Id].CommandStatuses.Keys.Contains(x)))
                {
                    SettingsMap[discordGuild.Id].CommandStatuses.Add(command, true);
                    await Set(discordGuild, SettingsMap[discordGuild.Id]);
                }
            }
            
            // Subscribe to Event Handlers
            Extension.CommandPrefixChange += OnExtensionOnCommandPrefixChange;
            Extension.CommandStatusChange += ExtensionOnCommandStatusChange;
            Extension.GroupStatusChange += ExtensionOnGroupStatusChange;
            Extension.CommandRegister += ExtensionOnCommandRegister;
            Extension.GroupRegister += ExtensionOnGroupRegister;
            Client.GuildCreated += ClientOnGuildCreated;
            Client.GuildDeleted += ClientOnGuildDeleted;
        }

        private async Task ClientOnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        { 
            if (SettingsMap.ContainsKey(e.Guild.Id))
                SettingsMap.Remove(e.Guild.Id);
            await Clear(e.Guild);
        }

        private async Task ClientOnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            var helper = new GuildSettingHelper(sender, e.Guild?.Id ?? 0);
            SettingsMap.Add(e.Guild?.Id ?? 0, helper);
            await Set(e.Guild, helper);
        }

        private async Task ExtensionOnGroupRegister(Group t1)
        {
            foreach (var (_, helper) in SettingsMap)
            {
                helper.GroupStatuses.Add(t1, true);
                await Set(helper.GuildId == 0 ? null : Client.Guilds[helper.GuildId], helper);
            }
        }

        private async Task ExtensionOnCommandRegister(Command t1)
        {
            foreach (var (_, helper) in SettingsMap)
            {
                helper.CommandStatuses.Add(t1, true);
                await Set(helper.GuildId == 0 ? null : Client.Guilds[helper.GuildId], helper);
            }
        }

        private async Task ExtensionOnGroupStatusChange(DiscordGuild t1, Group t2, bool t3)
        {
            var id = t1?.Id ?? 0;
            var guildSettingsHelper = SettingsMap[id];
            if (guildSettingsHelper.GroupStatuses.ContainsKey(t2))
                guildSettingsHelper.GroupStatuses[t2] = t3;
            else
                guildSettingsHelper.GroupStatuses.Add(t2, t3);
            await Set(t1, guildSettingsHelper);
        }

        private async Task ExtensionOnCommandStatusChange(DiscordGuild t1, Command t2, bool t3)
        {
            var id = t1?.Id ?? 0;
            var guildSettingsHelper = SettingsMap[id];
            if (guildSettingsHelper.CommandStatuses.ContainsKey(t2))
                guildSettingsHelper.CommandStatuses[t2] = t3;
            else
                guildSettingsHelper.CommandStatuses.Add(t2, t3);
            await Set(t1, guildSettingsHelper);
        }

        private async Task OnExtensionOnCommandPrefixChange(DiscordGuild guild, string prefix)
        {
            var id = guild?.Id ?? 0;
            var guildSettingsHelper = SettingsMap[id];
            guildSettingsHelper.Prefix = prefix;
            await Set(guild, guildSettingsHelper);
        }

        public override async Task Clear(DiscordGuild guild)
        {
            if (SettingsMap.ContainsKey(guild?.Id ?? 0))
                SettingsMap.Remove(guild?.Id ?? 0);
            var deleteCommand = Connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Settings WHERE Guild = $guildId";
            deleteCommand.Parameters.AddWithValue("$guildId", guild?.Id ?? 0);
            await deleteCommand.ExecuteNonQueryAsync();
        }

        public override Task Destroy()
        {
            // Remove All Handlers
            Extension.CommandPrefixChange -= OnExtensionOnCommandPrefixChange;
            Extension.CommandStatusChange -= ExtensionOnCommandStatusChange;
            Extension.GroupStatusChange -= ExtensionOnGroupStatusChange;
            Extension.CommandRegister -= ExtensionOnCommandRegister;
            Extension.GroupRegister -= ExtensionOnGroupRegister;
            Client.GuildCreated -= ClientOnGuildCreated;
            Client.GuildDeleted -= ClientOnGuildDeleted;
            return Task.CompletedTask;
        }

        public override async Task<GuildSettingHelper> Get(DiscordGuild guild)
        {
            if (SettingsMap.ContainsKey(guild?.Id ?? 0))
                return SettingsMap[guild?.Id ?? 0];
            var settings = new GuildSettingHelper(Client, guild?.Id ?? 0);
            await Set(settings.GuildId == 0 ? null : Client.Guilds[settings.GuildId], settings);
            return settings;
        }

        public override async Task Set(DiscordGuild guild, GuildSettingHelper helper)
        {
            try
            {
                if (SettingsMap.ContainsKey(guild?.Id ?? 0) && SettingsMap[guild?.Id ?? 0] != helper)
                    SettingsMap[guild?.Id ?? 0] = helper;
                else if (!SettingsMap.ContainsKey(guild?.Id ?? 0)) SettingsMap.Add(guild?.Id ?? 0, helper);
                var deleteCommand = Connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Settings WHERE Guild = $guildId";
                deleteCommand.Parameters.AddWithValue("$guildId", guild?.Id ?? 0);
                await deleteCommand.ExecuteNonQueryAsync();
                var addCommand = Connection.CreateCommand();
                addCommand.CommandText = "INSERT OR REPLACE INTO Settings VALUES($guildId, $json)";
                addCommand.Parameters.AddWithValue("$guildId", guild?.Id ?? 0);
                addCommand.Parameters.AddWithValue("$json", JsonConvert.SerializeObject(helper));
                await addCommand.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
                throw;
            }
        }
    }
}