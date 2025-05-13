using System.Runtime.InteropServices;

namespace MDK.SDK.NET.Gen
{
    internal partial struct mdkAudioFrame
    {
    }

    internal enum MDK_SampleFormat
    {
        MDK_SampleFormat_Unknown,
        MDK_SampleFormat_U8,
        MDK_SampleFormat_U8P,
        MDK_SampleFormat_S16,
        MDK_SampleFormat_S16P,
        MDK_SampleFormat_S32,
        MDK_SampleFormat_S32P,
        MDK_SampleFormat_F32,
        MDK_SampleFormat_F32P,
        MDK_SampleFormat_F64,
        MDK_SampleFormat_F64P,
    }

    internal unsafe partial struct mdkAudioFrameAPI
    {
        [NativeTypeName("struct mdkAudioFrame *")]
        public mdkAudioFrame* @object;

        [NativeTypeName("int (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int> planeCount;

        [NativeTypeName("enum MDK_SampleFormat (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, MDK_SampleFormat> sampleFormat;

        [NativeTypeName("uint64_t (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, ulong> channelMask;

        [NativeTypeName("int (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int> channels;

        [NativeTypeName("int (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int> sampleRate;

        [NativeTypeName("bool (*)(struct mdkAudioFrame *, const uint8_t *, size_t, int, void *, void (*)(void **))")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, byte*, nuint, int, void*, delegate* unmanaged[Cdecl]<void**, void>, byte> addBuffer;

        [NativeTypeName("void (*)(struct mdkAudioFrame *, const uint8_t **const, int)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, byte**, int, void> setBuffers;

        [NativeTypeName("const uint8_t *(*)(struct mdkAudioFrame *, int)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int, byte*> bufferData;

        [NativeTypeName("int (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int> bytesPerPlane;

        [NativeTypeName("void (*)(struct mdkAudioFrame *, int)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int, void> setSamplesPerChannel;

        [NativeTypeName("int (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, int> samplesPerChannel;

        [NativeTypeName("void (*)(struct mdkAudioFrame *, double)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, double, void> setTimestamp;

        [NativeTypeName("double (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, double> timestamp;

        [NativeTypeName("double (*)(struct mdkAudioFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, double> duration;

        [NativeTypeName("struct mdkAudioFrameAPI *(*)(struct mdkAudioFrame *, enum MDK_SampleFormat, int, int)")]
        public delegate* unmanaged[Cdecl]<mdkAudioFrame*, MDK_SampleFormat, int, int, mdkAudioFrameAPI*> to;

        [NativeTypeName("void *[8]")]
        public _reserved_e__FixedBuffer reserved;

        public unsafe partial struct _reserved_e__FixedBuffer
        {
            public void* e0;
            public void* e1;
            public void* e2;
            public void* e3;
            public void* e4;
            public void* e5;
            public void* e6;
            public void* e7;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    internal static unsafe partial class Methods
    {
        [DllImport("mdk", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern mdkAudioFrameAPI* mdkAudioFrameAPI_new([NativeTypeName("enum MDK_SampleFormat")] MDK_SampleFormat format, int channels, int sampleRate, int samples);

        [DllImport("mdk", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void mdkAudioFrameAPI_delete([NativeTypeName("struct mdkAudioFrameAPI **")] mdkAudioFrameAPI** param0);
    }
}
