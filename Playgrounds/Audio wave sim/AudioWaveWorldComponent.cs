using System.Numerics;
using Walgelijk;

namespace TestWorld;

public class AudioWaveWorldComponent : Component, IDisposable
{
    public readonly int Width, Height;

    public (int x, int y) ListenerPosition;

    public int ThreadsRunning = 0;
    public readonly ManualResetEvent ThreadReset;
    public readonly AudioWaveSystem.WorkGroupParams[] ThreadData;
    public readonly int ThreadCount;

    public float VelocityDampening = 1;
    public float VelocityRetainment = .99995f;
    public float ValueTransferRate = 0.45f;

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
            Grid<Cell>.GetCoordinatesFromIndex(i, width, height, out int x, out int y);
            data[i] = new Cell(0, x, y);
        }


        OutputFile = outputPath;
        ThreadCount = threadCount;
        ListenerPosition = (width / 2, height - 2);

        Field = new(width, height, data);
        foreach (var cell in data)
        {
            Field.TryGet(cell.X - 1, cell.Y, out cell.Left);
            Field.TryGet(cell.X + 1, cell.Y, out cell.Right);
            Field.TryGet(cell.X, cell.Y + 1, out cell.Up);
            Field.TryGet(cell.X, cell.Y - 1, out cell.Down);
        }
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
            ThreadData[i] = new AudioWaveSystem.WorkGroupParams(this, startIndex, endIndex);
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
