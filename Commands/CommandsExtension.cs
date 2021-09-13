﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Providers;
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
        public SettingProvider Provider { get; private set; }

        public event AsyncEventHandler<DiscordMessage> UnknownCommand; 
        public event AsyncEventHandler<SettingProvider> ProviderReady;
        public event AsyncEventHandler<Command, DiscordMessage, string, Permissions, Permissions, uint> CommandBlock;
        public event AsyncEventHandler<Command, string, DiscordMessage> CommandCancel;
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
            Provider?.Init();
            ProviderReadyInvoke(Provider);
            Registry = new CommandRegistry(Client);
            Dispatcher = new CommandDispatcher(Client, Registry);
            Client.Ready += (_, _) => FetchOwners();
            Client.MessageCreated += Dispatcher.Handle;
            Client.InteractionCreated += Dispatcher.Handle;
            Client.ContextMenuInteractionCreated +=  Dispatcher.Handle;
            Client.ComponentInteractionCreated += Dispatcher.Handle;
            Client.MessageUpdated += Dispatcher.Handle;
        }

        public async Task FetchOwners()
        {
            try
            {
                foreach (var ownerId in OwnerIds)
                    Owners.Add(await Client.GetUserAsync(ownerId));
            }
            catch
            {
                Client.Logger.LogError("Error Fetching Owners");
            }
        }

        public void SetProvider(SettingProvider provider)
        {
            Provider = provider;
            Provider?.Init();
        }

        public void CommandPrefixChanged(DiscordGuild channelGuild, string prefix)
        {
            CommandPrefixChange.SafeInvoke(channelGuild, prefix);
        }
        public void CommandStatusChanged(DiscordGuild channelGuild, Command command, bool enable)
        {
            CommandStatusChange.SafeInvoke(channelGuild, command, enable);
        }
        public void UnknownCommandRun(DiscordMessage message)
        {
            UnknownCommand.SafeInvoke(message);
        }
        public void CommandRegistered(Command command)
        {
            CommandRegister.SafeInvoke(command);
        }
        public void ProviderReadyInvoke(SettingProvider provider)
        {
            ProviderReady.SafeInvoke(provider);
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
        // TODO
        public void GroupStatusChanged(DiscordGuild guild, Group @group, bool enabled)
        {
            GroupStatusChange.SafeInvoke(guild, group, enabled);
        }
        public void CommandCanceled(Command command, string invalidArgs, DiscordInteraction interaction)
        {
            throw new System.NotImplementedException();
        }

        public void CommandBlocked(Command command, DiscordInteraction interaction, string reason, Permissions missingUserPermissions, Permissions missingClientPermissions, uint seconds)
        {
            throw new System.NotImplementedException();
        }
        public void UnknownCommandRun(DiscordInteraction interaction)
        {
            throw new System.NotImplementedException();
        }
    }
}