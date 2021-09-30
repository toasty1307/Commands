using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Commands.Commands.Utils
{
    public class Poll : Command
    {
        public override string GroupName => "Utils";
        public override string Description => "make a poll idk";

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "Description"
            },
            new()
            {
                Key = "Choice1",
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice2",
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice3",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice4",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice5",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice6",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice7",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice8",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice9",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice10",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice11",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice12",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice13",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice14",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice15",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice16",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice17",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice18",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice19",
                Optional = true,
                Types = new []{typeof(string)}
            },
            new()
            {
                Key = "Choice20",
                Optional = true,
                Types = new []{typeof(string)}
            },
        };

        public override async Task Run(MessageContext ctx)
        {
            var choices = Arguments.Select(x => ctx.GetArg<string>(x.Key)).ToArray()[1..].Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var description = ctx.GetArg<string>("Description");
            var votes = Enumerable.Repeat(0, choices.Count).ToList();
            var pollData = new PollData
            {
                Choices = choices,
                Votes = votes,
                Voters = votes.Select(_ => Array.Empty<DiscordUser>()).ToList()
            };
            var actionsRowsRequired = choices.Count % 5 == 0 ? choices.Count / 5 : choices.Count / 5 + 1;
            var actionsRows = new List<DiscordActionRowComponent>();
            for (var i = 0; i < actionsRowsRequired; i++)
            {
                var numberOfButtonsInThisRow = i == actionsRowsRequired - 1 ? choices.Count - i * 5 : 5;
                var buttons = new List<DiscordButtonComponent>();
                for (var j = 0; j < numberOfButtonsInThisRow; j++)
                {
                    var button = new DiscordButtonComponent(ButtonStyle.Primary, $"pollbutton_{choices[i * 5 + j]}_{i * 5 + j}_{ctx.Message.Id}_{ctx.Message.EditedTimestamp}", choices[i * 5 + j]);
                    var j1 = j;
                    var i1 = i;

                    async void Listener(ComponentInteractionCreateEventArgs args)
                    {
                        var result = pollData.VoteFor(choices[i1 * 5 + j1], args.Message.Author);
                        switch (result)
                        {
                            case 0:
                                await args.Interaction.FollowUpAsync("You have already voted for something, might wanna _unvote_ that idk");
                                break;
                            case -1:
                                await args.Interaction.FollowUpAsync("_unvoted_ xd");
                                break;
                            case 1:
                                await args.Interaction.FollowUpAsync("yes milk");
                                break;
                        }

                        if (result == 0) return;
                        var embed = new DiscordEmbedBuilder {Title = description, Footer = new DiscordEmbedBuilder.EmbedFooter {IconUrl = ctx.Author.AvatarUrl, Text = $"{ctx.Author.Username}#{ctx.Author.Discriminator}"}, Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))};
                        foreach (var choice in choices)
                        {
                            embed.AddField(choice, pollData.Votes[pollData.Choices.IndexOf(choice)].ToString(), true);
                        }

                        await args.Message.ModifyAsync(x => x.AddEmbed(embed).AddComponents(args.Message.Components));
                    }

                    button.AddListener(Listener, ctx.Extension.Dispatcher);
                    buttons.Add(button);
                }

                var actionRow = new DiscordActionRowComponent(buttons);
                actionsRows.Add(actionRow);
            }

            var endPollButton = new DiscordButtonComponent(ButtonStyle.Danger, $"pollclose_{ctx.Message.Id}_{ctx.Message.EditedTimestamp}", "End Poll");
            async void EndPollListener(ComponentInteractionCreateEventArgs args)
            {
                var embed = new DiscordEmbedBuilder {Title = description, Footer = new DiscordEmbedBuilder.EmbedFooter {IconUrl = ctx.Author.AvatarUrl, Text = $"{ctx.Author.Username}#{ctx.Author.Discriminator}"}, Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))};
                foreach (var choice in choices)
                {
                    embed.AddField(choice, pollData.Votes[pollData.Choices.IndexOf(choice)].ToString(), true);
                }

                await args.Message.ModifyAsync(x => x.WithEmbed(embed));
                await args.Message.Channel.SendMessageAsync(embed);
            }
            
            endPollButton.AddListener(EndPollListener, ctx.Extension.Dispatcher);
            actionsRows.Add(new DiscordActionRowComponent(new []{endPollButton}));

            var embed = new DiscordEmbedBuilder
            {
                Title = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter{IconUrl = ctx.Author.AvatarUrl, Text = $"{ctx.Author.Username}#{ctx.Author.Discriminator}"}
            };
            foreach (var choice in choices)
            {
                embed.AddField(choice, "0", true);
            }
            
            await ctx.ReplyAsync(new DiscordMessageBuilder().AddComponents(actionsRows).AddEmbed(embed));
        }

        public override async Task Run(InteractionContext ctx)
        {
            var choices = Arguments.Select(x => ctx.GetArg<string>(x.Key)).ToArray()[1..].Where(x => !string.IsNullOrEmpty(x)).ToList();
            var description = ctx.GetArg<string>("Description");
            var votes = Enumerable.Repeat(0, choices.Count).ToList();
            var pollData = new PollData
            {
                Choices = choices,
                Votes = votes,
                Voters = votes.Select(_ => Array.Empty<DiscordUser>()).ToList()
            };
            var actionsRowsRequired = choices.Count % 5 == 0 ? choices.Count / 5 : choices.Count / 5 + 1;
            var actionsRows = new List<DiscordActionRowComponent>();
            for (var i = 0; i < actionsRowsRequired; i++)
            {
                var numberOfButtonsInThisRow = i == actionsRowsRequired - 1 ? choices.Count - i * 5 : 5;
                var buttons = new List<DiscordButtonComponent>();
                for (var j = 0; j < numberOfButtonsInThisRow; j++)
                {
                    var button = new DiscordButtonComponent(ButtonStyle.Primary, $"pollbutton_{choices[i * 5 + j]}_{i * 5 + j}_{ctx.Interaction.Id}_{DateTime.Now}", choices[i * 5 + j]);
                    var j1 = j;
                    var i1 = i;

                    async void Listener(ComponentInteractionCreateEventArgs args)
                    {
                        var result = pollData.VoteFor(choices[i1 * 5 + j1], args.Message.Author);
                        switch (result)
                        {
                            case 0:
                                await args.Interaction.FollowUpAsync("You have already voted for something, might wanna _unvote_ that idk");
                                break;
                            case -1:
                                await args.Interaction.FollowUpAsync("_unvoted_ xd");
                                break;
                            case 1:
                                await args.Interaction.FollowUpAsync("yes milk");
                                break;
                        }

                        if (result == 0) return;
                        var embed = new DiscordEmbedBuilder {Title = description, Footer = new DiscordEmbedBuilder.EmbedFooter {IconUrl = ctx.Author.AvatarUrl, Text = $"{ctx.Author.Username}#{ctx.Author.Discriminator}"}, Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))};
                        foreach (var choice in choices)
                        {
                            embed.AddField(choice, pollData.Votes[pollData.Choices.IndexOf(choice)].ToString(), true);
                        }

                        await args.Message.ModifyAsync(x => x.AddEmbed(embed).AddComponents(args.Message.Components));
                    }

                    button.AddListener(Listener, ctx.Extension.Dispatcher);
                    buttons.Add(button);
                }

                var actionRow = new DiscordActionRowComponent(buttons);
                actionsRows.Add(actionRow);
            }

            var endPollButton = new DiscordButtonComponent(ButtonStyle.Danger, $"pollclose_{ctx.Interaction.Id}_{DateTime.Now}", "End Poll");

            async void EndPollListener(ComponentInteractionCreateEventArgs args)
            {
                var embed = new DiscordEmbedBuilder {Title = description, Footer = new DiscordEmbedBuilder.EmbedFooter {IconUrl = ctx.Author.AvatarUrl, Text = $"{ctx.Author.Username}#{ctx.Author.Discriminator}"}, Color = new Optional<DiscordColor>(new DiscordColor("2F3136"))};
                foreach (var choice in choices)
                {
                    embed.AddField(choice, pollData.Votes[pollData.Choices.IndexOf(choice)].ToString(), true);
                }

                await args.Message.ModifyAsync(x => x.WithEmbed(embed));
                await args.Message.Channel.SendMessageAsync(embed);
                await args.Interaction.FollowUpAsync("killed the pol");
            }
            
            endPollButton.AddListener(EndPollListener, ctx.Extension.Dispatcher);
            actionsRows.Add(new DiscordActionRowComponent(new []{endPollButton}));

            var embed = new DiscordEmbedBuilder
            {
                Title = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter{IconUrl = ctx.Author.AvatarUrl, Text = $"{ctx.Author.Username}#{ctx.Author.Discriminator}"}
            };
            foreach (var choice in choices)
            {
                embed.AddField(choice, "0", true);
            }

            await ctx.ReplyAsync("Sent a new poll");
            await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(actionsRows).AddEmbed(embed));
        }

        public Poll(DiscordClient client) : base(client)
        {
        }
    }

    public class PollData
    {
        public List<string> Choices { get; init; } = new();
        public List<int> Votes { get; init; } = new();
        public List<DiscordUser[]> Voters { get; init; } = new();

        public int VoteFor(string choice, DiscordUser user)
        {
            var voters = Voters[Choices.IndexOf(choice)];
            if (voters.Contains(user))
            {
                Votes[Voters.IndexOf(voters)]--;
                var temp = Voters[Choices.IndexOf(choice)].ToList();
                temp.Remove(user);
                Voters[Choices.IndexOf(choice)] = temp.ToArray();
                return -1;
            }

            var userHasVotedForAnyOption = Voters.SelectMany(x => x).ToArray().Contains(user);
            if (userHasVotedForAnyOption)
                return 0;
            Votes[Choices.IndexOf(choice)]++;
            var temp2 = Voters[Choices.IndexOf(choice)].ToList();
            temp2.Add(user);
            Voters[Choices.IndexOf(choice)] = temp2.ToArray();
            return 1;
        }
    }
}