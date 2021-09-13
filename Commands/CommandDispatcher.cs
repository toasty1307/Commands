using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Providers;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public class CommandDispatcher : CommandsBase
    {
        public CommandRegistry Registry { get; }
        
        public CommandDispatcher(DiscordClient client, CommandRegistry registry) : base(client)
        {
            Registry = registry;
        }

        public List<Inhibitor<DiscordMessage>> MessageInhibitors { get; set; } = new();
        public List<Inhibitor<DiscordInteraction>> InteractionInhibitors { get; set; } = new();
        public Dictionary<string, Action<ComponentInteractionCreateEventArgs>> ComponentActions { get; set; } = new();

        public void AddInhibitor(Inhibitor<DiscordMessage> inhibitor) => MessageInhibitors.Add(inhibitor);
        public void AddInhibitor(Inhibitor<DiscordInteraction> inhibitor) => InteractionInhibitors.Add(inhibitor);

        public async Task<string[]> GetCommandString(DiscordMessage message)
        {
            try
            {
                var prefix = (await Extension.Provider.Get(message.Channel.Guild)).Prefix;
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

        public async Task Handle(DiscordClient _, MessageCreateEventArgs args)
        {
            var message = args.Message;
            try
            {
                var providerIsNull = Extension.Provider is null;
                if (!providerIsNull)
                {
                    try
                    {
                        var helper = await Extension.Provider.Get(message.Channel.Guild);
                        if (helper is null)
                            await Extension.Provider.Set(message.Channel.Guild,
                                new GuildSettingHelper(Client, message.Channel.Guild?.Id ?? 0));
                    }
                    catch (Exception e)
                    {
                        Client.Logger.Error(e);
                    }
                }

                if (!await ShouldHandle(message)) return;

                var words = await GetCommandString(message);
                var commands = Registry.FindCommands(words[0].ToLower());

                switchStart: // cry about it
                switch (commands.Length)
                {
                    case 1:
                        await RunCommand(commands[0], message, words);
                        break;
                    case <= 15 and > 1:
                        commands = Registry.FindCommands(words[0].ToLower(), true);
                        goto switchStart;
                    case > 15:
                        await message.ReplyAsync("Multiple Commands Found, please be More Specific");
                        break;
                    case 0 when Registry.UnknownCommand is not null:
                        Extension.UnknownCommandRun(message);
                        await Registry.UnknownCommand.Run(new CommandContext
                        {
                            Client = Client,
                            Message = message,
                            Collector = new ArgumentCollector()
                        });
                        break;
                }
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
                var providerIsNull = Extension.Provider is null;
                if (!providerIsNull)
                {
                    try
                    {
                        var helper = await Extension.Provider.Get(interaction.Channel.Guild);
                        if (helper is null)
                            await Extension.Provider.Set(interaction.Channel.Guild,
                                new GuildSettingHelper(Client, interaction.Channel.Guild?.Id ?? 0));
                    }
                    catch (Exception e)
                    {
                        Client.Logger.Error(e);
                    }
                }
                
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
        
        private async Task RunCommand(Command command, DiscordMessage message, string[] strings)
        {
            if (MessageInhibitors.Select(inhibitor => inhibitor(message)).Any(result => result != null))
                return;

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
                await command.Run(new CommandContext
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
            if (InteractionInhibitors.Select(inhibitor => inhibitor(interaction)).Any(result => result != null))
                return;

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

        public ArgumentCollector FigureOutCommandArgsIdk(string commandString, Command command, DiscordMessage message)
        {
            if (command.Arguments is null || command.Arguments.Length == 0) return new ArgumentCollector();
            var collector = new ArgumentCollector();
            var words = SplitWords(commandString);
            var inputArgs = words[1..];
            var commandArgs = command.Arguments;
            var notOptionalArgs = commandArgs.Where(x => !x.Optional).ToArray();
            var invalidNumberOfArgs = notOptionalArgs.Length > inputArgs.Length;
            if (inputArgs.Length == 0 && notOptionalArgs.Length == 0) return collector;
            if (invalidNumberOfArgs) throw new FriendlyException("Invalid number of arguments");

            foreach (var commandArg in commandArgs)
            {
                var argString = string.Empty;
                try { argString = commandArg.Infinite ? string.Join(" ", inputArgs[commandArgs.ToList().IndexOf(commandArg)..]) : inputArgs[commandArgs.ToList().IndexOf(commandArg)]; }
                catch { if (commandArg.Optional) break; }
                if (commandArg.OneOf is not null && !commandArg.OneOf.Select(x => x.ToLower()).Contains(argString))
                    throw new FriendlyException($"Argument {commandArg.Key} should be one of `{string.Join(", ", commandArg.OneOf)}`");
                var genericType = commandArg.GetType().GetGenericArguments().First();
                var argumentTypeObject = Registry.GetArgumentType(genericType);
                var argumentTypeObjectType = argumentTypeObject.GetType();
                var validateMethod = argumentTypeObjectType.GetMethod("Validate");
                var validateResult = (bool) validateMethod!.Invoke(argumentTypeObject, new object[] {argString})!;
                if (!validateResult)
                {
                    if (!commandArg.Optional)
                    {
                        Extension.CommandCanceled(command, "INVALID_ARGS", message);
                        throw new FriendlyException($"Invalid Value (`{argString}`) for {commandArg.Key}");
                    }
                    var temp = inputArgs.ToList();
                    temp.Insert(commandArgs.ToList().IndexOf(commandArg) + 1, inputArgs[commandArgs.ToList().IndexOf(commandArg)]);
                    if (commandArg.Default is not null)
                    {
                        var parseMethod0 = argumentTypeObjectType.GetMethod("Parse");
                        var parseResult0 = parseMethod0!.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                        collector[commandArg.Key] = parseResult0;
                    }
                    inputArgs = temp.ToArray();
                    continue;
                }

                var parseMethod = argumentTypeObjectType.GetMethod("Parse");
                var parseResult = parseMethod!.Invoke(argumentTypeObject, new object[] {argString});
                collector[commandArg.Key] = parseResult;
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
                catch { if (commandArg.Optional) break; }
                if (string.IsNullOrEmpty(argString)) continue;
                if (commandArg.OneOf is not null && !commandArg.OneOf.Select(x => x.ToLower()).Contains(argString.ToLower()))
                    throw new FriendlyException($"Argument {commandArg.Key} should be one of `{string.Join(", ", commandArg.OneOf)}`");
                var genericType = commandArg.GetType().GetGenericArguments().First();
                var argumentTypeObject = Registry.GetArgumentType(genericType);
                var argumentTypeObjectType = argumentTypeObject.GetType();
                var validateMethod = argumentTypeObjectType.GetMethod("Validate");
                var validateResult = (bool) validateMethod!.Invoke(argumentTypeObject, new object[] {argString})!;
                if (!validateResult)
                {
                    if (!commandArg.Optional)
                    {
                        Extension.CommandCanceled(command, "INVALID_ARGS", interaction);
                        throw new FriendlyException($"Invalid Value (`{argString}`) for {commandArg.Key}");
                    }
                    var temp = inputArgs.ToList();
                    temp.Insert(words.ToList().IndexOf(argString) + 1, argString);
                    temp.Insert(commandArgs.ToList().IndexOf(commandArg) + 1, inputArgs[commandArgs.ToList().IndexOf(commandArg)]);
                    temp.RemoveAt(temp.IndexOf(argString));
                    inputArgs = temp.ToArray();
                    if (commandArg.Default is not null)
                    {
                        var parseMethod0 = argumentTypeObjectType.GetMethod("Parse");
                        var parseResult0 = parseMethod0!.Invoke(argumentTypeObject, new object[] {commandArg.Default});
                        collector[commandArg.Key] = parseResult0;
                    }
                    
                    continue;
                }

                var parseMethod = argumentTypeObjectType.GetMethod("Parse");
                var parseResult = parseMethod!.Invoke(argumentTypeObject, new object[] {argString});
                collector[commandArg.Key] = parseResult;
                if (commandArg.Infinite) break;
            }

            return collector;
        }

        private readonly Regex _splitWordsRegex = new("(?<=\')[^\']*(?=\')|[^\' ]+");
        public string[] SplitWords(string str)
        {
            var matches = _splitWordsRegex.Matches(str);
            var matchArray = matches.ToArray();
            var matchStrings = matchArray.Select(x => x.Value);
            return matchStrings.ToArray();
        }
        
        public async Task<bool> ShouldHandle(DiscordMessage message)
        {
            if (message.Author.IsBot) return false;
            if (string.IsNullOrWhiteSpace(message.Content)) return false;
            
            // Check for prefix or ping
            var prefix = (await Extension.Provider.Get(message.Channel.Guild)).Prefix;
            if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
            var mentionPrefix = $"<@!{Client.CurrentUser.Id}>";
            var prefixRegex = new Regex($@"^({prefix}|{mentionPrefix})");
            var match = prefixRegex.Match(message.Content.ToLower());
            if (!match.Success && message.Channel.Guild is not null) return false;
            
            return true;
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
                var providerIsNull = Extension.Provider is null;
                if (!providerIsNull)
                {
                    try
                    {
                        var helper = await Extension.Provider.Get(message.Channel.Guild);
                        if (helper is null)
                            await Extension.Provider.Set(message.Channel.Guild,
                                new GuildSettingHelper(Client, message.Channel.Guild?.Id ?? 0));
                    }
                    catch (Exception e)
                    {
                        Client.Logger.Error(e);
                    }
                }

                if (!await ShouldHandle(message)) return;

                var words = await GetCommandString(message);
                var commands = Registry.FindCommands(words[0].ToLower());

                switchStart: // cry about it
                switch (commands.Length)
                {
                    case 1:
                        await RunCommand(commands[0], message, words);
                        break;
                    case <= 15 and > 1:
                        commands = Registry.FindCommands(words[0].ToLower(), true);
                        goto switchStart;
                    case > 15:
                        await message.ReplyAsync("Multiple Commands Found, please be More Specific");
                        break;
                    case 0 when Registry.UnknownCommand is not null:
                        Extension.UnknownCommandRun(message);
                        await Registry.UnknownCommand.Run(new CommandContext
                        {
                            Client = Client,
                            Message = message,
                            Collector = new ArgumentCollector()
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);
            }
        }
    }

    public class Inhibition<T>
    {
        public string Reason { get; set; }
        public T Response { get; set; }
    }
    
    public delegate Inhibition<T> Inhibitor<T>(T t);
}