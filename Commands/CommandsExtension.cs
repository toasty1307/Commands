using System.Collections.Generic;
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
        public CommandsConfig Options { get; set; }
        public List<DiscordUser> Owners { get; set; } = new();
        public List<ulong> OwnerIds { get; set; }
        public CommandDispatcher Dispatcher { get; set; }
        public CommandRegistry Registry { get; set; }
        public SettingProvider Provider { get; set; }

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
            Registry = new CommandRegistry();
            Dispatcher = new CommandDispatcher(Registry);
        }
        
        protected override void Setup(DiscordClient client)
        {
            Client = client;
            CommandsExtensionBase.Client = client;
            Provider?.Init();
            ProviderReadyInvoke(Provider);
            Client.Ready += (_, _) => FetchOwners();
            Client.MessageCreated += (_, args) => Dispatcher.Handle(args.Message);
            Client.InteractionCreated += (_, args) => Dispatcher.Handle(args.Interaction);
            Client.ContextMenuInteractionCreated += (_, args) => Dispatcher.Handle(args);
            Client.ComponentInteractionCreated += (_, args) => Dispatcher.Handle(args);
            Client.MessageUpdated += (_, args) => Dispatcher.Handle(args.Message, args.MessageBefore);
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