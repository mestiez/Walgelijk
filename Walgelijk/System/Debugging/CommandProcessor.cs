using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Walgelijk
{
    /// <summary>
    /// Object responsible for command processing. Only has static methods.
    /// </summary>
    public struct CommandProcessor
    {
        private readonly static CommandCache commandCache = new CommandCache();

        /// <summary>
        /// Execute the given string as a command
        /// </summary>
        public static void Execute(string command, DebugConsole console)
        {
            console.Print(command);

            if (string.IsNullOrWhiteSpace(command))
                return;

            string[] parts = SplitCommandInput(command).ToArray(); //TODO linq langzaam deel 1

            if (parts.Length == 0) //dit kan eigenlijk nooit maar wat nou als iets gebeurt dat letterlijk onmogelijk is?
                return;

            string cmd = parts[0].ToLower();
            string[] arguments = parts.Skip(1).ToArray(); //TODO linq langzaam deel 2 maar zoveel maakt dat niet uit, het is een debug command ¯\_(ツ)_/¯

            var action = commandCache.Load(cmd);
            if (action == null)
                Logger.Warn($"There is no command that matches with \"{cmd}\"");
            else
                InvokeMethod(action, arguments, console);
        }

        private static void InvokeMethod(MethodInfo methodInfo, string[] args, DebugConsole console)
        {
            var expectedArgs = methodInfo.GetParameters();
            object[] toPass = new object[expectedArgs.Length];

            string argumentNoun = toPass.Length == 1 ? "argument" : "arguments";
            string isAreWord = args.Length == 1 ? "is" : "are";

            if (expectedArgs.Length < args.Length)
            {
                Logger.Error($"{methodInfo.Name} expects {toPass.Length} {argumentNoun}, but {args.Length} {isAreWord} given");
                return;
            }

            for (int i = 0; i < expectedArgs.Length; i++)
            {
                var expected = expectedArgs[i];
                if (args.Length <= i)
                    if (expected.HasDefaultValue)
                    {
                        toPass[i] = expected.DefaultValue;
                        continue;
                    }
                    else
                    {
                        Logger.Error($"{methodInfo.Name} expects {toPass.Length} {argumentNoun}, but {args.Length} {isAreWord} given");
                        return;
                    }

                var type = expected.ParameterType;

                if (type == typeof(float))
                    if (tryParseArgument<float>(args[i], i, out var result, float.TryParse))
                    {
                        toPass[i] = result;
                        continue;
                    }
                    else return;

                if (type == typeof(int))
                    if (tryParseArgument<int>(args[i], i, out var result, int.TryParse))
                    {
                        toPass[i] = result;
                        continue;
                    }
                    else return;

                if (type == typeof(bool))
                    if (tryParseArgument<bool>(args[i], i, out var result, bool.TryParse))
                    {
                        toPass[i] = result;
                        continue;
                    }
                    else return;

                if (type == typeof(string))
                {
                    toPass[i] = args[i];
                    continue;
                }

                Logger.Error($"{methodInfo.Name} expects {toPass.Length} {argumentNoun}, but {args.Length} {isAreWord} given");
                return;
            }

            var returned = methodInfo.Invoke(null, toPass);

            if (methodInfo.ReturnType != typeof(void))
            {
                switch (returned)
                {
                    case CommandResult commandResult:
                        switch (commandResult.Type)
                        {
                            case LogLevel.Info:
                                Logger.Log(commandResult.Message);
                                break;
                            case LogLevel.Warn:
                                Logger.Warn(commandResult.Message);
                                break;
                            case LogLevel.Error:
                                Logger.Error(commandResult.Message);
                                break;
                        }
                        break;
                    default:
                        console.Print(returned.ToString());
                        break;
                }

            }

            static bool tryParseArgument<T>(string given, int index, out T result, ParseFunc<T> parseFunction)
            {
                if (parseFunction(given, out result))
                    return true;
                else
                {
                    Logger.Error($"Argument {index} is not a {typeof(T).Name}");
                    return false;
                }
            }
        }

        private delegate bool ParseFunc<T>(string a, out T result);

        private static IEnumerable<string> SplitCommandInput(string given)
        {
            bool isInsideQuotes = false;
            string next = string.Empty;
            for (int i = 0; i < given.Length; i++)
            {
                char character = given[i];

                if (isInsideQuotes)
                {
                    switch (character)
                    {
                        case '\"':
                            isInsideQuotes = false;
                            break;
                        default:
                            next += character.ToString();
                            break;
                    }
                }
                else
                {
                    switch (character)
                    {
                        case '\"':
                            isInsideQuotes = true;
                            break;
                        case ' ':
                        case '\n':
                            if (!string.IsNullOrWhiteSpace(next))
                                yield return next;
                            next = string.Empty;
                            break;
                        default:
                            next += character.ToString();
                            break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(next))
                yield return next;
        }

        /// <summary>
        /// Get all commands
        /// </summary>
        public static IEnumerable<string> GetAllCommands()
        {
            return commandCache.GetAll();
        }
    }
}
