using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public class CommandRegistry : CommandsBase
    {
        public List<Command> Commands { get; } = new();
        public List<Group> Groups { get; } = new();
        public List<ArgumentType> ArgumentTypes { get; } = new();
        public Command UnknownCommand { get; private set; }

        public CommandRegistry(DiscordClient client) : base(client) { }
        
        public T GetArgumentType<T>() where T : ArgumentType
        {
            return ArgumentTypes.First(x => x is T) as T;
        }

        public ArgumentType GetArgumentType(Type type)
        {
            return ArgumentTypes.First(x => x.GetType() == type);
        }
        
        public ArgumentType GetArgumentTypeFromReturnType(Type type)
        {
            return ArgumentTypes.First(x => x.GetType().BaseType!.GetGenericArguments().First() == type || x.GetType() == type);
        }

        public void RegisterCommands(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Command)))
                    continue;
                
                var command = (Command) type.GetConstructor(new []{typeof(DiscordClient)})?.Invoke(new object[]{Client});
                RegisterCommand(command);
            }
        }

        public void RegisterCommands(Command[] commands) =>
            commands.ToList().ForEach(RegisterCommand);

        public void RegisterCommand(Command command)
        {
            var canRegister = CanRegister(command);
            if (canRegister is not null)
            {
                Logger.LogError($"Error registering command: {canRegister}");
                Environment.Exit(1);
            }
            var group = Groups.First(x => string.Equals(x.Name, command.GroupName, StringComparison.CurrentCultureIgnoreCase));
            command.Group = group;
            group.Commands.Add(command);
            if (command.Unknown) UnknownCommand = command;
            Commands.Add(command);
            Extension.CommandRegistered(command);
            Command.Commands.Add(command);
            Logger.LogInformation($"Registered Command {command.Name}");
        }

        public ApplicationCommandOptionType GetApplicationCommandOptionTypeFromArgumentType(ArgumentType type)
        {
            var genericParam = type.GetType().BaseType!.GenericTypeArguments[0];
            if (genericParam == typeof(bool) || genericParam == typeof(bool?)) return ApplicationCommandOptionType.Boolean;
            if (genericParam == typeof(DiscordChannel)) return ApplicationCommandOptionType.Channel;
            if (genericParam == typeof(int) || genericParam == typeof(uint)) return ApplicationCommandOptionType.Integer;
            if (genericParam == typeof(DiscordUser) || genericParam == typeof(DiscordRole)) return ApplicationCommandOptionType.Mentionable;
            if (genericParam == typeof(double)) return ApplicationCommandOptionType.Number;
            if (genericParam == typeof(DiscordRole)) return ApplicationCommandOptionType.Role;
            if (genericParam == typeof(string)) return ApplicationCommandOptionType.String;
            if (genericParam == typeof(DiscordUser)) return ApplicationCommandOptionType.User;
            if (genericParam == typeof(DiscordApplicationCommand) || genericParam == typeof(Command)) return ApplicationCommandOptionType.String;
            return ApplicationCommandOptionType.String;
        }

        public async Task RegisterSlashCommands(Command[] commands, DiscordGuild guild)
        {
            foreach (var command in commands.Where(x => !x.Hidden && x.RegisterSlashCommand))
            {
                var slashCommandArguments = new List<DiscordApplicationCommandOption>();
                if (command.Arguments is not null)
                {
                    var optionalArgs = command.Arguments.Where(x => x.Optional).ToList();
                    var nonOptionalArgs = command.Arguments.Where(x => !x.Optional).ToList();
                    var commandArguments = nonOptionalArgs.ToList();
                    commandArguments.AddRange(optionalArgs);
                    foreach (var commandArgument in commandArguments)
                    {
                        var oneOf = new List<DiscordApplicationCommandOptionChoice>();
                        if (commandArgument.OneOf is not null)
                        {
                            foreach (var oneOfArg in commandArgument.OneOf)
                            {
                                oneOf.Add(
                                    new DiscordApplicationCommandOptionChoice(oneOfArg!.ToLower(),
                                        oneOfArg));
                            }
                        }

                        if (commandArgument.OneOf is null && commandArgument.Types is not null && (commandArgument.Types[0] ?? typeof(string)) ==
                            typeof(CommandArgumentType))
                        {
                            oneOf.AddRange(Commands.Where(x => !x.Hidden).Select(x => new DiscordApplicationCommandOptionChoice(x.Name.ToString(), x.Name)));
                        }
                        if (commandArgument.OneOf is null && commandArgument.Types is not null && (commandArgument.Types[0] ?? typeof(string)) ==
                            typeof(GroupArgumentType))
                        {
                            oneOf.AddRange(Groups.Select(x => new DiscordApplicationCommandOptionChoice(x.Name.ToLower(), x.Name)));
                        }

                        if (oneOf.Count == 0 && commandArgument.Types is not null) 
                            slashCommandArguments.Add(new DiscordApplicationCommandOption(commandArgument.Key.ToLower(),
                            commandArgument.Description ?? "yet to add description",
                            GetApplicationCommandOptionTypeFromArgumentType(GetArgumentTypeFromReturnType(commandArgument.Types[0] ?? typeof(string))), !commandArgument.Optional));
                        else if (commandArgument.Types is not null)
                            slashCommandArguments.Add(new DiscordApplicationCommandOption(commandArgument.Key.ToLower(),
                                commandArgument.Description ?? "yet to add description",
                                GetApplicationCommandOptionTypeFromArgumentType(GetArgumentTypeFromReturnType(commandArgument.Types[0] ?? typeof(string))), !commandArgument.Optional, oneOf));
                    }
                }

                var slashCommand = slashCommandArguments.Count != 0 ? 
                    new DiscordApplicationCommand(command.Name.ToLower(), command.Description ?? "no description lolxd", slashCommandArguments, true) :
                    new DiscordApplicationCommand(command.Name.ToLower(), command.Description ?? "no description lolxd", defaultPermission:true);
                await guild.CreateApplicationCommandAsync(slashCommand);
                if (command.Arguments?[0].Types != null && command.Arguments[0].Types[0] == typeof(DiscordUser))
                {
                    slashCommand = new DiscordApplicationCommand(command.Name.ToLower(), null, null, true, ApplicationCommandType.UserContextMenu);
                    await guild.CreateApplicationCommandAsync(slashCommand);
                }
                else if (command.Arguments?[0].Types != null && command.Arguments[0].Types[0] == typeof(DiscordMessage))
                {
                    slashCommand = new DiscordApplicationCommand(command.Name.ToLower(), null, null, true, ApplicationCommandType.MessageContextMenu);
                    await guild.CreateApplicationCommandAsync(slashCommand);
                }

                await Task.Delay(500);
            }
        }

        public string CanRegister(Command command)
        {
            if (Commands.Any(x =>
                x.Name == command.Name || (x.Aliases is not null && x.Aliases.Contains(command.Name)) ||
                (command.Aliases is not null & x.Aliases is not null && command.Aliases.Intersect(x.Aliases).Any())))
                return $"A command with the same name/alias {command.Name} is already registered";
            if (!Groups.Any(x => string.Equals(x.Name, command.GroupName, StringComparison.CurrentCultureIgnoreCase)))
                return $"Group {command.GroupName} is not registered";
            if (Groups.First(x => string.Equals(x.Name, command.GroupName, StringComparison.CurrentCultureIgnoreCase))
                .Commands.Any(x => x.MemberName == command.MemberName))
                return $"A command with the member name {command.MemberName} is already registered in the group";
            if (command.Unknown && UnknownCommand is not null) return "A Unknown command is already registered";
            return null;
        }
        
        public void RegisterArgumentTypes(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<ArgumentTypeAttribute>();

                if (attribute == null) continue;
                if (!type.IsSubclassOf(typeof(ArgumentType)))
                    throw new InvalidOperationException($"Type {type.FullName} has {nameof(ArgumentTypeAttribute)} but doesn't extend {nameof(ArgumentType)}.");

                var argumentType = (ArgumentType) type.GetConstructor(new []{typeof(DiscordClient)})?.Invoke(new object[]{Client});
                RegisterArgumentType(argumentType);
            }
        }

        public void RegisterArgumentTypes(IEnumerable<ArgumentType> argumentTypes) =>
            argumentTypes.ToList().ForEach(RegisterArgumentType);
        public void RegisterArgumentType(ArgumentType type)
        {
            var canRegister = CanRegister(type);
            if (canRegister is not null)
            {
                Logger.LogError($"Error registering Argument Type: {canRegister}");
                Environment.Exit(1);
            }

            Extension.TypeRegistered(type);
            Logger.LogInformation($"Registered Argument type {type}");
            ArgumentTypes.Add(type);
        }

        private string CanRegister(ArgumentType type) => ArgumentTypes.Any(x =>
            x.GetType() == type.GetType()) 
            ? $"An argument type with the type {type} is already registered" : null;

        public void RegisterGroups(Group[] groups) =>
            groups.ToList().ForEach(RegisterGroup);

        public void RegisterGroup(Group group)
        {
            var canRegister = CanRegister(group);
            if (canRegister is not null)
            {
                Logger.LogError($"Error registering group: {canRegister}");
                Environment.Exit(1);
            }

            if (Groups.Count != 0)
                group.Id = Groups[^1].Id + 1;
            else
                group.Id = 0;
            Extension.GroupRegistered(group);
            Group.Groups.Add(group);
            Logger.LogInformation($"Registered group {group.Name}");
            Groups.Add(group);
        }

        private string CanRegister(Group group) => Groups.Any(x => x.Name == @group.Name) ? $"A group with the name {@group.Name} is already registered" : null;

        public void RegisterDefaults()
        {
            RegisterArgumentTypes(GetType().Assembly);
            RegisterGroups(new []{ new Group{Name = "Utils", Description = "Util Commands"}, new Group{Name = "Commands", Guarded = true, Description = "drink milk for strong bones"} });
#if DEBUG
            RegisterGroup(new Group{Name = "Debug", Description = "debug stuff"});   
#endif
            RegisterCommands(GetType().Assembly);
        }

        public Command[] FindCommands(string search, bool exact = false)
        {
            Func<Command, string, bool> searchFunc;
            if (exact)
                searchFunc = (command, searchStr) =>
                    command.Name.ToLower() == searchStr ||
                    command.Aliases is not null && command.Aliases.Select(x => x.ToLower()).ToList().Contains(searchStr) ||
                    $"{command.Group.Id}:{command.MemberName}".ToLower() == searchStr;
            else
                searchFunc = (command, searchStr) =>
                    command.Name.ToLower().Contains(searchStr) ||
                    command.Aliases is not null && command.Aliases.ToList().Any(x => x.ToLower().Contains(searchStr)) ||
                    $"{command.Group.Id}:{command.MemberName}".ToLower() == searchStr;
            
            return Commands.Where(x => !x.Hidden && searchFunc(x, search.ToLower())).ToArray();

        }
    }
}