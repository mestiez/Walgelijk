namespace MotionTK
{
    public unsafe class AudioPacket : Packet
    {
        public readonly short[] SampleBuffer;
        public readonly int TotalSampleCount;

        public AudioPacket(byte* sampleBuffer, int sampleCount, int channelCount)
        {
            TotalSampleCount = sampleCount * channelCount ;
            SampleBuffer = new short[TotalSampleCount];

            // copy buffer
            for (int i = 0; i < TotalSampleCount; i++)
            {
                SampleBuffer[i] = ((short*)sampleBuffer)[i];
            }
        }
    }
}
