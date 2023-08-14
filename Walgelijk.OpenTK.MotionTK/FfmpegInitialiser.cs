using FFmpeg.AutoGen;
using FFmpeg.Loader;
using System.Runtime.InteropServices;

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

        // from https://github.com/stjeong/ffmpeg_autogen_cs/blob/master/Libraries/FFmpegBinariesHelper.cs

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var current = Environment.CurrentDirectory;
            var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");

            while (current != null)
            {
                var ffmpegBinaryPath = Path.Combine(current, probe);

                if (Directory.Exists(ffmpegBinaryPath))
                {
                    Console.WriteLine($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                    ffmpeg.RootPath = ffmpegBinaryPath;
                    return;
                }

                current = Directory.GetParent(current)?.FullName;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            ffmpeg.RootPath = "/lib/x86_64-linux-gnu/";
        else
            throw new NotSupportedException();

    }
}
