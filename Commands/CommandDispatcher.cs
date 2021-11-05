using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Data;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public class CommandDispatcher : CommandsBase
    {
        #region Fields
        
        private readonly Regex _splitWordsRegex = new("(?<=\")[^\"]*(?=\")|[^\" ]+");
        
        public CommandRegistry Registry { get; }
        public ArgumentParser ArgumentParser { get; }
        
        public List<Inhibitor<DiscordMessage, Command>> MessageInhibitors { get; } = new();
        
        public List<Inhibitor<DiscordInteraction, Command>> InteractionInhibitors { get; } = new();
        
        public Dictionary<string, Action<ComponentInteractionCreateEventArgs>> ComponentActions { get; } = new();
        
        #endregion

        #region Constructors
        public CommandDispatcher(DiscordClient client, CommandRegistry registry) : base(client)
        {
            Registry = registry;
            ArgumentParser = new ArgumentParser(client, Registry);
        }
        #endregion

        #region Methods

        #region Utils
        
        public void AddInhibitor(Inhibitor<DiscordMessage, Command> inhibitor) => MessageInhibitors.Add(inhibitor);
        
        public void AddInhibitor(Inhibitor<DiscordInteraction, Command> inhibitor) => InteractionInhibitors.Add(inhibitor);
        
        public string[] GetCommandString(DiscordMessage message)
        {
            try
            {
                var prefix = (Extension.Provider.Get(message.Channel.Guild)).Prefix;
                if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
                prefix = prefix.ToLower();
                var mentionPrefix = $"<@!{Client.CurrentUser.Id}>";
                var words = message.Content.Split(" ");
                words[0] = words[0].ToLower().Replace(prefix.Trim(), string.Empty).Replace(mentionPrefix, string.Empty);
                var commandMessage = new Regex("\\s+").Replace(string.Join(" ", words), " ").Trim();
                words = commandMessage.Split(" ");
                return words;
            }
            catch (Exception e)
            {
                Logger.Error(e); throw;
            }
        }
        
        private void MakeSureDataBaseThingExists(DiscordGuild guild)
        {
            var settings = Extension.Provider.Get(guild);
            if (settings is null)
            {
                Extension.Provider.Set(new GuildEntity
                {
                    Prefix = Extension.CommandPrefix,
                    Commands = new Dictionary<DisabledCommandEntity, bool>(Extension.Registry.Commands.
                        Select(x => 
                            new KeyValuePair<DisabledCommandEntity, bool>(
                                new DisabledCommandEntity{Name = x.Name, GuildId = guild.Id}, true))),
                    Groups = new Dictionary<DisabledGroupEntity, bool>(Extension.Registry.Groups.
                        Select(x => 
                            new KeyValuePair<DisabledGroupEntity, bool>(
                                new DisabledGroupEntity{Name = x.Name, GuildId = guild.Id}, true))),
                    GuildId = guild?.Id ?? 0
                });
            }
        }
        
        public string[] SplitWords(string str)
        {
            var matches = _splitWordsRegex.Matches(str);
            var matchArray = matches.ToArray();
            var matchStrings = matchArray.Select(x => x.Value);
            return matchStrings.ToArray();
        }
        
        public bool ShouldHandle(DiscordMessage message)
        {
            if (message.Author.IsBot) return false;
            if (string.IsNullOrWhiteSpace(message.Content)) return false;
            
            // Check for prefix or ping
            var prefix = Extension.Provider.Get(message.Channel.Guild)?.Prefix ?? Extension.CommandPrefix;
            if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
            var mentionPrefix = $"<@!{Client.CurrentUser.Id}>";
            var prefixRegex = new Regex($@"^({prefix}|{mentionPrefix})");
            var match = prefixRegex.Match(message.Content.ToLower());
            if (!match.Success && message.Channel.Guild is not null) return false;
            
            return true;
        }
        
        #endregion

        #region Handlers

        public async Task Handle(DiscordClient _, MessageCreateEventArgs args)
        {
            var message = args.Message;
            try
            {
                MakeSureDataBaseThingExists(message.Channel.Guild);
                
                if (!ShouldHandle(message)) return;

                var words = GetCommandString(message);
                var commands = Registry.FindCommands(words[0].ToLower());
                if (commands.Any() && commands.Length > 15)
                    await message.ReplyAsync("thats a lot of commands, be more specific kid");
                else if (commands.Any() && commands.Length > 1)
                    commands = Registry.FindCommands(words[0].ToLower(), true);
                if (!commands.Any() && Registry.UnknownCommand is not null)
                {
                    await Registry.UnknownCommand.Run(new MessageContext
                    {
                        Message = message,
                        Client = Client,
                        Collector = new ArgumentCollector()
                    });
                    return;
                }

                await RunCommand(commands[0], message, SplitWords(string.Join(' ', words)));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        
        public async Task Handle(DiscordClient _, InteractionCreateEventArgs args)
        {
            var interaction = args.Interaction;
            try
            {
                if (interaction.Type is not InteractionType.ApplicationCommand) return;
                await interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder{IsEphemeral = true});
                var command = interaction.Data.Name;
                Logger.LogInformation($"Received Interaction For Slash Command {command}");

                MakeSureDataBaseThingExists(interaction.Channel.Guild);

                var argsList = interaction.Data.Options?.Select(x => x.Value?.ToString()).ToList() ?? new List<string>();

                var commands = Registry.FindCommands(command.ToLower());

                if (commands.Length == 0) Extension.UnknownCommandRun(interaction);
                
                try
                {
                    await RunCommand(commands[0], interaction, SplitWords(string.Join(' ', argsList)));
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);    
                throw;
            }
        }
        
        public async Task Handle(DiscordClient _, ContextMenuInteractionCreateEventArgs interaction)
        {
            var commandName = interaction.Interaction.Data.Name;
            var commands = Registry.FindCommands(commandName.ToLower());
            await interaction.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder{IsEphemeral = true});
            try
            {
                var collector = new ArgumentCollector
                {
                    [commands[0].Arguments[0].Key] = interaction.Type switch
                    {
                        ApplicationCommandType.SlashCommand => throw new Exception("no"),
                        ApplicationCommandType.UserContextMenu => interaction.TargetUser,
                        ApplicationCommandType.MessageContextMenu => interaction.TargetMessage,
                        _ => throw new ArgumentOutOfRangeException(nameof(interaction.Type))
                    }
                };
                await commands[0].Run(new InteractionContext
                {
                    Client = Client,
                    Interaction = interaction.Interaction,
                    Collector = collector
                });
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs interaction)
        {
            await interaction.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            if (ComponentActions.TryGetValue(interaction.Id, out var action))
                action.Invoke(interaction);
        }

        public async Task Handle(DiscordClient _, MessageUpdateEventArgs args)
        {
            var message = args.Message;
            if (!Extension.Options.NonCommandEditable) return;
            try
            {
                MakeSureDataBaseThingExists(message.Channel.Guild);
                
                if (!ShouldHandle(message)) return;

                var words = GetCommandString(message);
                var commands = Registry.FindCommands(words[0].ToLower());

                if (commands.Any() && commands.Length > 15)
                    await message.ReplyAsync("thats a lot of commands, be more specific kid");
                else if (commands.Any() && commands.Length > 1)
                    commands = Registry.FindCommands(words[0].ToLower(), true);
                if (!commands.Any() && Registry.UnknownCommand is not null)
                {
                    await Registry.UnknownCommand.Run(new MessageContext
                    {
                        Message = message,
                        Client = Client,
                        Collector = new ArgumentCollector()
                    });
                    return;
                }

                await RunCommand(commands[0], message, SplitWords(string.Join(' ', words)));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
        
        #region RunCommand

        private async Task RunCommand(Command command, DiscordMessage message, string[] strings)
        {
            var inhibition = MessageInhibitors.Select(inhibitor => inhibitor.Invoke(message, command))
                .FirstOrDefault(result => result != null);
            if (inhibition is not default(Inhibition))
            {
                Extension.CommandBlocked(command, message, inhibition.Reason);
                await message.ReplyAsync(inhibition.Response);
                await command.OnBlock(message, inhibition.Reason);
                return;
            }

            try
            {
                var (canCommandBeUsed, reason) = await command.IsUsable(message);
                if (!canCommandBeUsed)
                {
                    var missingClientPerms =
                        (message.Channel.Guild.Members[Client.CurrentUser.Id].PermissionsIn(message.Channel) ^
                         command.ClientPermissions) & command.ClientPermissions;
                    var missingUserPerms =
                        (message.Channel.Guild.Members[message.Author.Id].PermissionsIn(message.Channel) ^
                         command.UserPermissions) & command.UserPermissions;
                    var throttle = command.GetThrottle(message.Author);
                    var seconds = uint.MinValue;
                    if (throttle is not null)
                        seconds = (uint) (command.ThrottlingOptions.Duration - (DateTime.Now.ConvertToUnixTimestamp() -
                                          throttle.Start.ConvertToUnixTimestamp()));
                    Extension.CommandBlocked(command, message, reason, missingUserPerms, missingClientPerms, seconds);
                    await command.OnBlock(message, reason, missingUserPerms, missingClientPerms, seconds);
                    return;
                }
                var argumentCollector = ArgumentParser.Parse(strings, command, message);
                command.Throttle(message.Author);
                await command.Run(new MessageContext
                {
                    Client = Client, 
                    Message = message, 
                    Collector = argumentCollector
                });
            }
            catch (Exception e)
            {
                if (e is FriendlyException)
                {
                    await message.ReplyAsync(e.Message);
                    return;
                }
                Logger.Error(e);
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = $"Error (`{e.GetType().Name}`) while Running Command `{command.Name}`",
                    Color = new DiscordColor("2F3136"),
                    Timestamp = DateTimeOffset.Now,
                    Footer = new DiscordEmbedBuilder.EmbedFooter{IconUrl = message.Author.AvatarUrl, Text = $"Command ran by {message.Author.Username}#{message.Author.Discriminator}"}
                }.AddField("Error Message", $"`{e.Message}`").AddField("Stack Trace", $"```\n{e.StackTrace![..Math.Min(e.StackTrace.Length - 1, 1000)]}\n```");
                await message.ReplyAsync(embed);
                await message.ReplyAsync(
                    $"here be the support server if you want to join ig: {Extension.Options.Invite}");
            }
        }
        
        private async Task RunCommand(Command command, DiscordInteraction interaction, string[] strings)
        {
            var inhibition = InteractionInhibitors.Select(inhibitor => inhibitor(interaction, command))
                .FirstOrDefault(result => result != null);
            if (inhibition is not default(Inhibition))
            {
                Extension.CommandBlocked(command, interaction, inhibition.Reason);
                await interaction.FollowUpAsync(inhibition.Response);
                await command.OnBlock(interaction, inhibition.Reason);
                return;
            }

            try
            {
                
                var (canCommandBeUsed, reason) = await command.IsUsable(interaction);
                if (!canCommandBeUsed)
                {
                    var missingClientPerms =
                        (interaction.Channel.Guild.Members[Client.CurrentUser.Id].PermissionsIn(interaction.Channel) ^
                         command.ClientPermissions) & command.ClientPermissions;
                    var missingUserPerms =
                        (interaction.Channel.Guild.Members[interaction.User.Id].PermissionsIn(interaction.Channel) ^
                         command.UserPermissions) & command.UserPermissions;
                    var throttle = command.GetThrottle(interaction.User);
                    var seconds = uint.MinValue;
                    if (throttle is not null)
                    {
                        seconds = (uint) (command.ThrottlingOptions.Duration - (DateTime.Now.ConvertToUnixTimestamp() -
                                          throttle.Start.ConvertToUnixTimestamp()));
                    }
                    Extension.CommandBlocked(command, interaction, reason, missingUserPerms, missingClientPerms, seconds);
                    await command.OnBlock(interaction, reason, missingUserPerms, missingClientPerms, seconds);
                    return;
                }
                var argumentCollector = ArgumentParser.Parse(strings, command, interaction);
                command.Throttle(interaction.User);
                await command.Run(new InteractionContext
                {
                    Client = Client,
                    Interaction = interaction,
                    Collector = argumentCollector
                });
            }
            catch (Exception e)
            {
                if (e is FriendlyException)
                {
                    await interaction.FollowUpAsync(e.Message);
                    return;
                }
                Logger.Error(e);
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = $"Error (`{e.GetType().Name}`) while Running Command `{command.Name}`",
                    Color = new DiscordColor("2F3136"),
                    Timestamp = DateTimeOffset.Now,
                    Footer = new DiscordEmbedBuilder.EmbedFooter{IconUrl = interaction.User.AvatarUrl, Text = $"Command ran by {interaction.User.Username}#{interaction.User.Discriminator}"}
                }.AddField("Error Message", $"`{e.Message}`").AddField("Stack Trace", $"```\n{e.StackTrace![..Math.Min(e.StackTrace.Length - 1, 1000)]}\n```");
                await interaction.FollowUpAsync(embed);
                await interaction.FollowUpAsync(
                    $"here be the support server if you want to join ig: {Extension.Options.Invite}");
            }
        }
        
        #endregion

        #endregion
    }

    public class Inhibition
    {
        public string Reason { get; init; }
        public string Response { get; init; }
    }
    public delegate Inhibition Inhibitor<in T, in T2>(T t, T2 t2) where T2 : Command;
}