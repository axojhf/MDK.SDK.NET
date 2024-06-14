using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;
using CallbackToken = System.UInt64;

namespace MDK.SDK.NET;

/// <summary>
/// High level API with basic playback function.
/// </summary>
public class MDKPlayer : IDisposable
{
    unsafe private mdkPlayerAPI* p = null;
    private bool owner_ = false;

    private bool mute_ = false;
    private float volume_ = 1.0f;
    private CallbackCurrentMediaChanged? current_cb_ = null;
    readonly object current_mtx_ = new();
    private CallBackOnTimeout? timeout_cb_ = null;
    readonly object timeout_mtx_ = new();
    private CallBackOnPrepare? prepare_cb_ = null;
    private CallBackOnStateChanged? state_cb_ = null;
    readonly object state_mtx_ = new();
    private CallBackOnRender? render_cb_ = null;
    readonly object render_mtx_ = new();
    private CallBackOnSeek? seek_cb_ = null;
    private CallBackOnSwitchBitrate? switch_cb_ = null;
    readonly object switch_mtx_ = new();
    private CallBackOnSnapshot? snapshot_cb_ = null;
    private CallBackOnFrame? video_cb_ = null;
    readonly object video_mtx_ = new();
    private CallBackOnSync? sync_cb_ = null;
    readonly object sync_mtx_ = new();
    private Dictionary<CallbackToken, CallBackOnEvent> event_cb_ = [];
    private Dictionary<CallbackToken, CallbackToken> event_cb_key_ = [];
    readonly object event_mtx_ = new();
    private Dictionary<CallbackToken, CallBackOnLoop> loop_cb_ = [];
    private Dictionary<CallbackToken, CallbackToken> loop_cb_key_ = [];
    readonly object loop_mtx_ = new();
    private Dictionary<CallbackToken, CallBackOnMediaStatus> status_cb_ = [];
    private Dictionary<CallbackToken, CallbackToken> status_cb_key_ = [];
    readonly object status_mtx_ = new();
    private MediaInfo info_;

    private static CallbackToken onEvent_k = 1;
    private static CallbackToken onLoop_k = 1;
    private static CallbackToken onStatus_k = 1;

    /// <summary>
    /// Initializes a new instance of MDK player.
    /// </summary>
    public MDKPlayer()
    {
        unsafe
        {
            p = Methods.mdkPlayerAPI_new();
        }
        owner_ = true;
    }

    /// <summary>
    /// Release GL resources bound to the context.<br/>
    /// MUST be called when a foreign OpenGL context previously used is being destroyed and player object is already destroyed. The context MUST be current.<br/>
    /// If player object is still alive, setVideoSurfaceSize(-1, -1, ...) is preferred.<br/>
    /// If forget to call both foreignGLContextDestroyed() and setVideoSurfaceSize(-1, -1, ...) in the context, resources will be released in the next draw in the same context.  But the context may be destroyed later, then resource will never be released<br/>
    /// </summary>
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

    /// <summary>
    /// Set audio volume level<br/>
    /// The same as ms log2(SpeakerPosition), see https://docs.microsoft.com/windows-hardware/drivers/ddi/ksmedia/ns-ksmedia-ksaudio_channel_config#remarks
    /// </summary>
    /// <param name="value">linear volume level, range is >=0. 1.0 is source volume</param>
    /// <param name="channel">channel number, int value of AudioFormat::Channel, -1 for all channels.</param>
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

    /// <summary>
    /// Get audio volume level
    /// </summary>
    /// <returns>linear volume level, range from 0.0 to 1.0</returns>
    public float Volume()
    {
        return volume_;
    }

    /// <summary>
    /// Set frame rate, frames per seconds
    /// </summary>
    /// <param name="value">
    /// frame rate
    /// <para>0 (default): use frame timestamp, or default frame rate 25.0fps if stream has no timestamp</para>
    /// <para>&lt;0: render ASAP.</para>
    /// <para>&gt;0: target frame rate</para>
    /// </param>
    public void SetFrameRate(float value)
    {
        unsafe
        {
            p->setFrameRate(p->@object, value);
        }
    }

    /// <summary>
    /// Set a new media url.  If url changed, will stop current playback, and reset active tracks, external tracks set by setMedia(url, type)<br/>
    /// MUST call setActiveTracks() after setMedia(), otherwise the 1st track in the media is used
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

    /// <summary>
    /// Set an individual source as track of `type`, e.g. audio track file, external subtile file. **MUST** be after main media `setMedia(url)`.<br/>
    /// If url is empty, use `type` tracks in MediaType::Video url.<br/>
    /// The url can contains other track types, e.g.you can load an external audio/subtitle track from a video file, and use `setActiveTracks()` to select a track.<br/>
    ///  Note: because of filesystem restrictions on some platforms(iOS, macOS, uwp), and unable to access files in a sandbox, so you have to load subtitle files manually yourself via this function.
    /// <para>examples: set subtitle file: <code>setMedia("name.ass", MediaType::Subtitle)</code></para>
    /// </summary>
    /// <param name="url"></param>
    /// <param name="type"></param>
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

    /// <summary>
    /// Gapless play the next media after current media playback end<br/>
    /// set(State::Stopped) only stops current media. Call setNextMedia(nullptr, -1) first to disable next media.<br/>
    /// Usually you can call <code>currentMediaChanged()</code> to set a callback which invokes <code>setNextMedia()</code>, then call <code>setMedia()</code>.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="startPosition"></param>
    /// <param name="flags">seek flags if startPosition > 0, accurate or fast</param>
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

    /// <summary>
    /// Set a callback which is invoked when current media is stopped and a new media is about to play, or when setMedia() is called.<br/>
    /// Call before setMedia() to take effect.
    /// </summary>
    /// <param name="cb"></param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">if type is MediaType::Unknown, select a program(usually for mpeg ts streams). must contains only 1 value, N, indicates using the Nth program's audio and video tracks.
    /// Otherwise, select a set of tracks of given type.</param>
    /// <param name="tracks">tracks set of active track number, from 0~N. Invalid track numbers will be ignored</param>
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
    /// backends can be: AudioQueue(Apple only), OpenSL, AudioTrack(Android only), ALSA(linux only), XAudio2(Windows only), OpenAL
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
    /// <summary>
    /// callback ms: elapsed milliseconds<br/>
    /// callback return: true to abort current operation on timeout.<br/>
    /// A null callback can abort current operation.<br/>
    /// Negative timeout infinit.<br/>
    /// Default timeout is 10s
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="cb"></param>
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

    /// <summary>
    /// PrepareCallback<br/>
    /// example: always return false can be used as media information reader
    /// </summary>
    /// <param name="position">position in callback is the timestamp of the 1st frame(video if exists) after seek, or &lt; 0 (TODO: error code as position) if prepare() failed.</param>
    /// <param name="boost">boost in callback can be set by user(*boost = true/false) to boost the first frame rendering. default is true.</param>
    /// <returns>false to unload media immediately when media is loaded and MediaInfo is ready, true to continue.</returns>
    public delegate bool CallBackOnPrepare(long position, IntPtr boost);
    /// <summary>
    /// Preload a media and then becomes State::Paused.<br/>
    /// To play a media from a given position, call prepare(ms) then set(State::Playing)<br/>
    /// For fast seek(has flag SeekFlag::Fast), the first frame is a key frame whose timestamp >= startPosition<br/>
    /// For accurate seek(no flag SeekFlag::Fast), the first frame is the nearest frame whose timestamp &lt;= startPosition, but the position passed to callback is the key frame position &lt;= startPosition
    /// </summary>
    /// <param name="startPosition">start from position, relative to media start position(i.e. MediaInfo.start_time)</param>
    /// <param name="cb">if startPosition > 0, same as callback of seek(), called after the first frame is decoded or load/seek/decode error. If startPosition == 0, called when media is loaded and mediaInfo is ready or load error.</param>
    /// <param name="flags">seek flag if startPosition != 0.</param>
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

    /// <summary>
    /// Current MediaInfo. You can call it in prepare() callback which is called when loaded or load failed.<br/>
    /// Some fields can change during playback, e.g.video frame size change(via MediaEvent), live stream duration change, realtime bitrate change.<br/>
    /// You may get an invalid value if mediaInfo() is called immediately after `set(State::Playing)` or `prepare()` because media is still opening but not loaded , i.e.mediaStatus() has no MediaStatus::Loaded flag.<br/>
    /// A live stream's duration is 0 in prepare() callback or when MediaStatus::Loaded is added, then duration increases current read duration.
    /// </summary>
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

    /// <summary>
    /// If failed to open a media, e.g. invalid media, unsupported format, waitFor() will finish without state change
    /// </summary>
    /// <param name="value"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
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
        unsafe
        {
            lock (status_mtx_)
            {
                mdkMediaStatusCallback callback = new();
                if (cb == null)
                {
                    p->onMediaStatus(p->@object, callback, (ulong*)(token != IntPtr.Zero ? loop_cb_key_[*(ulong*)token] : 0));
                    if (token != IntPtr.Zero)
                    {
                        status_cb_.Remove(*(ulong*)token);
                        status_cb_key_.Remove(*(ulong*)token);
                    }
                    else
                    {
                        status_cb_.Clear();
                        status_cb_key_.Clear();
                    }
                }
                else
                {
                    status_cb_[onStatus_k] = cb;
                    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
                    static byte temp(MDK_MediaStatus old, MDK_MediaStatus value, void* opaque)
                    {
                        return (byte)(Marshal.GetDelegateForFunctionPointer<CallBackOnMediaStatus>((nint)opaque)((MediaStatus)old, (MediaStatus)value) ? 1 : 0);
                    }
                    callback = new()
                    {
                        cb = &temp,
                        opaque = (void*)Marshal.GetFunctionPointerForDelegate(status_cb_[onStatus_k]),
                    };
                    CallbackToken t = 0;
                    p->onMediaStatus(p->@object, callback, (ulong*)token);
                    status_cb_key_[onStatus_k] = t;
                    if (token != 0)
                    {
                        *(ulong*)token = t;
                    }
                    onStatus_k++;
                }

            }
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
    /// <param name="type">ignored if win ptr does not change (request to resize)</param>
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
    /// <summary>
    /// take a snapshot from current renderer. The result is in bgra format, or null on failure.<br/>
    /// When `snapshot()` is called, redraw is scheduled for `vo_opaque`'s renderer, then renderer will take a snapshot in rendering thread.<br/>
    /// So for a foreign context, if renderer's surface/window/widget is invisible or minimized, snapshot may do nothing because of system or gui toolkit painting optimization.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cb"></param>
    /// <param name="vo_opaque"></param>
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

    /// <summary>
    /// Set additional properties. Can be used to store user data, or change player behavior if the property is defined internally.
    ///Predefined properties are:
    /// <para></para>
    /// <para>"audio.avfilter": ffmpeg avfilter filter graph string for audio track. take effect immediately</para>
    /// <para>"continue_at_end" or "keep_open": "0" or "1". do not stop playback when decode and render to end of stream. only set(State::Stopped) can stop playback. Useful for timeline preview.</para>
    /// <para>"cc": "0" or "1"(default). enable closed caption decoding and rendering.</para>
    /// <para>"subtitle": "0" or "1"(default). enable subtitle(including cc) rendering. setActiveTracks(MediaType::Subtitle, {...}) enables decoding only.</para>
    /// <para>"avformat.some_name": avformat option, e.g. {"avformat.fpsprobesize": "0"}. if global option "demuxer.io=0", it also can be AVIOContext/URLProtocol option</para>
    /// <para>"avio.some_name": AVIOContext/URLProtocol option, e.g. avio.user_agent for UA, avio.headers for http headers.</para>
    /// <para>"avcodec.some_name": AVCodecContext option, will apply for all FFmpeg based video/audio/subtitle decoders. To set for a single decoder, use setDecoders() with options</para>
    /// <para>"audio.decoders": decoder list for setDecoders(), with or without decoder properties. "name1,name2:key21=val21"</para>
    /// <para>"video.decoders": decoder list for setDecoders(), with or without decoder properties. "name1,name2:key21=val21"</para>
    /// <para>"audio.decoder": audio decoder property, value is "key=value" or "key1=value1:key2=value2". override "decoder" property</para>
    /// <para>"video.decoder": video decoder property, value is "key=value" or "key1=value1:key2=value2". override "decoder" property</para>
    /// <para>"decoder": video and audio decoder property, value is "key=value" or "key1=value1:key2=value2"</para>
    /// <para>"recorder.copyts": "1" or "0"(default), use input packet timestamp, or correct packet timestamp to be continuous.</para>
    /// <para>"reader.starts_with_key": "0" or "1"(default). if "1", video decoder starts with key-frame, and drop non-key packets before the first decode.</para>
    /// <para>"buffer" or "buffer.range": parameters setBufferRange(). value is "minMs", "minMs+maxMs", "minMs+maxMs-", "minMs-". the last '-' indicates drop mode</para>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
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

    /// <summary>
    /// The rectangular viewport where the scene will be drawn relative to surface viewport.<br/>
    /// x, y, w, h are normalized to[0, 1]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="vo_opaque"></param>
    public void SetVideoViewport(float x, float y, float width, float height, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setVideoViewport(p->@object, x, y, width, height, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// Video display aspect ratio.
    /// IgnoreAspectRatio(0): ignore aspect ratio and scale to fit renderer viewport<br/>
    /// KeepAspectRatio(default) : keep frame aspect ratio and scale as large as possible inside renderer viewport<br/>
    /// KeepAspectRatioCrop: keep frame aspect ratio and scale as small as possible outside renderer viewport<br/>
    /// other value &gt; 0: like KeepAspectRatio, but keep given aspect ratio and scale as large as possible inside renderer viewport<br/>
    /// other value &lt; 0: like KeepAspectRatioCrop, but keep given aspect ratio and scale as small as possible inside renderer viewport
    /// </summary>
    /// <param name="value"></param>
    /// <param name="vo_opaque"></param>
    public void SetAspectRatio(float value, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setAspectRatio(p->@object, value, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// rotate around video frame center
    /// </summary>
    /// <param name="degree">0, 90, 180, 270, counterclockwise</param>
    /// <param name="vo_opaque"></param>
    public void Rotate(int degree, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->rotate(p->@object, degree, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// scale frame size. x, y can be &lt; 0, means scale and flip.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="vo_opaque"></param>
    public void Scale(float x, float y, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->scale(p->@object, x, y, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// map a point from one coordinates to another. a frame must be rendered. coordinates is normalized to [0, 1].
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="x">points to x coordinate of viewport or currently rendered video frame</param>
    /// <param name="y"></param>
    /// <param name="z">not used</param>
    /// <param name="vo_opaque"></param>
    public void MapPoint(MapDirection dir, IntPtr x, IntPtr y, IntPtr z = default, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->mapPoint(p->@object, (MDK_MapDirection)dir, (float*)x, (float*)y, (float*)z, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// Can be called on any thread
    /// </summary>
    /// <param name="videoRoi">array of 2d point (x, y) in video frame. coordinate: top-left = (0, 0), bottom-right=(1, 1). set null to disable mapping</param>
    /// <param name="viewRoi">array of 2d point (x, y) in video renderer. coordinate: top-left = (0, 0), bottom-right=(1, 1). null is the whole renderer.</param>
    /// <param name="count">point count. only support 2. set 0 to disable mapping</param>
    /// <param name="vo_opaque"></param>
    public void SetPointMap(IntPtr videoRoi, IntPtr viewRoi, int count = 2, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setPointMap(p->@object, (float*)videoRoi, (float*)viewRoi, count, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// set render api for a vo, useful for non-opengl(no way to get current context)
    /// </summary>
    /// <param name="api">To release gfx resources, set null api in rendering thread/context(required by vulkan)</param>
    /// <param name="vo_opaque"></param>
    /// <returns></returns>
    public MDKPlayer SetRenderAPI(IntPtr api, IntPtr vo_opaque = default)
    {
        unsafe
        {
            p->setRenderAPI(p->@object, (mdkRenderAPI*)api, (void*)vo_opaque);
        }
        return this;
    }

    /// <summary>
    /// get render api. For offscreen rendering, may only api type be valid in setRenderAPI(), and other members are filled internally, and used by user after renderVideo()
    /// </summary>
    /// <param name="vo_opaque"></param>
    /// <returns></returns>
    public IntPtr RenderAPI(IntPtr vo_opaque = default)
    {
        unsafe
        {
            return (IntPtr)p->renderAPI(p->@object, (void*)vo_opaque);
        }
    }

    /// <summary>
    /// Render the next or current(redraw) frame. Foreign render context only (i.e. not created by createSurface()/updateNativeSurface()).<br/>
    /// OpenGL: Can be called in multiple foreign contexts for the same vo_opaque.
    /// </summary>
    /// <param name="vo_opaque"></param>
    /// <returns>timestamp of rendered frame, or &lt; 0 if no frame is rendered. precision is microsecond</returns>
    public double RenderVideo(IntPtr vo_opaque = 0)
    {
        unsafe
        {
            return (double)(p->renderVideo(p->@object, (void*)vo_opaque));
        }
    }

    /// <summary>
    /// Send the frame to video renderer. You must call renderVideo() later in render thread
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="opaque"></param>
    public void Enqueue(IntPtr frame, IntPtr opaque = 0)
    {
        unsafe
        {
            p->enqueueVideo(p->@object, (mdkVideoFrameAPI*)frame, (void*)opaque);
        }
    }

    /// <summary>
    /// r, g, b, a range is [0, 1]. default is 0. if out of range, background color will not be filled
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    /// <param name="vo_opaque"></param>
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

    /// <summary>
    /// Set output color space.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="value">
    /// <para>invalid (ColorSpaceUnknown): renderer will try to use the value of decoded frame, and will send hdr10 metadata when possible. i.e. hdr videos will enable hdr display. Currently only supported by metal, and `MetalRenderAPI.layer` must be a `CAMetalLayer` ([example](https://github.com/wang-bin/swift-mdk/blob/master/Player.swift#L184))</para>
    /// <para>hdr colorspace(ColorSpaceBT2100_PQ): no hdr metadata will be sent to the display, sdr will map to hdr.Can be used by the gui toolkits which support hdr swapchain but no api to change swapchain colorspace and format on the fly, see[Qt example] (https://github.com/wang-bin/mdk-examples/blob/master/Qt/qmlrhi/VideoTextureNodePub.cpp#L83)</para>
    /// <para>sdr color space(ColorSpaceBT709) : the default. HDR videos will tone map to SDR.</para>
    /// </param>
    /// <param name="vo_opaque"></param>
    public void Set(ColorSpace value, IntPtr vo_opaque = 0)
    {
        unsafe
        {
            p->setColorSpace(p->@object, (MDK_ColorSpace)value, (void*)vo_opaque);
        }
    }

    public delegate void CallBackOnRender(IntPtr vo_opaque);
    /// <summary>
    /// set a callback which is invoked when the vo coresponding to vo_opaque needs to update/draw content, e.g. when a new frame is received in the renderer.<br/>
    /// Also invoked in setVideoSurfaceSize(), setVideoViewport(), setAspectRatio() and rotate(), take care of dead lock in callback and above functions.<br/>
    /// with vo_opaque, user can know which vo/renderer is rendering, useful for multiple renderers<br/>
    /// There may be no frames or playback not even started, but renderer update is required internally<br/>
    /// DO NOT call renderVideo() in the callback, otherwise will results in dead lock
    /// </summary>
    /// <param name="cb"></param>
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

    /// <summary>
    /// Current playback time in milliseconds. Relative to media's first timestamp, which usually is 0.<br/>
    /// If has active video tracks, it's currently presented video frame time. otherwise, it's audio time.
    /// </summary>
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

    /// <summary>
    /// get buffered undecoded data duration and size
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>buffered data(packets) duration</returns>
    public long Buffed(IntPtr bytes = 0)
    {
        unsafe
        {
            return p->buffered(p->@object, (long*)bytes);
        }
    }

    /// <summary>
    /// set duration range of buffered data.<br/>
    /// For realtime streams like(rtp, rtsp, rtmp sdp etc.), the default range is [0, INT64_MAX, true].<br/>
    /// Usually you don't need to call this api. This api can be used for low latency live videos, for example setBufferRange(0, INT64_MAX, true) will decode as soon as possible when media data received, and no accumulated delay.
    /// </summary>
    /// <param name="minMs">
    /// default 1000. wait for buffered duration >= minMs when before popping a packet.
    /// <para>If minMs &lt; 0, then minMs, maxMs and drop will be reset to the default value.</para>
    /// <para>If minMs > 0, when packets queue becomes empty, `MediaStatus::Buffering` will be set until queue duration >= minMs, "reader.buffering" MediaEvent will be triggered.</para>
    /// <para>If minMs == 0, decode ASAP.</para>
    /// </param>
    /// <param name="maxMs">
    /// default 4000. max buffered duration. Large value is recommended. Latency is not affected.
    /// <para>If maxMs &lt; 0, then maxMs and drop will be reset to the default value</para>
    /// <para>If maxMs == 0, same as INT64_MAX drop = true:</para>
    /// </param>
    /// <param name="drop">
    /// <para>drop = true:<br/>
    /// drop old non-key frame packets to reduce buffered duration until &lt; maxMs. If maxMs(!= 0 or INT64_MAX) is smaller than key-frame interval, no drop effect.<br/>
    /// If maxMs == 0 or INT64_MAX, always drop old packets and keep at most 1 key-frame packet</para>
    /// <para>drop = false: <br/>
    /// wait for buffered duration &lt; maxMs before pushing packets</para>
    /// </param>
    public void SetBufferRange(long minMs = -1, long maxMs = -1, bool drop = false)
    {
        unsafe
        {
            p->setBufferRange(p->@object, minMs, maxMs, (byte)(drop ? 1 : 0));
        }
    }

    public delegate void CallBackOnSwitchBitrate(bool a);

    /// <summary>
    /// A new media will be played later
    /// </summary>
    /// <param name="url"></param>
    /// <param name="delay">switch after at least delay ms. TODO: determined by buffered time, e.g. from high bit rate without enough buffered samples to low bit rate</param>
    /// <param name="cb">(true/false) called when finished/failed</param>
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

    /// <summary>
    /// Only 1 media is loaded. The previous media is unloaded and the playback continues. When new media is preloaded, stop the previous media at some point<br/>
    /// * MUST call setPreloadImmediately(false) because PreloadImmediately for signal connection preload is not possible.<br/>
    /// This will not affect next media set by user
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cb"></param>
    /// <returns>false if preload immediately</returns>
    public bool SwitchBitrateSingleConnection(string url, CallBackOnSwitchBitrate? cb = null)
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
            var ret = p->switchBitrateSingleConnection(p->@object, _url, callback);
            Marshal.FreeCoTaskMem(_url);
            return ret != 0;
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
            lock (event_mtx_)
            {
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
        }
        return this;
    }

    /// <summary>
    /// Start to record or stop recording current media by remuxing packets read. If media is not loaded, recorder will start when playback starts<br/>
    /// examples:<br/>
    /// player.record("record.mov");<br/>
    /// player.record("rtmp://127.0.0.1/live/0", "flv");<br/>
    /// player.record("rtsp://127.0.0.1/live/0", "rtsp");
    /// </summary>
    /// <param name="url">destination. null or the same value as recording one to stop recording. can be a local file, or a network stream</param>
    /// <param name="format">forced format. if null, guess from url. if null and format guessed from url does not support all codecs of current media, another suitable format will be used</param>
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

    /// <summary>
    /// Set A-B loop repeat count.
    /// </summary>
    /// <param name="count">repeat count. 0 to disable looping and stop when out of range(B)</param>
    public void SetLoop(int count)
    {
        unsafe { p->setLoop(p->@object, count); }
    }

    public delegate void CallBackOnLoop(int count);
    /// <summary>
    /// add/remove a callback which will be invoked right before a new A-B loop
    /// </summary>
    /// <param name="cb">callback with current loop count elapsed</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public MDKPlayer OnLoop(CallBackOnLoop cb, IntPtr token = 0)
    {
        unsafe
        {
            mdkLoopCallback callback = new();
            lock (loop_mtx_)
            {
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
        }
        return this;
    }

    /// <summary>
    /// Set A-B loop range, or playback range
    /// </summary>
    /// <param name="a">loop position begin, in ms.</param>
    /// <param name="b">loop position end, in ms. -1, INT64_MAX or numeric_limit&lt;int64_t>::max() indicates b is the end of media</param>
    public void SetRange(long a, long b = long.MaxValue)
    {
        unsafe
        {
            p->setRange(p->@object, a, b);
        }
    }

    public delegate double CallBackOnSync();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cb">a callback invoked when about to render a frame. return expected current playback position(seconds), e.g. DBL_MAX(TimestampEOS) indicates render video frame ASAP.
    /// sync callback clock should handle pause, resume, seek and seek finish events</param>
    /// <param name="minInterval"></param>
    /// <returns></returns>
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
    /// <summary>
    /// A callback to be invoked before delivering a frame to renderers. Frame can be VideoFrame and AudioFrame(NOT IMPLEMENTED).<br/>
    /// The callback can be used as a filter.<br/>
    /// TODO: frames not in host memory<br/>
    /// For most filters, 1 input frame generates 1 output frame, then return 0.
    /// </summary>
    /// <param name="cb">callback to be invoked. returns pending number of frames. callback parameter is input and output frame. if input frame is an invalid frame, output a pending frame.</param>
    /// <returns></returns>
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