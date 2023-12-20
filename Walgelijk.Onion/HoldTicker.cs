namespace Walgelijk.Onion;

/// <summary>
/// Calculates the "hold ticker" for a control. 
/// A hold ticker is a timer that ticks repeatedly after a control is held for a set amount of time
/// It is meant for features such as "key repeat delay"
/// </summary>
public class HoldTicker
{
    public float MinHoldDuration = 0.5f;
    public float RepeatInterval = 0.06f;

    private float timer = 0;

    public bool IsTicked(ControlInstance inst)
    {
        timer += Game.Main.State.Time.DeltaTimeUnscaled;
        if (Onion.Input.MousePrimaryPressed || (inst.TimeSinceStateChange > MinHoldDuration && timer > RepeatInterval))
        {
            timer = 0;
            return true;
        }
        return false;
    }
}
