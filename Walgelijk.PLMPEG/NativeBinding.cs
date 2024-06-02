using System.Runtime.InteropServices;

namespace Walgelijk.PLMPEG;

internal static class NativeBinding
{
    public class Plm
    {
        public IntPtr Pointer { get; private set; }

        public Plm(IntPtr pointer)
        {
            Pointer = pointer;
        }

        public void Destroy()
        {
            if (Pointer != IntPtr.Zero)
            {
                PlmNative.plm_destroy(Pointer);
                Pointer = IntPtr.Zero;
            }
        }

        public bool HasHeaders()
        {
            return PlmNative.plm_has_headers(Pointer) != 0;
        }

        public bool VideoEnabled
        {
            get => PlmNative.plm_get_video_enabled(Pointer) == 1;
            set => PlmNative.plm_set_video_enabled(Pointer, value ? 1 : 0);
        }

        public bool AudioEnabled
        {
            get => PlmNative.plm_get_audio_enabled(Pointer) == 1;
            set => PlmNative.plm_set_audio_enabled(Pointer, value ? 1 : 0);
        }

        public int NumVideoStreams => PlmNative.plm_get_num_video_streams(Pointer);

        public int Width => PlmNative.plm_get_width(Pointer);

        public int Height => PlmNative.plm_get_height(Pointer);

        public double FrameRate => PlmNative.plm_get_framerate(Pointer);

        public int NumAudioStreams => PlmNative.plm_get_num_audio_streams(Pointer);

        public int SampleRate => PlmNative.plm_get_samplerate(Pointer);

        public double AudioLeadTime
        {
            get => PlmNative.plm_get_audio_lead_time(Pointer);
            set => PlmNative.plm_set_audio_lead_time(Pointer, value);
        }

        public double Time => PlmNative.plm_get_time(Pointer);

        public double Duration => PlmNative.plm_get_duration(Pointer);

        public void Rewind() => PlmNative.plm_rewind(Pointer);

        public bool Loop
        {
            get => PlmNative.plm_get_loop(Pointer) != 0;
            set => PlmNative.plm_set_loop(Pointer, value ? 1 : 0);
        }

        public bool HasEnded => Pointer == 0 || PlmNative.plm_has_ended(Pointer) != 0;

        public void SetVideoDecodeCallback(Action<PlmFrame> callback)
        {
            videoDecodeCallback = callback;
            PlmNative.plm_set_video_decode_callback(Pointer, VideoDecodeCallback, IntPtr.Zero);
        }

        private Action<PlmFrame> videoDecodeCallback;

        private void VideoDecodeCallback(IntPtr plm, ref PlmFrame frame, IntPtr user)
        {
            videoDecodeCallback?.Invoke(frame);
        }

        public void SetAudioDecodeCallback(Action<PlmSamples> callback)
        {
            _audioDecodeCallback = callback;
            PlmNative.plm_set_audio_decode_callback(Pointer, AudioDecodeCallback, IntPtr.Zero);
        }

        private Action<PlmSamples> _audioDecodeCallback;

        private void AudioDecodeCallback(IntPtr plm, ref PlmSamples samples, IntPtr user)
        {
            _audioDecodeCallback?.Invoke(samples);
        }

        public void Decode(double seconds) => PlmNative.plm_decode(Pointer, seconds);

        public PlmFrame? DecodeVideo()
        {
            var frame = PlmNative.plm_decode_video(Pointer);
            return frame != IntPtr.Zero ? Marshal.PtrToStructure<PlmFrame>(frame) : (PlmFrame?)null;
        }

        public PlmSamples? DecodeAudio()
        {
            var samples = PlmNative.plm_decode_audio(Pointer);
            return samples != IntPtr.Zero
                ? Marshal.PtrToStructure<PlmSamples>(samples)
                : null;
        }

        public void Seek(double time, bool seekExact) =>
            PlmNative.plm_seek(Pointer, time, seekExact ? 1 : 0);

        public PlmFrame? SeekFrame(double time, bool seekExact)
        {
            var frame = PlmNative.plm_seek_frame(Pointer, time, seekExact ? 1 : 0);
            return frame != IntPtr.Zero ? Marshal.PtrToStructure<PlmFrame>(frame) : (PlmFrame?)null;
        }

        public static Plm? CreateWithFilename(string filename)
        {
            var ptr = PlmNative.plm_create_with_filename(filename);
            return ptr != IntPtr.Zero ? new Plm(ptr) : null;
        }

        public static Plm? CreateWithBuffer(Stream stream, bool destroyWhenDone)
        {
            PlmBufferHandler handler = new PlmBufferHandler(stream);
            PlmBuffer buffer = handler.CreatePlmBuffer();

            var ptr = PlmNative.plm_create_with_buffer(ref buffer, destroyWhenDone ? 1 : 0);
            return ptr != IntPtr.Zero ? new Plm(ptr) : null;
        }

        public static Plm? CreateWithMemory(byte[] bytes)
        {
            var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var ptr = PlmNative.plm_create_with_memory(
                gcHandle.AddrOfPinnedObject(),
                (uint)bytes.Length,
                1
            );
            return ptr != IntPtr.Zero ? new Plm(ptr) : null;
        }

        public void FrameToRgba(PlmFrame frame, byte[] destBuffer, int width, int height)
        {
            if (frame.Width != width || frame.Height != height)
            {
                throw new ArgumentException(
                    "Destination buffer dimensions do not match frame dimensions."
                );
            }

            int stride = width * 4; // 4 bytes per pixel for RGBA
            var gcHandle = GCHandle.Alloc(destBuffer, GCHandleType.Pinned);
            try
            {
                PlmNative.plm_frame_to_rgba(
                    Marshal.PtrToStructure<IntPtr>(frame.Y.Data),
                    gcHandle.AddrOfPinnedObject(),
                    stride
                );
            }
            finally
            {
                gcHandle.Free();
            }
        }
    }

    public class PlmBufferHandler
    {
        private Stream stream;

        public PlmBufferHandler(Stream stream)
        {
            this.stream = stream;
        }

        // Define the callback method
        public void LoadCallback(ref PlmBuffer self, IntPtr user)
        {
            int bufferSize = (int)self.capacity - (int)self.length;
            byte[] buffer = new byte[bufferSize];

            int bytesRead = stream.Read(buffer, 0, bufferSize);
            if (bytesRead > 0)
            {
                // Assuming the `bytes` pointer is writable and the buffer is large enough
                Marshal.Copy(buffer, 0, self.bytes, bytesRead);
                self.length = ((int)self.length + bytesRead);
                self.total_size = ((int)self.total_size + bytesRead);
            }
            else
            {
                self.has_ended = 1;
            }
        }

        public PlmBuffer CreatePlmBuffer()
        {
            PlmBuffer buffer = new PlmBuffer
            {
                capacity = (IntPtr)1024 * 1024, // 1 MB buffer
                length = IntPtr.Zero,
                total_size = IntPtr.Zero,
                has_ended = 0,
                load_callback = new PlmBufferLoadCallback(LoadCallback),
                load_callback_user_data = IntPtr.Zero
            };

            buffer.bytes = Marshal.AllocHGlobal((int)buffer.capacity);
            return buffer;
        }

        public void FreePlmBuffer(PlmBuffer buffer)
        {
            Marshal.FreeHGlobal(buffer.bytes);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PlmFrame
    {
        public double Time;
        public uint Width;
        public uint Height;
        public PlmPlane Y;
        public PlmPlane Cr;
        public PlmPlane Cb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PlmPlane
    {
        public uint Width;
        public uint Height;
        public IntPtr Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PlmSamples
    {
        public double Time;
        public uint Count;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1152 * 2)]
        public float[] Interleaved;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PlmBufferLoadCallback(ref PlmBuffer self, IntPtr user);

    [StructLayout(LayoutKind.Sequential)]
    public struct PlmBuffer
    {
        public IntPtr bit_index;
        public IntPtr capacity;
        public IntPtr length;
        public IntPtr total_size;
        public int discard_read_bytes;
        public int has_ended;
        public int free_when_done;
        public int close_when_done;
        public IntPtr fh;
        public PlmBufferLoadCallback load_callback;
        public IntPtr load_callback_user_data;
        public IntPtr bytes;
        public PlmBufferMode mode;
    }

    public enum PlmBufferMode
    {
        PLM_BUFFER_MODE_FILE,
        PLM_BUFFER_MODE_FIXED_MEM,
        PLM_BUFFER_MODE_RING,
        PLM_BUFFER_MODE_APPEND
    }

    public static class PlmNative
    {
        private const string DllName = "pl_mpeg";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_create_with_filename(
            [MarshalAs(UnmanagedType.LPStr)] string filename
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_create_with_memory(IntPtr bytes, uint length, int freeWhenDone);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_create_with_file(IntPtr readCallback, int closeWhenDone);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_create_with_buffer(ref PlmBuffer buffer, int closeWhenDone);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_destroy(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_has_headers(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_video_enabled(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_set_video_enabled(IntPtr plm, int enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_audio_enabled(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_set_audio_enabled(IntPtr plm, int enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_num_video_streams(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_width(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_height(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double plm_get_framerate(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_num_audio_streams(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_samplerate(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double plm_get_audio_lead_time(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_set_audio_lead_time(IntPtr plm, double leadTime);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double plm_get_time(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double plm_get_duration(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_rewind(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_get_loop(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_set_loop(IntPtr plm, int loop);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_has_ended(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_set_video_decode_callback(
            IntPtr plm,
            VideoDecodeCallback callback,
            IntPtr user
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_set_audio_decode_callback(
            IntPtr plm,
            AudioDecodeCallback callback,
            IntPtr user
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_frame_to_rgba(IntPtr frame, IntPtr dest, int stride);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void plm_decode(IntPtr plm, double seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_decode_video(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_decode_audio(IntPtr plm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int plm_seek(IntPtr plm, double time, int seekExact);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plm_seek_frame(IntPtr plm, double time, int seekExact);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VideoDecodeCallback(IntPtr plm, ref PlmFrame frame, IntPtr user);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioDecodeCallback(IntPtr plm, ref PlmSamples samples, IntPtr user);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ReadDelegate(IntPtr buffer, int count);
    }

}