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
            Game.Compositor.Flags = RenderTextureFlags.DepthStencil;
            RenderQueue.Add(new ClearRenderTask(), RenderOrder.Bottom);
            //RenderQueue.Add(task);
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

            g.DrawQuad(new Rect(25, 25, 300, 150), Material.DefaultTextured);

            g.Stencil = StencilState.Disabled;

            g.DrawQuad(new Rect(new Vector2(512), new Vector2(64)), Material.DefaultTextured);
        }
    }
}

public struct AudioTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new AudioTestSystem());

        game.UpdateRate = 120;
        game.FixedUpdateRate = 50;

        return scene;
    }

    public class AudioTestSystem : Walgelijk.System
    {
        private Sound OneShot = new Sound(Resources.Load<FixedAudioData>("cannot-build.wav"));
        private Sound Streaming = new Sound(Resources.Load<StreamAudioData>("warning_party_imminent.ogg"));

        public override void FixedUpdate()
        {
            if (Input.IsKeyHeld(Key.Space))
                Audio.PlayOnce(OneShot);

            if (Input.IsKeyPressed(Key.Enter))
            {
                Audio.Play(Streaming);
            }
        }

        public override void Update()
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Colour = Colors.Black;
            Draw.Quad(new Rect(0, 0, Window.Width, Window.Height));

            Draw.Colour = Colors.White;
            if (Audio is Walgelijk.OpenTK.OpenALAudioRenderer audio)
            {
                Draw.Text("Sources in use: " + audio.TemporarySourceBuffer.Count(), new Vector2(30, 30), Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Top);
                Draw.Text("Sources created: " + audio.CreatedTemporarySourceCount, new Vector2(30, 40), Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Top);
            }
        }
    }
}