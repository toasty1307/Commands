using System.Collections.Generic;
using System.Linq;

namespace Commands.CommandsStuff
{
    public readonly struct ArgumentParserData
    {
        public List<Argument> NonOptionalArgs { get; init; }
        public List<Argument> OptionalArgs { get; init; }
        public bool InvalidNumberOfArgs { get; init; }
        public string[] SlashCommandShiftedArgs { get; init; }

        public static ArgumentParserData From(string[] input, Command command, bool isSlashCommand)
        {
            var commandArgs = command.Arguments;
            var nonOptionalArgs = command.Arguments.Where(x => !x.Optional).ToList();
            var optionalArgs    = command.Arguments.Where(x =>  x.Optional).ToList();
            var invalidNumberOfArgs = nonOptionalArgs.Count > input.Length;
            if (!isSlashCommand)
            {
                return new ArgumentParserData
                {
                    NonOptionalArgs = nonOptionalArgs,
                    OptionalArgs = optionalArgs,
                    InvalidNumberOfArgs = invalidNumberOfArgs,
                    SlashCommandShiftedArgs = input[1..]
                };
            }

            var repeatCount = nonOptionalArgs.Count + optionalArgs.Count - input.Length;
            var tempInput = input.ToList();
            tempInput.AddRange(Enumerable.Repeat<string>(null, repeatCount));
            
            // do the shifting ig
            
            for (var i = 0; i < command.Arguments.Length; i++)
            {
                var optional = command.Arguments[i].Optional;
                if (!optional) continue;
                var indexInOptionalArgs = optionalArgs.IndexOf(commandArgs[i]);
                if (indexInOptionalArgs == -1) indexInOptionalArgs = 0;
                var argsAtThatPos = tempInput[^(optionalArgs.Count - indexInOptionalArgs)];
                tempInput.Remove(argsAtThatPos);
                tempInput.Insert(i, argsAtThatPos);
            }

            return new ArgumentParserData
            {
                NonOptionalArgs = nonOptionalArgs,
                OptionalArgs = optionalArgs,
                InvalidNumberOfArgs = invalidNumberOfArgs,
                SlashCommandShiftedArgs = tempInput.ToArray()
            };
        }

        public void Deconstruct(out List<Argument> nonOptionalArgs, out List<Argument> optionalArgs,
            out bool invalidNumberOfArgs, out string[] slashCommandShiftedArgs)
        {
            nonOptionalArgs = NonOptionalArgs;
            optionalArgs = OptionalArgs;
            invalidNumberOfArgs = InvalidNumberOfArgs;
            slashCommandShiftedArgs = SlashCommandShiftedArgs;
        }
    }
}