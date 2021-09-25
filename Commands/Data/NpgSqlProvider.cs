using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace Commands.Data
{
    public class NpgSqlProvider : SettingProvider
    {
        public NpgsqlConnection Connection { get; }
        public List<GuildSettings> Cache { get; } = new();

        public NpgSqlProvider(string connectionString, DiscordClient client) : base(client)
        {
            Connection = new NpgsqlConnection(connectionString);
        }

        public override void Init()
        {
            try
            {
                Connection.Open();

                var createTableCommand = Connection.CreateCommand();
                createTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS \"Settings\"(\"GuildId\" BIGINT PRIMARY KEY, \"Json\" VARCHAR)";
                createTableCommand.ExecuteNonQuery();

                var loadCommand = Connection.CreateCommand();
                loadCommand.CommandText = "SELECT * FROM \"Settings\"";
                using var reader = loadCommand.ExecuteReader();
                while (reader.Read())
                {
                    var guildSettings = JsonConvert.DeserializeObject<GuildSettings>(reader.GetString(1).Replace("\n", ""));
                    Cache.Add(guildSettings);
                }
            
                Client.GuildCreated += ClientOnGuildCreated;
                Extension.CommandPrefixChange += ExtensionOnCommandPrefixChange;
                Extension.CommandStatusChange += ExtensionOnCommandStatusChange;
                Extension.GroupStatusChange += ExtensionOnGroupStatusChange;
                Extension.CommandRegister += ExtensionOnCommandRegister;
                Extension.GroupRegister += ExtensionOnGroupRegister;
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
            }
        }

        private Task ExtensionOnGroupRegister(Group t1)
        {
            Cache.ForEach(x => x.Groups.Add(t1, true));
            Update();
            return Task.CompletedTask;
        }

        private Task ExtensionOnCommandRegister(Command t1)
        {
            Cache.ForEach(x => x.Commands.Add(t1, true));
            Update();
            return Task.CompletedTask;
        }

        private Task ExtensionOnGroupStatusChange(DiscordGuild t1, Group t2, bool t3)
        {
            var settings = Cache.First(x => x.Id == (t1?.Id ?? 0));
            if (!settings.Groups.ContainsKey(t2))
                settings.Groups.Add(t2, t3);
            else
                settings.Groups[t2] = t3;
            Set(t1, settings, true);
            return Task.CompletedTask;
        }

        private Task ExtensionOnCommandStatusChange(DiscordGuild t1, Command t2, bool t3)
        {
            var settings = Cache.First(x => x.Id == (t1?.Id ?? 0));
            if (!settings.Commands.ContainsKey(t2))
                settings.Commands.Add(t2, t3);
            else
                settings.Commands[t2] = t3;
            Set(t1, settings, true);
            return Task.CompletedTask;
        }

        private Task ExtensionOnCommandPrefixChange(DiscordGuild t1, string t2)
        {
            var settings = Cache.First(x => x.Id == (t1?.Id ?? 0));
            settings.Prefix = t2;
            Set(t1, settings, true);
            return Task.CompletedTask;
        }

        private Task ClientOnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            var guild = e.Guild;
            var settings = new GuildSettings
            {
                Id = guild.Id,
                Commands = new Dictionary<Command, bool>(Extension.Registry.Commands.Select(x => new KeyValuePair<Command, bool>(x, true))),
                Groups = new Dictionary<Group, bool>(Extension.Registry.Groups.Select(x => new KeyValuePair<Group, bool>(x, true))),
                Prefix = Extension.CommandPrefix
            };
            Set(guild, settings);
            return Task.CompletedTask;
        }

        public override GuildSettings Get(DiscordGuild guild) => Cache.Exists(x => x.Id == (guild?.Id ?? 0))
            ? Cache.First(x => x.Id == (guild?.Id ?? 0))
            : null;

        public override void Set(DiscordGuild guild, GuildSettings settings, bool bypassChecks = false)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var cachedSettings = Cache.FirstOrDefault(x => x.Id == (guild?.Id ?? 0));
                    var shouldUpdate = true;
                    if (cachedSettings is not default(GuildSettings))
                        shouldUpdate = cachedSettings != settings;
                    if (!shouldUpdate && !bypassChecks) return;
                    if (Cache.Contains(cachedSettings))
                        Cache.Remove(cachedSettings);
                    Cache.Add(settings);
                    var insertOrReplaceCommand = Connection.CreateCommand();
                    insertOrReplaceCommand.CommandText =
                        "INSERT INTO \"Settings\" (\"GuildId\", \"Json\") VALUES (@PARAM_GUILD_ID, @PARAM_JSON) ON CONFLICT (\"GuildId\") DO UPDATE SET \"GuildId\" = @PARAM_GUILD_ID, \"Json\" = @PARAM_JSON;";
                    insertOrReplaceCommand.Parameters.AddWithValue("PARAM_GUILD_ID", NpgsqlDbType.Bigint, (long)(guild?.Id ?? 0));
                    insertOrReplaceCommand.Parameters.AddWithValue("PARAM_JSON", $"{JsonConvert.SerializeObject(settings)}");
                    await insertOrReplaceCommand.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Client.Logger.Error(e);
                }
            });
        }

        public override void Clear(DiscordGuild guild) => Set(guild, null);
        public override void Update() => Update(null);
        public override void Update(DiscordGuild guild) => Cache.Where(x => x.Id == (guild?.Id ?? 0)).ToList().ForEach(x => Set(null, x, true));
        public override void Destroy()
        {
            Client.GuildCreated -= ClientOnGuildCreated;
            Extension.CommandPrefixChange -= ExtensionOnCommandPrefixChange;
            Extension.CommandStatusChange -= ExtensionOnCommandStatusChange;
            Extension.GroupStatusChange -= ExtensionOnGroupStatusChange;
            Extension.CommandRegister -= ExtensionOnCommandRegister;
            Extension.GroupRegister -= ExtensionOnGroupRegister;
        }
    }
}