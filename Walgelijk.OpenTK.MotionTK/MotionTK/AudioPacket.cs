using FFmpeg.AutoGen;
using System.Runtime.InteropServices;

namespace MotionTK;

public unsafe class AudioPacket : Packet
{
    public readonly short[] SampleBuffer;
    public readonly int TotalSampleCount;

    public AudioPacket(byte* sampleBuffer, int sampleCount, int channelCount)
    {
        TotalSampleCount = sampleCount * channelCount;
        SampleBuffer = new short[TotalSampleCount];
        Marshal.Copy((IntPtr)sampleBuffer, SampleBuffer, 0, TotalSampleCount);

        //Buffer.BlockCopy(sampleBuffer, 0, SampleBuffer, 0, TotalSampleCount);
        //// copy buffer
        //for (int i = 0; i < TotalSampleCount; i++)
        //{
        //    SampleBuffer[i] = ((short*)sampleBuffer)[i];
        //}
    }
}
