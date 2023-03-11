namespace Walgelijk.OpenTK
{
    internal struct AudioObjects
    {
        public static readonly FixedAudioCache FixedBuffers = new();
        public static readonly SourceCache Sources = new();
        public static readonly OggStreamerCache OggStreamers = new();
    }
}
