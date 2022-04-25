using FFmpeg.AutoGen;
using System;

namespace Walgelijk.Video
{
    public class Video
    {
        public readonly string Path;

        public Video(string path)
        {
            Path = path;

            VideoManager.GetFrame(VideoManager.OpenFile(path));
        }
    }

    internal unsafe class LoadedVideo
    {
        public AVCodecContext* CodecContext;
        public AVCodec* Codec;
    }

    public static class VideoManager
    {
        static VideoManager()
        {
            ffmpeg.RootPath = $"runtimes/win-x64/native";
        }

        internal static unsafe LoadedVideo OpenFile(string path)
        {
            AVFormatContext* formatContext = null;

            if (ffmpeg.avformat_open_input(&formatContext, path, null, null) != 0)
            {
                ffmpeg.avformat_close_input(&formatContext);
                Logger.Error($"Failed to open media file at \"{path}\"");
                return null;
            }

            if (ffmpeg.avformat_find_stream_info(formatContext, null) < 0)
            {
                ffmpeg.avformat_close_input(&formatContext);
                Logger.Error($"Failed to read stream info at \"{path}\"");
                return null;
            }


            int videoStream = -1;
            for (int i = 0; i < formatContext->nb_streams; i++)
            {
                if (formatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    videoStream = i;
                    break;
                }
            }

            if (videoStream == -1)
            {
                ffmpeg.avformat_close_input(&formatContext);
                Logger.Error($"Media file has no video stream at \"{path}\"");
                return null;
            }

            AVCodecParameters* codecParameters = formatContext->streams[videoStream]->codecpar;

            AVCodecContext* codecContext = null;
            AVCodec* codec = null;

            codec = ffmpeg.avcodec_find_decoder(codecParameters->codec_id);
            if (codec == null)
            {
                ffmpeg.avformat_close_input(&formatContext);
                Logger.Error($"Unsupported codec at \"{path}\"");
                return null;
            }

            codecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (ffmpeg.avcodec_parameters_to_context(codecContext, codecParameters) != 0)
            {
                ffmpeg.avformat_close_input(&formatContext);
                Logger.Error($"Failed to copy codec at \"{path}\"");
                return null;
            }

            if (ffmpeg.avcodec_open2(codecContext, codec, null) < 0)
            {
                ffmpeg.avformat_close_input(&formatContext);
                Logger.Error($"Failed to open codec at \"{path}\"");
                return null;
            }

            return new LoadedVideo
            {
                Codec = codec,
                CodecContext = codecContext
            };
        }

        internal static unsafe void GetFrame(LoadedVideo video)
        {
            AVFrame* frame = null;

            frame = ffmpeg.av_frame_alloc();
            if (frame == null)
            {
                Logger.Error($"Failed to allocate frame");
                return;
            }

            var byteCount = ffmpeg.avpicture_get_size(AVPixelFormat.AV_PIX_FMT_RGBA, video.CodecContext->width, video.CodecContext->height);
            if (byteCount == -1)
            {
                Logger.Error($"Failed to allocate image buffer");
                return;
            }
            var buffer = (byte*)ffmpeg.av_malloc((ulong)byteCount * sizeof(byte));

            ffmpeg.avpicture_fill((AVPicture*)frame, buffer, AVPixelFormat.AV_PIX_FMT_RGBA, video.CodecContext->width, video.CodecContext->height);


        }
    }
}
