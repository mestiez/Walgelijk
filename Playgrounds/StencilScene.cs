using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct StencilScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new StencilSystem());

        game.UpdateRate = 120;
        game.FixedUpdateRate = 8;

        return scene;
    }

    public class StencilSystem : Walgelijk.System
    {
        StencilTestTask task = new();

        public override void Render()
        {
            Game.Compositor.Flags = RenderTextureFlags.DepthStencil;
            RenderQueue.Add(task);
        }
    }

    public class StencilTestTask : IRenderTask
    {
        public void Execute(IGraphics g)
        {
            //TODO stencil clear werkt niet? iets met stencil buffer zelf maken?

            g.CurrentTarget.ProjectionMatrix = g.CurrentTarget.OrthographicMatrix;
            g.Clear(Colors.Purple.Brightness(0.2f));

            g.Stencil = new StencilState
            {
                Enabled = true,
                AccessMode = StencilAccessMode.Write,
            };

            g.DrawQuad(new Rect(Game.Main.State.Input.WindowMousePosition, new Vector2(250)), Material.DefaultTextured);

            g.Stencil = new StencilState
            {
                Enabled = true,
                AccessMode = StencilAccessMode.NoWrite,
                TestMode = StencilTestMode.Inside
            };

            g.DrawQuad(new Rect(25,25,300,150), Material.DefaultTextured);

            g.Stencil = StencilState.Disabled;

            g.DrawQuad(new Rect(new Vector2(512), new Vector2(64)), Material.DefaultTextured);
        }
    }
}