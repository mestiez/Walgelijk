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

        [Command(HelpString = "Get or set the window type. Enter ?? as a parameter to see available types.")]
        private static CommandResult Window(string? type = null)
        {
            if (string.IsNullOrEmpty(type))
                return "Current window type is " + Game.Window.WindowType.ToString();

            if (type == "??")
                return string.Join('\n', Enum.GetNames<WindowType>());

            if (Enum.TryParse<WindowType>(type, true, out var t))
            {
                Game.Window.WindowType = t;
                return CommandResult.Info($"Window type set to {t}");
            }
            else
            {
                return CommandResult.Error($"{type} is not a valid window type.");
            }
        }

        [Command(HelpString = "Get the console filter")]
        private static CommandResult GetFilter()
        {
            return Game.Console.Filter.ToString();
        }

        [Command(HelpString = "Set the console filter. E.g 'SetFilter Error Warn'")]
        private static CommandResult SetFilter(string type)
        {
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
                        if (Enum.TryParse(ss, true, out result))
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

            return CommandResult.Error($"Invalid filter value. Seperated by a space, you can only enter these values:\nAll\n{string.Join("\n", Enum.GetNames<ConsoleMessageType>())}");
        }

        [Command(HelpString = "Sets the update rate. Expects an integer. If a number smaller than 0 is given, this will just print the current rate.")]
        private static CommandResult UpsRate(int target = -1)
        {
            if (target < 0)
                return $"Update rate is {Game.UpdateRate} Hz";
            Game.UpdateRate = target;
            if (target <= 0)
                return "Update rate set to infinite";
            return "Update rate set to " + target;
        }

        [Command(HelpString = "Sets the fixed update rate. Expects an integer. If a number smaller than 0 is given, this will just print the current rate.")]
        private static CommandResult FupsRate(int target = -1)
        {
            if (target < 0)
                return $"Fixed update rate is {Game.FixedUpdateRate} Hz";
            Game.FixedUpdateRate = target;
            return "Fixed update rate set to " + target;
        }

        [Command(HelpString = "Lists all systems in the scene")]
        private static CommandResult ListSystems()
        {
            if (Game.Scene == null)
                return CommandResult.Error("No scene loaded");

            var systems = Game.Scene.GetSystems();
            return string.Join("\n", systems.Select(s => ">" + s.GetType().Name));
        }

        [Command(HelpString = "Remove the system with the given name")]
        private static CommandResult RemoveSystem(string typeName)
        {
            if (Game.Scene == null)
                return CommandResult.Error("No scene loaded");

            var system = Game.Scene.GetSystems().FirstOrDefault(s => s.GetType().Name == typeName);
            if (system == null)
                return CommandResult.Error($"There is no system that matches \"{typeName}\"");
            var type = system.GetType();
            var removeMethod = typeof(Scene).GetMethod(nameof(Game.Scene.RemoveSystem))?.MakeGenericMethod(type);
            bool success = (bool)(removeMethod?.Invoke(Game.Scene, null) ?? false);
            return success ? type.Name + " removed" : CommandResult.Error("Could not remove " + type.Name);
        }

        [Command(HelpString = "List all entities in the scene")]
        private static CommandResult ListEntities()
        {
            if (Game.Scene == null)
                return CommandResult.Error("No scene loaded");

            var systems = Game.Scene.GetAllEntities();
            return string.Join("\n", systems.Select(s => ">" + s));
        }

        [Command(HelpString = "List all components in an entity. Expects an integer")]
        private static CommandResult ListComponents(int entityID)
        {
            if (Game.Scene == null)
                return CommandResult.Error("No scene loaded");

            if (!Game.Scene.HasEntity(entityID))
                return "No matching entity found";

            var components = Game.Scene.GetAllComponentsFrom(entityID);
            return string.Join("\n", components.OfType<object>().Select(s => ">" + s.GetType().Name));
        }

        [Command(HelpString = "Remove a component from an entity. Expects an integer and a string")]
        private static CommandResult RemoveComponent(int entityID, string componentType)
        {
            if (Game.Scene == null)
                return CommandResult.Error("No scene loaded");

            if (!Game.Scene.HasEntity(entityID))
                return CommandResult.Error("No matching entity found");

            var component = Game.Scene.GetAllComponentsFrom(entityID).OfType<object>().FirstOrDefault(s => s.GetType().Name == componentType);
            if (component == null)
                return CommandResult.Error($"{entityID} has no component that matches \"{componentType}\"");
            var type = component.GetType();
            var removeMethod = typeof(Scene).GetMethod(nameof(Game.Scene.DetachComponent))?.MakeGenericMethod(type);
            bool success = (bool)(removeMethod?.Invoke(Game.Scene, new object[] { new Entity { Identity = entityID } }) ?? false);
            return success ? type.Name + " removed" : CommandResult.Error("Could not remove " + type.Name);
        }

        [Command(HelpString = "Remove and entity. Expects an integer")]
        private static CommandResult RemoveEntity(int entityID)
        {
            if (Game.Scene == null)
                return CommandResult.Error("No scene loaded");

            Game.Scene.RemoveEntity(entityID);
            return entityID + " removed";
        }

        [Command(HelpString = "List all commands")]
        private static string List()
        {
            var builder = new StringBuilder();
            foreach (var item in CommandProcessor.GetAllCommands())
            {
                builder.Append(item.Item1);
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
        }
    }
}
