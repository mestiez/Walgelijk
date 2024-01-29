using System.Numerics;
using System.Runtime.ExceptionServices;
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

        public override void Update()
        {
            Draw.Reset();
            Draw.ScreenSpace = true;

            Draw.ClearMask();
            Draw.WriteMask();
            Draw.Circle(Input.WindowMousePosition, new Vector2(64));

            Draw.OutsideMask();
            Draw.Image(Resources.Load<Texture>("opening_bg.png"), new Rect(Window.Size / 2, new Vector2(512)), ImageContainmentMode.Contain);

            Draw.InsideMask();
            Draw.Image(Resources.Load<Texture>("qoitest.qoi"), new Rect(Window.Size / 2, new Vector2(512)), ImageContainmentMode.Contain);


            Draw.DisableMask();
        }

        public override void Render()
        {
            Game.Compositor.Enabled = false;
            RenderQueue.Add(new ClearRenderTask(), RenderOrder.Bottom);
            //RenderQueue.Add(task);
        }
    }

    public class StencilTestTask : IRenderTask
    {
        public void Execute(IGraphics g)
        {
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

            g.DrawQuad(new Rect(25, 25, 300, 150), Material.DefaultTextured);

            g.Stencil = StencilState.Disabled;

            g.DrawQuad(new Rect(new Vector2(512), new Vector2(64)), Material.DefaultTextured);
        }
    }
}
