﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.OpenTK;
using Walgelijk.ParticleSystem;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public class Program
{
    private static Game game = new Game(
            new OpenTKWindow("playground", new Vector2(-1, -1), new Vector2(800, 600)),
            new OpenALAudioRenderer()
            );

    static void Main(string[] args)
    {
        try
        {
            var p = Process.GetCurrentProcess();
            p.PriorityBoostEnabled = true;
            p.PriorityClass = ProcessPriorityClass.High;
        }
        catch (global::System.Exception e)
        {
            Logger.Warn($"Failed to set process priority: {e}");
        }

        game.UpdateRate = 0;
        game.FixedUpdateRate = 60;
        game.Console.DrawConsoleNotification = true;
        game.Window.VSync = false;

        TextureLoader.Settings.FilterMode = FilterMode.Linear;

        Resources.SetBasePathForType<AudioData>("audio");
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");

        //game.Scene = SplashScreen.CreateScene(Resources.Load<Texture>("opening_bg.png"), new[]
        //{
        //    new SplashScreen.Logo(Resources.Load<Texture>("splash1.png"), new Vector2(180, 0), 5, new Sound(Resources.Load<AudioData>("opening.wav"), false, false)),
        //    new SplashScreen.Logo(Resources.Load<Texture>("splash2.png"), new Vector2(180, 0), 3f),
        //    new SplashScreen.Logo(Resources.Load<Texture>("splash3.png"), new Vector2(180, 0), 3f),
        //
        //}, () => game.Scene = new AudioWaveScene().Load(game), SplashScreen.Transition.FadeInOut);

        game.Scene = new AudioWaveScene().Load(game);

#if DEBUG
        game.DevelopmentMode = true;
#else
        game.DevelopmentMode = false;
#endif
        game.Window.SetIcon(Resources.Load<Texture>("icon.png"));
        game.Profiling.DrawQuickProfiler = false;

        game.Start();
    }
}

public interface IOscillator
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void Evaluate(double time, Grid<Cell> field);
}

public interface ISceneCreator
{
    public Scene Load(Game game);
}

public struct AudioWaveScene : ISceneCreator
{
    public class AudioWaveWorldComponent : IDisposable
    {
        public readonly int Width, Height;

        public (int x, int y) ListenerPosition;

        public int ThreadsRunning = 0;
        public readonly ManualResetEvent ThreadReset;
        public readonly AudioWaveSystem.WorkGroupParams[] ThreadData;
        public readonly int ThreadCount ;

        public float VelocityRetainment = .99999f;
        public float ValueTransferRate = 0.48f;

        public int SampleRate => (int)(1 / TimeStep);

        public double TimeStep = 1 / 500d;
        public double Time = 0;

        public Grid<Cell> Field;
        public List<IOscillator> Oscillators = new();

        public InstancedShapeRenderTask RenderTask;

        private readonly Vector2AttributeArray positions;
        private readonly Vector4AttributeArray colors;

        public string OutputFile;
        public List<byte> OutputFileData = new();

        public AudioWaveWorldComponent(int width, int height, string outputPath, int threadCount)
        {
            var data = new Cell[width * height];
            for (int i = 0; i < data.Length; i++)
            {
                Grid<Cell>.GetCoordinatesFromIndex(i, width, out int x, out int y);
                data[i] = new Cell(0, x, y);
            }

            OutputFile = outputPath;
            ThreadCount = threadCount;
            ListenerPosition = (width / 2, height - 2);

            Field = new(width, height, data);

            Width = width;
            Height = height;

            var mt = new Material(new Shader(
                File.ReadAllText("resources/shaders/instanced-worldspace-vertex.vert"),
                File.ReadAllText("resources/shaders/textured-fragment.frag")
                ));

            mt.SetUniform(ShaderDefaults.MainTextureUniform, Texture.White);

            positions = new Vector2AttributeArray(new Vector2[Width * Height]);
            colors = new Vector4AttributeArray(new Vector4[Width * Height]);

            RenderTask = new InstancedShapeRenderTask(
                new VertexBuffer(PrimitiveMeshes.CenteredQuad.Vertices, PrimitiveMeshes.CenteredQuad.Indices, new VertexAttributeArray[] {
                    positions,
                    colors
                }),
                Matrix3x2.Identity, mt);

            RenderTask.InstanceCount = Width * Height;
            RenderTask.ScreenSpace = true;
            RenderTask.ModelMatrix = Matrix3x2.CreateScale(2);

            for (int i = 0; i < Field.Flat.Length; i++)
            {
                Field.GetCoordinatesFromIndex(i, out var x, out var y);
                positions.Data[i] = new Vector2(x, y);
            }

            ThreadReset = new ManualResetEvent(false);
            ThreadData = new AudioWaveSystem.WorkGroupParams[ThreadCount];

            int stride = Field.Flat.Length / ThreadCount;
            int startIndex = 0;
            int endIndex = 0;

            for (int i = 0; i < ThreadCount; i++)
            {
                startIndex = endIndex;
                endIndex = startIndex + stride;
                ThreadData[i] = new AudioWaveSystem.WorkGroupParams(this, startIndex, endIndex - 1);
            }
        }

        public void UpdateTask()
        {
            for (int i = 0; i < Field.Flat.Length; i++)
            {
                var cell = Field.Flat[i];
                Color color;
                if (cell.Previous > 0)
                    color = Utilities.Lerp(Colors.Black, Colors.Cyan, Utilities.Clamp(cell.Previous));
                else
                    color = Utilities.Lerp(Colors.Black, Colors.Red, Utilities.Clamp(-cell.Previous));

                color = Utilities.Lerp(color, Colors.White, Utilities.Clamp(MathF.Abs(cell.Velocity)));
                color = Utilities.Lerp(color, Colors.Green, Utilities.Clamp(cell.Absorption - 1));

                colors.Data[i] = color;
            }
            RenderTask.VertexBuffer.ExtraDataHasChanged = true;
        }

        public void Dispose()
        {
            RenderTask.VertexBuffer.Dispose();
            RenderTask.Material.Dispose();
            ThreadReset.Dispose();
        }
    }

    public class AudioWaveSystem : Walgelijk.System
    {
        private BufferedWaveProvider w;
        private WaveOutEvent waveOut;
        byte[] buffer;
        int bufferPos = 0;

        public override void Initialise()
        {
            if (!Scene.FindAnyComponent<AudioWaveWorldComponent>(out var world))
                throw new Exception("no world");

            float timeScale = world.SampleRate / (float)Game.FixedUpdateRate;
            int mSampleRate = (int)(world.SampleRate / timeScale);

            buffer = new byte[64];
            w = new BufferedWaveProvider(new WaveFormat(mSampleRate, 8, 1));
            w.BufferLength = mSampleRate;
            w.DiscardOnBufferOverflow = false;

            waveOut = new WaveOutEvent();
            waveOut.Init(w);
            waveOut.Play();
        }

        public override void Render()
        {
            if (!Scene.FindAnyComponent<AudioWaveWorldComponent>(out var world))
                return;

            world.UpdateTask();
            RenderQueue.Add(world.RenderTask);

            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.TransformMatrix = world.RenderTask.ModelMatrix;

            Draw.Colour = Colors.Yellow;
            Draw.Circle(new Vector2(world.ListenerPosition.x, world.ListenerPosition.y), new Vector2(5));
        }

        public override void Update()
        {
            if (!Scene.FindAnyComponent<AudioWaveWorldComponent>(out var world))
                return;

            var field = world.Field;

            if (Input.IsButtonReleased(Button.Middle))
                for (int i = 0; i < field.Flat.Length; i++)
                    field.Flat[i].ForceSet(0);

            if (!Matrix3x2.Invert(world.RenderTask.ModelMatrix, out var inverse))
                inverse = default;

            var transformPosition = Vector2.Transform(Input.WindowMousePosition, inverse);

            if (Input.IsKeyPressed(Key.E))
            {
                world.ListenerPosition.x = Utilities.Clamp((int)transformPosition.X, 0, world.Width - 1);
                world.ListenerPosition.y = Utilities.Clamp((int)transformPosition.Y, 0, world.Height - 1);
            }

            if (Input.IsKeyPressed(Key.Space))
                for (int x = 0; x < field.Width; x++)
                    for (int y = 0; y < field.Height; y++)
                    {
                        var dist = Vector2.Distance(transformPosition, new Vector2(x, y));
                        if (dist < 20)
                        {
                            field.Get(x, y).ForceSet(-5);
                            if (dist < 4)
                                field.Get(x, y).ForceSet(40);
                        }
                    }

            if (Input.IsButtonHeld(Button.Left))
            {
                for (int x = 0; x < field.Width; x++)
                    for (int y = 0; y < field.Height; y++)
                        if (Vector2.Distance(transformPosition, new Vector2(x, y)) < 5)
                        {
                            var cell = field.Get(x, y);
                            cell.Absorption = 231.5f;
                            cell.VelocityAbsorption = 231.5f;
                        }
            }

            if (Input.IsButtonHeld(Button.Right))
            {
                for (int x = 0; x < field.Width; x++)
                    for (int y = 0; y < field.Height; y++)
                        if (Vector2.Distance(transformPosition, new Vector2(x, y)) < 5)
                        {
                            var cell = field.Get(x, y);
                            cell.Absorption = 1;
                            cell.VelocityAbsorption = 1;
                        }
            }

            // world.ListenerPosition.x = Utilities.Clamp((int)transformPosition.X, 0, world.Width - 1);
            // world.ListenerPosition.y = Utilities.Clamp((int)transformPosition.Y, 0, world.Height - 1);

            if (Input.IsKeyReleased(Key.Enter))
            {
                using var file = File.Open(world.OutputFile, FileMode.Create, FileAccess.Write);
                foreach (var b in world.OutputFileData)
                    file.WriteByte(b);
                file.Dispose();
                Logger.Log("Written output to file");
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override void FixedUpdate()
        {
            if (!Scene.FindAnyComponent<AudioWaveWorldComponent>(out var world))
                return;

            var field = world.Field;

            world.Time += world.TimeStep;

            for (int i = 0; i < field.Flat.Length; i++)
                field.Flat[i].Sync();

            foreach (var oscillator in world.Oscillators)
                oscillator.Evaluate(world.Time, field);

            world.ThreadsRunning = world.ThreadCount;
            world.ThreadReset.Reset();

            foreach (var data in world.ThreadData)
                ThreadPool.QueueUserWorkItem(ProcessGroup, data, true);

            world.ThreadReset.WaitOne();

            SampleAudioAtListener(world);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void SampleAudioAtListener(AudioWaveWorldComponent world)
        {
            const float gain = 2;
            //const int kernelhsize = 1;

            //float avg = 0;
            //int c = 0;

            //for (int x = world.ListenerPosition.x - kernelhsize; x < world.ListenerPosition.x + kernelhsize; x++)
            //    for (int y = world.ListenerPosition.y - kernelhsize; y < world.ListenerPosition.y + kernelhsize; y++)
            //    {
            //        c++;
            //        avg += world.Field.Get(x, y).Current;
            //    }

            //avg /= c;

            byte b = (byte)(Utilities.MapRange(-1, 1, 0, 255, world.Field.Get(world.ListenerPosition.x, world.ListenerPosition.y).Current * gain));
            world.OutputFileData.Add(b);
            buffer[bufferPos++] = b;
            if (bufferPos >= buffer.Length)
            {
                w.AddSamples(buffer, 0, buffer.Length);
                bufferPos = 0;

                if (w.BufferedBytes > w.BufferLength)
                    w.ClearBuffer();
            }
        }

        public class WorkGroupParams
        {
            public readonly AudioWaveWorldComponent World;
            public readonly Grid<Cell> Field;
            public readonly int Start;
            public readonly int End;
            public readonly Cell[] Data;

            public WorkGroupParams(AudioWaveWorldComponent world, int start, int end)
            {
                this.World = world;
                this.Field = world.Field;
                this.Start = start;
                this.End = end;

                Data = world.Field.Flat.AsSpan(start, end-start+1).ToArray();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void ProcessGroup(WorkGroupParams data)
        {
            for (int i = 0; i < data.Data.Length; i++)
            {
                var cell = data.Data[i];

                //cell.Velocity += (0 - cell.Previous) * data.world.ValueDropRate;
                int x = cell.X;
                int y = cell.Y;

                cell.Velocity += GetDelta(x - 1, y, cell.Previous, data.World);
                cell.Velocity += GetDelta(x + 1, y, cell.Previous, data.World);
                cell.Velocity += GetDelta(x, y - 1, cell.Previous, data.World);
                cell.Velocity += GetDelta(x, y + 1, cell.Previous, data.World);

                cell.Velocity *= data.World.VelocityRetainment;
                cell.Velocity /= cell.VelocityAbsorption;

                cell.Current += cell.Velocity;

                cell.Current /= cell.Absorption;
            }

            if (Interlocked.Decrement(ref data.World.ThreadsRunning) == 0)
                data.World.ThreadReset.Set();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static float GetDelta(int x, int y, float value, AudioWaveWorldComponent world)
        {
            if (x < 0 || y < 0 || x >= world.Width || y >= world.Height)
                return  -value * 0.1f;

            var o = world.Field.Get(x, y);
            return (o.Previous - value) * world.ValueTransferRate / o.Absorption;
        }
    }

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
        ThreadPool.SetMaxThreads(world.ThreadCount, 4);
        world.RenderTask.ModelMatrix = Matrix3x2.CreateScale(4);

        world.ListenerPosition = (5, 5);

        world.Oscillators.Add(new FileOscillator("resources/bf1942.raw", new Vector2(15, 26)) { Volume = 7 });
        //world.Oscillators.Add(new FileOscillator("resources/bf1942.raw", new Vector2(724, 80)) { Volume = 9 });
        // world.Oscillators.Add(new FileOscillator("resources/bf1942.raw", new Vector2(120, 120)) { Volume = 0.5f });
        //world.Oscillators.Add(new FileOscillator("resources/politie.raw", new Vector2(25,25)));
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
            cell.VelocityAbsorption = 0.8f * p.R + 1;
            cell.Absorption = 0.3f * p.R + 1;
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

public class SineOscillator : IOscillator
{
    public float FrequencyHertz;
    public (int x, int y) Position;

    public SineOscillator(float frequencyHertz, Vector2 position)
    {
        FrequencyHertz = frequencyHertz;
        Position = ((int)position.X, (int)position.Y);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        //if (time % 1 < 0.05f)
        {
            var value = Math.Sin(time * FrequencyHertz * Math.Tau);
            field.Get(Position.x, Position.y).ForceSet((float)value * 2);
        }
    }
}

public class NoiseOscillator : IOscillator
{
    public (int x, int y) Position;

    public NoiseOscillator(Vector2 position)
    {
        Position = ((int)position.X, (int)position.Y);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        field.Get(Position.x, Position.y).ForceSet(Utilities.RandomFloat(-2, 2));
    }
}

public class ExplosionOscillator : IOscillator
{
    public (int x, int y) Position;
    public float Volume = 0.5f;

    public ExplosionOscillator(Vector2 position)
    {
        Position = ((int)position.X, (int)position.Y);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        double t = time % 3;
        const double duration = 0.4d;

        var radius = Utilities.RandomFloat(7, 9) * (t / duration);
        var intensity = Math.Max(0, Math.Min(1, Utilities.MapRange(0, duration, 1, 0, t)));
        intensity *= intensity;

        if (intensity > 0)
            for (int x = 0; x < field.Width; x++)
                for (int y = 0; y < field.Height; y++)
                    if (Vector2.Distance(new Vector2(Position.x, Position.y), new Vector2(x, y)) < radius)
                    {
                        field.Get(x, y).ForceSet(Volume * (float)(-5 + (Utilities.RandomFloat(-3, 3) * (1 - intensity) * intensity)));
                    }
    }
}

public class FileOscillator : IOscillator
{
    public (int x, int y) Position;
    public bool Loops = true;
    public float Volume = 1;

    public readonly byte[] InputData;

    private int i = 0;

    public FileOscillator(string path, Vector2 position)
    {
        Position = ((int)position.X, (int)position.Y);
        InputData = File.ReadAllBytes(path);
    }

    public void Evaluate(double time, Grid<Cell> field)
    {
        //Position.x = (int)Utilities.Lerp(0, field.Width - 1, ((float)time * 2.8f) % 1);
        //Position.y = (int)Utilities.Lerp(0, field.Height - 1, ((float)time * 0.5f) % 1);
        var b = InputData[Loops ? (i++ % InputData.Length) : Utilities.Clamp(i++, 0, InputData.Length - 1)];
        var v = Utilities.MapRange(0, 255, -1, 1, b);
        field.Get(Position.x, Position.y).ForceSet(v * Volume);
    }
}
