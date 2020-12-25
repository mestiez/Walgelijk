using System;
using System.Text;

namespace Walgelijk
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "These are console commands, they are used and invoked via reflection")]
    internal struct DebugCommands
    {
        [Command]
        private static void ShowStats(bool state)
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
        private static string FpsCap(int target = 0)
        {
            Game.Main.Window.TargetFrameRate = target;
            return "Target frame render rate set to " + target;
        }

        [Command]
        private static string UpsCap(int target = 0)
        {
            Game.Main.Window.TargetUpdateRate = target;
            return "Target update rate set to " + target;
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
