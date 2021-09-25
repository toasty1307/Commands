using System;
using System.Linq;
using System.Threading.Tasks;
using Commands.CommandsStuff;
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
            new()
            {
                Key = "Code",
                Infinite = true
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var prefix = ctx.Extension.Provider?.Get(ctx.Guild)!.Prefix ?? ctx.Extension.CommandPrefix;
            var cs = ctx.GetArg<string>("Code");
            if (cs.ToLower().Contains("process") || cs.ToLower().Contains("shutdown"))
            {
	            await ctx.ReplyAsync("no.");
	            return;
            }
            if (ctx.Message.ReferencedMessage is null && ctx.Message.Content.Length > prefix.Length + 4)
            {
	            await EvalCSharpCode(cs, ctx.Message);
	            return;
            }
            var code = ctx.Message.ReferencedMessage?.Content ?? ctx.Message.Content;
            if (!(code?.Contains(prefix) ?? false))
            {
	            await EvalCSharpCode(code, ctx.Message);
	            return;
            }
            var index = code.IndexOf(' ');
            code = code[++index..];
            await EvalCSharpCode(code, ctx.Message);
        }

        public override Task Run(InteractionContext ctx) => Task.CompletedTask;

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
				.WithColor(new DiscordColor("2F3136"))
				.WithDescription("Evaluating...")
				.Build());

			try
			{
				var globals = new MessageContext
				{
					Client = Client,
					Message = message
				};

				var sopts = ScriptOptions.Default;
				sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
					"System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "Microsoft.Extensions.Logging", "Commands.Utils", "Commands", "Commands.CommandsStuff", "Commands.Data", "Commands.Types");
				var asm = AppDomain.CurrentDomain.GetAssemblies()
					.Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location));


				sopts = sopts.WithReferences(asm);

				var script = CSharpScript.Create(cs, sopts, typeof(MessageContext));
				script.Compile();

				var result = await script.RunAsync(globals);
				if (result?.ReturnValue is DiscordEmbedBuilder or DiscordEmbed)
					await msg.ModifyAsync(m => m.WithEmbed(result.ReturnValue as DiscordEmbedBuilder ?? result.ReturnValue as DiscordEmbed));
				else if (result?.ReturnValue is not null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
					await msg.ModifyAsync(new DiscordEmbedBuilder
						{
							Title = "Evaluation Result", Description = result.ReturnValue.ToString(),
							Color = new DiscordColor("2F3136"),
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

        public Eval(DiscordClient client) : base(client)
        {
        }
    }
}