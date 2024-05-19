using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MDK.SDK.NET.Gen
{
    internal enum MDK_MediaType
    {
        MDK_MediaType_Unknown = -1,
        MDK_MediaType_Video = 0,
        MDK_MediaType_Audio = 1,
        MDK_MediaType_Subtitle = 3,
    }

    internal enum MDK_MediaStatus
    {
        MDK_MediaStatus_NoMedia = 0,
        MDK_MediaStatus_Unloaded = 1,
        MDK_MediaStatus_Loading = 1 << 1,
        MDK_MediaStatus_Loaded = 1 << 2,
        MDK_MediaStatus_Prepared = 1 << 8,
        MDK_MediaStatus_Stalled = 1 << 3,
        MDK_MediaStatus_Buffering = 1 << 4,
        MDK_MediaStatus_Buffered = 1 << 5,
        MDK_MediaStatus_End = 1 << 6,
        MDK_MediaStatus_Seeking = 1 << 7,
        MDK_MediaStatus_Invalid = 1 << 31,
    }

    internal unsafe partial struct mdkMediaStatusChangedCallback
    {
        [NativeTypeName("bool (*)(MDK_MediaStatus, void *)")]
        internal delegate* unmanaged[Cdecl]<MDK_MediaStatus, void*, byte> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkMediaStatusCallback
    {
        [NativeTypeName("bool (*)(MDK_MediaStatus, MDK_MediaStatus, void *)")]
        internal delegate* unmanaged[Cdecl]<MDK_MediaStatus, MDK_MediaStatus, void*, byte> cb;

        internal void* opaque;
    }

    internal enum MDK_State
    {
        MDK_State_NotRunning,
        MDK_State_Stopped = MDK_State_NotRunning,
        MDK_State_Running,
        MDK_State_Playing = MDK_State_Running,
        MDK_State_Paused,
    }

    internal unsafe partial struct mdkStateChangedCallback
    {
        [NativeTypeName("void (*)(MDK_State, void *)")]
        internal delegate* unmanaged[Cdecl]<MDK_State, void*, void> cb;

        internal void* opaque;
    }

    internal enum MDKSeekFlag
    {
        MDK_SeekFlag_From0 = 1,
        MDK_SeekFlag_FromStart = 1 << 1,
        MDK_SeekFlag_FromNow = 1 << 2,
        MDK_SeekFlag_Frame = 1 << 6,
        MDK_SeekFlag_KeyFrame = 1 << 8,
        MDK_SeekFlag_Fast = MDK_SeekFlag_KeyFrame,
        MDK_SeekFlag_InCache = 1 << 10,
        MDK_SeekFlag_Backward = 1 << 16,
        MDK_SeekFlag_Default = MDK_SeekFlag_KeyFrame | MDK_SeekFlag_FromStart | MDK_SeekFlag_InCache,
    }

    internal enum MDK_VideoEffect
    {
        MDK_VideoEffect_Brightness,
        MDK_VideoEffect_Contrast,
        MDK_VideoEffect_Hue,
        MDK_VideoEffect_Saturation,
    }

    internal enum MDK_ColorSpace
    {
        MDK_ColorSpace_Unknown,
        MDK_ColorSpace_BT709,
        MDK_ColorSpace_BT2100_PQ,
        MDK_ColorSpace_scRGB,
        MDK_ColorSpace_ExtendedLinearDisplayP3,
        MDK_ColorSpace_ExtendedSRGB,
        MDK_ColorSpace_ExtendedLinearSRGB,
    }

    internal enum MDK_LogLevel
    {
        MDK_LogLevel_Off,
        MDK_LogLevel_Error,
        MDK_LogLevel_Warning,
        MDK_LogLevel_Info,
        MDK_LogLevel_Debug,
        MDK_LogLevel_All,
    }

    internal unsafe partial struct mdkLogHandler
    {
        [NativeTypeName("void (*)(MDK_LogLevel, const char *, void *)")]
        internal delegate* unmanaged[Cdecl]<MDK_LogLevel, sbyte*, void*, void> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkMediaEvent
    {
        [NativeTypeName("int64_t")]
        internal long error;

        [NativeTypeName("const char *")]
        internal sbyte* category;

        [NativeTypeName("const char *")]
        internal sbyte* detail;

        [NativeTypeName("__AnonymousRecord_global_L240_C5")]
        internal _Anonymous_e__Union Anonymous;

        [UnscopedRef]
        internal ref _Anonymous_e__Union._decoder_e__Struct decoder
        {
            get
            {
                return ref Anonymous.decoder;
            }
        }

        [UnscopedRef]
        internal ref _Anonymous_e__Union._video_e__Struct video
        {
            get
            {
                return ref Anonymous.video;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_global_L241_C9")]
            internal _decoder_e__Struct decoder;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_global_L244_C9")]
            internal _video_e__Struct video;

            internal partial struct _decoder_e__Struct
            {
                internal int stream;
            }

            internal partial struct _video_e__Struct
            {
                internal int width;

                internal int height;
            }
        }
    }

    internal unsafe partial struct mdkStringMapEntry
    {
        [NativeTypeName("const char *")]
        internal sbyte* key;

        [NativeTypeName("const char *")]
        internal sbyte* value;

        internal void* next;
        //internal void* priv;
    }

    internal static unsafe partial class Methods
    {
        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial int MDK_version();

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void* MDK_javaVM(void* vm);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void MDK_setLogLevel(MDK_LogLevel value);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial MDK_LogLevel MDK_logLevel();

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void MDK_setLogHandler(mdkLogHandler param0);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_setGlobalOptionString([NativeTypeName("const char *")] string key, [NativeTypeName("const char *")] string value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_setGlobalOptionInt32([NativeTypeName("const char *")] string key, int value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_setGlobalOptionFloat([NativeTypeName("const char *")] string key, float value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_setGlobalOptionPtr([NativeTypeName("const char *")] string key, void* value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_getGlobalOptionString([NativeTypeName("const char *")] string key, [NativeTypeName("const char **")] sbyte** value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_getGlobalOptionInt32([NativeTypeName("const char *")] string key, int* value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_getGlobalOptionPtr([NativeTypeName("const char *")] string key, void** value);

        [LibraryImport("mdk", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("char *")]
        internal static partial sbyte* MDK_strdup([NativeTypeName("const char *")] string strSource);

        [NativeTypeName("#define MDK_MAJOR 0")]
        internal const int MDK_MAJOR = 0;

        [NativeTypeName("#define MDK_MINOR 27")]
        internal const int MDK_MINOR = 27;

        [NativeTypeName("#define MDK_MICRO 0")]
        internal const int MDK_MICRO = 0;

        [NativeTypeName("#define MDK_VERSION MDK_VERSION_INT(MDK_MAJOR, MDK_MINOR, MDK_MICRO)")]
        internal const int MDK_VERSION = (((0 & 0xff) << 16) | ((27 & 0xff) << 8) | (0 & 0xff));
    }
}
