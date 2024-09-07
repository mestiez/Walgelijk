using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Walgelijk;

/// <summary>
/// Throwing this exception from a command invocation will crash the game
/// </summary>
public class ManualCrashException : Exception
{
    public ManualCrashException(string? message) : base(message)
    {
    }
}

/// <summary>
/// Object responsible for command processing. Only has static methods.
/// </summary>
public struct CommandProcessor
{
    private readonly static CommandCache commandCache = new CommandCache();

    static CommandProcessor()
    {
        commandCache.Initialise();
    }

    public static void RegisterAssembly(Assembly assembly) => commandCache.RegisterAssembly(assembly);

    /// <summary>
    /// Execute the given string as a command
    /// </summary>
    public static void Execute(string command, DebugConsole console)
    {
        console.WriteLine(command, ConsoleMessageType.Plain);

        if (string.IsNullOrWhiteSpace(command))
            return;

        string[] parts = SplitCommandInput(command).ToArray(); //TODO linq langzaam deel 1

        if (parts.Length == 0) //dit kan eigenlijk nooit maar wat nou als iets gebeurt dat letterlijk onmogelijk is?
            return;

        string cmd = parts[0].ToLower();
        string[] arguments = parts.Skip(1).ToArray(); //TODO linq langzaam deel 2 maar zoveel maakt dat niet uit, het is een debug command ¯\_(ツ)_/¯

        var action = commandCache.Load(cmd);
        if (action == null)
            console.WriteLine($"There is no command that matches \"{cmd}\"", ConsoleMessageType.Error);
        else
        {
            bool success;
            try
            {
                success = InvokeMethod(action, arguments, console);
            }
            catch (global::System.Exception e)
            {
                if (e.InnerException != null && e.InnerException is ManualCrashException m)
                    throw m;

                success = false;
                var message = e.InnerException != null ? e.InnerException.Message : e.Message;
                console.WriteLine("Command error: " + message, ConsoleMessageType.Error);
            }
            if (!success && !string.IsNullOrWhiteSpace(action.CommandAttr.HelpString))
                console.WriteLine(action.CommandAttr.HelpString, ConsoleMessageType.Warning);
        }
    }

    private static string CreateSyntaxExampleString(MethodInfo method)
    {
        var s = new StringBuilder();
        s.Append(method.Name);
        s.Append(' ');
        var @params = method.GetParameters();
        for (int i = 0; i < @params.Length; i++)
        {
            var item = @params[i];
            s.AppendFormat("[{0} {1}] ", getTypeName(item.ParameterType), item.Name);
        }
        return s.ToString();

        string getTypeName(global::System.Type type)
        {
            if (type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(string))
                return "string";
            return type.Name.ToLower();
        }
    }

    private static bool InvokeMethod(CommandEntry v, string[] args, DebugConsole console)
    {
        if (args.Length == 1 && args[0] == "?")
            return false; //Return help string

        var expectedArgs = v.Method.GetParameters();
        object[] toPass = new object[expectedArgs.Length];

        string argumentNoun = toPass.Length == 1 ? "argument" : "arguments";
        string isAreWord = args.Length == 1 ? "is" : "are";

        if (expectedArgs.Length < args.Length)
        {
            if (expectedArgs.Length == 1 && expectedArgs[0].ParameterType == typeof(string))
            {
                args = new string[] { string.Join(' ', args) };
                isAreWord = "is";
            }
            else
            {
                console.WriteLine($"{v.Name} expects {toPass.Length} {argumentNoun}, but {args.Length} {isAreWord} given\nSyntax: {CreateSyntaxExampleString(v.Method)}", ConsoleMessageType.Error);
                return false;
            }
        }

        for (int i = 0; i < expectedArgs.Length; i++)
        {
            var expected = expectedArgs[i];
            if (args.Length <= i)
                if (expected.HasDefaultValue)
                {
                    toPass[i] = expected.DefaultValue ?? throw new global::System.Exception("HasDefaultValue returned true but it doesn't actually have one, somehow");
                    continue;
                }
                else
                {
                    console.WriteLine($"{v.Name} expects {toPass.Length} {argumentNoun}, but {args.Length} {isAreWord} given\nSyntax: {CreateSyntaxExampleString(v.Method)}", ConsoleMessageType.Error);
                    return false;
                }

            var type = expected.ParameterType;

            if (type == typeof(float))
                if (tryParseArgument<float>(args[i], i, out var result, float.TryParse))
                {
                    toPass[i] = result;
                    continue;
                }
                else return true;

            if (type == typeof(int))
                if (tryParseArgument<int>(args[i], i, out var result, int.TryParse))
                {
                    toPass[i] = result;
                    continue;
                }
                else return true;

            if (type == typeof(bool))
                if (tryParseArgument<bool>(args[i], i, out var result, bool.TryParse))
                {
                    toPass[i] = result;
                    continue;
                }
                else return true;

            if (type == typeof(string))
            {
                toPass[i] = args[i];
                continue;
            }

            console.WriteLine($"{v.Name} expects {toPass.Length} {argumentNoun}, but {args.Length} {isAreWord} given\nSyntax: {CreateSyntaxExampleString(v.Method)}", ConsoleMessageType.Error);
            return false;
        }

        var returned = v.Method.Invoke(null, toPass);

        if (v.Method.ReturnType != typeof(void))
        {
            switch (returned)
            {
                case CommandResult commandResult:
                    switch (commandResult.Type)
                    {
                        case LogLevel.Warning:
                            console.WriteLine(commandResult.Message, ConsoleMessageType.Warning);
                            break;
                        case LogLevel.Error or LogLevel.Critical:
                            console.WriteLine(commandResult.Message, ConsoleMessageType.Error);
                            return false;
                        default:
                            console.WriteLine(commandResult.Message, ConsoleMessageType.Info);
                            break;
                    }
                    break;
                default:
                    console.WriteLine(returned?.ToString() ?? "null");
                    break;
            }
        }

        bool tryParseArgument<T>(string given, int index, out T result, ParseFunc<T> parseFunction)
        {
            if (parseFunction(given, out result))
                return true;
            else
            {
                console.WriteLine($"Argument {index} is not a {typeof(T).Name}", ConsoleMessageType.Error);
                return false;
            }
        }

        return true;
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
    public static IEnumerable<(string, CommandEntry)> GetAllCommands()
    {
        return commandCache.GetAll();
    }

    /// <summary>
    /// Gets the first command that starts with the given string
    /// </summary>
    public static int GetSuggestions(ReadOnlySpan<char> input, Span<string> buffer)
    {
        var cc = commandCache.GetAll();
        int i = 0;

        var v = input.Trim();
        foreach (var item in cc)
            if (item.Item1.AsSpan().StartsWith(v, StringComparison.InvariantCultureIgnoreCase))
            {
                buffer[i] = item.Item1;
                i++;
                if (i >= buffer.Length)
                    break;
            }

        return i;
    }
}
