using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MDK.SDK.NET.Gen
{
    internal partial struct mdkMediaInfo
    {
    }

    internal partial struct mdkAudioFrame
    {
    }

    internal partial struct mdkVideoFrameAPI
    {
    }

    internal partial struct mdkPlayer
    {
    }

    internal enum MDK_SurfaceType
    {
        MDK_SurfaceType_Auto,
        MDK_SurfaceType_X11,
        MDK_SurfaceType_GBM,
        MDK_SurfaceType_Wayland,
    }

    internal unsafe partial struct mdkCurrentMediaChangedCallback
    {
        [NativeTypeName("void (*)(void *)")]
        internal delegate* unmanaged[Cdecl]<void*, void> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkPrepareCallback
    {
        [NativeTypeName("bool (*)(int64_t, bool *, void *)")]
        internal delegate* unmanaged[Cdecl]<long, bool*, void*, byte> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkRenderCallback
    {
        [NativeTypeName("void (*)(void *, void *)")]
        internal delegate* unmanaged[Cdecl]<void*, void*, void> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkVideoCallback
    {
        [NativeTypeName("int (*)(struct mdkVideoFrameAPI **, int, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkVideoFrameAPI**, int, void*, int> cb;

        internal void* opaque;
    }

    internal unsafe partial struct SwitchBitrateCallback
    {
        [NativeTypeName("void (*)(bool, void *)")]
        internal delegate* unmanaged[Cdecl]<byte, void*, void> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkSeekCallback
    {
        [NativeTypeName("void (*)(int64_t, void *)")]
        internal delegate* unmanaged[Cdecl]<long, void*, void> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkTimeoutCallback
    {
        [NativeTypeName("bool (*)(int64_t, void *)")]
        internal delegate* unmanaged[Cdecl]<long, void*, byte> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkMediaEventCallback
    {
        [NativeTypeName("bool (*)(const mdkMediaEvent *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkMediaEvent*, void*, byte> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkLoopCallback
    {
        [NativeTypeName("void (*)(int, void *)")]
        internal delegate* unmanaged[Cdecl]<int, void*, void> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkSnapshotRequest
    {
        [NativeTypeName("uint8_t *")]
        internal byte* data;

        internal int width;

        internal int height;

        internal int stride;

        [NativeTypeName("bool")]
        internal byte subtitle;
    }

    internal enum MDK_MapDirection
    {
        MDK_MapDirection_FrameToViewport,
        MDK_MapDirection_ViewportToFrame,
    }

    internal unsafe partial struct mdkSnapshotCallback
    {
        [NativeTypeName("char *(*)(mdkSnapshotRequest *, double, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkSnapshotRequest*, double, void*, sbyte*> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkSyncCallback
    {
        [NativeTypeName("double (*)(void *)")]
        internal delegate* unmanaged[Cdecl]<void*, double> cb;

        internal void* opaque;
    }

    internal unsafe partial struct mdkPlayerAPI
    {
        [NativeTypeName("struct mdkPlayer *")]
        internal mdkPlayer* @object;

        [NativeTypeName("void (*)(struct mdkPlayer *, bool)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, byte, void> setMute;

        [NativeTypeName("void (*)(struct mdkPlayer *, float)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, void> setVolume;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, void> setMedia;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char *, MDK_MediaType)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, MDK_MediaType, void> setMediaForType;

        [NativeTypeName("const char *(*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr> url;

        [NativeTypeName("void (*)(struct mdkPlayer *, bool)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, byte, void> setPreloadImmediately;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char *, int64_t, enum MDKSeekFlag)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, long, MDKSeekFlag, void> setNextMedia;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkCurrentMediaChangedCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkCurrentMediaChangedCallback, void> currentMediaChanged;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char **)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, void> setAudioBackends;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char **)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, void> setAudioDecoders;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char **)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, void> setVideoDecoders;

        [NativeTypeName("void (*)(struct mdkPlayer *, int64_t, mdkTimeoutCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long, mdkTimeoutCallback, void> setTimeout;

        [NativeTypeName("void (*)(struct mdkPlayer *, int64_t, mdkPrepareCallback, enum MDKSeekFlag)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long, mdkPrepareCallback, MDKSeekFlag, void> prepare;

        [NativeTypeName("const struct mdkMediaInfo *(*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkMediaInfo*> mediaInfo;

        [NativeTypeName("void (*)(struct mdkPlayer *, MDK_State)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_State, void> setState;

        [NativeTypeName("MDK_State (*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_State> state;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkStateChangedCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkStateChangedCallback, void> onStateChanged;

        [NativeTypeName("bool (*)(struct mdkPlayer *, MDK_State, long)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_State, int, byte> waitFor;

        [NativeTypeName("MDK_MediaStatus (*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_MediaStatus> mediaStatus;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkMediaStatusChangedCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkMediaStatusChangedCallback, void> onMediaStatusChanged;

        [NativeTypeName("void (*)(struct mdkPlayer *, void *, int, int, enum MDK_SurfaceType)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, void*, int, int, MDK_SurfaceType, void> updateNativeSurface;

        [NativeTypeName("void (*)(struct mdkPlayer *, void *, enum MDK_SurfaceType)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, void*, MDK_SurfaceType, void> createSurface;

        [NativeTypeName("void (*)(struct mdkPlayer *, int, int)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, int, int, void> resizeSurface;

        [NativeTypeName("void (*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, void> showSurface;

        [NativeTypeName("void (*)(struct mdkPlayer* p, struct mdkVideoFrameAPI* frame, void* vo_opaque)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkVideoFrameAPI*, void*, void> getVideoFrame;

        [NativeTypeName("void (*)(struct mdkPlayer *, int, int, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, int, int, void*, void> setVideoSurfaceSize;

        [NativeTypeName("void (*)(struct mdkPlayer *, float, float, float, float, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, float, float, float, void*, void> setVideoViewport;

        [NativeTypeName("void (*)(struct mdkPlayer *, float, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, void*, void> setAspectRatio;

        [NativeTypeName("void (*)(struct mdkPlayer *, int, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, int, void*, void> rotate;

        [NativeTypeName("void (*)(struct mdkPlayer *, float, float, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, float, void*, void> scale;

        [NativeTypeName("double (*)(struct mdkPlayer *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, void*, double> renderVideo;

        [NativeTypeName("void (*)(struct mdkPlayer *, float, float, float, float, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, float, float, float, void*, void> setBackgroundColor;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkRenderCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkRenderCallback, void> setRenderCallback;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkVideoCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkVideoCallback, void> onVideo;

        [NativeTypeName("void (*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, void> onAudio;

        [NativeTypeName("void (*)(struct mdkPlayer *, void (*)(struct mdkVideoFrameAPI *, void *))")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*, void*, void>, void> beforeVideoRender;

        [NativeTypeName("void (*)(struct mdkPlayer *, void (*)(struct mdkVideoFrameAPI *, void *))")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, delegate* unmanaged[Cdecl]<mdkVideoFrameAPI*, void*, void>, void> afterVideoRender;

        [NativeTypeName("int64_t (*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long> position;

        [NativeTypeName("bool (*)(struct mdkPlayer *, int64_t, MDK_SeekFlag, mdkSeekCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long, MDKSeekFlag, mdkSeekCallback, byte> seekWithFlags;

        [NativeTypeName("bool (*)(struct mdkPlayer *, int64_t, mdkSeekCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long, mdkSeekCallback, byte> seek;

        [NativeTypeName("void (*)(struct mdkPlayer *, float)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, void> setPlaybackRate;

        [NativeTypeName("float (*)(struct mdkPlayer *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float> playbackRate;

        [NativeTypeName("int64_t (*)(struct mdkPlayer *, int64_t *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long*, long> buffered;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char *, int64_t, SwitchBitrateCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, long, SwitchBitrateCallback, void> switchBitrate;

        [NativeTypeName("bool (*)(struct mdkPlayer *, const char *, SwitchBitrateCallback)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, SwitchBitrateCallback, byte> switchBitrateSingleConnection;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkMediaEventCallback, MDK_CallbackToken *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkMediaEventCallback, ulong*, void> onEvent;

        [NativeTypeName("void (*)(struct mdkPlayer *, int64_t, int64_t, bool)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long, long, byte, void> setBufferRange;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkSnapshotRequest *, mdkSnapshotCallback, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkSnapshotRequest*, mdkSnapshotCallback, void*, void> snapshot;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char *, const char *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, IntPtr, void> setProperty;

        [NativeTypeName("const char *(*)(struct mdkPlayer *, const char *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, IntPtr> getProperty;

        [NativeTypeName("void (*)(struct mdkPlayer *, const char *, const char *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, IntPtr, IntPtr, void> record;

        [NativeTypeName("void (*)(struct mdkPlayer *, int, int64_t, int64_t)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, int, long, long, void> setLoopRange;

        [NativeTypeName("void (*)(struct mdkPlayer *, int)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, int, void> setLoop;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkLoopCallback, MDK_CallbackToken *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkLoopCallback, ulong*, void> onLoop;

        [NativeTypeName("void (*)(struct mdkPlayer *, int64_t, int64_t)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, long, long, void> setRange;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkRenderAPI *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkRenderAPI*, void*, void> setRenderAPI;

        [NativeTypeName("mdkRenderAPI *(*)(struct mdkPlayer *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, void*, mdkRenderAPI*> renderAPI;

        [NativeTypeName("void (*)(struct mdkPlayer *, enum MDK_MapDirection, float *, float *, float *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_MapDirection, float*, float*, float*, void*, void> mapPoint;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkSyncCallback, int)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkSyncCallback, int, void> onSync;

        [NativeTypeName("void (*)(struct mdkPlayer *, enum MDK_VideoEffect, const float *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_VideoEffect, float*, void*, void> setVideoEffect;

        [NativeTypeName("void (*)(struct mdkPlayer *, enum MDK_MediaType, const int *, size_t)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_MediaType, int*, nuint, void> setActiveTracks;

        [NativeTypeName("void (*)(struct mdkPlayer *, enum MDK_MediaType, const char **)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_MediaType, sbyte*, void> setDecoders;

        [NativeTypeName("void (*)(struct mdkPlayer *, float, int)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, int, void> setChannelVolume;

        [NativeTypeName("void (*)(struct mdkPlayer *, float)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float, void> setFrameRate;

        [NativeTypeName("void (*)(struct mdkPlayer *, const float *, const float *, int, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, float*, float*, int, void*, void> setPointMap;

        [NativeTypeName("void (*)(struct mdkPlayer *, enum MDK_ColorSpace, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, MDK_ColorSpace, void*, void> setColorSpace;

        [NativeTypeName("void (*)(struct mdkPlayer *, mdkMediaStatusCallback, MDK_CallbackToken *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkMediaStatusCallback, ulong*, void> onMediaStatus;

        [NativeTypeName("__AnonymousRecord_Player_L499_C5")]
        internal _Anonymous_e__Union Anonymous;

        [NativeTypeName("void (*)(struct mdkPlayer *, struct mdkVideoFrameAPI *, void *)")]
        internal delegate* unmanaged[Cdecl]<mdkPlayer*, mdkVideoFrameAPI*, void*, void> enqueueVideo;

        [NativeTypeName("int (*)(struct mdkPlayer *, int64_t *, int)")]
        public delegate* unmanaged[Cdecl]<mdkPlayer*, long*, int, int> bufferedTimeRanges;

        [NativeTypeName("bool (*)(struct mdkPlayer *, const uint8_t *, size_t, int)")]
        public delegate* unmanaged[Cdecl]<mdkPlayer*, byte*, nuint, int, byte> appendBuffer;

        [UnscopedRef]
        internal ref void* reserved2
        {
            get
            {
                return ref Anonymous.reserved2;
            }
        }

        [UnscopedRef]
        internal ref int size
        {
            get
            {
                return ref Anonymous.size;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            internal void* reserved2;

            [FieldOffset(0)]
            internal int size;
        }
    }

    internal static unsafe partial class Methods
    {
        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: NativeTypeName("const mdkPlayerAPI *")]
        internal static partial mdkPlayerAPI* mdkPlayerAPI_new();

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void mdkPlayerAPI_delete([NativeTypeName("const struct mdkPlayerAPI **")] mdkPlayerAPI** param0);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void mdkPlayerAPI_reset([NativeTypeName("const struct mdkPlayerAPI **")] mdkPlayerAPI** param0, [NativeTypeName("bool")] byte release);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        internal static partial void MDK_foreignGLContextDestroyed();
    }
}
