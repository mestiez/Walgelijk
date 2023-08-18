using FFmpeg.AutoGen;

namespace Walgelijk.OpenTK.MotionTK;

internal readonly struct FfmpegInitialiser
{
    public static bool Initialised = false;

    public static void Initialise()
    {
        if (Initialised)
            return;
        Initialised = true;

        ffmpeg.RootPath = Environment.CurrentDirectory;
        return;
    }
}
