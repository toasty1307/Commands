using System;
using System.Collections.Generic;
using System.Linq;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Commands.CoolStuff.Pagination
{
    public class PaginatedMessage
    {
        private readonly CommandDispatcher _dispatcher;
        private readonly DiscordUser _userThatHasPerms;
        private readonly List<Page> _pages;
        private readonly DiscordMessageBuilder _messageBuilder;
        private readonly bool _wrapAround;
        private int _currentPage;

        public PaginatedMessage(List<Page> pages, CommandDispatcher dispatcher, DiscordUser userThatHasPerms, bool wrapAround = true)
        {
            if (pages.Count == 0) throw new ArgumentException("imagine pages but 0", nameof(pages));
            _dispatcher = dispatcher;
            _userThatHasPerms = userThatHasPerms;
            _wrapAround = wrapAround;
            _pages = pages = pages.Where(x => x.Content is not null || x.Embed is not null).ToList();
            var builder = new DiscordMessageBuilder();
            var firstButton = new DiscordButtonComponent(ButtonStyle.Primary,
                Guid.NewGuid() + "_pagination_first", string.Empty, !wrapAround,
                new DiscordComponentEmoji("⏮"));
            var backButton = new DiscordButtonComponent(ButtonStyle.Primary,
                Guid.NewGuid() + "_pagination_back", string.Empty, !wrapAround,
                new DiscordComponentEmoji("⏪"));
            var stopButton = new DiscordButtonComponent(ButtonStyle.Primary,
                Guid.NewGuid() + "_pagination_stop", string.Empty, false,
                new DiscordComponentEmoji("⏹"));
            var nextButton = new DiscordButtonComponent(ButtonStyle.Primary,
                Guid.NewGuid() + "_pagination_next", string.Empty, pages.Count < 2 && !wrapAround,
                new DiscordComponentEmoji("⏩"));
            var lastButton = new DiscordButtonComponent(ButtonStyle.Primary,
                Guid.NewGuid() + "_pagination_last", string.Empty, pages.Count < 2 && !wrapAround,
                new DiscordComponentEmoji("⏭"));
            stopButton.AddListener(StopButtonClicked, dispatcher);
            nextButton.AddListener(NextButtonClicked, dispatcher);
            lastButton.AddListener(LastButtonClicked, dispatcher);
            firstButton.AddListener(FirstButtonClicked, dispatcher);
            backButton.AddListener(BackButtonClicked, dispatcher);
            builder.AddComponents(firstButton, backButton, stopButton, nextButton, lastButton);
            builder.WithContent($"Page 1 of {pages.Count}\n{pages.First().Content}");
            builder.WithEmbed(pages.First().Embed);
            _currentPage = 0;
            _messageBuilder = builder;
        }

        private async void FirstButtonClicked(ComponentInteractionCreateEventArgs obj)
        {
            if (obj.User != _userThatHasPerms) return;
            var components = obj.Message.Components.SelectMany(x => x.Components)
                .Where(x => x.CustomId.Contains("pagination"))
                .Select(x => (DiscordButtonComponent) x)
                .ToList();
            if (!_wrapAround)
            {
                components[0].Disable();
                components[1].Disable();
                components[3].Enable();
                components[4].Enable();
            }

            _currentPage = 0;
            if ((obj.Message.Flags & MessageFlags.Ephemeral) != 0)
                await obj.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .AddEmbed(_pages[_currentPage].Embed).AddComponents(components));
            else
                await obj.Message.ModifyAsync(x =>
                    x.WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .WithEmbed(_pages[_currentPage].Embed).AddComponents(components));
        }
        
        private async void BackButtonClicked(ComponentInteractionCreateEventArgs obj)
        {
            if (obj.User != _userThatHasPerms) return;
            var components = obj.Message.Components.SelectMany(x => x.Components)
                .Where(x => x.CustomId.Contains("pagination"))
                .Select(x => (DiscordButtonComponent) x)
                .ToList();
            
            _currentPage = ((_currentPage--) % _pages.Count) - 1;
            if (_currentPage < 0)
                _currentPage = _pages.Count - Math.Abs(_currentPage);
            
            if (_currentPage == 0 && !_wrapAround)
            {
                components[0].Disable();
                components[1].Disable();
            }
            components[3].Enable();
            components[4].Enable();
            if ((obj.Message.Flags & MessageFlags.Ephemeral) != 0)
                await obj.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .AddEmbed(_pages[_currentPage].Embed).AddComponents(components));
            else
                await obj.Message.ModifyAsync(x =>
                    x.WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .WithEmbed(_pages[_currentPage].Embed).AddComponents(components));
        }
        
        private async void StopButtonClicked(ComponentInteractionCreateEventArgs obj)
        {
            if (obj.User != _userThatHasPerms) return;
            var components = obj.Message.Components.SelectMany(x => x.Components)
                .Where(x => x.CustomId.Contains("pagination"))
                .Select(x => (DiscordButtonComponent) x)
                .ToList();
            components.ForEach(x => x.Disable());
            if ((obj.Message.Flags & MessageFlags.Ephemeral) != 0)
                await obj.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Interaction completed\n{obj.Message.Content}").AddEmbed(obj.Message?.Embeds?[0]).AddComponents(components));
            else 
                await obj.Message.ModifyAsync(x => x.AddComponents(components).WithContent(obj.Message.Content));
            components[0].RemoveListener(StopButtonClicked, _dispatcher);
            components[1].RemoveListener(NextButtonClicked, _dispatcher);
            components[2].RemoveListener(LastButtonClicked, _dispatcher);
            components[3].RemoveListener(FirstButtonClicked, _dispatcher);
            components[4].RemoveListener(BackButtonClicked, _dispatcher);
        }

        private async void NextButtonClicked(ComponentInteractionCreateEventArgs obj)
        {
            if (obj.User != _userThatHasPerms) return;
            var components = obj.Message.Components.SelectMany(x => x.Components)
                .Where(x => x.CustomId.Contains("pagination"))
                .Select(x => (DiscordButtonComponent) x)
                .ToList();
            
            _currentPage = ((_currentPage++) % _pages.Count) + 1;
            if (_currentPage >= _pages.Count)
                _currentPage -= _pages.Count; 
            
            if (_currentPage == _pages.Count - 1 && !_wrapAround)
            {
                components[3].Disable();
                components[4].Disable();
            }
            components[0].Enable();
            components[1].Enable();
            if ((obj.Message.Flags & MessageFlags.Ephemeral) != 0)
            {
                await obj.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .AddEmbed(_pages[_currentPage].Embed).AddComponents(components));
            }
            else
            {
                await obj.Message.ModifyAsync(x =>
                    x.WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .WithEmbed(_pages[_currentPage].Embed).AddComponents(components));
            }
        }
        
        private async void LastButtonClicked(ComponentInteractionCreateEventArgs obj)
        {
            if (obj.User != _userThatHasPerms) return;
            var components = obj.Message.Components.SelectMany(x => x.Components)
                .Where(x => x.CustomId.Contains("pagination"))
                .Select(x => (DiscordButtonComponent) x)
                .ToList();
            if (!_wrapAround)
            {
                components[0].Enable();
                components[1].Enable();
                components[3].Disable();
                components[4].Disable();
            }

            _currentPage = _pages.Count - 1;
            if ((obj.Message.Flags & MessageFlags.Ephemeral) != 0)
                await obj.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .AddEmbed(_pages[_currentPage].Embed).AddComponents(components));
            else
                await obj.Message.ModifyAsync(x =>
                    x.WithContent($"Page {_currentPage + 1} of {_pages.Count}\n{_pages[_currentPage].Content}")
                        .WithEmbed(_pages[_currentPage].Embed).AddComponents(components));
        }
        
        
        public static implicit operator DiscordMessageBuilder(PaginatedMessage message) => message._messageBuilder;
    }
}