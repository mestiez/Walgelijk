using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Walgelijk;

/// <summary>
/// Provides performance information
/// </summary>
public sealed class Profiler
{
    /// <summary>
    /// Amount of frames rendered in the last second
    /// </summary>
    public float FramesPerSecond => fpsCounter.Frequency;
    /// <summary>
    /// Enables or disables a small debug performance information display
    /// </summary>
    public bool DrawQuickProfiler { get; set; } = true;

    private readonly Game game;
    private readonly QuickProfiler quickProfiler;
    private readonly Stopwatch stopwatch;

    private readonly TickRateCounter fpsCounter = new();

    private readonly Stack<ProfiledTask> profiledTaskStack = new();
    private readonly List<ProfiledTask> profiledTasks = new();

    /// <summary>
    /// Create a profiler for the given game
    /// </summary>
    /// <param name="game"></param>
    public Profiler(Game game)
    {
        this.game = game;
        quickProfiler = new QuickProfiler(this);

        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    /// <summary>
    /// Force the profiler to calculate render information. Should be handled by the window.
    /// </summary>
    public void Tick()
    {
        CalculateFPS();

        if (DrawQuickProfiler)
            quickProfiler.Render(game.RenderQueue);

        profiledTasks.Clear();
    }

    /// <summary>
    /// Start a profiled task with a name
    /// </summary>
    public void StartTask(string name)
    {
        profiledTaskStack.Push(new ProfiledTask { Name = name, StartTick = stopwatch.ElapsedTicks });
    }

    /// <summary>
    /// End the ongoing profiled task
    /// </summary>
    /// <returns>The amount of time that has passed</returns>
    public TimeSpan EndTask()
    {
        if (!profiledTaskStack.TryPop(out var result))
            return default;
        result.EndTick = stopwatch.ElapsedTicks;
        profiledTasks.Add(result);
        return result.Duration;
    }

    /// <summary>
    /// Get all profiled tasks for this frame
    /// </summary>
    public IEnumerable<ProfiledTask> GetProfiledTasks()
    {
        foreach (var p in profiledTasks)
            yield return p;
    }

    private void CalculateFPS()
    {
        fpsCounter.Tick(game.State.Time.SecondsSinceLoad);
    }
}

/// <summary>
/// Structure that holds a task name and relevant time data
/// </summary>
public struct ProfiledTask
{
    /// <summary>
    /// Name
    /// </summary>
    public string Name;

    internal long StartTick;
    internal long EndTick;

    /// <summary>
    /// How long the task took
    /// </summary>
    public TimeSpan Duration => TimeSpan.FromTicks(EndTick - StartTick);
}

internal class TickRateCounter
{
    public float Frequency { get; private set; }

    public float MeasureInterval { get; set; } = 1f;

    private int counter;
    private float lastMeasurementTime;

    public void Tick(float currentTime)
    {
        counter++;
        if ((currentTime - lastMeasurementTime) > MeasureInterval)
        {
            lastMeasurementTime = currentTime;
            Frequency = counter / MeasureInterval;
            counter = 0;
        }
    }
}
