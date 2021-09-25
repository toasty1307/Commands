using System;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus;
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
            new()
            {
                Key = "CommandOrGroup",
                Types = new []{typeof(Command), typeof(Group)},
                Optional = true
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var command = ctx.GetArg<Command>("CommandOrGroup", out var isCommand);
            var group = ctx.GetArg<Group>("CommandOrGroup", out _);
            var showAll = command is null && group is null;
            if (showAll) await ctx.ReplyAsync(ProcessAllCommands(ctx.Guild, ctx.Author, ctx.Message.Id, ctx.Message.EditedTimestamp));
            else if (isCommand) await ctx.ReplyAsync(ProcessCommand(ctx.Guild, ctx.Author, command));
            else await ctx.ReplyAsync(GetGroupEmbed(group, ctx.Guild, ctx.Author));
        }

        public override async Task Run(InteractionContext ctx)
        {
            var command = ctx.GetArg<Command>("CommandOrGroup", out var isCommand);
            var group = ctx.GetArg<Group>("CommandOrGroup", out _);
            var showAll = command is null && group is null;
            if (showAll) await ctx.FollowUpAsync(ProcessAllCommands(ctx.Guild, ctx.Author, ctx.Interaction.Id, DateTimeOffset.Now));
            else if (isCommand) await ctx.FollowUpAsync(ProcessCommand(ctx.Guild, ctx.Author, command));
            else await ctx.FollowUpAsync(GetGroupEmbed(group, ctx.Guild, ctx.Author));
        }

        private DiscordEmbedBuilder ProcessCommand(DiscordGuild guild, DiscordUser user, Command command)
        {
            try
            {
                var currentUser = Client.CurrentUser;
                var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator} ";
                var prefix = Extension.Provider.Get(guild).Prefix;
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
                        Text = $"Requested by {user.Username}",
                        IconUrl = user.AvatarUrl
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
            
                return embed;
            }
            catch (Exception e)
            {
                Client.Logger.Error(e); 
                throw;
            }
        }
        private DiscordMessageBuilder ProcessAllCommands(DiscordGuild guild, DiscordUser user, ulong id, DateTimeOffset? editedTimeProtectionIdk)
        {
            var groups = Extension.Registry.Groups;
            var currentUser = Client.CurrentUser;
            var privateChannel = guild is null;
            var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator}";
            var prefix = Extension.Provider.Get(guild).Prefix;
            if (string.IsNullOrEmpty(prefix)) prefix = Extension.CommandPrefix;
            var completePrefixString = privateChannel
                ? $"`{mentionPrefix} command`"
                : $"`{mentionPrefix} command` or `{prefix} command`";
            var description = $"To run a command in {(privateChannel ? "any server" : guild.Name)}, use {completePrefixString}. For example, {completePrefixString.Replace("command", "prefix")}\nTo run a command in DMs simply use `command` with no prefix\n\n Use `help <command>` to view detailed information about a specific command.\nUse `help all` to view a list of all commands, not just available ones\n\n\n__**Available Commands in {(privateChannel ? "this DM" : guild.Name)}**__";
            
            var embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Requested by {user.Username}",
                    IconUrl = user.AvatarUrl
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
            var component = new DiscordSelectComponent($"helpCommandSelect_{id}_{editedTimeProtectionIdk}", "Group",
                groups.Select(x => new DiscordSelectComponentOption(x.Name, x.Name)));

            async void Listener(ComponentInteractionCreateEventArgs args)
            {
                var group = Extension.Registry.Groups.First(x => string.Equals(x.Name, args.Values[0], StringComparison.CurrentCultureIgnoreCase));
                await args.Interaction.FollowUpAsync(GetGroupEmbed(group, args.Interaction.Channel.Guild, args.Interaction.User));
            }

            component.AddListener(Listener, Extension.Dispatcher);
            return new DiscordMessageBuilder().AddEmbed(embed).AddComponents(component);
        }
        private DiscordEmbed GetGroupEmbed(Group group, DiscordGuild guild, DiscordUser user)
        {
            var embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Requested by {user.Username}",
                    IconUrl = user.AvatarUrl
                },
                Title = "Help Menu",
                Timestamp = DateTimeOffset.Now,
                Description = $"Group `{group.Name}`\nDescription:`{group.Description}`",
                Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))
            };
            var currentUser = Client.CurrentUser;
            var mentionPrefix = $"@{currentUser.Username}#{currentUser.Discriminator} ";
            var prefix = Extension.Provider.Get(guild).Prefix ?? Extension.CommandPrefix;
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

            return embed;
        }

        public Help(DiscordClient client) : base(client)
        {
        }
    }
}