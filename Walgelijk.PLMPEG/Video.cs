using System.Collections.Concurrent;

namespace Walgelijk.PLMPEG;

public class Video : IDisposable, IAudioStream, IReadableTexture
{
    public bool IsPlaying = false;
    public Stream BaseStream { get; }
    public float Framerate => (float)plm.FrameRate;
    public TimeSpan Duration => TimeSpan.FromSeconds(plm.Duration);
    public TimeSpan Time => TimeSpan.FromSeconds(plm.Time);
    public bool Loop
    {
        get => plm.Loop;
        set => plm.Loop = value;
    }

    private readonly bool leaveOpen;
    private readonly NativeBinding.Plm plm;

    private Thread updateThread;
    public Sound Sound { get; private set; }

    long IAudioStream.Position
    {
        get => audioBuffer.ReadCursor;
        set => plm.Seek(audioBuffer.ReadCursor / plm.SampleRate / 2d, true);
    }

    TimeSpan IAudioStream.TimePosition
    {
        get => Time;
        set => plm.Seek(value.TotalSeconds, true);
    }

    public int Width => texture.Width;
    public int Height => texture.Height;
    public WrapMode WrapMode => texture.WrapMode;
    public FilterMode FilterMode => texture.FilterMode;
    public bool HDR => texture.HDR;
    public bool GenerateMipmaps => texture.GenerateMipmaps;
    public bool NeedsUpdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool DisposeLocalCopyAfterUpload => throw new NotImplementedException();

    private readonly MediaBuffer<float> audioBuffer = new(new float[64 * 512]);
    private readonly Texture texture;

    public Video(Stream baseStream, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        this.leaveOpen = leaveOpen;

        plm = NativeBinding.Plm.CreateWithFilename("sodium_chloride.mpg") ?? throw new Exception("Failed to initialise PLM buffer");
        plm.Loop = true;

        //plm = NativeBinding.Plm.CreateWithBuffer(baseStream, true) ?? throw new Exception("Failed to initialise PLM buffer");
        //plm.SetAudioDecodeCallback(OnAudioDecode);
        //plm.SetVideoDecodeCallback(OnVideoDecode);

        Sound = new(new StreamAudioData(() => this, plm.SampleRate, 2, (long)(plm.SampleRate * plm.Duration)));
        Sound.Pitch = 1;

        texture = new Texture(plm.Width, plm.Height, true, false);

        updateThread = new Thread(Update);
        updateThread.IsBackground = true;
        updateThread.Start();
    }


    private void Update(object? obj)
    {
        while (plm != null)
        {
            if (!IsPlaying)
                Thread.Sleep(16);
            else
            {
                if (plm.HasEnded)
                    return;

                while (audioBuffer.Gap < 16 * 512)
                {
                    var samples = plm.DecodeAudio();
                    if (samples.HasValue)
                    {
                        audioBuffer.Push(samples.Value.Interleaved);
#if DEBUG
                        Console.WriteLine("gap {0} @ {1}", audioBuffer.Gap, samples.Value.Time);
#endif
                    }
                }

                Thread.Sleep(16);
            }
        }
    }

    public void Play()
    {
        IsPlaying = true;
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void Dispose()
    {
        plm.Destroy();
        if (!leaveOpen)
            BaseStream.Dispose();
    }

    int IAudioStream.ReadSamples(Span<float> buffer)
    {
        if (!IsPlaying)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0;
            return buffer.Length;
        }

        audioBuffer.Take(buffer);
        return buffer.Length;
    }

    public Color GetPixel(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void DisposeLocalCopy()
    {
        throw new NotImplementedException();
    }

    public ReadOnlyMemory<Color>? GetData()
    {
        throw new NotImplementedException();
    }
}
