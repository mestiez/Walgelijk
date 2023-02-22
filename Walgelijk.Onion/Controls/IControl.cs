namespace Walgelijk.Onion.Controls;

public interface IControl
{
    public void OnAdd(in ControlParams p);

    public void OnStart(in ControlParams p);

    public void OnProcess(in ControlParams p);
    public void OnRender(in ControlParams p);

    public void OnEnd(in ControlParams p);

    public void OnRemove(in ControlParams p);
}

public static class IControlExtensions
{
    public static void ConsiderParentScroll(this IControl c)
    {
        TODO!!
    }
}
