using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Walgelijk
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "These are console commands, they are used and invoked via reflection")]
    internal struct DebugCommands
    {
        private static Game Game => Game.Main;

        [Command]
        private static void ShowStats(bool state)
        {
            var inst = Game.Profiling;
            inst.DrawQuickProfiler = state;
            Logger.Log("Profiler " + (state ? "enabled" : "disabled"));
        }

        [Command]
        private static string Echo(string input)
        {
            return input;
        }

        [Command]
        private static void Cls()
        {
            Game.Console.Clear();
        }

        [Command]
        private static void Quit()
        {
            Game.Stop();
        }

        [Command]
        private static string FpsCap(int target = 0)
        {
            Game.Window.TargetFrameRate = target;
            return "Target frame render rate set to " + target;
        }

        [Command]
        private static string UpsCap(int target = 0)
        {
            Game.Window.TargetUpdateRate = target;
            return "Target update rate set to " + target;
        }

        [Command]
        public static string ListSystems()
        {
            var systems = Game.Scene.GetSystems();
            return string.Join("\n", systems.Select(s => ">" + s.GetType().Name));
        }

        [Command]
        public static string RemoveSystem(string typeName)
        {
            var system = Game.Scene.GetSystems().FirstOrDefault(s => s.GetType().Name == typeName);
            if (system == null)
                return $"There is no system that matches \"{typeName}\"";
            var type = system.GetType();
            var removeMethod = typeof(Scene).GetMethod(nameof(Game.Scene.RemoveSystem)).MakeGenericMethod(type);
            bool success = (bool)removeMethod.Invoke(Game.Scene, null);
            return success ? type.Name + " removed" : "Could not remove " + type.Name;
        }

        [Command]
        public static string ListEntities()
        {
            var systems = Game.Scene.GetAllEntities();
            return string.Join("\n", systems.Select(s => ">" + s));
        }

        [Command]
        public static string ListComponents(int entityID)
        {
            if (!Game.Scene.HasEntity(entityID))
                return "No matching entity found";

            var components = Game.Scene.GetAllComponentsFrom(entityID);
            return string.Join("\n", components.Select(s => ">" + s.GetType().Name));
        }

        [Command]
        public static string RemoveComponent(int entityID, string componentType)
        {
            if (!Game.Scene.HasEntity(entityID))
                return "No matching entity found";

            var component = Game.Scene.GetAllComponentsFrom(entityID).FirstOrDefault(s => s.GetType().Name == componentType);
            if (component == null)
                return $"{entityID} has no component that matches \"{componentType}\"";
            var type = component.GetType();
            var removeMethod = typeof(Scene).GetMethod(nameof(Game.Scene.DetachComponent)).MakeGenericMethod(type);
            bool success = (bool)removeMethod.Invoke(Game.Scene, new object[] { new Entity { Identity = entityID } });
            return success ? type.Name + " removed" : "Could not remove " + type.Name;
        }

        [Command]
        public static string RemoveEntity(int entityID)
        {
            bool success = Game.Scene.RemoveEntity(entityID);
            return success ? entityID + " removed" : "Could not remove " + entityID;
        }

        [Command]
        private static void List()
        {
            var builder = new StringBuilder();
            builder.Append(Environment.NewLine);
            builder.Append("All commands:");
            builder.Append(Environment.NewLine);
            foreach (var item in CommandProcessor.GetAllCommands())
            {
                builder.Append(item);
                builder.Append(Environment.NewLine);
            }
            Logger.Log(builder);
        }
    }
}
