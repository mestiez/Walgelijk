using System.Numerics;
using Walgelijk;

namespace TestWorld;

public struct AudioWaveScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        const double timestep = 1d / 10000; //time resolution, 1 / [steps per second]
        const float visualTimescale = 1f / 1f; //x times slower than real time
        var tex = Texture.Load("resources/world.png", false);

        var scene = new Scene(game);

        var world = scene.AttachComponent(scene.CreateEntity(), new AudioWaveWorldComponent(tex.Width, tex.Height, "result.pcm", 8)
        {
            TimeStep = timestep
        });
        ThreadPool.SetMaxThreads(world.ThreadCount, 8);
        world.RenderTask.ModelMatrix = Matrix3x2.CreateScale(4);

        world.ListenerPosition = (tex.Width / 2, tex.Height / 2);

        world.Oscillators.Add(new PointFileOscillator("resources/daily_beat.raw", new Vector2(153, 35)) { Volume = 5 });
        world.Oscillators.Add(new PointFileOscillator("resources/bf1942.raw", new Vector2(29, 56)) { Volume = 1f });
        // world.Oscillators.Add(new FileOscillator("resources/bf1942.raw", new Vector2(120, 120)) { Volume = 0.5f });
      //  world.Oscillators.Add(new AreaFileOscillator("resources/politie.raw", new Vector2(204, 0), new Vector2(204, 112)) { Volume = 0.04f });
        //world.Oscillators.Add(new FileOscillator("resources/james.raw", new Vector2(15, 50)));
        //world.Oscillators.Add(new SineOscillator(500, new Vector2(64, 64)));
        // world.Oscillators.Add(new SineOscillator(400, new Vector2(25, 25)));
        //world.Oscillators.Add(new NoiseOscillator(new Vector2(64, 64)));
        // world.Oscillators.Add(new ExplosionOscillator(new Vector2(15, 15)));

        // world.AddWall(new Rect(64, 0, 80, 128 - 10), 2f);
        // world.AddWall(new Rect(64, 128 + 10, 80, 256), 2f);

        const int padding = 5;
        const float freq = 0.009f;
        foreach (var (x, y, cell) in world.Field)
        {
            var p = tex.GetPixel(x, y);
            cell.Absorption = 1.3f * p.R + 1;
            cell.VelocityAbsorption = 0.1f * p.G + 1;
            cell.ConductivityAdd = 0.05f * p.B;
            //cell.Absorption = 1 + (Noise.GetSimplex(x * freq, y * freq, 0) * 0.5f + 0.5f) * 0.002f;
            //if (x <= padding || x > world.Width - padding || y <= padding || y > world.Height - padding)
            //{
            //    cell.VelocityAbsorption = 5.25f;
            //    cell.Absorption = 5.25f;
            //}
        }

        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        scene.AddSystem(new AudioWaveSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#a8a3c1")
        });

        game.FixedUpdateRate = (int)((1 / timestep) * visualTimescale);
        game.Window.Size = Vector2.TransformNormal(new Vector2(world.Width, world.Height), world.RenderTask.ModelMatrix);

        return scene;
    }
}
