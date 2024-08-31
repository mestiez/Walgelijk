namespace Walgelijk.OpenTK;

public struct AudioFileData
{
    public short NumChannels;
    public int SampleRate;

    /// <summary>
    /// Sample count per chanel
    /// </summary>
    public long SampleCount;

    public float[]? Data;
}
