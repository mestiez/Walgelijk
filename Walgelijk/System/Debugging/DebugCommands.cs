using System;
using System.Collections.Generic;
using System.Text;

namespace Walgelijk
{
    internal struct DebugCommands
    {
        [Command]
        private static void ToggleProfiler()
        {
            var inst = Game.Main.Profiling;
            inst.DrawQuickProfiler = !inst.DrawQuickProfiler;
            Logger.Log("Profiler " + (inst.DrawQuickProfiler ? "enabled" : "disabled"));
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
