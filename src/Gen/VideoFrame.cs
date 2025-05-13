using System.Runtime.InteropServices;

namespace MDK.SDK.NET.Gen
{
    internal unsafe partial struct mdkDX11Resource
    {
        public int size;

        [NativeTypeName("ID3D11DeviceChild *")]
        public void* resource;

        public int subResource;
    }

    internal unsafe partial struct mdkDX9Resource
    {
        public int size;

        [NativeTypeName("IDirect3DSurface9 *")]
        public void* surface;
    }
    
    internal unsafe partial struct mdkVAAPIResource
    {
        public int size;

        [NativeTypeName("VASurfaceID")]
        public uint surface;

        [NativeTypeName("VADisplay")]
        public void* display;

        public void* x11Display;

        [NativeTypeName("const void *")]
        public void* opaque;

        [NativeTypeName("void (*)(const void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> unref;
    }

    internal partial struct mdkVideoBufferPool
    {
    }

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
    
    internal unsafe partial struct mdkCUDAResource
    {
        public int size;

        [NativeTypeName("void *[4]")]
        public _ptr_e__FixedBuffer ptr;

        public int width;

        public int height;

        [NativeTypeName("int[4]")]
        public fixed int stride[4];

        [NativeTypeName("enum MDK_PixelFormat")]
        public MDK_PixelFormat format;

        public void* context;

        public void* stream;

        [NativeTypeName("const void *")]
        public void* opaque;

        [NativeTypeName("void (*)(const void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> unref;

        public unsafe partial struct _ptr_e__FixedBuffer
        {
            public void* e0;
            public void* e1;
            public void* e2;
            public void* e3;

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

    internal unsafe partial struct mdkVideoFrameAPI
    {
        [NativeTypeName("struct mdkVideoFrame *")]
        public mdkVideoFrame* @object;

        [NativeTypeName("int (*)(struct mdkVideoFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, int> planeCount;

        [NativeTypeName("int (*)(struct mdkVideoFrame *, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, int> width;

        [NativeTypeName("int (*)(struct mdkVideoFrame *, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, int> height;

        [NativeTypeName("enum MDK_PixelFormat (*)(struct mdkVideoFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, MDK_PixelFormat> format;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, const uint8_t *, int, void *, void (*)(void **), int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, byte*, int, void*, delegate* unmanaged[Cdecl]<void**, void>, int, byte> addBuffer;

        [NativeTypeName("void (*)(struct mdkVideoFrame *, const uint8_t **const, int *)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, byte**, int*, void> setBuffers;

        [NativeTypeName("const uint8_t *(*)(struct mdkVideoFrame *, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, byte*> bufferData;

        [NativeTypeName("int (*)(struct mdkVideoFrame *, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, int, int> bytesPerLine;

        [NativeTypeName("void (*)(struct mdkVideoFrame *, double)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, double, void> setTimestamp;

        [NativeTypeName("double (*)(struct mdkVideoFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, double> timestamp;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)(struct mdkVideoFrame *, enum MDK_PixelFormat, int, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, MDK_PixelFormat, int, int, mdkVideoFrameAPI*> to;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, const char *, const char *, float)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, sbyte*, sbyte*, float, byte> save;

        [NativeTypeName("struct mdkVideoFrameAPI *(*)(struct mdkVideoFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, mdkVideoFrameAPI*> onDestroy;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, mdkVideoBufferPool **, const mdkDX11Resource *, int, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, mdkVideoBufferPool**, mdkDX11Resource*, int, int, byte> fromDX11;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, mdkVideoBufferPool **, const mdkDX9Resource *, int, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, mdkVideoBufferPool**, mdkDX9Resource*, int, int, byte> fromDX9;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, mdkVideoBufferPool **, const mdkVAAPIResource *, int, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, mdkVideoBufferPool**, mdkVAAPIResource*, int, int, byte> fromVAAPI;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *, mdkVideoBufferPool **, const mdkCUDAResource *, int, int)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, mdkVideoBufferPool**, mdkCUDAResource*, int, int, byte> fromCUDA;

        [NativeTypeName("bool (*)()")]
        public delegate* unmanaged[Cdecl]<byte> fromMetal;

        [NativeTypeName("bool (*)()")]
        public delegate* unmanaged[Cdecl]<byte> fromVk;

        [NativeTypeName("bool (*)()")]
        public delegate* unmanaged[Cdecl]<byte> fromGL;

        [NativeTypeName("bool (*)()")]
        public delegate* unmanaged[Cdecl]<byte> fromDX12;

        [NativeTypeName("bool (*)(struct mdkVideoFrame *)")]
        public delegate* unmanaged[Cdecl]<mdkVideoFrame*, byte> toHost;

        [NativeTypeName("void *[10]")]
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
            public void* e8;
            public void* e9;

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
        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial mdkVideoFrameAPI* mdkVideoFrameAPI_new(int width, int height, [NativeTypeName("enum MDK_PixelFormat")] MDK_PixelFormat format);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void mdkVideoFrameAPI_delete([NativeTypeName("struct mdkVideoFrameAPI **")] mdkVideoFrameAPI** param0);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void mdkVideoBufferPoolFree(mdkVideoBufferPool** pool);
    }
}