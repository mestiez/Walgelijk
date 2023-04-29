using System.Collections.Generic;
using System.Reflection;

namespace Walgelijk;

public record CommandEntry(MethodInfo Method, CommandAttribute CommandAttr, string Name);

internal class CommandCache : Cache<string, CommandEntry>
{
    private readonly HashSet<CommandEntry> methods = new();
    private bool initialised;

    protected override CommandEntry CreateNew(string raw)
    {
        if (!initialised)
            Initialise();

        foreach (var entry in methods)
        {
            if (entry.Name.Equals(raw, global::System.StringComparison.InvariantCultureIgnoreCase))
                return entry;
        }

        return null!;
    }

    public new IEnumerable<(string, CommandEntry)> GetAll()
    {
        foreach (var item in methods)
            yield return (item.Name, item);
    }

    public int Count => methods.Count;

    internal void Initialise()
    {
        if (initialised)
            return;

        RegisterAssembly(Assembly.GetEntryAssembly());
        RegisterAssembly(Assembly.GetAssembly(typeof(CommandCache)));

        initialised = true;
    }

    public void RegisterAssembly(Assembly? ass)
    {
        if (ass == null)
        {
            Logger.Warn("Attempt to register null assembly to command cache");
            return;
        }
        else
            Logger.Log("Registering commands in assembly " + ass.FullName);

        foreach (var type in ass.GetTypes())
        {
            var allMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in allMethods)
            {
                var cmd = method.GetCustomAttribute<CommandAttribute>();
                if (cmd == null)
                    continue;

                var commandName = cmd.Alias ?? method.Name;

                if (!method.IsStatic)
                {
                    Logger.Warn("Non-static methods may not be registered as commands (\"" + commandName + "\")");
                    continue;
                }

                methods.Add(new CommandEntry(method, cmd, commandName));
                Logger.Log("Command registered: (\"" + commandName + "\")");
            }
        }

        foreach (var a in methods)
            foreach (var b in methods)
            {
                if (a != b && a.Name.Equals(b.Name, global::System.StringComparison.InvariantCultureIgnoreCase))
                    Logger.Warn($"Command \"{b.Name}\" has two entries. Only one of them will work. This behaviour is undefined.");
            }
    }

    protected override void DisposeOf(CommandEntry _)
    {
        //hier hoeft niks te gebeuren
    }
}
