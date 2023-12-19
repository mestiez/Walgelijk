using Walgelijk;
using Walgelijk.Onion;

namespace Playgrounds;

public struct CompositorScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new CompositorTestSystem());
        scene.AddSystem(new OnionSystem());
        game.UpdateRate = 120;
        game.FixedUpdateRate = 30;

        game.Compositor.Enabled = true;

        game.Compositor.Clear();

        var mix = new MixNode();
        var source1 = new TextureValueNode(Resources.Load<Texture>("qoitest.qoi"));
        source1.Output.ConnectTo(mix.Inputs[0]);
        game.Compositor.SourceNode.Output.ConnectTo(mix.Inputs[1]);

        mix.Output.ConnectTo(game.Compositor.DestinationNode.Inputs[0]);

        return scene;
    }

    public class CompositorTestSystem : Walgelijk.System
    {
        public override void Render()
        {
            RenderQueue.Add(new ClearRenderTask(new Color(0.13f, 0.08f, 0.16f, 1)));
        }

        public override void Update()
        {
            Ui.Layout.Size(120, 32).Move(16);
            Ui.Checkbox(ref Game.Compositor.Enabled, "Enable compositor");


            if (Game.Compositor.DestinationNode.Inputs[0].Connected?.Node is MixNode mix)
                mix.Factor = MathF.Sin(Time.SecondsSinceSceneChange) * 0.5f + 0.5f;
        }
    }
}
