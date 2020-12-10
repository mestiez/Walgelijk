using System;
using System.Text;

namespace Walgelijk
{
    internal struct DebugCommands
    {
        [Command]
        private static void Profiler(bool state)
        {
            var inst = Game.Main.Profiling;
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
            Game.Main.Console.Clear();
        }
        
        [Command]
        private static void Quit()
        {
            Game.Main.Stop();
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
