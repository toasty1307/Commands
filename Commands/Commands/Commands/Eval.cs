using System;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Commands.Commands.Commands
{
    public class Eval : Command
    {
        public override string GroupName => "Commands";
        public override string Description => "eval code idk";
        public override bool RegisterSlashCommand => false;
        public override bool OwnerOnly => true;

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<StringArgumentType>
            {
                Key = "Code",
                Infinite = true
            }
        };

        public override async Task Run(DiscordMessage message, ArgumentCollector collector)
        {
            var prefix = (await Extension.Provider?.Get(message.Channel.Guild)!).Prefix ?? Extension.CommandPrefix;
            var cs = collector.Get<string>("Code");
            if (message.ReferencedMessage is null && message.Content.Length > prefix.Length + 4)
            {
	            await EvalCSharpCode(cs, message);
	            return;
            }
            var code = message.ReferencedMessage?.Content ?? message.Content;
            if (!(code?.Contains(prefix) ?? false))
            {
	            await EvalCSharpCode(code, message);
	            return;
            }
            var index = code.IndexOf(' ');
            code = code[++index..];
            await EvalCSharpCode(code, message);
        }

        public override Task Run(DiscordInteraction interaction, ArgumentCollector collector) => Task.CompletedTask;

        private async Task EvalCSharpCode(string code, DiscordMessage message)
		{
			var cs1 = code.IndexOf("```", StringComparison.Ordinal) + 3;
			cs1 = code.IndexOf(' ', cs1) + 1;
			var cs2 = code.LastIndexOf("```", StringComparison.Ordinal);

			if (cs1 is -1 || cs2 is -1)
			{
				cs1 = 0;
				cs2 = code.Length;
			}

			var cs = code.Substring(cs1, cs2 - cs1);

			var msg = await message.ReplyAsync("", new DiscordEmbedBuilder()
				.WithColor(new("2F3136"))
				.WithDescription("Evaluating...")
				.Build());

			try
			{
				var globals = new TestVariables(message, Client, Extension);

				var sopts = ScriptOptions.Default;
				sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
					"System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "Microsoft.Extensions.Logging");
				var asm = AppDomain.CurrentDomain.GetAssemblies()
					.Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location));


				sopts = sopts.WithReferences(asm);

				var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
				script.Compile();

				var result = await script.RunAsync(globals);
				if (result?.ReturnValue is (DiscordEmbedBuilder or DiscordEmbed))
					await msg.ModifyAsync(m => m.WithEmbed(result.ReturnValue as DiscordEmbedBuilder ?? result.ReturnValue as DiscordEmbed));
				else if (result?.ReturnValue is not null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
					await msg.ModifyAsync(new DiscordEmbedBuilder
						{
							Title = "Evaluation Result", Description = result.ReturnValue.ToString(),
							Color = new DiscordColor("2F3136")
						}.Build());
				else
					await msg.ModifyAsync(new DiscordEmbedBuilder
						{
							Title = "Evaluation Successful", Description = "No result was returned.",
							Color = new DiscordColor("2F3136")
						}.Build());
			}
			catch (Exception ex)
			{
				await msg.ModifyAsync(new DiscordEmbedBuilder
				{
					Title = "Evaluation Failure",
					Description = $"**{ex.GetType()}**: {ex.Message.Split('\n')[0]}",
					Color = new DiscordColor("2F3136")
				}.Build());
			}
		}
    }
    
    public record TestVariables
    {
        public TestVariables(DiscordMessage msg, DiscordClient client, CommandsExtension extension)
        {
	        Client = client;
	        Extension = extension;
	        Message = msg;
            Channel = msg.Channel;
            Guild = Channel.Guild;
            User = Message.Author;
            Reply = Message.ReferencedMessage;

            if (Guild != null) Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public DiscordMessage Message { get; }
        public DiscordMessage Reply { get; }
        public DiscordChannel Channel { get; }
        public DiscordGuild Guild { get; }
        public DiscordUser User { get; }
        public DiscordMember Member { get; }
        public DiscordClient Client { get; }
        public CommandsExtension Extension { get; }
    }
}