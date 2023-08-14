namespace Walgelijk.OpenTK.MotionTK;

public class VideoSystem : Walgelijk.System
{
    public override void Render()
    {
        foreach (var c in Scene.GetAllComponentsOfType<VideoComponent>())
            c.Video.UpdateAndRender(Graphics);
    }
}
