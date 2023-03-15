namespace Walgelijk.Onion.Controls;

public static class ControlExtensions
{
    public static Node GetNode(this ControlInstance i) => Onion.Tree.Nodes[i.Identity];
    public static ControlInstance GetInstance(this Node n) => Onion.Tree.EnsureInstance(n.Identity);

    public static float GetAnimationTime(this Node n)
    {
        var instance = n.GetInstance();
        return n.AliveLastFrame ? n.SecondsAlive / instance.AllowedDeadTime : (1 - n.SecondsDead / instance.AllowedDeadTime);
    }

    public static float GetAnimationTime(this ControlInstance i)
    {
        var n = i.GetNode();
        return n.AliveLastFrame ? n.SecondsAlive / i.AllowedDeadTime : (1 - n.SecondsDead / i.AllowedDeadTime);
    }
}