using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.CoolStuff.Pagination;
using Commands.Utils;
using CommandsTest.Data;
using CommandsTest.Modules;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using GuildContext = CommandsTest.Data.GuildContext;

namespace CommandsTest.Commands.Misc
{
    public class Tag : Command
    {
        private static readonly List<MethodInfo> _globalTags = new();
        private static readonly List<TagsModule> _tagsModule = new();
        private readonly GuildContext _guildContext;
        private readonly List<TagEntity> _allTags;

        public Tag(DiscordClient client) : base(client)
        {
            _guildContext = new GuildContext();
            client.Logger.LogInformation("Loading Tags");
            _allTags = _guildContext.Tags.ToList();
            client.Logger.LogInformation("Loaded Tags");
        }
        public override string GroupName => "Misc";

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "CreateOrDeleteOrGet",
                Default = "get",
                Types = new []{typeof(string)},
                OneOf = new []{"get", "create", "delete"},
                Optional = true
            },
            new()
            {
                Key = "TagName",
                Types = new []{typeof(string)},
                Default = "all",
                Optional = true
            },
            new()
            {
                Key = "TagContent",
                Types = new []{typeof(string)},
                Optional = true,
                Default = "_",
                Infinite = true
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var tagContent = ctx.GetArg<string>("TagContent");
            var tagName = tagContent == "_" ? ctx.GetArg<string>("TagName") : $"{ctx.GetArg<string>("TagName")} {tagContent}";
            var createOrDeleteOrGet = ctx.GetArg<string>("CreateOrDeleteOrGet");
            switch (createOrDeleteOrGet)
            {
                case "get":
                    if (tagName is "all" or "list")
                    {
                        var pagesStringArrayListIHaveNoIdea = _globalTags.Select(x => x.Name).ToList();
                        pagesStringArrayListIHaveNoIdea.AddRange(_allTags.Where(x => x.GuildId == ctx.Guild.Id).Select(x => x.Name)); 
                        var pagesData = pagesStringArrayListIHaveNoIdea.Partition(Math.Max(pagesStringArrayListIHaveNoIdea.Count / 2, 1)).Where(x => x.Count != 0).ToList().SelectMany(x => x).Distinct();
                        var pages = pagesData.Select(tag => new Page {Content = string.Join('\n', $"Global tag `{tag}`")}).ToList();
                        var paginatedMessage = new PaginatedMessage(pages, ctx.Extension.Dispatcher, ctx.Author, false);
                        await ctx.ReplyAsync(paginatedMessage);
                        return;
                    }
                    var method = _globalTags.FirstOrDefault(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase) && x.GetParameters().First().ParameterType == typeof(MessageContext));
                    if (method is null)
                    {
                        var tag = _allTags.FirstOrDefault(x =>
                            string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase) && x.GuildId == ctx.Guild.Id);
                        if (tag is null) throw new FriendlyException("Tag doesnt exist");
                        await ctx.ReplyAsync(tag.Content);
                        break;
                    }
                    var type = _tagsModule.First(x => method!.DeclaringType == x.GetType());
                    var tagResult = (string)method.Invoke(type, new object[] {ctx});
                    await ctx.ReplyAsync(tagResult);
                    break;
                case "create":
                    if (_allTags.Any(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase)))
                        throw new FriendlyException("nah that tag exist");
                    var newTag = new TagEntity
                    {
                        Name = tagName,
                        Content = tagContent,
                        ThePersonWhoMadeThisTagUserId = ctx.Author.Id,
                        GuildId = ctx.Guild.Id
                    };
                    _allTags.Add(newTag);
                    _guildContext.Tags.Add(newTag);
                    _guildContext.Guilds.First(x => x.Id == ctx.Guild.Id).Tags.Add(newTag);
                    await _guildContext.SaveChangesAsync();
                    await ctx.ReplyAsync($"Created new Tag `{tagName}`");
                    break;
                case "delete":
                    var tagToDelete = _allTags.FirstOrDefault(x => x.Name == tagName);
                    if (tagToDelete is null) throw new FriendlyException("that tag dont exist");
                    _allTags.Remove(tagToDelete);
                    _guildContext.Remove(tagToDelete);
                    (await _guildContext.Guilds.FindAsync(ctx.Guild.Id))?.Tags.Remove(tagToDelete);
                    await _guildContext.SaveChangesAsync();
                    await ctx.ReplyAsync($"Guild Tag `{tagToDelete.Name}` was deleted");
                    break;
                default:
                    throw new FriendlyException("how did you manage to do that");
            }
        }

        public override async Task Run(InteractionContext ctx)
        {
            var tagName = ctx.GetArg<string>("TagName");
            var tagContent = ctx.GetArg<string>("TagContent");
            var createOrDeleteOrGet = ctx.GetArg<string>("CreateOrDeleteOrGet");
            switch (createOrDeleteOrGet)
            {
                case "get":
                    if (tagName is "all" or "list")
                    {
                        var pagesStringArrayListIHaveNoIdea = _globalTags.Select(x => x.Name).ToList();
                        pagesStringArrayListIHaveNoIdea.AddRange(_allTags.Where(x => x.GuildId == ctx.Guild.Id).Select(x => x.Name)); 
                        var pagesData = pagesStringArrayListIHaveNoIdea.Partition(Math.Max(pagesStringArrayListIHaveNoIdea.Count / 2, 1)).Where(x => x.Count != 0).ToList().SelectMany(x => x).Distinct();
                        var pages = pagesData.Select(tag => new Page {Content = string.Join('\n', $"Global tag `{tag}`")}).ToList();
                        var paginatedMessage = new PaginatedMessage(pages, ctx.Extension.Dispatcher, ctx.Author, false);
                        await ctx.ReplyAsync(paginatedMessage);
                        return;
                    }
                    var method = _globalTags.FirstOrDefault(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase) && x.GetParameters().First().ParameterType == typeof(InteractionContext));
                    if (method is null)
                    {
                        var tag = _allTags.FirstOrDefault(x =>
                            string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase) && x.GuildId == ctx.Guild.Id);
                        if (tag is null) throw new FriendlyException("Tag doesnt exist");
                        await ctx.ReplyAsync(tag.Content);
                        break;
                    }
                    var type = _tagsModule.First(x => method!.DeclaringType == x.GetType());
                    var tagResult = (string)method?.Invoke(type, new object[] {ctx});
                    await ctx.ReplyAsync(tagResult);
                    break;
                case "create":
                    if (_allTags.Any(x => string.Equals(x.Name, tagName, StringComparison.CurrentCultureIgnoreCase)))
                        throw new FriendlyException("nah that tag exist");
                    var newTag = new TagEntity
                    {
                        Name = tagName,
                        Content = tagContent,
                        ThePersonWhoMadeThisTagUserId = ctx.Author.Id,
                        GuildId = ctx.Guild.Id
                    };
                    _allTags.Add(newTag);
                    _guildContext.Tags.Add(newTag);
                    _guildContext.Guilds.First(x => x.Id == ctx.Guild.Id).Tags.Add(newTag);
                    await _guildContext.SaveChangesAsync();
                    await ctx.ReplyAsync($"Created new Tag `{tagName}`");
                    break;
                case "delete":
                    var tagToDelete = _allTags.FirstOrDefault(x => x.Name == tagName);
                    if (tagToDelete is null) throw new FriendlyException("that tag dont exist");
                    _allTags.Remove(tagToDelete);
                    _guildContext.Remove(tagToDelete);
                    (await _guildContext.Guilds.FindAsync(ctx.Guild.Id))?.Tags.Remove(tagToDelete);
                    await _guildContext.SaveChangesAsync();
                    await ctx.ReplyAsync($"Guild Tag `{tagToDelete.Name}` was deleted");
                    break;
                default:
                    throw new FriendlyException("how did you manage to do that");
            }
        }

        public static void RegisterTags(Assembly assembly)
        {
            _tagsModule.AddRange(assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(TagsModule))).Select(x => (TagsModule)Activator.CreateInstance(x)));
            _globalTags.AddRange(_tagsModule.SelectMany(x => x.GetType().GetMethods()).Where(x => x.GetCustomAttribute<TagAttribute>() is not null));
        }
    }
    
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class TagAttribute : Attribute { }
}