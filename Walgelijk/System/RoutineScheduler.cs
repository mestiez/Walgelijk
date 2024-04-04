using System;
using System.Collections.Generic;

namespace Walgelijk;

/// <summary>
/// Routines are like Unity coroutines.<br></br>
/// These are <b>NOT</b> threads.<br></br>
/// They're not async or parallel, but instead ran on the main thread.
/// </summary>
public static class RoutineScheduler
{
    private static readonly List<Routine> routines = [];

    /// <summary>
    /// Start a routine.
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static Routine Start(IEnumerator<IRoutineCommand> task)
    {
        var routine = new Routine
        {
            Task = task
        };
        routines.Add(routine);
        return routine;
    }

    /// <summary>
    /// Stop a routine.
    /// </summary>
    /// <param name="routine"></param>
    public static void Stop(Routine routine)
    {
        routines.Remove(routine);
    }

    /// <summary>
    /// Stop any and all routines.
    /// </summary>
    public static void StopAll()
    {
        routines.Clear();
    }

    /// <summary>
    /// Is this routine running?
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public static bool IsOngoing(Routine? routine)
    {
        return routine != null && routines.Contains(routine);
    }

    /// <summary>
    /// Runs all the routines.
    /// </summary>
    /// <param name="dt"></param>
    public static void StepRoutines(float dt)
    {
        for (int i = routines.Count - 1; i >= 0; i--)
        {
            if (routines.Count <= i)
                continue;

            var routine = routines[i];
            var item = routine.Task;
            if (item == null)
                continue;

            var cmd = item.Current;

            if (cmd == null)
                advance();
            else if (cmd.CanAdvance(dt))
                advance();

            void advance()
            {
                if (!item.MoveNext())
                    routines.Remove(routine);
            }
        }
    }
}

public class Routine
{
    public IEnumerator<IRoutineCommand>? Task;
}

public interface IRoutineCommand
{
    public bool CanAdvance(float deltaTime);
}

public struct RoutineDelay : IRoutineCommand
{
    public float DelayInSeconds;
    public float CurrentTime;

    public RoutineDelay(float delayInSeconds)
    {
        DelayInSeconds = delayInSeconds;
        CurrentTime = 0;
    }

    public bool CanAdvance(float dt)
    {
        CurrentTime += dt;
        return CurrentTime >= DelayInSeconds;
    }
}

public struct RoutineFrameDelay : IRoutineCommand
{
    public bool CanAdvance(float dt) => true;
}

public struct RoutineWaitUntil : IRoutineCommand
{
    public readonly Func<bool> Function;

    public RoutineWaitUntil(Func<bool> function)
    {
        Function = function;
    }

    public bool CanAdvance(float dt) => Function();
}
