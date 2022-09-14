using NAudio.Wave;
using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public class AudioWaveSystem : Walgelijk.System
{
    private BufferedWaveProvider? bufferedWaveProvider;
    private WaveOutEvent? waveOut;
    //private byte[]? buffer;
    //private int bufferPos = 0;

    //private static readonly float[] random = new float[512];

    public override void Initialise()
    {
        //for (int i = 0; i < random.Length; i++)
        //    random[i] = Utilities.RandomFloat();

        if (!Scene.FindAnyComponent<AudioWaveWorldComponent>(out var world))
            throw new Exception("no world");

        float timeScale = world.SampleRate / (float)Game.FixedUpdateRate;
        int mSampleRate = (int)(world.SampleRate / timeScale);

        //buffer = new byte[64 * 4];
        bufferedWaveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(mSampleRate, 1))
        {
            BufferLength = mSampleRate,
            DiscardOnBufferOverflow = true
        };

        waveOut = new WaveOutEvent();
        waveOut.Init(bufferedWaveProvider);
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
                    if (dist < Utilities.RandomFloat(20, 5))
                    {
                        field.Get(x, y).ForceSet(-5);
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

        world.ThreadsRunning = world.ThreadCount;
        world.ThreadReset.Reset();
        foreach (var data in world.ThreadData)
            ThreadPool.QueueUserWorkItem(SyncField, data, true);
        world.ThreadReset.WaitOne();

        foreach (var oscillator in world.Oscillators)
            oscillator.Evaluate(world.Time, field);

        world.ThreadsRunning = world.ThreadCount;
        world.ThreadReset.Reset();
        foreach (var data in world.ThreadData)
            ThreadPool.QueueUserWorkItem(StepSimulation, data, true);
        world.ThreadReset.WaitOne();

        SampleAudioAtListener(world);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void SampleAudioAtListener(AudioWaveWorldComponent world)
    {
        const float gain = 1;
        var v = world.Field.Get(world.ListenerPosition.x, world.ListenerPosition.y).Current * gain;
        var bb = BitConverter.GetBytes(v);
        world.OutputFileData.AddRange(bb);
        bufferedWaveProvider.AddSamples(bb, 0, bb.Length);
        if (bufferedWaveProvider.BufferedBytes >= bufferedWaveProvider.BufferLength)
            bufferedWaveProvider.ClearBuffer();
    }

    public class WorkGroupParams
    {
        public readonly AudioWaveWorldComponent World;
        public readonly Grid<Cell> Field;
        public readonly int Start;
        public readonly int End;
        //  public readonly Cell[] Data;

        public WorkGroupParams(AudioWaveWorldComponent world, int start, int end)
        {
            this.World = world;
            this.Field = world.Field;
            this.Start = start;
            this.End = end;
            //  Data = world.Field.Flat.AsSpan(start, end - start).ToArray();
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void StepSimulation(WorkGroupParams data)
    {
        for (int i = data.Start; i < data.End; i++)
        {
            var cell = data.Field.Flat[i];

            float tr = data.World.ValueTransferRate + cell.ConductivityAdd;

            float d1 = GetDeltaCell(cell.Left, cell.Previous, tr);
            float d2 = GetDeltaCell(cell.Right, cell.Previous, tr);
            float d3 = GetDeltaCell(cell.Up, cell.Previous, tr);
            float d4 = GetDeltaCell(cell.Down, cell.Previous, tr);
            cell.Velocity += d1 + d2 + d3 + d4;

            cell.Velocity *= data.World.VelocityRetainment;
            cell.Velocity /= cell.VelocityAbsorption;
            cell.Current += cell.Velocity;
            cell.Current /= cell.Absorption;

            //global::OpenTK.Graphics.OpenGL.GL.DispatchCompute()
        }

        if (Interlocked.Decrement(ref data.World.ThreadsRunning) == 0)
            data.World.ThreadReset.Set();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void SyncField(WorkGroupParams data)
    {
        for (int i = data.Start; i < data.End; i++)
        {
            var cell = data.Field.Flat[i];
            cell.Previous = cell.Current;
        }

        if (Interlocked.Decrement(ref data.World.ThreadsRunning) == 0)
            data.World.ThreadReset.Set();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static float GetDeltaCell(Cell o, float value, float transferRate)
    {
        if (o == null)
            return -value;
        return (o.Previous - value) * transferRate / o.Absorption;
    }
}
