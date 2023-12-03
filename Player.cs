using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;
using CallbackToken = System.UInt64;

namespace MDK.SDK.NET;

public class MDKPlayer : IDisposable
{
    unsafe private mdkPlayerAPI* p = null;
    private bool owner_ = false;

    private bool mute_ = false;
    private float volume_ = 1.0f;
    private CallbackCurrentMediaChanged? current_cb_ = null;
    private CallBackOnTimeout? timeout_cb_ = null;
    private CallBackOnPrepare? prepare_cb_ = null;
    private CallBackOnStateChanged? state_cb_ = null;
    private CallBackOnMediaStatus? status_cb_ = null;
    private CallBackOnRender? render_cb_ = null;
    private CallBackOnSeek? seek_cb_ = null;
    private CallBackOnSwitchBitrate? switch_cb_ = null;
    private CallBackOnSnapshot? snapshot_cb_ = null;
    private CallBackOnFrame? video_cb_ = null;
    private CallBackOnSync? sync_cb_ = null;
    private Dictionary<CallbackToken, CallBackOnEvent> event_cb_ = [];
    private Dictionary<CallbackToken, CallbackToken> event_cb_key_ = [];
    private Dictionary<CallbackToken, CallBackOnLoop> loop_cb_ = [];
    private Dictionary<CallbackToken, CallbackToken> loop_cb_key_ = [];
    private MediaInfo info_;

    private static CallbackToken onEvent_k = 1;
    private static CallbackToken onLoop_k = 1;

    public MDKPlayer()
    {
        unsafe
        {
            p = Methods.mdkPlayerAPI_new();
        }
        owner_ = true;
    }

    public static void ForeignGLContextDestroyed()
    {
        Methods.MDK_foreignGLContextDestroyed();
    }

    public void SetMute(bool value = true)
    {
        unsafe
        {
            p->setMute(p->@object, (byte)(value ? 1 : 0));
        }
        mute_ = value;
    }

    public bool IsMute()
    {
        return mute_;
    }

    public void SetVolume(float value, int channel = -1)
    {
        unsafe
        {
            if (channel == -1)
            {
                p->setVolume(p->@object, value);
            }
            else
            {
                p->setChannelVolume(p->@object, value, channel);
            }
        }
        volume_ = value;
    }

    public float Volume()
    {
        return volume_;
    }

    public void SetFrameRate(float value)
    {
        unsafe
        {
            p->setFrameRate(p->@object, value);
        }
    }

    /// <summary>
    ///  Set a new media url.  If url changed, will stop current playback, and reset active tracks, external tracks set by SetMedia(url, type)
    /// </summary>
    /// <param name="url"></param>
    public void SetMedia(string url)
    {
        unsafe
        {
            var _url = Marshal.StringToCoTaskMemUTF8(url);
            p->setMedia(p->@object, _url);
            Marshal.FreeCoTaskMem(_url);
        }
    }

    public void SetMedia(string url, MediaType type)
    {
        unsafe
        {
            var _url = Marshal.StringToCoTaskMemUTF8(url);
            p->setMediaForType(p->@object, _url, (MDK_MediaType)type);
            Marshal.FreeCoTaskMem(_url);
        }
    }

    public string Url()
    {
        unsafe
        {
            var url = p->url(p->@object);
            return Marshal.PtrToStringUTF8(url) ?? "";
        }
    }

    public void SetPreloadImmediately(bool value = true)
    {
        unsafe
        {
            p->setPreloadImmediately(p->@object, (byte)(value ? 1 : 0));
        }
    }

    public void SetNextMedia(string url, long startPosition = 0, SeekFlag flags = SeekFlag.FromStart)
    {
        unsafe
        {
            var _url = Marshal.StringToCoTaskMemUTF8(url);
            p->setNextMedia(p->@object, _url, startPosition, (MDKSeekFlag)flags);
            Marshal.FreeCoTaskMem(_url);
        }
    }

    public delegate void CallbackCurrentMediaChanged();

    public void CurrentMediaChanged(CallbackCurrentMediaChanged cb)
    {
        current_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(void* opaque)
            {
                Marshal.GetDelegateForFunctionPointer<CallbackCurrentMediaChanged>((nint)opaque)();
            }
            mdkCurrentMediaChangedCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(current_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(current_cb_)),
            };
            p->currentMediaChanged(p->@object, callback);
        }
    }

    public void SetActiveTracks(MediaType type, HashSet<int> tracks)
    {
        unsafe
        {
            var pTs = stackalloc int[tracks.Count];
            int i = 0;
            foreach (var t in tracks)
                pTs[i++] = t;
            p->setActiveTracks(p->@object, (MDK_MediaType)type, pTs, (nuint)tracks.Count);
        }
    }

    /// <summary>
    /// backends can be: AudioQueue(Apple only), OpenSL(Android only), ALSA(linux only), XAudio2(Windows only), OpenAL
    /// </summary>
    /// <param name="names"></param>
    public void SetAudioBackends(List<string> names)
    {
        unsafe
        {
            var pdata = stackalloc sbyte*[names.Count];
            for (int i = 0; i < names.Count; i++)
            {
                pdata[i++] = (sbyte*)Marshal.StringToCoTaskMemUTF8(names[i]);
            }
            p->setAudioBackends(p->@object, (nint)pdata);
            for (int i = 0; i < names.Count; i++)
            {
                Marshal.FreeCoTaskMem((IntPtr)pdata[i]);
            }
        }
    }

    /// <summary>
    /// // see https://github.com/wang-bin/mdk-sdk/wiki/Player-APIs#void-setdecodersmediatype-type-const-stdvectorstdstring-names
    /// </summary>
    /// <param name="type"></param>
    /// <param name="names"></param>
    public void SetDecoders(MediaType type, List<string> names)
    {
        unsafe
        {
            var pdata = stackalloc sbyte*[names.Count];
            for (int i = 0; i < names.Count; i++)
            {
                pdata[i] = (sbyte*)Marshal.StringToCoTaskMemUTF8(names[i]);
            }
            p->setDecoders(p->@object, (MDK_MediaType)type, (sbyte*)pdata);
            for (int i = 0; i < names.Count; i++)
            {
                Marshal.FreeCoTaskMem((IntPtr)pdata[i]);
            }

        }
    }

    public delegate bool CallBackOnTimeout(long ms);
    public void SetTimeout(long ms, CallBackOnTimeout cb)
    {
        unsafe
        {
            timeout_cb_ = cb;
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static byte temp(long ms, void* opaque)
            {
                return (byte)(Marshal.GetDelegateForFunctionPointer<CallBackOnTimeout>((nint)opaque)(ms) ? 1 : 0);
            }
            mdkTimeoutCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(timeout_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(timeout_cb_)),
            };
            p->setTimeout(p->@object, ms, callback);
        }
    }

    public delegate bool CallBackOnPrepare(long position, IntPtr boost);
    public void Prepare(long startPosition = 0, CallBackOnPrepare? cb = null, SeekFlag flags = SeekFlag.FromStart)
    {
        unsafe
        {
            prepare_cb_ = cb;
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static byte temp(long position, void* boost, void* opaque)
            {
                return (byte)(Marshal.GetDelegateForFunctionPointer<CallBackOnPrepare>((nint)opaque)(position, (IntPtr)boost) ? 1 : 0);
            }
            mdkPrepareCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(prepare_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(prepare_cb_)),
            };
            p->prepare(p->@object, startPosition, callback, (MDKSeekFlag)flags);
        }
    }

    public MediaInfo? mediaInfo
    {
        get
        {
            unsafe
            {
                var info = p->mediaInfo(p->@object);
                var ret = new MediaInfo();
                MediaInfo.From_c(info, ref ret);
                return ret;
            }
        }
    }

    /// <summary>
    /// Request a new state. It's async and may take effect later.
    /// <para>set(State::Stopped) only stops current media.Call setNextMedia(nullptr, -1) before stop to disable next media.</para>
    /// <para>set(State::Stopped) will release all resouces and clear video renderer viewport.While a normal playback end will keep renderer resources
    /// and the last video frame.Manually call set(State::Stopped) to clear them.</para>
    /// <para>NOTE: the requested state is not queued. so set one state immediately after another may have no effect.</para>
    /// <para>e.g.State::Playing after State::Stopped may have no effect if playback have not been stopped and still in Playing state
    /// so the final state is State::Stopped.Current solution is waitFor(State::Stopped) before set(State::Playing).</para>
    /// <para>Usually no waitFor(State::Playing) because we want async load</para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public MDKPlayer Set(State value)
    {
        unsafe
        {
            p->setState(p->@object, (MDK_State)value);
            return this;
        }
    }

    public PlaybackState? State
    {
        get
        {
            unsafe
            {
                return (PlaybackState)p->state(p->@object);
            }
        }
    }

    public delegate void CallBackOnStateChanged(State a);
    public MDKPlayer OnStateChanged(CallBackOnStateChanged cb)
    {
        state_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(MDK_State value, void* opaque)
            {
                Marshal.GetDelegateForFunctionPointer<CallBackOnStateChanged>((nint)opaque)((State)value);
            }
            mdkStateChangedCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(state_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(state_cb_)),
            };
            p->onStateChanged(p->@object, callback);
        }
        return this;
    }

    public bool WaitFor(State value, long timeout = -1)
    {
        unsafe
        {
            return p->waitFor(p->@object, (MDK_State)value, (int)timeout) != 0;
        }
    }

    public MediaStatus MediaStatus
    {
        get { unsafe { return (MediaStatus)p->mediaStatus(p->@object); } }
    }

    public delegate bool CallBackOnMediaStatus(MediaStatus old, MediaStatus @new);
    public MDKPlayer OnMediaStatus(CallBackOnMediaStatus cb, IntPtr token = 0)
    {
        status_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static byte temp(MDK_MediaStatus old, MDK_MediaStatus value, void* opaque)
            {
                return (byte)(Marshal.GetDelegateForFunctionPointer<CallBackOnMediaStatus>((nint)opaque)((MediaStatus)old, (MediaStatus)value) ? 1 : 0);
            }
            mdkMediaStatusCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(status_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(status_cb_)),
            };
            p->onMediaStatus(p->@object, callback, (ulong*)token);
        }
        return this;
    }

    /// <summary>
    /// If surface is not created, create rendering context internally by createSurface() and attached to native surface 
    /// <para>native surface MUST be not null before destroying player</para>
    /// <para>ignored if win ptr does not change (request to resize)</para>
    /// </summary>
    /// <param name="surface"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="type"></param>
    public void UpdateNativeSurface(IntPtr surface, int width = -1, int height = -1, SurfaceType type = SurfaceType.Auto)
    {
        unsafe
        {
            p->updateNativeSurface(p->@object, (void*)surface, width, height, (MDK_SurfaceType)type);
        }
    }

    public void CreateSurface(IntPtr nativeHandle, SurfaceType type = SurfaceType.Auto)
    {
        unsafe
        {
            p->createSurface(p->@object, (void*)nativeHandle, (MDK_SurfaceType)type);
        }
    }

    public void ResizeSurface(int width, int height)
    {
        unsafe
        {
            p->resizeSurface(p->@object, width, height);
        }
    }

    public void ShowSurface()
    {
        unsafe
        {
            p->showSurface(p->@object);
        }
    }

    public delegate string CallBackOnSnapshot(IntPtr request, double position);
    public void Snapshot(IntPtr request, CallBackOnSnapshot cb, IntPtr vo_opaque = 0)
    {
        snapshot_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static sbyte* temp(mdkSnapshotRequest* request, double position, void* opaque)
            {
                return (sbyte*)Marshal.StringToCoTaskMemUTF8(Marshal.GetDelegateForFunctionPointer<CallBackOnSnapshot>((nint)opaque)((nint)request, position));
            }
            mdkSnapshotCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(snapshot_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(snapshot_cb_)),
            };
            p->snapshot(p->@object, (mdkSnapshotRequest*)request, callback, (void*)vo_opaque);
        }
    }

    public void SetProperty(string name, string value)
    {
        unsafe
        {
            var _name = Marshal.StringToCoTaskMemUTF8(name);
            var _value = Marshal.StringToCoTaskMemUTF8(value);
            p->setProperty(p->@object, _name, _value);
            Marshal.FreeCoTaskMem(_name);
            Marshal.FreeCoTaskMem(_value);
        }
    }

    public string Property(string name)
    {
        unsafe
        {
            var _name = Marshal.StringToCoTaskMemUTF8(name);
            var ret = Marshal.PtrToStringUTF8(p->getProperty(p->@object, _name)) ?? "";
            Marshal.FreeCoTaskMem(_name);
            return ret;
        }
    }

    /// <summary>
    /// Window size, surface size or drawable size. Render callback(if exists) will be invoked if width and height > 0.
    /// <para>Usually for foreign contexts, i.e.not use updateNativeSurface().</para>
    /// <para>If width or heigh &lt; 0, corresponding video renderer (for vo_opaque) will be removed and gfx resources will be released(need the context to be current for GL).</para>
    /// <para>But subsequence call with this vo_opaque will create renderer again.So it can be used before destroying the renderer.</para>
    /// <para>OpenGL: resources must be released by setVideoSurfaceSize(-1, -1, ...) in a correct context.If player is destroyed before context, MUST call Player::foreignGLContextDestroyed() when destroying the context.</para>
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="vo_opaque"></param>
    public void SetVideoSurfaceSize(int width, int height, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setVideoSurfaceSize(p->@object, width, height, (void*)vo_opaque);
        }
    }

    public void SetVideoViewport(float x, float y, float width, float height, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setVideoViewport(p->@object, x, y, width, height, (void*)vo_opaque);
        }
    }

    public void SetAspectRatio(float value, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setAspectRatio(p->@object, value, (void*)vo_opaque);
        }
    }

    public void Rotate(int degree, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->rotate(p->@object, degree, (void*)vo_opaque);
        }
    }

    public void Scale(float x, float y, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->scale(p->@object, x, y, (void*)vo_opaque);
        }
    }

    public void MapPoint(MapDirection dir, IntPtr x, IntPtr y, IntPtr z = default, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->mapPoint(p->@object, (MDK_MapDirection)dir, (float*)x, (float*)y, (float*)z, (void*)vo_opaque);
        }
    }

    public void SetPointMap(IntPtr videoRoi, IntPtr viewRoi, int count = 2, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setPointMap(p->@object, (float*)videoRoi, (float*)viewRoi, count, (void*)vo_opaque);
        }
    }

    public MDKPlayer SetRenderAPI(IntPtr api, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setRenderAPI(p->@object, (mdkRenderAPI*)api, (void*)vo_opaque);
        }
        return this;
    }

    public IntPtr RenderAPI(IntPtr vo_opaque = default)
    {
        unsafe
        {
            return (IntPtr)p->renderAPI(p->@object, (void*)vo_opaque);
        }
    }

    public double RenderVideo(IntPtr vo_opaque = 0)
    {
        unsafe
        {
            return (double)(p->renderVideo(p->@object, (void*)vo_opaque));
        }
    }

    public void Enqueue(IntPtr frame, IntPtr opaque = 0)
    {
        unsafe
        {
            p->enqueueVideo(p->@object, (mdkVideoFrameAPI*)frame, (void*)opaque);
        }
    }

    public void SetBackgroundColor(float r, float g, float b, float a, IntPtr vo_opaque = 0)
    {
        unsafe
        {
            p->setBackgroundColor(p->@object, r, g, b, a, (void*)vo_opaque);
        }
    }

    public void Set(VideoEffect effect, IntPtr value, IntPtr vo_opaque = 0)
    {
        unsafe
        {
            p->setVideoEffect(p->@object, (MDK_VideoEffect)effect, (float*)value, (void*)vo_opaque);
        }
    }

    public void Set(ColorSpace value, IntPtr vo_opaque = 0)
    {
        unsafe
        {
            p->setColorSpace(p->@object, (MDK_ColorSpace)value, (void*)vo_opaque);
        }
    }

    public delegate void CallBackOnRender(IntPtr vo_opaque);
    public void SetRenderCallback(CallBackOnRender cb)
    {
        render_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(void* vo_opaque, void* opaque)
            {
                Marshal.GetDelegateForFunctionPointer<CallBackOnRender>((nint)opaque)((IntPtr)vo_opaque);
            }
            mdkRenderCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(render_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(render_cb_)),
            };
            p->setRenderCallback(p->@object, callback);
        }
    }

    public long Position
    {
        get { unsafe { return p->position(p->@object); } }
    }

    public delegate void CallBackOnSeek(long ms);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position">seek target. if flags has SeekFlag::Frame, pos is frame count, otherwise it's milliseconds.<br/>
    /// If pos > media time range, e.g.INT64_MAX, will seek to the last frame of media for SeekFlag::AnyFrame, and the last key frame of media for SeekFlag::Fast.<br/>
    /// If pos > media time range with SeekFlag::AnyFrame, playback will stop unless setProperty("continue_at_end", "1") was called<br/>
    /// FIXME: a/v sync broken if SeekFlag::Frame|SeekFlag::FromNow.</param>
    /// <param name="flags"></param>
    /// <param name="cb">if succeeded, callback is called when stream seek finished and after the 1st frame decoded or decode error(e.g. video tracks disabled), ret(&gt;=0) is the timestamp of the 1st frame(video if exists) after seek.<br/>
    /// If error(io, demux, not decode) occured(ret &lt; 0, usually -1) or skipped because of unfinished previous seek(ret == -2), out of range(-4) or media unloaded(-3).</param>
    public void Seek(long position, SeekFlag flags, CallBackOnSeek? cb = null)
    {
        seek_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(long ms, void* opaque)
            {
                Marshal.GetDelegateForFunctionPointer<CallBackOnSeek>((nint)opaque)(ms);
            }
            mdkSeekCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(seek_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(seek_cb_)),
            };
            p->seekWithFlags(p->@object, position, (MDKSeekFlag)flags, callback);
        }
    }

    /// <summary>
    /// Seek ms from start
    /// </summary>
    /// <param name="position">milliseconds seek target</param>
    /// <param name="cb"></param>
    public void Seek(long position, CallBackOnSeek? cb = null)
    {
        Seek(position, SeekFlag.Default, cb);
    }

    //public void SetPlaybackRate(float value)
    //{
    //    unsafe
    //    {
    //        p->setPlaybackRate(p->@object, value);
    //    }
    //}

    public float PlaybackRate
    {
        get { unsafe { return p->playbackRate(p->@object); } }
        set { unsafe { p->setPlaybackRate(p->@object, value); } }
    }

    public void Buffed(IntPtr bytes = 0)
    {
        unsafe
        {
            p->buffered(p->@object, (long*)bytes);
        }
    }

    public void SetBufferRange(long min = -1, long max = -1, bool drop = false)
    {
        unsafe
        {
            p->setBufferRange(p->@object, min, max, (byte)(drop ? 1 : 0));
        }
    }

    public delegate void CallBackOnSwitchBitrate(bool a);
    public void SwitchBitrate(string url, long delay = -1, CallBackOnSwitchBitrate? cb = null)
    {
        switch_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(byte value, void* opaque)
            {
                Marshal.GetDelegateForFunctionPointer<CallBackOnSwitchBitrate>((nint)opaque)(value != 0);
            }
            SwitchBitrateCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(switch_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(switch_cb_)),
            };
            var _url = Marshal.StringToCoTaskMemUTF8(url);
            p->switchBitrate(p->@object, _url, delay, callback);
            Marshal.FreeCoTaskMem(_url);
        }
    }

    public void SwitchBitrateSingleConnection(string url, CallBackOnSwitchBitrate? cb = null)
    {
        switch_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(byte value, void* opaque)
            {
                Marshal.GetDelegateForFunctionPointer<CallBackOnSwitchBitrate>((nint)opaque)(value != 0);
            }
            SwitchBitrateCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(switch_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(switch_cb_)),
            };
            var _url = Marshal.StringToCoTaskMemUTF8(url);
            p->switchBitrateSingleConnection(p->@object, _url, callback);
            Marshal.FreeCoTaskMem(_url);
        }
    }

    public delegate bool CallBackOnEvent(MediaEvent a);
    /// <summary>
    /// Add/Remove a CallBackOnEvent listener, or remove listeners.
    /// </summary>
    /// <param name="cb">the callback. return true if event is processed and should stop dispatching.</param>
    /// <param name="token">see https://github.com/wang-bin/mdk-sdk/wiki/Types#callbacktoken</param>
    /// <returns></returns>
    public MDKPlayer OnEvent(CallBackOnEvent cb, IntPtr token = 0)
    {
        unsafe
        {
            mdkMediaEventCallback callback = new();
            if (cb == null)
            {
                p->onEvent(p->@object, callback, (ulong*)(token != IntPtr.Zero ? event_cb_key_[*(ulong*)token] : 0));
                if (token != IntPtr.Zero)
                {
                    event_cb_.Remove(*(ulong*)token);
                    event_cb_key_.Remove(*(ulong*)token);
                }
                else
                {
                    event_cb_.Clear();
                    event_cb_key_.Clear();
                }
            }
            else
            {
                event_cb_[onEvent_k] = cb;
                [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
                static byte temp(mdkMediaEvent* me, void* opaque)
                {
                    var f = Marshal.GetDelegateForFunctionPointer<CallBackOnEvent>((nint)opaque);
                    MediaEvent e = new()
                    {
                        Error = me->error,
                        Category = Marshal.PtrToStringUTF8((nint)me->category) ?? "",
                        Detail = Marshal.PtrToStringUTF8((nint)me->detail) ?? "",
                        Stream = me->decoder.stream,
                        VideoHeight = me->video.height,
                        VideoWidth = me->video.width,
                    };
                    return (byte)(f(e) ? 1 : 0);
                }
                callback = new()
                {
                    cb = &temp,
                    opaque = (void*)Marshal.GetFunctionPointerForDelegate(event_cb_[onEvent_k]),
                };
                CallbackToken t = 0;
                p->onEvent(p->@object, callback, (ulong*)t);
                event_cb_key_[onEvent_k] = t;
                if (token != IntPtr.Zero)
                {
                    *(ulong*)token = t;
                }
                onEvent_k++;
            }
        }
        return this;
    }

    public void Record(string url, string format)
    {
        unsafe
        {
            var _url = Marshal.StringToCoTaskMemUTF8(url);
            var _format = Marshal.StringToCoTaskMemUTF8(format);
            p->record(p->@object, _url, _format);
            Marshal.FreeCoTaskMem(_url);
            Marshal.FreeCoTaskMem(_format);
        }
    }

    public void SetLoop(int count)
    {
        unsafe { p->setLoop(p->@object, count); }
    }

    public delegate void CallBackOnLoop(int count);
    public MDKPlayer OnLoop(CallBackOnLoop cb, IntPtr token)
    {
        unsafe
        {
            mdkLoopCallback callback = new();
            if (cb == null)
            {
                p->onLoop(p->@object, callback, (ulong*)(token != IntPtr.Zero ? loop_cb_key_[*(ulong*)token] : 0));
                if (token != IntPtr.Zero)
                {
                    loop_cb_.Remove(*(ulong*)token);
                    loop_cb_key_.Remove(*(ulong*)token);
                }
                else
                {
                    loop_cb_.Clear();
                    loop_cb_key_.Clear();
                }
            }
            else
            {
                loop_cb_[onLoop_k] = cb;
                [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
                static void temp(int count, void* opaque)
                {
                    Marshal.GetDelegateForFunctionPointer<CallBackOnLoop>((nint)opaque)(count);
                }
                callback = new()
                {
                    cb = &temp,
                    opaque = (void*)Marshal.GetFunctionPointerForDelegate(loop_cb_[onLoop_k]),
                };
                CallbackToken t = 0;
                p->onLoop(p->@object, callback, (ulong*)t);
                loop_cb_key_[onLoop_k] = t;
                if (token != IntPtr.Zero)
                {
                    *(ulong*)token = t;
                }
                onLoop_k++;
            }
        }
        return this;
    }

    public void SetRange(long a, long b = long.MaxValue)
    {
        unsafe
        {
            p->setRange(p->@object, a, b);
        }
    }

    public delegate double CallBackOnSync();
    public MDKPlayer OnSync(CallBackOnSync cb, int minInterval = 10)
    {
        sync_cb_ = cb;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static double temp(void* opaque)
            {
                var f = Marshal.GetDelegateForFunctionPointer<CallBackOnSync>((nint)opaque);
                return f();
            }
            mdkSyncCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(sync_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(sync_cb_)),
            };
            p->onSync(p->@object, callback, minInterval);
        }
        return this;
    }

    public delegate int CallBackOnFrame(VideoFrame frame, int track);
    public MDKPlayer OnFrame(CallBackOnFrame cb)
    {
        unsafe
        {
            video_cb_ = cb;
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static int temp(mdkVideoFrameAPI** pFrame, int track, void* opaque)
            {
                var f = Marshal.GetDelegateForFunctionPointer<CallBackOnFrame>((nint)opaque);
                VideoFrame frame = new(null);
                frame.Attach(*pFrame);
                var pendings = f(frame, track);
                *pFrame = frame.Detach();
                return pendings;
            }
            mdkVideoCallback callback = new()
            {
                cb = &temp,
                opaque = (void*)(video_cb_ == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(video_cb_)),
            };
            p->onVideo(p->@object, callback);
        }
        return this;
    }

    public void Dispose()
    {
        unsafe
        {
            if (owner_)
            {
                fixed (mdkPlayerAPI** pp = &p)
                {
                    Methods.mdkPlayerAPI_delete(pp);
                }
            }
        }
    }
}