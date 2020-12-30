using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Walgelijk
{
    internal class CommandCache : Cache<string, MethodInfo>
    {
        private readonly HashSet<MethodInfo> methods = new HashSet<MethodInfo>();
        private bool initialised;

        protected override MethodInfo CreateNew(string raw)
        {
            if (!initialised)
                Initialise();

            raw = raw.ToLower();

            foreach (var method in methods)
            {
                if (method.Name.ToLower() != raw) continue;

                return method;
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
                    if (cmd == null)
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

        protected override void DisposeOf(MethodInfo loaded)
        {
            //hier hoeft niks te gebeuren
        }
    }
}
