using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Walgelijk
{
    internal class CommandCache : Cache<string, (MethodInfo method, CommandAttribute cmd)>
    {
        private readonly HashSet<(MethodInfo method, CommandAttribute cmd)> methods = new();
        private bool initialised;

        protected override (MethodInfo method, CommandAttribute cmd) CreateNew(string raw)
        {
            if (!initialised)
                Initialise();

            raw = raw.ToLower();

            foreach (var (method, cmd) in methods)
            {
                if (method.Name.ToLower() != raw) continue;
                return (method, cmd);
            }

            return default;
        }

        private void Initialise()
        {
            RegisterAssembly(Assembly.GetEntryAssembly());
            RegisterAssembly(Assembly.GetAssembly(typeof(CommandCache)));

            initialised = true;
        }

        public IEnumerable<string> GetAll()
        {
            return methods.Select(m => m.method.Name);
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

                    //TODO alias

                    if (!method.IsStatic)
                    {
                        Logger.Warn("Non-static methods may not be registered as commands (\"" + method.Name + "\")");
                        continue;
                    }

                    methods.Add((method, cmd));
                    Logger.Log("Command registered: (\"" + method.Name + "\")");
                }
            }

            foreach (var a in methods)
                foreach (var b in methods)
                {
                    if (a != b && a.method.Name.ToLower() == b.method.Name.ToLower())
                        Logger.Warn($"Command \"{b.method.Name}\" has two entries. Only one of them will work. This behaviour is undefined.");
                }
        }

        protected override void DisposeOf((MethodInfo method, CommandAttribute cmd) _)
        {
            //hier hoeft niks te gebeuren
        }
    }
}
