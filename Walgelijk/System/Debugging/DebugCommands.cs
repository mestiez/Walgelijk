using System;
using System.Linq;
using System.Text;

namespace Walgelijk
{
    internal struct DebugCommands
    {
        private static Game Game => Game.Main;

        [Command(HelpString = "Shows some performance stats in the top left corner of the screen. Expects true or false")]
        private static string ShowStats(bool state)
        {
            var inst = Game.Profiling;
            inst.DrawQuickProfiler = state;
            return "Profiler " + (state ? "enabled" : "disabled");
        }

        [Command(HelpString = "Returns the input string")]
        private static string Echo(string input)
        {
            return input;
        }

        [Command(HelpString = "Clears the console")]
        private static void Cls()
        {
            Game.Console.Clear();
        }

        [Command(HelpString = "Quits the game")]
        private static void Quit()
        {
            Game.Stop();
        }

        [Command(HelpString = "Get the console filter")]
        private static CommandResult GetFilter()
        {
            return Game.Console.Filter.ToString();
        }

        [Command(HelpString = "Set the console filter. E.g 'setfilter error warn'")]
        private static CommandResult SetFilter(string type)
        {
            if (type == "All")
            {
                Game.Console.Filter = ConsoleMessageType.All;
                return "Filter disabled. Everything is shown.";
            }

            int spanIndex = 0;
            var s = type.AsSpan();
            var final = (ConsoleMessageType)0;
            while (true)
            {
                var part = s[spanIndex..];
                var eaten = eatEnum(part, out var result);
                if (eaten > 0)
                {
                    final |= result;
                    spanIndex += eaten;
                    if (spanIndex >= s.Length)
                        break;
                }
                else
                    break;

                static int eatEnum(ReadOnlySpan<char> input, out ConsoleMessageType result)
                {
                    for (int i = 0; i <= input.Length; i++)
                    {
                        var ss = input[..i];
                        if (Enum.TryParse(ss, out result))
                            return i;
                    }
                    result = ConsoleMessageType.Error;
                    return 0;
                }
            }

            if (final > 0)
            {
                Game.Console.Filter = final;
                return $"Set filter to show: {final}";
            }

            return CommandResult.Error($"Invalid filter value. Seperated by a space, you can only enter these values:\nAll\n{string.Join("\n", Enum.GetNames<LogLevel>())}");
        }

        [Command(HelpString = "Sets the render rate cap. Expects an integer")]
        private static CommandResult FpsCap(int target = 0)
        {
            Game.Window.TargetFrameRate = target;
            return "Target frame render rate set to " + target;
        }

        [Command(HelpString = "Sets the update rate cap. Expects an integer")]
        private static CommandResult UpsCap(int target = 0)
        {
            Game.Window.TargetUpdateRate = target;
            return "Target update rate set to " + target;
        }

        [Command(HelpString = "Lists all systems in the scene")]
        private static string ListSystems()
        {
            var systems = Game.Scene.GetSystems();
            return string.Join("\n", systems.Select(s => ">" + s.GetType().Name));
        }

        [Command(HelpString = "Remove the system with the given name")]
        private static CommandResult RemoveSystem(string typeName)
        {
            var system = Game.Scene.GetSystems().FirstOrDefault(s => s.GetType().Name == typeName);
            if (system == null)
                return CommandResult.Error($"There is no system that matches \"{typeName}\"");
            var type = system.GetType();
            var removeMethod = typeof(Scene).GetMethod(nameof(Game.Scene.RemoveSystem)).MakeGenericMethod(type);
            bool success = (bool)removeMethod.Invoke(Game.Scene, null);
            return success ? type.Name + " removed" : CommandResult.Error("Could not remove " + type.Name);
        }

        [Command(HelpString = "List all entities in the scene")]
        private static string ListEntities()
        {
            var systems = Game.Scene.GetAllEntities();
            return string.Join("\n", systems.Select(s => ">" + s));
        }

        [Command(HelpString = "List all components in an entity. Expects an integer")]
        private static string ListComponents(int entityID)
        {
            if (!Game.Scene.HasEntity(entityID))
                return "No matching entity found";

            var components = Game.Scene.GetAllComponentsFrom(entityID);
            return string.Join("\n", components.Select(s => ">" + s.GetType().Name));
        }

        [Command(HelpString = "Remove a component from an entity. Expects an integer and a string")]
        private static CommandResult RemoveComponent(int entityID, string componentType)
        {
            if (!Game.Scene.HasEntity(entityID))
                return CommandResult.Error("No matching entity found");

            var component = Game.Scene.GetAllComponentsFrom(entityID).FirstOrDefault(s => s.GetType().Name == componentType);
            if (component == null)
                return CommandResult.Error($"{entityID} has no component that matches \"{componentType}\"");
            var type = component.GetType();
            var removeMethod = typeof(Scene).GetMethod(nameof(Game.Scene.DetachComponent)).MakeGenericMethod(type);
            bool success = (bool)removeMethod.Invoke(Game.Scene, new object[] { new Entity { Identity = entityID } });
            return success ? type.Name + " removed" : CommandResult.Error("Could not remove " + type.Name);
        }

        [Command(HelpString = "Remove and entity. Expects an integer")]
        private static CommandResult RemoveEntity(int entityID)
        {
            Game.Scene.RemoveEntity(entityID);
            return entityID + " removed";
        }

        [Command(HelpString = "List all commands")]
        private static string List()
        {
            var builder = new StringBuilder();
            foreach (var item in CommandProcessor.GetAllCommands())
            {
                builder.Append(item);
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }
    }
}
