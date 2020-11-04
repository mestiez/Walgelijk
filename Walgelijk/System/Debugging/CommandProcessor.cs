using System;
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
        public static void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) //dit kan eigenlijk nooit maar wat nou als iets gebeurt dat letterlijk onmogelijk is?
                return;

            string cmd = parts[0].ToLower();
            string[] arguments = parts.Skip(1).ToArray(); //TODO linq langzaam

            //TODO command arguments

            var action = commandCache.Load(cmd);
            if (action == null)
            {
                Logger.Warn($"There is no command that matches with \"{cmd}\"");
            }
            else
                action();
        }

        public static IEnumerable<string> GetAllCommands()
        {
            return commandCache.GetAll();
        }

        private class CommandCache : Cache<string, Action>
        {
            private readonly HashSet<MethodInfo> methods = new HashSet<MethodInfo>();
            private bool initialised;

            protected override Action CreateNew(string raw)
            {
                if (!initialised)
                    Initialise();

                raw = raw.ToLower();

                foreach (var method in methods)
                {
                    if (method.Name.ToLower() != raw) continue;

                    var action = method.CreateDelegate(typeof(Action)) as Action;
                    return action;
                }

                return null;
            }

            private void Initialise()
            {
                RegisterAssembly(Assembly.GetEntryAssembly());
                RegisterAssembly(Assembly.GetAssembly(typeof(CommandCache)));

                initialised = true;
            }

            public IEnumerable<string> GetAll()
            {
                return methods.Select(m => m.Name);
            }

            public void RegisterAssembly(Assembly ass)
            {
                Logger.Log("Registering commands in assembly " + ass.FullName);

                foreach (var type in ass.GetTypes())
                {
                    var allMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (var method in allMethods)
                    {
                        var cmd = method.GetCustomAttribute<CommandAttribute>();
                        if (cmd  == null)
                            continue;

                        //TODO alias

                        if (!method.IsStatic)
                        {
                            Logger.Warn("Non-static methods may not be registered as commands (\"" + method.Name + "\")");
                            continue;
                        }

                        methods.Add(method);
                        Logger.Log("Command registered: (\"" + method.Name + "\")");
                    }
                }

                foreach (var a in methods)
                    foreach (var b in methods)
                    {
                        if (a != b && a.Name.ToLower() == b.Name.ToLower())
                            Logger.Warn($"Command \"{b.Name}\" has two entries. Only one of them will work. This behaviour is undefined.");
                    }
            }

            protected override void DisposeOf(Action loaded)
            {
                //hier hoeft niks te gebeuren
            }
        }
    }
}
