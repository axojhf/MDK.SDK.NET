using System.Runtime.InteropServices;

namespace MDK.SDK.NET.Gen
{
    internal partial struct mdkVideoFrame
    {
    }

    internal enum MDK_PixelFormat
    {
        MDK_PixelFormat_Unknown = -1,
        MDK_PixelFormat_YUV420P,
        MDK_PixelFormat_NV12,
        MDK_PixelFormat_YUV422P,
        MDK_PixelFormat_YUV444P,
        MDK_PixelFormat_P010LE,
        MDK_PixelFormat_P016LE,
        MDK_PixelFormat_YUV420P10LE,
        MDK_PixelFormat_UYVY422,
        MDK_PixelFormat_RGB24,
        MDK_PixelFormat_RGBA,
        MDK_PixelFormat_RGBX,
        MDK_PixelFormat_BGRA,
        MDK_PixelFormat_BGRX,
        MDK_PixelFormat_RGB565LE,
        MDK_PixelFormat_RGB48LE,
        MDK_PixelFormat_RGB48 = MDK_PixelFormat_RGB48LE,
        MDK_PixelFormat_GBRP,
        MDK_PixelFormat_GBRP10LE,
        MDK_PixelFormat_XYZ12LE,
        MDK_PixelFormat_YUVA420P,
        MDK_PixelFormat_BC1,
        MDK_PixelFormat_BC3,
        MDK_PixelFormat_RGBA64,
        MDK_PixelFormat_BGRA64,
        MDK_PixelFormat_RGBP16,
        MDK_PixelFormat_RGBPF32,
        MDK_PixelFormat_BGRAF32,
    }

    internal unsafe partial struct mdkVideoFrameAPI
    {
        [NativeTypeName("struct mdkVideoFrame *")]
        internal mdkVideoFrame* @object;

        [NativeTypeName("int (*)(struct mdkVideoFrame *)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, int> planeCount;

        [NativeTypeName("int (*)(struct mdkVideoFrame *, int)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, int> width;

        [NativeTypeName("int (*)(struct mdkVideoFrame *, int)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, int> height;

        [NativeTypeName("enum MDK_PixelFormat (*)(struct mdkVideoFrame *)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, MDK_PixelFormat> format;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, const uint8_t *, int, void *, void (*)(void **), int)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, byte*, int, void*, delegate* unmanaged[Cdecl]<void**, void>, int, byte> addBuffer;

        [NativeTypeName("void (*)(struct mdkVideoFrame *, const uint8_t **const, int *)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, byte**, int*, void> setBuffers;

        [NativeTypeName("const uint8_t *(*)(struct mdkVideoFrame *, int)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, byte*> bufferData;

        [NativeTypeName("int (*)(struct mdkVideoFrame *, int)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, int> bytesPerLine;

        [NativeTypeName("void (*)(struct mdkVideoFrame *, double)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, double, void> setTimestamp;

        [NativeTypeName("double (*)(struct mdkVideoFrame *)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, double> timestamp;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)(struct mdkVideoFrame *, enum MDK_PixelFormat, int, int)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, MDK_PixelFormat, int, int, mdkVideoFrameAPI*> to;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, const char *, const char *, float)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, IntPtr, IntPtr, float, byte> save;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)(struct mdkVideoFrame *)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrame*, mdkVideoFrameAPI*> toHost;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)()")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*> fromGL;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)()")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*> fromMetal;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)()")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*> fromVk;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)()")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*> fromD3D9;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)()")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*> fromD3D11;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)()")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*> fromD3D12;

        [NativeTypeName("void *[13]")]
        internal _reserved_e__FixedBuffer reserved;

        internal unsafe partial struct _reserved_e__FixedBuffer
        {
            internal void* e0;
            internal void* e1;
            internal void* e2;
            internal void* e3;
            internal void* e4;
            internal void* e5;
            internal void* e6;
            internal void* e7;
            internal void* e8;
            internal void* e9;
            internal void* e10;
            internal void* e11;
            internal void* e12;

            internal ref void* this[int index]
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
        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial mdkVideoFrameAPI* mdkVideoFrameAPI_new(int width, int height, [NativeTypeName("enum MDK_PixelFormat")] MDK_PixelFormat format);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void mdkVideoFrameAPI_delete([NativeTypeName("struct mdkVideoFrameAPI **")] mdkVideoFrameAPI** param0);
    }
}