using System.Linq;
using System.Reflection;
using Commands.Types;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.CommandsStuff
{
    public class ArgumentParser : CommandsBase
    {
        private CommandRegistry Registry { get; }

        public ArgumentParser(DiscordClient client, CommandRegistry registry) : base(client) => Registry = registry;
        
        public ArgumentCollector Parse<T>(string[] words, Command command, T interactionOrMessage)
        {
            var collector = new ArgumentCollector();
            var commandArgs = command.Arguments?.ToList();
            var isInteraction = interactionOrMessage is DiscordInteraction;
            if (commandArgs is null || !commandArgs.Any()) return collector;
            var parserData = ArgumentParserData.From(words, command, isInteraction);
            var (_, _, _, slashCommandShiftedArgs) = parserData;
            var inputArgs = slashCommandShiftedArgs[..];
            foreach (var argument in commandArgs)
            {
                var index = commandArgs.IndexOf(argument);
                var (key, description, types, optional,
                    @default, oneOf, infinite) = argument;
                var argumentStringProvided = string.Empty;
                try
                {
                    argumentStringProvided = infinite ? string.Join(" ", inputArgs[index..]) : inputArgs[index];
                }
                catch
                {
                    if (optional && @default is null) 
                        continue;
                }

                if ((types?.Length ?? 0) == 0)
                    types = new[] {typeof(string)};
                if (!oneOf?.Select(x => x.ToLower()).Contains(argumentStringProvided.ToLower()) == true)
                {
                    if (!optional) throw new FriendlyException($"Argument `{key}`(`{description}`) should be one of `{string.Join(", ", oneOf)}`");
                    var temp = inputArgs.ToList();
                    if (temp.Count != 0)
                        temp.Insert(commandArgs.IndexOf(argument) + 1,
                            string.IsNullOrEmpty(argumentStringProvided) ? "_" : argumentStringProvided); // i have no idea
                    if (@default is not null)
                    {
                        var argumentTypeObject = Registry.GetArgumentTypeFromReturnType(types.First());
                        var (_, parseMethod, _) = 
                            GetMethods(argumentTypeObject);
                        var parseResult = parseMethod.Invoke(argumentTypeObject, new object[] {@default});
                        collector[key] = parseResult;
                    }

                    inputArgs = temp.ToArray();
                    continue;
                }

                foreach (var type in types)
                {
                    var argumentTypeObject = Registry.GetArgumentTypeFromReturnType(type);
                    var (validateMethod, parseMethod,
                        isEmptyMethod) = GetMethods(argumentTypeObject);
                    var validateResult =
                        (bool) validateMethod.Invoke(argumentTypeObject, new object[] {argumentStringProvided})!;
                    if (!validateResult)
                    {
                        if (!optional && type == types.Last() && @default is not null)
                        {
                            if (isInteraction)
                                Extension.CommandCanceled(command, "INVALID_ARGS",
                                    interactionOrMessage as DiscordInteraction);
                            else
                                Extension.CommandCanceled(command, "INVALID_ARGS",
                                    interactionOrMessage as DiscordMessage);
                            throw new FriendlyException($"Invalid value (`{(string.IsNullOrEmpty(argumentStringProvided) ? "null" : argumentStringProvided)}`) for `{key}`(`{description ?? "there aint no description for this arg, cuz toasty lazy"}`)");
                        }
                        
                        if (type != types.Last())
                            continue;
                        var temp = inputArgs.ToList();
                        try
                        {
                            temp.Insert(index + 1, inputArgs[index]);
                        }
                        catch { /* ignored */ }

                        if (@default is not null)
                        {
                            var parseResultDefault = parseMethod.Invoke(argumentTypeObject, 
                                new object[] {@default});
                            collector[key] = parseResultDefault;
                            break;
                        }

                        inputArgs = temp.ToArray();
                        break;
                    }

                    var parseResult = parseMethod.Invoke(argumentTypeObject, new object[] {argumentStringProvided});
                    var isEmpty = (bool) isEmptyMethod.Invoke(argumentTypeObject, new[] {parseResult})!;

                    if (isEmpty && @default is not null)
                        collector[key] = parseMethod.Invoke(argumentTypeObject, new object[] {@default});
                    else
                        collector[key] = parseResult;

                    break;
                }

                if (infinite) break;
            }

            return collector;
        }

        private (MethodInfo, MethodInfo, MethodInfo) GetMethods(ArgumentType argumentType)
        {
            var type = argumentType.GetType();
            var validate = type.GetMethod(nameof(ArgumentType.Validate));
            var parse = type.GetMethod(nameof(ArgumentType<bool>.Parse));
            var isEmpty = type.GetMethod(nameof(ArgumentType<bool>.IsEmpty));
            return (validate, parse, isEmpty);
        }
    }
}