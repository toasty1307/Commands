using System.Collections.Generic;
using CommandsTest.Modules;
using DSharpPlus;

namespace CommandsTest.Utils
{
    public static class Extentions
    {
        private static Dictionary<DiscordClient, BlacklistModule> BlacklistModules { get; set; } = new();
        public static BlacklistModule GetBlacklistModule(this DiscordClient client) => BlacklistModules[client];
        public static BlacklistModule AddBlacklistModule(this DiscordClient client)
        {
            var module = new BlacklistModule();
            BlacklistModules.Add(client, module);
            return module;
        }
        
        private static Dictionary<DiscordClient, AfkModule> AfkModules { get; set; } = new();
        public static AfkModule GetAfkModule(this DiscordClient client) => AfkModules[client];
        public static AfkModule AddAfkModule(this DiscordClient client)
        {
            var module = new AfkModule();
            AfkModules.Add(client, module);
            return module;
        }
    }
}