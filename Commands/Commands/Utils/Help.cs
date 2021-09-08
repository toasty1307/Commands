using System;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Commands.Commands.Utils
{
    public class Help : Command
    {
        public override string GroupName => "Utils";
        public override string Description => "just your everyday help command";

        public override string[] Examples => new[]
        {
            "help",
            "help prefix"
        };

        public override bool Guarded => true;

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<CommandArgumentType>
            {
                Key = "Command",
                Optional = true
            }
        };

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var command = collector.Get<Command>("Command");
            var showAll = command is null;
            if (showAll) return await ProcessAllCommands(message);
            return await ProcessCommand(message, command);
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector collector)
        {
            var command = collector.Get<Command>("Command");
            var showAll = command is null;
            if (showAll) await ProcessAllCommands(interaction);
            else await ProcessCommand(interaction, command);
        }

        private async Task<DiscordMessage[]> ProcessCommand(DiscordMessage message, Command command)
        {
            try
            {
                var currentUser = Client.CurrentUser;
                var guild = message.Channel.Guild;
                var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator} ";
                var prefix = (await Extension.Provider.Get(guild)).Prefix;
                if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
                var format = $"`{prefix}{command.Name}";
                if (command.Arguments is not null)
                {
                    foreach (var commandArgument in command.Arguments)
                    {
                        if (commandArgument.Optional)
                            format += $" [{commandArgument.Key}]";
                        else
                            format += $" <{commandArgument.Key}>";
                    }
                }

                format += $"` or {format.Replace(prefix, mentionPrefix)}`";
                var embed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Requested by {message.Author.Username}",
                        IconUrl = message.Author.AvatarUrl
                    },
                    Title = "Help Menu",
                    Timestamp = DateTimeOffset.Now,
                    Description = $"Command **{command.Name}**: {command.Description}\n\n" +
                                  $"**Format**: {format}\n" +
                                  $"{(command.Aliases is not null ? $"**Aliases**: {string.Join(", ", command.Aliases)}\n" : "")}" +
                                  $"**Group**: {command.Group.Name}(`{command.Group.Id}:{command.MemberName}`)\n" +
                                  $"{(command.DetailedDescription is not null ? $"**Details**: {command.DetailedDescription}\n" : "")}" +
                                  $"{(command.Examples is not null ? $"**Examples**:\n{string.Join("\n", command.Examples)}\n" : "")}",
                    Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))
                };
            
                var helpMessage = await message.ReplyAsync(embed);
                return new[] {helpMessage};
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private async Task ProcessCommand(DiscordInteraction interaction, Command command)
        {
            try
            {
                var currentUser = Client.CurrentUser;
                var guild = interaction.Channel.Guild;
                var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator} ";
                var prefix = (await Extension.Provider.Get(guild)).Prefix;
                if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
                var format = $"`{prefix}{command.Name}";
                if (command.Arguments is not null)
                {
                    foreach (var commandArgument in command.Arguments)
                    {
                        if (commandArgument.Optional)
                            format += $" [{commandArgument.Key}]";
                        else
                            format += $" <{commandArgument.Key}>";
                    }
                }

                format += $"` or {format.Replace(prefix, mentionPrefix)}`";
                var embed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Requested by {interaction.User.Username}",
                        IconUrl = interaction.User.AvatarUrl
                    },
                    Title = "Help Menu",
                    Timestamp = DateTimeOffset.Now,
                    Description = $"Command **{command.Name}**: {command.Description}\n\n" +
                                  $"**Format**: {format}\n" +
                                  $"{(command.Aliases is not null ? $"**Aliases**: {string.Join(", ", command.Aliases)}\n" : "")}" +
                                  $"**Group**: {command.Group.Name}(`{command.Group.Id}:{command.MemberName}`)\n" +
                                  $"{(command.DetailedDescription is not null ? $"**Details**: {command.DetailedDescription}\n" : "")}" +
                                  $"{(command.Examples is not null ? $"**Examples**:\n{string.Join("\n", command.Examples)}\n" : "")}",
                    Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))
                };
            
                await interaction.FollowUpAsync(embed);
            }
            catch (Exception e)
            {
                Client.Logger.Error(e);                throw;
            }
        }
        private async Task ProcessAllCommands(DiscordInteraction interaction)
        {
            var groups = Extension.Registry.Groups;
            var currentUser = Client.CurrentUser;
            var guild = interaction.Channel.Guild;
            var privateChannel = guild is null;
            var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator}";
            var prefix = (await Extension.Provider.Get(guild)).Prefix;
            if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
            var completePrefixString = privateChannel
                ? $"`{mentionPrefix} command`"
                : $"`{mentionPrefix} command` or `{prefix} command`";
            var description = $"To run a command in {(privateChannel ? "any server" : guild.Name)}, use {completePrefixString}. For example, {completePrefixString.Replace("command", "prefix")}\nTo run a command in DMs simply use `command` with no prefix\n\n Use `help <command>` to view detailed information about a specific command.\nUse `help all` to view a list of all commands, not just available ones\n\n\n__**Available Commands in {(privateChannel ? "this DM" : guild.Name)}**__";
            
            var embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Requested by {interaction.User.Username}",
                    IconUrl = interaction.User.AvatarUrl
                },
                Title = "Help Menu",
                Timestamp = DateTimeOffset.Now,
                Description = description,
                Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))
            };
            foreach (var group in groups)
            {
                var commandInfo = string.Join("\n", group.Commands.Where(x => !x.Hidden).Select(x => $"**{x.Name}**: {x.Description}").ToArray());
                if (string.IsNullOrWhiteSpace(commandInfo)) continue;
                embed.AddField(group.Name, commandInfo, true);
            }
            var component = new DiscordSelectComponent("helpCommandSelect", "Group",
                groups.Select(x => new DiscordSelectComponentOption(x.Name, x.Name)));
            component.AddListener(SendGroupEmbed);
            await interaction.FollowUpAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(component));
        }
        private async Task<DiscordMessage[]> ProcessAllCommands(DiscordMessage message)
        {
            var groups = Extension.Registry.Groups;
            var currentUser = Client.CurrentUser;
            var guild = message.Channel.Guild;
            var privateChannel = guild is null;
            var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator}";
            var prefix = (await Extension.Provider.Get(guild)).Prefix;
            if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
            var completePrefixString = privateChannel
                ? $"`{mentionPrefix} command`"
                : $"`{mentionPrefix} command` or `{prefix} command`";
            var description = $"To run a command in {(privateChannel ? "any server" : guild.Name)}, use {completePrefixString}. For example, {completePrefixString.Replace("command", "prefix")}\nTo run a command in DMs simply use `command` with no prefix\n\n Use `help <command>` to view detailed information about a specific command.\nUse `help all` to view a list of all commands, not just available ones\n\n\n__**Available Commands in {(privateChannel ? "this DM" : guild.Name)}**__";
            
            var embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Requested by {message.Author.Username}",
                    IconUrl = message.Author.AvatarUrl
                },
                Title = "Help Menu",
                Timestamp = DateTimeOffset.Now,
                Description = description,
                Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))
            };
            foreach (var group in groups)
            {
                var commandInfo = string.Join("\n", group.Commands.Where(x => !x.Hidden).Select(x => $"**{x.Name}**: {x.Description}").ToArray());
                if (string.IsNullOrWhiteSpace(commandInfo)) continue;
                embed.AddField(group.Name, commandInfo, true);
            }

            var component = new DiscordSelectComponent("helpCommandSelect", "Group",
                groups.Select(x => new DiscordSelectComponentOption(x.Name, x.Name)));
            component.AddListener(SendGroupEmbed);
            var helpMessage = await message.ReplyAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(component));
            return new[] {helpMessage};
        }

        public async void SendGroupEmbed(ComponentInteractionCreateEventArgs args)
        {
            var group = Extension.Registry.Groups.First(x =>
                string.Equals(x.Name, args.Values[0], StringComparison.CurrentCultureIgnoreCase));
            var embed = new DiscordEmbedBuilder()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Requested by {args.Interaction.User.Username}",
                    IconUrl = args.Interaction.User.AvatarUrl
                },
                Title = "Help Menu",
                Timestamp = DateTimeOffset.Now,
                Description = $"Group `{group.Name}`",
                Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))
            };
            var currentUser = Client.CurrentUser;
            var guild = args.Interaction.Channel.Guild;
            var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator} ";
            var prefix = (await Extension.Provider.Get(guild)).Prefix ?? Extension.CommandPrefix;
            foreach (var command in group.Commands)
            {
                var format = $"`{prefix}{command.Name}";
                if (command.Arguments is not null)
                {
                    foreach (var commandArgument in command.Arguments)
                    {
                        if (commandArgument.Optional)
                            format += $" [{commandArgument.Key}]";
                        else
                            format += $" <{commandArgument.Key}>";
                    }
                }

                format += $"` or {format.Replace(prefix, mentionPrefix)}`";
                embed.AddField(command.Name, $"Command **{command.Name}**: {command.Description}\n\n" +
                                             $"**Format**: {format}\n" +
                                             $"{(command.Aliases is not null ? $"**Aliases**: {string.Join(", ", command.Aliases)}\n" : "")}" +
                                             $"**Group**: {command.Group.Name}(`{command.Group.Id}:{command.MemberName}`)\n" +
                                             $"{(command.DetailedDescription is not null ? $"**Details**: {command.DetailedDescription}\n" : "")}" +
                                             $"{(command.Examples is not null ? $"**Examples**:\n{string.Join("\n", command.Examples)}\n" : "")}");
            }

            await args.Interaction.FollowUpAsync(embed);
        }
    }
}