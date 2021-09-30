using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Data;
using Commands.Types;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Group = Commands.CommandsStuff.Group;

namespace Commands
{
    public class CommandDispatcher : CommandsBase
    {
        #region Fields
        
        private readonly Regex _splitWordsRegex = new("(?<=\')[^\']*(?=\')|[^\' ]+");
        
        public CommandRegistry Registry { get; }
        
        public List<Inhibitor<DiscordMessage, Command>> MessageInhibitors { get; set; } = new();
        
        public List<Inhibitor<DiscordInteraction, Command>> InteractionInhibitors { get; set; } = new();
        
        public Dictionary<string, Action<ComponentInteractionCreateEventArgs>> ComponentActions { get; set; } = new();
        
        #endregion

        #region Constructors
        public CommandDispatcher(DiscordClient client, CommandRegistry registry) : base(client)
        {
            Registry = registry;
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
                Client.Logger.Error(e); throw;
            }
        }
        
        private void MakeSureDataBaseThingExists(DiscordGuild guild)
        {
            var settings = Extension.Provider.Get(guild);
            if (settings is null)
            {
                Extension.Provider.Set(guild, new GuildSettings
                {
                    Prefix = Extension.CommandPrefix,
                    Commands = new Dictionary<Command, bool>(Extension.Registry.Commands.Select(x => new KeyValuePair<Command, bool>(x, true))),
                    Groups = new Dictionary<Group, bool>(Extension.Registry.Groups.Select(x => new KeyValuePair<Group, bool>(x, true))),
                    Id = guild?.Id ?? 0
                }, true);
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

                await RunCommand(commands[0], message, words);
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
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
                Client.Logger.LogInformation($"Received Interaction For Slash Command {command}");

                MakeSureDataBaseThingExists(interaction.Channel.Guild);

                var argsList = interaction.Data.Options?.Select(x => x.Value?.ToString()).ToList() ?? new List<string>();

                var commands = Registry.FindCommands(command.ToLower());

                if (commands.Length == 0) Extension.UnknownCommandRun(interaction);
                
                try
                {
                    await RunCommand(commands[0], interaction, argsList.ToArray());
                }
                catch (Exception e)
                {
                    Client.Logger.Error(e);
                }
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);    
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
                Client.Logger.Error(e);
            }
        }

        public async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs interaction)
        {
            await interaction.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder{IsEphemeral = true});
            if (ComponentActions.ContainsKey(interaction.Id))
                ComponentActions[interaction.Id].Invoke(interaction);
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

                await RunCommand(commands[0], message, words);
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
            }
        }

        #endregion

        #region ArgStuff

        public ArgumentCollector FigureOutCommandArgsIdk(string commandString, Command command, DiscordMessage message)
        {
            if (command.Arguments is null || command.Arguments.Length == 0) return new ArgumentCollector();
            var collector = new ArgumentCollector();
            var words = SplitWords(commandString);
            var inputArgs = words[1..];
            var commandArgs = command.Arguments;
            var notOptionalArgs = commandArgs.Where(x => !x.Optional).ToArray();
            var invalidNumberOfArgs = notOptionalArgs.Length > inputArgs.Length;
            if (inputArgs.Length == 0 && notOptionalArgs.Length == 0 && !commandArgs.Any(x => x.Default is not null)) return collector;
            if (invalidNumberOfArgs) throw new FriendlyException("Invalid number of arguments");

            foreach (var commandArg in commandArgs)
            {
                var argString = string.Empty;
                try { argString = commandArg.Infinite ? string.Join(" ", inputArgs[commandArgs.ToList().IndexOf(commandArg)..]) : inputArgs[commandArgs.ToList().IndexOf(commandArg)]; }
                catch { if (commandArg.Optional && commandArg.Default is null) continue; }

                if (commandArg.Types is null || commandArg.Types.Length == 0) commandArg.Types = new[] {typeof(string)};
                if (commandArg.OneOf is not null && !commandArg.OneOf.Select(x => x.ToLower()).Contains(argString))
                {
                    if (commandArg.Optional)
                    {
                        var temp = inputArgs.ToList();
                        if (temp.Count != 0)
                            temp.Insert(commandArgs.ToList().IndexOf(commandArg) + 1, string.IsNullOrEmpty(argString) ? "_" : argString);
                        if (commandArg.Default is not null)
                        {
                            var argumentTypeObject = Registry.GetArgumentTypeFromReturnType(commandArg.Types[0]);
                            var argumentTypeObjectType = argumentTypeObject.GetType();
                            var parseMethod0 = argumentTypeObjectType.GetMethod("Parse");
                            var parseResult0 = parseMethod0!.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                            collector[commandArg.Key] = parseResult0;
                        }
                        inputArgs = temp.ToArray();
                        continue;
                    }
                    throw new FriendlyException($"Argument {commandArg.Key} should be one of `{string.Join(", ", commandArg.OneOf)}`");
                }
                foreach (var commandArgType in commandArg.Types)
                {
                    var argumentTypeObject = Registry.GetArgumentTypeFromReturnType(commandArgType);
                    var argumentTypeObjectType = argumentTypeObject.GetType();
                    var validateMethod = argumentTypeObjectType.GetMethod(nameof(ArgumentType.Validate));
                    var validateResult = (bool) validateMethod!.Invoke(argumentTypeObject, new object[] {argString})!;
                    if (!validateResult)
                    {
                        if (!commandArg.Optional && commandArgType == commandArg.Types[^1])
                        {
                            Extension.CommandCanceled(command, "INVALID_ARGS", message);
                            throw new FriendlyException($"Invalid Value (`{argString}`) for {commandArg.Key}");
                        }
                        if (commandArgType != commandArg.Types[^1])
                            continue;
                        var temp = inputArgs.ToList();
                        try
                        {
                            temp.Insert(commandArgs.ToList().IndexOf(commandArg) + 1,
                                inputArgs[commandArgs.ToList().IndexOf(commandArg)]);
                        }
                        catch { /* TODO idk */ }

                        if (commandArg.Default is not null)
                        {
                            var parseMethod0 = argumentTypeObjectType.GetMethod(nameof(ArgumentType<string>.Parse));
                            var parseResult0 = parseMethod0!.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                            collector[commandArg.Key] = parseResult0;
                            break;
                        }
                        inputArgs = temp.ToArray();
                        break;
                    }

                    var parseMethod = argumentTypeObjectType.GetMethod(nameof(ArgumentType<string>.Parse));
                    var parseResult = parseMethod!.Invoke(argumentTypeObject, new object[] {argString});
                    var isEmptyMethod = argumentTypeObjectType.GetMethod(nameof(ArgumentType<string>.IsEmpty));
                    var isEmptyResult = isEmptyMethod.Invoke(argumentTypeObject, new[] {parseResult});
                    // TODO prompt that arg is empty?
                    var argIsEmpty = (bool) isEmptyResult!;
                    if (argIsEmpty && commandArg.Default is not null)
                    {
                        var anotherParse = parseMethod.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                        var anotherIsEmpty = isEmptyMethod.Invoke(argumentTypeObject, new[] {anotherParse});
                        var anotherArgIsEmpty = (bool) anotherIsEmpty!;
                        if (anotherArgIsEmpty)
                        {
                            collector[commandArg.Key] = null;
                            throw new FriendlyException("that arg was empty lol <:whynoOOOOOooooooOOO:889008360278605854>");
                        }

                        collector[commandArg.Key] = anotherParse;
                    }
                    else
                        collector[commandArg.Key] = parseResult;
                    break;
                }
                if (commandArg.Infinite) break;
            }

            return collector;
        }

        public ArgumentCollector FigureOutCommandArgsIdk(string[] strings, Command command, DiscordInteraction interaction)
        {
            if (command.Arguments is null || command.Arguments.Length == 0) return new ArgumentCollector();
            var collector = new ArgumentCollector();
            var words = strings;
            var inputArgs = words[..];
            var commandArgs = command.Arguments;
            var notOptionalArgs = commandArgs.Where(x => !x.Optional).ToList();
            var invalidNumberOfArgs = notOptionalArgs.Count > inputArgs.Length;
            if (inputArgs.Length == 0 && notOptionalArgs.Count == 0) return collector;
            if (invalidNumberOfArgs) throw new FriendlyException("Invalid number of arguments");
            var optionalArgs = commandArgs.Where(x => x.Optional).ToList();
            var slashCommandArgs = inputArgs.ToList();
            var repeatCount = (notOptionalArgs.Count + optionalArgs.Count) - slashCommandArgs.Count;
            if (repeatCount > 0)
                slashCommandArgs.AddRange(Enumerable.Repeat<string>(null, (notOptionalArgs.Count + optionalArgs.Count) - slashCommandArgs.Count));
            for (var i = 0; i < command.Arguments.Length; i++)
            {
                var optional = command.Arguments[i].Optional;
                if (!optional) continue;
                var indexInOptionalArgs = optionalArgs.IndexOf(commandArgs[i]);
                var argsAtThatPos = slashCommandArgs[^(optionalArgs.Count - indexInOptionalArgs)];
                slashCommandArgs.Remove(argsAtThatPos);
                slashCommandArgs.Insert(i, argsAtThatPos);
            }

            inputArgs = slashCommandArgs.ToArray();
            foreach (var commandArg in commandArgs)
            {
                var argString = string.Empty;
                try { argString = commandArg.Infinite ? string.Join(" ", inputArgs[commandArgs.ToList().IndexOf(commandArg)..]) : inputArgs[commandArgs.ToList().IndexOf(commandArg)]; }
                catch { if (commandArg.Optional && commandArg.Default is null) continue; }

                if (commandArg.Types is null || commandArg.Types.Length == 0) commandArg.Types = new[] {typeof(string)};
                if (commandArg.OneOf is not null && !commandArg.OneOf.Select(x => x.ToLower()).Contains(argString))
                {
                    if (commandArg.Optional)
                    {
                        var temp = inputArgs.ToList();
                        if (temp.Count != 0)
                            temp.Insert(commandArgs.ToList().IndexOf(commandArg) + 1, string.IsNullOrEmpty(argString) ? "_" : argString);
                        if (commandArg.Default is not null)
                        {
                            var argumentTypeObject = Registry.GetArgumentTypeFromReturnType(commandArg.Types[0]);
                            var argumentTypeObjectType = argumentTypeObject.GetType();
                            var parseMethod0 = argumentTypeObjectType.GetMethod("Parse");
                            var parseResult0 = parseMethod0!.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                            collector[commandArg.Key] = parseResult0;
                        }
                        inputArgs = temp.ToArray();
                        continue;
                    }
                    throw new FriendlyException($"Argument {commandArg.Key} should be one of `{string.Join(", ", commandArg.OneOf)}`");
                }
                foreach (var commandArgType in commandArg.Types)
                {
                    var argumentTypeObject = Registry.GetArgumentTypeFromReturnType(commandArgType);
                    var argumentTypeObjectType = argumentTypeObject.GetType();
                    var validateMethod = argumentTypeObjectType.GetMethod(nameof(ArgumentType.Validate));
                    var validateResult = (bool) validateMethod!.Invoke(argumentTypeObject, new object[] {argString})!;
                    if (!validateResult)
                    {
                        if (!commandArg.Optional && commandArgType == commandArg.Types[^1])
                        {
                            Extension.CommandCanceled(command, "INVALID_ARGS", interaction);
                            throw new FriendlyException($"Invalid Value (`{argString}`) for {commandArg.Key}");
                        }
                        if (commandArgType != commandArg.Types[^1])
                            continue;
                        var temp = inputArgs.ToList();
                        try
                        {
                            temp.Insert(commandArgs.ToList().IndexOf(commandArg) + 1,
                                inputArgs[commandArgs.ToList().IndexOf(commandArg)]);
                        }
                        catch { /* TODO idk */ }

                        if (commandArg.Default is not null)
                        {
                            var parseMethod0 = argumentTypeObjectType.GetMethod(nameof(ArgumentType<string>.Parse));
                            var parseResult0 = parseMethod0!.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                            collector[commandArg.Key] = parseResult0;
                            break;
                        }
                        inputArgs = temp.ToArray();
                        break;
                    }

                    var parseMethod = argumentTypeObjectType.GetMethod(nameof(ArgumentType<string>.Parse));
                    var parseResult = parseMethod!.Invoke(argumentTypeObject, new object[] {argString});
                    var isEmptyMethod = argumentTypeObjectType.GetMethod(nameof(ArgumentType<string>.IsEmpty));
                    var isEmptyResult = isEmptyMethod.Invoke(argumentTypeObject, new[] {parseResult});
                    // TODO prompt that arg is empty?
                    var argIsEmpty = (bool) isEmptyResult!;
                    if (argIsEmpty && commandArg.Default is not null)
                    {
                        var anotherParse = parseMethod.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                        var anotherIsEmpty = isEmptyMethod.Invoke(argumentTypeObject, new[] {anotherParse});
                        var anotherArgIsEmpty = (bool) anotherIsEmpty!;
                        if (anotherArgIsEmpty)
                        {
                            collector[commandArg.Key] = null;
                            throw new FriendlyException("that arg was empty lol <:whynoOOOOOooooooOOO:889008360278605854>");
                        }

                        collector[commandArg.Key] = anotherParse;
                    }
                    else
                        collector[commandArg.Key] = parseResult;
                    break;
                }
                if (commandArg.Infinite) break;
            }

            return collector;
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
                    {
                        seconds = (uint) (command.ThrottlingOptions.Duration - (DateTime.Now.ConvertToUnixTimestamp() -
                                          throttle.Start.ConvertToUnixTimestamp()));
                    }
                    Extension.CommandBlocked(command, message, reason, missingUserPerms, missingClientPerms, seconds);
                    await command.OnBlock(message, reason, missingUserPerms, missingClientPerms, seconds);
                    return;
                }
                var argumentCollector = FigureOutCommandArgsIdk(string.Join(" ", strings), command, message);
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
                Client.Logger.Error(e);
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
                var argumentCollector = FigureOutCommandArgsIdk(strings, command, interaction);
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
                Client.Logger.Error(e);
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