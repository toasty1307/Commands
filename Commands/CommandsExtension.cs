using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Data;
using Commands.Types;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public class CommandsExtension : BaseExtension
    {
        public CommandsConfig Options { get; }
        public List<DiscordUser> Owners { get; } = new();
        public List<ulong> OwnerIds { get; }
        public CommandDispatcher Dispatcher { get; private set; }
        public CommandRegistry Registry { get; private set; }
        public GuildContext Provider { get; private set; }

        public event AsyncEventHandler<DiscordMessage> UnknownCommand; 
        public event AsyncEventHandler<DiscordInteraction> UnknownCommandInteraction; 
        public event AsyncEventHandler<Command, DiscordMessage, string, Permissions, Permissions, uint> CommandBlock;
        public event AsyncEventHandler<Command, DiscordInteraction, string, Permissions, Permissions, uint> CommandBlockInteraction;
        public event AsyncEventHandler<Command, string, DiscordMessage> CommandCancel;
        public event AsyncEventHandler<Command, string, DiscordInteraction> CommandCancelInteraction;
        public event AsyncEventHandler<Group> GroupRegister;
        public event AsyncEventHandler<Command> CommandRegister;
        public event AsyncEventHandler<DiscordGuild, string> CommandPrefixChange;
        public event AsyncEventHandler<DiscordGuild, Command, bool> CommandStatusChange;
        public event AsyncEventHandler<DiscordGuild, Group, bool> GroupStatusChange;
        public event AsyncEventHandler<ArgumentType> TypeRegister;

        public string CommandPrefix => Options.Prefix;

        public CommandsExtension(CommandsConfig config)
        {
            Options = config;
            OwnerIds = Options.Owners.ToList();
        }
        
        protected override void Setup(DiscordClient client)
        {
            Client = client;
            Registry = new CommandRegistry(Client);
            Dispatcher = new CommandDispatcher(Client, Registry);
            Provider = new GuildContext();
            Client.GuildDownloadCompleted += (_, _) => FetchOwners();
            Client.MessageCreated += Dispatcher.Handle;
            Client.InteractionCreated += Dispatcher.Handle;
            Client.ContextMenuInteractionCreated +=  Dispatcher.Handle;
            Client.ComponentInteractionCreated += Dispatcher.Handle;
            Client.MessageUpdated += Dispatcher.Handle;
            Provider.DoCacheStuff();
        }

        public async Task FetchOwners()
        {
            try
            {
                foreach (var ownerId in OwnerIds)
                    Owners.Add(await Client.GetUserAsync(ownerId));
            }
            catch (Exception e)
            {
                Client.Logger.LogError("Error Fetching Owners:");
                Client.Logger.Error(e);
            }
        }

        #region DontOpen
        
        public void CommandPrefixChanged(DiscordGuild channelGuild, string prefix)
        {
            CommandPrefixChange.SafeInvoke(channelGuild, prefix);
            var entity = Provider.Get(channelGuild);
            entity.Prefix = prefix;
            Provider.Update(channelGuild, entity);
        }
        public void CommandStatusChanged(DiscordGuild guild, Command command, bool enable)
        {
            CommandStatusChange.SafeInvoke(guild, command, enable);
            var entity = Provider.Get(guild);
            entity.Commands.Remove(entity.Commands.First(x => x.Key.Name == command.Name).Key);
            entity.Commands.Add(new DisabledCommandEntity
            {
                Name = command.Name,
                GuildId = guild.Id
            }, enable);
            Provider.Update(guild, entity);
        }
        public void UnknownCommandRun(DiscordMessage message)
        {
            UnknownCommand.SafeInvoke(message);
        }
        public void CommandRegistered(Command command)
        {
            CommandRegister.SafeInvoke(command);
        }
        public void CommandBlocked(Command command, DiscordMessage message, string reason, Permissions missingUserPermissions = Permissions.None, Permissions missingClientPermissions = Permissions.None, uint seconds = 0)
        {
            CommandBlock.SafeInvoke(command, message, reason, missingUserPermissions, missingClientPermissions, seconds);
        }
        public void CommandCanceled(Command command, string reason, DiscordMessage message)
        {
            CommandCancel.SafeInvoke(command, reason, message);
        }
        public void TypeRegistered(ArgumentType type)
        {
            TypeRegister.SafeInvoke(type);
        }
        public void GroupRegistered(Group group)
        {
            GroupRegister.SafeInvoke(group);
            
        }
        public void GroupStatusChanged(DiscordGuild guild, Group @group, bool enabled)
        {
            GroupStatusChange.SafeInvoke(guild, group, enabled);
            var entity = Provider.Get(guild);
            entity.Groups.Remove(entity.Groups.First(x => x.Key.Name == group.Name).Key);
            entity.Groups.Add(new DisabledGroupEntity
            {
                Name = group.Name,
                GuildId = guild.Id
            }, enabled);
            Provider.Update(guild, entity);
        }
        public void CommandCanceled(Command command, string invalidArgs, DiscordInteraction interaction)
        {
            CommandCancelInteraction.SafeInvoke(command, invalidArgs, interaction);
        }

        public void CommandBlocked(Command command, DiscordInteraction interaction, string reason, Permissions missingUserPermissions = Permissions.None, Permissions missingClientPermissions = Permissions.None, uint seconds = 0U)
        {
            CommandBlockInteraction.SafeInvoke(command, interaction, reason, missingUserPermissions,
                missingClientPermissions, seconds);
        }
        public void UnknownCommandRun(DiscordInteraction interaction)
        {
            UnknownCommandInteraction.SafeInvoke(interaction);
        }
        
        #endregion
    }
}