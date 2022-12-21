namespace MotionTK
{
    public unsafe class AudioPacket : Packet
    {
        public readonly byte[] SampleBuffer;
        public readonly int TotalSampleCount;

        public AudioPacket(byte* sampleBuffer, int sampleCount, int channelCount)
        {
            TotalSampleCount = sampleCount * channelCount;
            SampleBuffer = new byte[TotalSampleCount];

            // copy buffer
            for (int i = 0; i < TotalSampleCount; i++)
            {
                SampleBuffer[i] = ((byte*)sampleBuffer)[i];
            }
        }
    }
}
