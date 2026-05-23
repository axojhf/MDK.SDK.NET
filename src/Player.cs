using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MDK.SDK.NET.Gen;
using CallbackToken = ulong;

namespace MDK.SDK.NET;

/// <summary>
/// High level API with basic playback function.
/// </summary>
// ReSharper disable once InconsistentNaming
public class MDKPlayer : IDisposable
{
    private unsafe mdkPlayerAPI* _p;
    private readonly bool _owner;

    private bool _mute;
    private float _volume = 1.0f;
    private readonly Dictionary<CallbackToken, CallBackOnEvent> _eventCb = [];
    private readonly Dictionary<CallbackToken, CallbackToken> _eventCbKey = [];
    private readonly Dictionary<CallbackToken, CallBackOnLoop> _loopCb = [];
    private readonly Dictionary<CallbackToken, CallbackToken> _loopCbKey = [];
    private readonly Lock _loopMtx = new();
    private readonly Dictionary<CallbackToken, CallBackOnMediaStatus> _statusCb = [];
    private readonly Dictionary<CallbackToken, CallbackToken> _statusCbKey = [];
    private readonly Lock _statusMtx = new();

    private static CallbackToken _onEventK = 1;
    private static CallbackToken _onLoopK = 1;
    private static CallbackToken _onStatusK = 1;

    /// <summary>
    /// Initializes a new instance of MDK player.
    /// </summary>
    public MDKPlayer()
    {
        unsafe
        {
            _p = Methods.mdkPlayerAPI_new();
        }
        _owner = true;
    }

    /// <summary>
    /// Release GL resources bound to the context.<br/>
    /// MUST be called when a foreign OpenGL context previously used is being destroyed and player object is already destroyed. The context MUST be current.<br/>
    /// If player object is still alive, setVideoSurfaceSize(-1, -1, ...) is preferred.<br/>
    /// If forget to call both foreignGLContextDestroyed() and setVideoSurfaceSize(-1, -1, ...) in the context, resources will be released in the next draw in the same context.  But the context may be destroyed later, then resource will never be released<br/>
    /// </summary>
    public static void ForeignGlContextDestroyed()
    {
        Methods.MDK_foreignGLContextDestroyed();
    }

    /// <summary>
    /// mute or not
    /// </summary>
    /// <param name="value"></param>
    public void SetMute(bool value = true)
    {
        unsafe
        {
            _p->setMute(_p->@object, (byte)(value ? 1 : 0));
        }
        _mute = value;
    }

    /// <summary>
    /// is audio muted
    /// </summary>
    /// <returns></returns>
    public bool IsMute()
    {
        return _mute;
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
                _p->setVolume(_p->@object, value);
            }
            else
            {
                _p->setChannelVolume(_p->@object, value, channel);
            }
        }
        _volume = value;
    }

    /// <summary>
    /// Get audio volume level
    /// </summary>
    /// <returns>linear volume level, range from 0.0 to 1.0</returns>
    public float Volume()
    {
        return _volume;
    }

    /// <summary>
    /// Set frame rate, frames per seconds. Useful for videos without audio and timestamp.
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
            _p->setFrameRate(_p->@object, value);
        }
    }

    /// <summary>
    /// Set a new media url.  If url changed, will stop current playback, and reset active tracks, external tracks set by setMedia(url, type)<br/>
    /// MUST call setActiveTracks() after setMedia(), otherwise the 1st track in the media is used<br/>
    /// Supported protocols: FFmpeg protocols, Android(content, android.resource, assets), iOS(assets-library), UWP(winrt), mem(mem:addr+size), stream(see appendBuffer).<br/>
    /// A url query <c>mdkopt=avformat&amp;...</c> will be treated as ffmpeg avformat options.
    /// </summary>
    /// <param name="url"></param>
    public void SetMedia(string url)
    {
        unsafe
        {
            var urlUtf8 = Marshal.StringToCoTaskMemUTF8(url);
            _p->setMedia(_p->@object, (sbyte*)urlUtf8);
            Marshal.FreeCoTaskMem(urlUtf8);
        }
    }

    /// <summary>
    /// Set an individual source as track of `type`, e.g. audio track file, external subtile file. **MUST** be after main media `setMedia(url)`.<br/>
    /// If url is empty, use `type` tracks in MediaType::Video url.<br/>
    /// The url can contains other track types, e.g.you can load an external audio/subtitle track from a video file, and use `setActiveTracks()` to select a track.<br/>
    /// To switch back to embedded(internal) tracks, disable external track first via <c>setMedia(nullptr, type)</c>, then call <c>setActiveTracks(type, {0})</c>.<br/>
    ///  Note: because of filesystem restrictions on some platforms(iOS, macOS, uwp), and unable to access files in a sandbox, so you have to load subtitle files manually yourself via this function.
    /// <para>examples: set subtitle file: <code>setMedia("name.ass", MediaType::Subtitle)</code></para>
    /// </summary>
    /// <param name="url"></param>
    /// <param name="type"></param>
    public void SetMedia(string url, MediaType type)
    {
        unsafe
        {
            var bytes = Encoding.UTF8.GetBytes(url + char.MinValue);
            fixed (byte* ptr = bytes)
                _p->setMediaForType(_p->@object, (sbyte*)ptr, (MDK_MediaType)type);
        }
    }

    /// <summary>
    /// get current media url
    /// </summary>
    /// <returns></returns>
    public string Url()
    {
        unsafe
        {
            var url = _p->url(_p->@object);
            return Marshal.PtrToStringUTF8((nint)url) ?? "";
        }
    }

    /// <summary>
    /// When media url protocol is "stream:", i.e. <c>setMedia("stream:empty_or_any_string")</c>, player is in stream playback mode, and user must provide data via this method.<br/>
    /// setTimeout can abort current stream playback if timedout to read data from user.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="size"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public bool AppendBuffer(IntPtr data, nuint size, int options = 0)
    {
        unsafe
        {
            return _p->appendBuffer(_p->@object, (byte*)data, size, options) != 0;
        }
    }

    /// <summary>
    /// set if preload media immediately after SetMedia() or Prepare() is called
    /// </summary>
    /// <param name="value"></param>
    public void SetPreloadImmediately(bool value = true)
    {
        unsafe
        {
            _p->setPreloadImmediately(_p->@object, (byte)(value ? 1 : 0));
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
            var bytes = Encoding.UTF8.GetBytes(url + char.MinValue);
            fixed (byte* ptr = bytes)
                _p->setNextMedia(_p->@object, (sbyte*)ptr, startPosition, (MDKSeekFlag)flags);
        }
    }

    /// <summary>
    /// a delegate for CurrentMediaChanged
    /// </summary>
    public delegate void CallbackCurrentMediaChanged();
    private sealed class CurrentMediaChangedCtx
    {
        public CallbackCurrentMediaChanged? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly CurrentMediaChangedCtx _currentCtx = new();
    private GCHandle? _currentCtxGcHandle;

    /// <summary>
    /// Set a callback which is invoked when current media is stopped and a new media is about to play, or when setMedia() is called.<br/>
    /// Call before setMedia() to take effect.
    /// </summary>
    /// <param name="cb"></param>
    public void CurrentMediaChanged(CallbackCurrentMediaChanged? cb)
    {
        nint opaque = 0;
        lock (_currentCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_currentCtx.Disposed, this);
            _currentCtx.Callback = cb;

            if (cb is not null)
            {
                _currentCtxGcHandle ??= GCHandle.Alloc(_currentCtx);
                opaque = GCHandle.ToIntPtr(_currentCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkCurrentMediaChangedCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnCurrentMediaChanged,
                    opaque = (void*)opaque,
                };
            _p->currentMediaChanged(_p->@object, callback);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnCurrentMediaChanged(void* opaque)
    {
        if (opaque == null)
            return;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not CurrentMediaChangedCtx ctx) return;

            CallbackCurrentMediaChanged? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return;

                callback = ctx.Callback;
            }

            callback?.Invoke();
        }
        catch
        {
            // Managed exceptions must not cross the unmanaged callback boundary.
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
            _p->setActiveTracks(_p->@object, (MDK_MediaType)type, pTs, (nuint)tracks.Count);
        }
    }

    /// <summary>
    /// Set a list of audio renderer implementations. Available backends:
    /// <para>Apple: AudioQueue(default), OpenAL</para>
    /// <para>Android: AAudio(default since 0.36.0), OpenSL, AudioTrack</para>
    /// <para>OHOS: OHAudio(default), OpenSL</para>
    /// <para>Windows: XAudio2(default), DirectSound</para>
    /// <para>Linux: PulseAudio(default), ALSA</para>
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
            _p->setAudioBackends(_p->@object, pdata);
            for (int i = 0; i < names.Count; i++)
            {
                Marshal.FreeCoTaskMem((IntPtr)pdata[i]);
            }
        }
    }

    /// <summary>
    /// Try decoders by name(case sensitive) in the given order and select it if works for current media. Can be called at anytime. When state is Playing, new decoders will be applied immediately.<br/>
    /// names can contain decoder options/properties separated by ':' and in key=value pattern, e.g. <c>"MFT:d3d=11"</c>.<br/>
    /// If a decoder is not found, will try to load a dynamic plugin <c>mdk-$name</c>.<br/>
    /// See https://github.com/wang-bin/mdk-sdk/wiki/Player-APIs#void-setdecodersmediatype-type-const-stdvectorstdstring-names
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
            _p->setDecoders(_p->@object, (MDK_MediaType)type, (sbyte*)pdata);
            for (int i = 0; i < names.Count; i++)
            {
                Marshal.FreeCoTaskMem((IntPtr)pdata[i]);
            }

        }
    }

    /// <summary>
    /// a delegate for SetTimeout
    /// </summary>
    public delegate bool CallBackOnTimeout(long ms);
    private sealed class SetTimeoutCtx
    {
        public CallBackOnTimeout? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly SetTimeoutCtx _timeoutCtx = new();
    private GCHandle? _timeoutCtxGcHandle;
    /// <summary>
    /// callback ms: elapsed milliseconds<br/>
    /// callback return: true to abort current operation on timeout.<br/>
    /// A null callback can abort current operation.<br/>
    /// Negative timeout infinit.<br/>
    /// Default timeout is 10s
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="cb"></param>
    public void SetTimeout(long ms, CallBackOnTimeout? cb)
    {
        nint opaque = 0;
        lock (_timeoutCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_timeoutCtx.Disposed, this);
            _timeoutCtx.Callback = cb;

            if (cb is not null)
            {
                _timeoutCtxGcHandle ??= GCHandle.Alloc(_timeoutCtx);
                opaque = GCHandle.ToIntPtr(_timeoutCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkTimeoutCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnTimeout,
                    opaque = (void*)opaque,
                };
            _p->setTimeout(_p->@object, ms, callback);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe byte OnTimeout(long ms, void* opaque)
    {
        if (opaque == null)
            return 0;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not SetTimeoutCtx ctx) return 0;

            CallBackOnTimeout? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return 0;

                callback = ctx.Callback;
            }

            return callback?.Invoke(ms) == true ? (byte)1 : (byte)0;
        }
        catch
        {
            return 0;
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
    private sealed class CallBackOnPrepareCtx
    {
        public required CallBackOnPrepare Callback { get; init; }
        private GCHandle _handle;
        private int _released;

        public void SetHandle(GCHandle handle)
        {
            _handle = handle;
        }

        public void ReleaseHandle()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0 && _handle.IsAllocated)
                _handle.Free();
        }
    }

    /// <summary>
    /// Preload a media and then becomes State::Paused.<br/>
    /// Must ensure playback is stopped before prepare() or new media: set(State::Stopped) + waitFor(State::Stopped) + prepare().<br/>
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
            CallBackOnPrepareCtx? ctx = null;
            nint opaque = 0;
            if (cb is not null)
            {
                ctx = new() { Callback = cb };
                var handle = GCHandle.Alloc(ctx);
                ctx.SetHandle(handle);
                opaque = GCHandle.ToIntPtr(handle);
            }

            mdkPrepareCallback callback = new()
            {
                cb = &OnPrepare,
                opaque = (void*)opaque,
            };

            try
            {
                _p->prepare(_p->@object, startPosition, callback, (MDKSeekFlag)flags);
            }
            catch
            {
                ctx?.ReleaseHandle();
                throw;
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe byte OnPrepare(long position, bool* boost, void* opaque)
    {
        if (opaque == null)
            return 1;

        CallBackOnPrepareCtx? ctx = null;
        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            ctx = handle.Target as CallBackOnPrepareCtx;
            return ctx?.Callback(position, (IntPtr)boost) == true ? (byte)1 : (byte)0;
        }
        catch
        {
            return 0;
        }
        finally
        {
            ctx?.ReleaseHandle();
        }
    }

    /// <summary>
    /// Current MediaInfo. You can call it in prepare() callback which is called when loaded or load failed.<br/>
    /// Some fields can change during playback, e.g.video frame size change(via MediaEvent), live stream duration change, realtime bitrate change.<br/>
    /// You may get an invalid value if mediaInfo() is called immediately after `set(State::Playing)` or `prepare()` because media is still opening but not loaded , i.e.mediaStatus() has no MediaStatus::Loaded flag.<br/>
    /// A live stream's duration is 0 in prepare() callback or when MediaStatus::Loaded is added, then duration increases current read duration.
    /// </summary>
    public MediaInfo? MediaInfo
    {
        get
        {
            unsafe
            {
                var info = _p->mediaInfo(_p->@object);
                if (info == null)
                    return null;

                var ret = new MediaInfo();
                NET.MediaInfo.From_c(info, ref ret);
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
            _p->setState(_p->@object, (MDK_State)value);
            return this;
        }
    }

    /// <summary>
    /// get PlaybackState
    /// </summary>
    public PlaybackState? State
    {
        get
        {
            unsafe
            {
                return (PlaybackState)_p->state(_p->@object);
            }
        }
    }

    /// <summary>
    /// a delegate for OnStateChanged
    /// </summary>
    public delegate void CallBackOnStateChanged(State a);
    private sealed class OnStateChangedCtx
    {
        public CallBackOnStateChanged? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly OnStateChangedCtx _stateCtx = new();
    private GCHandle? _stateCtxGcHandle;

    /// <summary>
    /// set a callback on state changed
    /// </summary>
    /// <param name="cb"></param>
    /// <returns></returns>
    public MDKPlayer OnStateChanged(CallBackOnStateChanged? cb)
    {
        nint opaque = 0;
        lock (_stateCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_stateCtx.Disposed, this);
            _stateCtx.Callback = cb;

            if (cb is not null)
            {
                _stateCtxGcHandle ??= GCHandle.Alloc(_stateCtx);
                opaque = GCHandle.ToIntPtr(_stateCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkStateChangedCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnStateChanged,
                    opaque = (void*)opaque,
                };
            _p->onStateChanged(_p->@object, callback);
        }
        return this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnStateChanged(MDK_State value, void* opaque)
    {
        if (opaque == null)
            return;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not OnStateChangedCtx ctx) return;

            CallBackOnStateChanged? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return;

                callback = ctx.Callback;
            }

            callback?.Invoke((State)value);
        }
        catch
        {
            // Managed exceptions must not cross the unmanaged callback boundary.
        }
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
            return _p->waitFor(_p->@object, (MDK_State)value, (int)timeout) != 0;
        }
    }

    /// <summary>
    /// get MediaStatus
    /// </summary>
    public MediaStatus MediaStatus
    {
        get { unsafe { return (MediaStatus)_p->mediaStatus(_p->@object); } }
    }

    /// <summary>
    /// a delegate for OnMediaStatus
    /// </summary>
    public delegate bool CallBackOnMediaStatus(MediaStatus old, MediaStatus @new);

    /// <summary>
    /// Add/Remove a callback or clear all callbacks for MediaStatus change.
    /// </summary>
    /// <param name="cb">the callback. return true. null to clear callbacks.</param>
    /// <param name="token">see https://github.com/wang-bin/mdk-sdk/wiki/Types#callbacktoken</param>
    /// <returns></returns>
    public MDKPlayer OnMediaStatus(CallBackOnMediaStatus? cb, IntPtr token = 0)
    {
        unsafe
        {
            lock (_statusMtx)
            {
                mdkMediaStatusCallback callback = new();
                if (cb == null)
                {
                    _p->onMediaStatus(_p->@object, callback, (CallbackToken*)(token != IntPtr.Zero ? _statusCbKey[*(CallbackToken*)token] : 0));
                    if (token != IntPtr.Zero)
                    {
                        _statusCb.Remove(*(CallbackToken*)token);
                        _statusCbKey.Remove(*(CallbackToken*)token);
                    }
                    else
                    {
                        _statusCb.Clear();
                        _statusCbKey.Clear();
                    }
                }
                else
                {
                    _statusCb[_onStatusK] = cb;
                    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
                    static byte Temp(MDK_MediaStatus old, MDK_MediaStatus value, void* opaque)
                    {
                        return (byte)(Marshal.GetDelegateForFunctionPointer<CallBackOnMediaStatus>((nint)opaque)((MediaStatus)old, (MediaStatus)value) ? 1 : 0);
                    }
                    callback = new()
                    {
                        cb = &Temp,
                        opaque = (void*)Marshal.GetFunctionPointerForDelegate(_statusCb[_onStatusK]),
                    };
                    CallbackToken t = 0;
                    _p->onMediaStatus(_p->@object, callback, &t);
                    _statusCbKey[_onStatusK] = t;
                    if (token != 0)
                    {
                        *(CallbackToken*)token = t;
                    }
                    _onStatusK++;
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
            _p->updateNativeSurface(_p->@object, (void*)surface, width, height, (MDK_SurfaceType)type);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nativeHandle"></param>
    /// <param name="type"></param>
    public void CreateSurface(IntPtr nativeHandle, SurfaceType type = SurfaceType.Auto)
    {
        unsafe
        {
            _p->createSurface(_p->@object, (void*)nativeHandle, (MDK_SurfaceType)type);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ResizeSurface(int width, int height)
    {
        unsafe
        {
            _p->resizeSurface(_p->@object, width, height);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowSurface()
    {
        unsafe
        {
            _p->showSurface(_p->@object);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public delegate string CallBackOnSnapshot(IntPtr request, double position);
    private sealed class SnapshotCallBackCtx
    {
        public required CallBackOnSnapshot Callback { get; init; }
        private GCHandle _handle;
        private int _released;

        public void SetHandle(GCHandle handle)
        {
            _handle = handle;
        }

        public void ReleaseHandle()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0 && _handle.IsAllocated)
                _handle.Free();
        }
    }

    /// <summary>
    /// take a snapshot from current renderer. The result is in bgra format, or null on failure.<br/>
    /// When `snapshot()` is called, redraw is scheduled for `vo_opaque`'s renderer, then renderer will take a snapshot in rendering thread.<br/>
    /// So for a foreign context, if renderer's surface/window/widget is invisible or minimized, snapshot may do nothing because of system or gui toolkit painting optimization.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cb"></param>
    /// <param name="voOpaque"></param>
    public void Snapshot(IntPtr request, CallBackOnSnapshot? cb, IntPtr voOpaque = 0)
    {
        unsafe
        {
            SnapshotCallBackCtx? ctx = null;
            nint opaque = 0;
            if (cb is not null)
            {
                ctx = new() { Callback = cb };
                var handle = GCHandle.Alloc(ctx);
                ctx.SetHandle(handle);
                opaque = GCHandle.ToIntPtr(handle);
            }

            mdkSnapshotCallback callback = new()
            {
                cb = &OnSnapshot,
                opaque = (void*)opaque,
            };

            try
            {
                _p->snapshot(_p->@object, (mdkSnapshotRequest*)request, callback, (void*)voOpaque);
            }
            catch
            {
                ctx?.ReleaseHandle();
                throw;
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe sbyte* OnSnapshot(mdkSnapshotRequest* request, double position, void* opaque)
    {
        if (opaque == null)
            return null;

        SnapshotCallBackCtx? ctx = null;
        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            ctx = handle.Target as SnapshotCallBackCtx;
            var ret = ctx?.Callback((IntPtr)request, position);
            return string.IsNullOrEmpty(ret) ? null : Methods.MDK_strdup(ret);
        }
        catch
        {
            return null;
        }
        finally
        {
            ctx?.ReleaseHandle();
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
    /// <para>"audio.decoder": audio decoder properties, value is "key=value" or "key1=value1:key2=value2". override "decoder" properties</para>
    /// <para>"video.decoder": video decoder properties, value is "key=value" or "key1=value1:key2=value2". override "decoder" properties</para>
    /// <para>"decoder": video and audio decoder properties, value is "key=value" or "key1=value1:key2=value2"</para>
    /// <para>"record.copyts", "recorder.copyts": "1" or "0"(default), use input packet timestamp, or correct packet timestamp to be continuous.</para>
    /// <para>"record.$opt_name": option for recorder's muxer or io, opt_name can also be an ffmpeg option, e.g. "record.avformat.$opt_name" and "record.avio.$opt_name".</para>
    /// <para>"reader.decoder.$DecoderName": $DecoderName decoder properties, value is "key=value" or "key1=value1:key2=value2". override "decoder" properties</para>
    /// <para>"reader.starts_with_key": "0" or "1"(default). if "1", video decoder starts with key-frame, and drop non-key packets before the first decode.</para>
    /// <para>"reader.pause": "0"(default) or "1". if "1", will try to pause/resume stream(rtsp) in set(State)</para>
    /// <para>"buffer" or "buffer.range": parameters setBufferRange(). value is "minMs", "minMs+maxMs", "minMs+maxMs-", "minMs-". the last '-' indicates drop mode</para>
    /// <para>"demux.buffer.ranges": default "0". set a positive integer to enable demuxer's packet cache(if protocol is listed in property "demux.buffer.protocols"), the value is cache ranges count. Cache is useful for network streams, download data only once(if a cache range is not dropped), speedup seeking. Cache ranges are increased by seeking to a uncached position, decreased by merging ranges which are overlapped and LRU algorithm.</para>
    /// <para>"demux.buffer.protocols": default is "http,https". only these protocols will enable demuxer cache.</para>
    /// <para>"demux.max_errors": continue to demux the stream if error count is less than this value. same as global option "demuxer.max_errors"</para>
    /// <para>"timeout": timeout in milliseconds. Will unload media if timeout occurs.</para>
    /// <para>"speed", "playbackRate": same as <see cref="PlaybackRate"/>.</para>
    /// <para>"loop": same as <see cref="SetLoop"/>.</para>
    /// <para>"fps", "frameRate": same as <see cref="SetFrameRate"/>.</para>
    /// <para>"volume": same as <see cref="SetVolume"/>.</para>
    /// <para>"mute": same as <see cref="SetMute"/>.</para>
    /// <para>"audio.tracks": track list "T1,T2,...", same as <c>setActiveTracks(MediaType::Audio, {T1, T2, ...})</c>.</para>
    /// <para>"video.tracks": track list "T1,T2,...", same as <c>setActiveTracks(MediaType::Video, {T1, T2, ...})</c>.</para>
    /// <para>"subtitle.tracks": track list "T1,T2,...", same as <c>setActiveTracks(MediaType::Subtitle, {T1, T2, ...})</c>.</para>
    /// <para>"avformat.input": force input format. Useful for h264 raw streams over udp.</para>
    /// <para>"avsync.audio": set "false" to sync video to a steady clock.</para>
    /// <para>"subtitle.size": non-bitmap subtitle resolution. "video"(default) uses video frame size, "fit" keeps aspect ratio scaled to renderer size, otherwise uses renderer size.</para>
    /// <para>"subtitle.ass.regions.max": int value, default is 1. split ASS rgb texture into smaller textures to reduce gpu bandwidth.</para>
    /// <para>"subtitle.ass.regions.debug": 0(default) or 1. visualize text and ass rgb texture regions.</para>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetProperty(string name, string value)
    {
        unsafe
        {
            var nameUtf8 = Marshal.StringToCoTaskMemUTF8(name);
            var valueUtf8 = Marshal.StringToCoTaskMemUTF8(value);
            _p->setProperty(_p->@object, (sbyte*)nameUtf8, (sbyte*)valueUtf8);
            Marshal.FreeCoTaskMem(nameUtf8);
            Marshal.FreeCoTaskMem(valueUtf8);
        }
    }

    /// <summary>
    /// Get additional property value.
    /// </summary>
    /// <param name="name">property name</param>
    /// <returns>property value, or empty string if not found</returns>
    public string Property(string name)
    {
        unsafe
        {
            var nameUtf8 = Marshal.StringToCoTaskMemUTF8(name);
            var ret = Marshal.PtrToStringUTF8((nint)_p->getProperty(_p->@object, (sbyte*)nameUtf8)) ?? "";
            Marshal.FreeCoTaskMem(nameUtf8);
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
    /// <param name="voOpaque"></param>
    public void SetVideoSurfaceSize(int width, int height, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setVideoSurfaceSize(_p->@object, width, height, (void*)voOpaque);
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
    /// <param name="voOpaque"></param>
    public void SetVideoViewport(float x, float y, float width, float height, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setVideoViewport(_p->@object, x, y, width, height, (void*)voOpaque);
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
    /// <param name="voOpaque"></param>
    public void SetAspectRatio(float value, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setAspectRatio(_p->@object, value, (void*)voOpaque);
        }
    }

    /// <summary>
    /// rotate around video frame center
    /// </summary>
    /// <param name="degree">0, 90, 180, 270, counterclockwise</param>
    /// <param name="voOpaque"></param>
    public void Rotate(int degree, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->rotate(_p->@object, degree, (void*)voOpaque);
        }
    }

    /// <summary>
    /// scale frame size. x, y can be &lt; 0, means scale and flip.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="voOpaque"></param>
    public void Scale(float x, float y, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->scale(_p->@object, x, y, (void*)voOpaque);
        }
    }

    /// <summary>
    /// map a point from one coordinates to another. a frame must be rendered. coordinates is normalized to [0, 1].
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="x">points to x coordinate of viewport or currently rendered video frame</param>
    /// <param name="y"></param>
    /// <param name="z">not used</param>
    /// <param name="voOpaque"></param>
    public void MapPoint(MapDirection dir, IntPtr x, IntPtr y, IntPtr z = 0, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->mapPoint(_p->@object, (MDK_MapDirection)dir, (float*)x, (float*)y, (float*)z, (void*)voOpaque);
        }
    }

    /// <summary>
    /// Can be called on any thread
    /// </summary>
    /// <param name="videoRoi">array of 2d point (x, y) in video frame. coordinate: top-left = (0, 0), bottom-right=(1, 1). set null to disable mapping</param>
    /// <param name="viewRoi">array of 2d point (x, y) in video renderer. coordinate: top-left = (0, 0), bottom-right=(1, 1). null is the whole renderer.</param>
    /// <param name="count">point count. only support 2. set 0 to disable mapping</param>
    /// <param name="voOpaque"></param>
    public void SetPointMap(IntPtr videoRoi, IntPtr viewRoi, int count = 2, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setPointMap(_p->@object, (float*)videoRoi, (float*)viewRoi, count, (void*)voOpaque);
        }
    }

    /// <summary>
    /// set render api for a vo, useful for non-opengl(no way to get current context)
    /// </summary>
    /// <param name="api">To release gfx resources, set null api in rendering thread/context(required by vulkan)</param>
    /// <param name="voOpaque"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public MDKPlayer SetRenderAPI(IntPtr api, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setRenderAPI(_p->@object, (mdkRenderAPI*)api, (void*)voOpaque);
        }
        return this;
    }

    /// <summary>
    /// Set render API using a managed RenderAPI structure. This is a generic template method that accepts various RenderAPI structures
    /// that implement IRenderAPI interface, calls their Pin() method, and uses AddrOfPinnedObject to get the pointer for setting the render API.
    /// </summary>
    /// <typeparam name="T">The type of RenderAPI structure that implements IRenderAPI interface</typeparam>
    /// <param name="renderApi">The RenderAPI structure instance</param>
    /// <param name="voOpaque">Video output opaque pointer</param>
    /// <returns>This MDKPlayer instance for method chaining</returns>
    // ReSharper disable once InconsistentNaming
    public MDKPlayer SetRenderAPI<T>(T renderApi, IntPtr voOpaque = 0) where T : struct, IRenderAPI
    {
        using var pinnedHandle = renderApi.Pin();
        var apiPtr = pinnedHandle.AddrOfPinnedObject();
        return SetRenderAPI(apiPtr, voOpaque);
    }

    /// <summary>
    /// get render api. For offscreen rendering, may only api type be valid in setRenderAPI(), and other members are filled internally, and used by user after renderVideo()
    /// </summary>
    /// <param name="voOpaque"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public IntPtr RenderAPI(IntPtr voOpaque = 0)
    {
        unsafe
        {
            return (IntPtr)_p->renderAPI(_p->@object, (void*)voOpaque);
        }
    }

    /// <summary>
    /// Render the next or current(redraw) frame. Foreign render context only (i.e. not created by createSurface()/updateNativeSurface()).<br/>
    /// If called before the first frame is decoded, will clear render target to ensure render target has no garbage data.<br/>
    /// OpenGL: Can be called in multiple foreign contexts for the same vo_opaque.
    /// </summary>
    /// <param name="voOpaque"></param>
    /// <returns>timestamp of rendered frame, or &lt; 0 if no frame is rendered. precision is microsecond</returns>
    public double RenderVideo(IntPtr voOpaque = 0)
    {
        unsafe
        {
            return _p->renderVideo(_p->@object, (void*)voOpaque);
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
            _p->enqueueVideo(_p->@object, (mdkVideoFrameAPI*)frame, (void*)opaque);
        }
    }

    /// <summary>
    /// r, g, b, a range is [0, 1]. default is 0. if out of range, background color will not be filled
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    /// <param name="voOpaque"></param>
    public void SetBackgroundColor(float r, float g, float b, float a, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setBackgroundColor(_p->@object, r, g, b, a, (void*)voOpaque);
        }
    }

    /// <summary>
    /// Set video effect. See https://github.com/wang-bin/mdk-sdk/wiki/Types#enum-videoeffect
    /// </summary>
    /// <param name="effect">video effect type</param>
    /// <param name="value">effect dependent float value or float array</param>
    /// <param name="voOpaque"></param>
    public void Set(VideoEffect effect, IntPtr value, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setVideoEffect(_p->@object, (MDK_VideoEffect)effect, (float*)value, (void*)voOpaque);
        }
    }

    /// <summary>
    /// Set output color space. To render multiple HDR and SDR videos at the same time, choose ColorSpaceBT2100_PQ and ensure your gui toolkit is running in hdr10 colorspace.
    /// </summary>
    /// <param name="value">
    /// <para>invalid (ColorSpaceUnknown): renderer will try to use the value of decoded frame, and will send hdr10 metadata when possible. i.e. hdr videos will enable hdr display. Currently only supported by metal, and `MetalRenderAPI.layer` must be a `CAMetalLayer` ([example](https://github.com/wang-bin/swift-mdk/blob/master/Player.swift#L184))</para>
    /// <para>hdr colorspace(ColorSpaceBT2100_PQ): no hdr metadata will be sent to the display, sdr will map to hdr.Can be used by the gui toolkits which support hdr swapchain but no api to change swapchain colorspace and format on the fly, see[Qt example] (https://github.com/wang-bin/mdk-examples/blob/master/Qt/qmlrhi/VideoTextureNodePub.cpp#L83)</para>
    /// <para>sdr color space(ColorSpaceBT709) : the default. HDR videos will tone map to SDR.</para>
    /// </param>
    /// <param name="voOpaque"></param>
    public void Set(ColorSpace value, IntPtr voOpaque = 0)
    {
        unsafe
        {
            _p->setColorSpace(_p->@object, (MDK_ColorSpace)value, (void*)voOpaque);
        }
    }

    /// <summary>
    /// a delegate for SetRenderCallback
    /// </summary>
    public delegate void CallBackOnRender(IntPtr voOpaque);
    private sealed class RenderCallbackCtx
    {
        public CallBackOnRender? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly RenderCallbackCtx _renderCtx = new();
    private GCHandle? _renderCtxGcHandle;
    /// <summary>
    /// set a callback which is invoked when the vo coresponding to vo_opaque needs to update/draw content, e.g. when a new frame is received in the renderer.<br/>
    /// Also invoked in setVideoSurfaceSize(), setVideoViewport(), setAspectRatio() and rotate(), take care of dead lock in callback and above functions.<br/>
    /// with vo_opaque, user can know which vo/renderer is rendering, useful for multiple renderers<br/>
    /// There may be no frames or playback not even started, but renderer update is required internally<br/>
    /// DO NOT call renderVideo() in the callback, otherwise will results in dead lock
    /// </summary>
    /// <param name="cb"></param>
    public void SetRenderCallback(CallBackOnRender? cb)
    {
        nint opaque = 0;
        lock (_renderCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_renderCtx.Disposed, this);
            _renderCtx.Callback = cb;

            if (cb is not null)
            {
                _renderCtxGcHandle ??= GCHandle.Alloc(_renderCtx);
                opaque = GCHandle.ToIntPtr(_renderCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkRenderCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnRender,
                    opaque = (void*)opaque,
                };
            _p->setRenderCallback(_p->@object, callback);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnRender(void* voOpaque, void* opaque)
    {
        if (opaque == null)
            return;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not RenderCallbackCtx ctx) return;

            CallBackOnRender? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return;

                callback = ctx.Callback;
            }

            callback?.Invoke((IntPtr)voOpaque);
        }
        catch
        {
            // Managed exceptions must not cross the unmanaged callback boundary.
        }
    }

    /// <summary>
    /// Current playback time in milliseconds. Relative to media's first timestamp, which usually is 0.<br/>
    /// If has active video tracks, it's currently presented video frame time. otherwise, it's audio time.
    /// </summary>
    public long Position
    {
        get { unsafe { return _p->position(_p->@object); } }
    }

    /// <summary>
    /// a delegate for Seek
    /// </summary>
    public delegate void CallBackOnSeek(long ms);
    private sealed class SeekCallbackCtx
    {
        public required CallBackOnSeek Callback { get; init; }
        private GCHandle _handle;
        private int _released;

        public void SetHandle(GCHandle handle)
        {
            _handle = handle;
        }

        public void ReleaseHandle()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0 && _handle.IsAllocated)
                _handle.Free();
        }
    }

    /// <summary>
    /// Seek to a given position.
    /// </summary>
    /// <param name="position">seek target. if flags has SeekFlag::Frame, pos is frame count, otherwise it's milliseconds.<br/>
    /// If pos > media time range, e.g.INT64_MAX, will seek to the last frame of media for SeekFlag::AnyFrame, and the last key frame of media for SeekFlag::Fast.<br/>
    /// If pos > media time range with SeekFlag::AnyFrame, playback will stop unless setProperty("continue_at_end", "1") was called<br/>
    /// FIXME: a/v sync broken if SeekFlag::Frame|SeekFlag::FromNow.</param>
    /// <param name="flags"></param>
    /// <param name="cb">if succeeded, callback is called when stream seek finished and after the 1st frame decoded or decode error(e.g. video tracks disabled), ret(&gt;=0) is the timestamp of the 1st frame(video if exists) after seek.<br/>
    /// If error(io, demux, not decode) occured(ret &lt; 0, usually -1) or skipped because of unfinished previous seek(ret == -2), out of range(-4) or media unloaded(-3).</param>
    public bool Seek(long position, SeekFlag flags, CallBackOnSeek? cb = null)
    {
        unsafe
        {
            SeekCallbackCtx? ctx = null;
            nint opaque = 0;
            if (cb is not null)
            {
                ctx = new() { Callback = cb };
                var handle = GCHandle.Alloc(ctx);
                ctx.SetHandle(handle);
                opaque = GCHandle.ToIntPtr(handle);
            }

            mdkSeekCallback callback = new()
            {
                cb = &OnSeek,
                opaque = (void*)opaque,
            };

            bool accepted;
            try
            {
                accepted = _p->seekWithFlags(_p->@object, position, (MDKSeekFlag)flags, callback) != 0;
            }
            catch
            {
                ctx?.ReleaseHandle();
                throw;
            }

            if (!accepted)
                ctx?.ReleaseHandle();

            return accepted;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnSeek(long ms, void* opaque)
    {
        if (opaque == null)
            return;

        SeekCallbackCtx? ctx = null;
        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            ctx = handle.Target as SeekCallbackCtx;
            ctx?.Callback(ms);
        }
        catch
        {
            // Managed exceptions must not cross the unmanaged callback boundary.
        }
        finally
        {
            ctx?.ReleaseHandle();
        }
    }

    /// <summary>
    /// Seek ms from start
    /// </summary>
    /// <param name="position">milliseconds seek target</param>
    /// <param name="cb"></param>
    public bool Seek(long position, CallBackOnSeek? cb = null)
    {
        return Seek(position, SeekFlag.Default, cb);
    }

    /// <summary>
    /// Get or set playback speed. FFmpeg atempo filter is required. Value >= 0.5, 1.0 is original speed.
    /// </summary>
    public float PlaybackRate
    {
        get { unsafe { return _p->playbackRate(_p->@object); } }
        set { unsafe { _p->setPlaybackRate(_p->@object, value); } }
    }

    /// <summary>
    ///  time(position) is relative to media start time.<br/>
    ///  Available if demuxer cache is enabled by property "demux.buffer.ranges" and "demux.buffer.protocols"
    /// </summary>
    public List<TimeRange> BufferedRanges
    {
        get
        {
            unsafe
            {
                var block = stackalloc TimeRange[16];
                var count = _p->bufferedTimeRanges(_p->@object, (long*)block, 16 * sizeof(TimeRange));
                if (count > 16)
                {
                    block = (TimeRange*)Marshal.AllocHGlobal(Marshal.SizeOf<TimeRange>() * count);
                    count = _p->bufferedTimeRanges(_p->@object, (long*)block, count * sizeof(TimeRange));
                    List<TimeRange> ret = [];
                    for (int i = 0; i < count; i++)
                    {
                        ret.Add(block[i]);
                    }
                    Marshal.FreeHGlobal((IntPtr)block);
                    return ret;
                }
                else
                {
                    List<TimeRange> ret = [];
                    for (int i = 0; i < 16; i++)
                    {
                        ret.Add(block[i]);
                    }
                    return ret;
                }
            }
        }
    }

    /// <summary>
    /// Get buffered undecoded data duration and size.
    /// </summary>
    /// <param name="bytes">output: buffered bytes</param>
    /// <returns>buffered data(packets) duration in milliseconds</returns>
    public long Buffed(IntPtr bytes = 0)
    {
        unsafe
        {
            return _p->buffered(_p->@object, (long*)bytes);
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
            _p->setBufferRange(_p->@object, minMs, maxMs, (byte)(drop ? 1 : 0));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public delegate void CallBackOnSwitchBitrate(bool a);
    private sealed class SwitchCallbackCtx
    {
        public CallBackOnSwitchBitrate? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly SwitchCallbackCtx _switchCtx = new();
    private GCHandle? _switchCtxGcHandle;

    /// <summary>
    /// A new media will be played later
    /// </summary>
    /// <param name="url"></param>
    /// <param name="delay">switch after at least delay ms. TODO: determined by buffered time, e.g. from high bit rate without enough buffered samples to low bit rate</param>
    /// <param name="cb">(true/false) called when finished/failed</param>
    public void SwitchBitrate(string url, long delay = -1, CallBackOnSwitchBitrate? cb = null)
    {
        nint opaque = 0;
        lock (_switchCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_switchCtx.Disposed, this);
            _switchCtx.Callback = cb;

            if (cb is not null)
            {
                _switchCtxGcHandle ??= GCHandle.Alloc(_switchCtx);
                opaque = GCHandle.ToIntPtr(_switchCtxGcHandle.Value);
            }
        }

        unsafe
        {
            SwitchBitrateCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnSwitchBitrate,
                    opaque = (void*)opaque,
                };
            var urlUtf8 = Marshal.StringToCoTaskMemUTF8(url);
            try
            {
                _p->switchBitrate(_p->@object, (sbyte*)urlUtf8, delay, callback);
            }
            finally
            {
                Marshal.FreeCoTaskMem(urlUtf8);
            }
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
        nint opaque = 0;
        lock (_switchCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_switchCtx.Disposed, this);
            _switchCtx.Callback = cb;

            if (cb is not null)
            {
                _switchCtxGcHandle ??= GCHandle.Alloc(_switchCtx);
                opaque = GCHandle.ToIntPtr(_switchCtxGcHandle.Value);
            }
        }

        unsafe
        {
            SwitchBitrateCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnSwitchBitrate,
                    opaque = (void*)opaque,
                };
            var urlUtf8 = Marshal.StringToCoTaskMemUTF8(url);
            try
            {
                var ret = _p->switchBitrateSingleConnection(_p->@object, (sbyte*)urlUtf8, callback);
                return ret != 0;
            }
            finally
            {
                Marshal.FreeCoTaskMem(urlUtf8);
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnSwitchBitrate(byte value, void* opaque)
    {
        if (opaque == null)
            return;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not SwitchCallbackCtx ctx) return;

            CallBackOnSwitchBitrate? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return;

                callback = ctx.Callback;
            }

            callback?.Invoke(value != 0);
        }
        catch
        {
            // Managed exceptions must not cross the unmanaged callback boundary.
        }
    }

    /// <summary>
    /// a delegate for OnEvent
    /// </summary>
    public delegate bool CallBackOnEvent(MediaEvent a);
    private class EventCallbackCtx
    {
        public CallBackOnEvent? Callback { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly EventCallbackCtx _eventCtx = new();
    // private GCHandle? _eventCtxGcHandle;

    /// <summary>
    /// Add/Remove a CallBackOnEvent listener, or remove listeners.
    /// </summary>
    /// <param name="cb">the callback. return true if event is processed and should stop dispatching.</param>
    /// <param name="token">see https://github.com/wang-bin/mdk-sdk/wiki/Types#callbacktoken</param>
    /// <returns></returns>
    public MDKPlayer OnEvent(CallBackOnEvent? cb, IntPtr token = 0)
    {

        unsafe
        {
            lock (_eventCtx.Lock)
            {
                mdkMediaEventCallback callback = new();
                if (cb == null)
                {
                    _p->onEvent(_p->@object, callback,
                        (CallbackToken*)(token != IntPtr.Zero ? _eventCbKey[*(CallbackToken*)token] : 0));
                    if (token != IntPtr.Zero)
                    {
                        _eventCb.Remove(*(CallbackToken*)token);
                        _eventCbKey.Remove(*(CallbackToken*)token);
                    }
                    else
                    {
                        _eventCb.Clear();
                        _eventCbKey.Clear();
                    }
                }
                else
                {
                    _eventCb[_onEventK] = cb;

                    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
                    static byte Temp(mdkMediaEvent* me, void* opaque)
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
                        cb = &Temp,
                        opaque = (void*)Marshal.GetFunctionPointerForDelegate(_eventCb[_onEventK]),
                    };
                    CallbackToken t = 0;
                    _p->onEvent(_p->@object, callback, &t);
                    _eventCbKey[_onEventK] = t;
                    if (token != IntPtr.Zero)
                    {
                        *(CallbackToken*)token = t;
                    }

                    _onEventK++;
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
            var urlUtf8 = Marshal.StringToCoTaskMemUTF8(url);
            var formatUtf8 = Marshal.StringToCoTaskMemUTF8(format);
            _p->record(_p->@object, (sbyte*)urlUtf8, (sbyte*)formatUtf8);
            Marshal.FreeCoTaskMem(urlUtf8);
            Marshal.FreeCoTaskMem(formatUtf8);
        }
    }

    /// <summary>
    /// Set A-B loop repeat count.
    /// </summary>
    /// <param name="count">repeat count. 0 to disable looping and stop when out of range(B)</param>
    public void SetLoop(int count)
    {
        unsafe { _p->setLoop(_p->@object, count); }
    }

    /// <summary>
    /// a delegate for OnLoop
    /// </summary>
    public delegate void CallBackOnLoop(int count);
    /// <summary>
    /// add/remove a callback which will be invoked right before a new A-B loop
    /// </summary>
    /// <param name="cb">callback with current loop count elapsed</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public MDKPlayer OnLoop(CallBackOnLoop? cb, IntPtr token = 0)
    {
        unsafe
        {
            mdkLoopCallback callback = new();
            lock (_loopMtx)
            {
                if (cb == null)
                {
                    _p->onLoop(_p->@object, callback, (CallbackToken*)(token != IntPtr.Zero ? _loopCbKey[*(CallbackToken*)token] : 0));
                    if (token != IntPtr.Zero)
                    {
                        _loopCb.Remove(*(CallbackToken*)token);
                        _loopCbKey.Remove(*(CallbackToken*)token);
                    }
                    else
                    {
                        _loopCb.Clear();
                        _loopCbKey.Clear();
                    }
                }
                else
                {
                    _loopCb[_onLoopK] = cb;
                    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
                    static void Temp(int count, void* opaque)
                    {
                        Marshal.GetDelegateForFunctionPointer<CallBackOnLoop>((nint)opaque)(count);
                    }
                    callback = new()
                    {
                        cb = &Temp,
                        opaque = (void*)Marshal.GetFunctionPointerForDelegate(_loopCb[_onLoopK]),
                    };
                    CallbackToken t = 0;
                    _p->onLoop(_p->@object, callback, &t);
                    _loopCbKey[_onLoopK] = t;
                    if (token != IntPtr.Zero)
                    {
                        *(CallbackToken*)token = t;
                    }
                    _onLoopK++;
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
            _p->setRange(_p->@object, a, b);
        }
    }

    /// <summary>
    /// a delegate for OnSync
    /// </summary>
    public delegate double CallBackOnSync();
    private sealed class SyncCallbackCtx
    {
        public CallBackOnSync? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly SyncCallbackCtx _syncCtx = new();
    private GCHandle? _syncCtxGcHandle;
    /// <summary>
    /// Set custom sync callback as clock.
    /// </summary>
    /// <param name="cb">a callback invoked when about to render a frame. return expected current playback position(seconds), e.g. DBL_MAX(TimestampEOS) indicates render video frame ASAP.
    /// sync callback clock should handle pause, resume, seek and seek finish events</param>
    /// <param name="minInterval"></param>
    /// <returns></returns>
    public MDKPlayer OnSync(CallBackOnSync? cb, int minInterval = 10)
    {
        nint opaque = 0;
        lock (_syncCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_syncCtx.Disposed, this);
            _syncCtx.Callback = cb;

            if (cb is not null)
            {
                _syncCtxGcHandle ??= GCHandle.Alloc(_syncCtx);
                opaque = GCHandle.ToIntPtr(_syncCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkSyncCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnSync,
                    opaque = (void*)opaque,
                };
            _p->onSync(_p->@object, callback, minInterval);
        }
        return this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe double OnSync(void* opaque)
    {
        if (opaque == null)
            return 0;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not SyncCallbackCtx ctx) return 0;

            CallBackOnSync? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return 0;

                callback = ctx.Callback;
            }

            return callback?.Invoke() ?? 0;
        }
        catch
        {
            return 0;
        }
    }


    /// <summary>
    /// Get subtitle text. Only for text based subtitle, e.g. ass, srt.
    /// </summary>
    /// <param name="time">time in second. if &lt; 0, get currently rendered text</param>
    /// <param name="style">ass style option. 0: no style, plain text. 1: ass style. 2: full ass style</param>
    /// <returns></returns>
    public string SubtitleText(double time = -1, int style = 0)
    {
        unsafe
        {
            var result = "";
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void Temp(sbyte* text, void* opaque)
            {
                if (text == null || opaque == null)
                    return;
                var ret = GCHandle.FromIntPtr((IntPtr)opaque);
                // ReSharper disable once NotAccessedVariable
                if (ret.Target is string s)
                    // ReSharper disable once RedundantAssignment
                    s = Marshal.PtrToStringUTF8((IntPtr)text) ?? "";
            }
            mdkSubtitleCallback cb = new();
            var retHandle = GCHandle.Alloc(result);
            cb.cb = &Temp;
            cb.opaque = (void*)GCHandle.ToIntPtr(retHandle);
            _p->subtitleText(_p->@object, time, style, cb);
            retHandle.Free();
            return result;
        }
    }

    /// <summary>
    /// a delegate for OnSubtitleText
    /// </summary>
    public delegate void CallBackOnSubtitleText(double start, double end, List<string> text);
    private sealed class SubtitleTextCallbackCtx
    {
        public CallBackOnSubtitleText? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly SubtitleTextCallbackCtx _subtitleTextCtx = new();
    private GCHandle? _subtitleTextCtxGcHandle;

    /// <summary>
    /// Subtitle text callback invoked when new text is ready to render.
    /// </summary>
    /// <param name="cb">callback with start time, end time, and subtitle text lines</param>
    /// <param name="plainText">true to get plain text, false for styled text</param>
    /// <returns></returns>
    public MDKPlayer OnSubtitleText(CallBackOnSubtitleText? cb, bool plainText = true)
    {
        nint opaque = 0;
        lock (_subtitleTextCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_subtitleTextCtx.Disposed, this);
            _subtitleTextCtx.Callback = cb;

            if (cb is not null)
            {
                _subtitleTextCtxGcHandle ??= GCHandle.Alloc(_subtitleTextCtx);
                opaque = GCHandle.ToIntPtr(_subtitleTextCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkSubtitleCallback callback = cb is null
                ? default
                : new()
                {
                    cb2 = &OnSubtitleText,
                    opaque = (void*)opaque,
                };
            _p->onSubtitleText(_p->@object, callback, (byte)(plainText ? 1 : 0), null);
        }
        return this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnSubtitleText(double start, double end, sbyte* texts, int textCount, void* opaque)
    {
        if (opaque == null)
            return;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not SubtitleTextCallbackCtx ctx) return;

            CallBackOnSubtitleText? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return;

                callback = ctx.Callback;
            }

            if (callback is null)
                return;

            List<string> list = new(textCount);
            var ptrs = (sbyte**)texts;
            for (var i = 0; i < textCount; i++)
            {
                var strPtr = (IntPtr)ptrs[i];
                var str = Marshal.PtrToStringUTF8(strPtr) ?? "";
                list.Add(str);
            }

            callback.Invoke(start, end, list);
        }
        catch
        {
            // Managed exceptions must not cross the unmanaged callback boundary.
        }
    }

    /// <summary>
    /// a delegate for OnFrame
    /// </summary>
    public delegate int CallBackOnFrame4Video(VideoFrame frame, int track);
    private sealed class Frame4VideoCallbackCtx
    {
        public CallBackOnFrame4Video? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly Frame4VideoCallbackCtx _videoCtx = new();
    private GCHandle? _videoCtxGcHandle;

    /// <summary>
    /// A callback to be invoked before delivering a frame to renderers. Frame can be VideoFrame and AudioFrame(NOT IMPLEMENTED).<br/>
    /// The callback can be used as a filter.<br/>
    /// TODO: frames not in host memory<br/>
    /// For most filters, 1 input frame generates 1 output frame, then return 0.<br/>
    /// WARNING: calling set(State::Stopped) in the callback may result in a dead lock.<br/>
    /// If a video decoder has property "sei=1", then <c>frame.metadata(".sei.$type")</c> returns rbsp data of SEI type if exists.
    /// </summary>
    /// <param name="cb">callback to be invoked. returns pending number of frames. callback parameter is input and output frame. if input frame is an invalid frame, output a pending frame.</param>
    /// <returns></returns>
    public MDKPlayer OnFrame(CallBackOnFrame4Video? cb)
    {
        nint opaque = 0;
        lock (_videoCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_videoCtx.Disposed, this);
            _videoCtx.Callback = cb;

            if (cb is not null)
            {
                _videoCtxGcHandle ??= GCHandle.Alloc(_videoCtx);
                opaque = GCHandle.ToIntPtr(_videoCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkVideoCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnVideoFrame,
                    opaque = (void*)opaque,
                };
            _p->onVideo(_p->@object, callback);
        }
        return this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe int OnVideoFrame(mdkVideoFrameAPI** pFrame, int track, void* opaque)
    {
        if (opaque == null || pFrame == null)
            return 0;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not Frame4VideoCallbackCtx ctx) return 0;

            CallBackOnFrame4Video? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return 0;

                callback = ctx.Callback;
            }

            if (callback is null)
                return 0;

            using VideoFrame frame = new(null);
            frame.Attach(*pFrame);
            try
            {
                return callback.Invoke(frame, track);
            }
            finally
            {
                *pFrame = frame.Detach();
            }
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// a delegate for OnFrame
    /// </summary>
    public delegate int CallBackOnFrame4Audio(AudioFrame frame, int track);
    private sealed class Frame4AudioCallbackCtx
    {
        public CallBackOnFrame4Audio? Callback { get; set; }
        public bool Disposed { get; set; }
        public Lock Lock { get; } = new();
    }
    private readonly Frame4AudioCallbackCtx _audioCtx = new();
    private GCHandle? _audioCtxGcHandle;

    /// <summary>
    /// A callback to be invoked before delivering a frame to renderers. Frame can be VideoFrame and AudioFrame.<br/>
    /// The callback can be used as a filter.<br/>
    /// TODO: frames not in host memory<br/>
    /// For most filters, 1 input frame generates 1 output frame, then return 0.
    /// </summary>
    /// <param name="cb">callback to be invoked. returns pending number of frames. callback parameter is input and output frame. if input frame is an invalid frame, output a pending frame.</param>
    /// <returns></returns>
    public MDKPlayer OnFrame(CallBackOnFrame4Audio? cb)
    {
        nint opaque = 0;
        lock (_audioCtx.Lock)
        {
            ObjectDisposedException.ThrowIf(_audioCtx.Disposed, this);
            _audioCtx.Callback = cb;

            if (cb is not null)
            {
                _audioCtxGcHandle ??= GCHandle.Alloc(_audioCtx);
                opaque = GCHandle.ToIntPtr(_audioCtxGcHandle.Value);
            }
        }

        unsafe
        {
            mdkAudioCallback callback = cb is null
                ? default
                : new()
                {
                    cb = &OnAudioFrame,
                    opaque = (void*)opaque,
                };
            _p->onAudio(_p->@object, callback);
        }
        return this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe int OnAudioFrame(mdkAudioFrameAPI** pFrame, int track, void* opaque)
    {
        if (opaque == null || pFrame == null)
            return 0;

        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            if (handle.Target is not Frame4AudioCallbackCtx ctx) return 0;

            CallBackOnFrame4Audio? callback;
            lock (ctx.Lock)
            {
                if (ctx.Disposed)
                    return 0;

                callback = ctx.Callback;
            }

            if (callback is null)
                return 0;

            using AudioFrame frame = new(null);
            frame.Attach(*pFrame);
            try
            {
                return callback.Invoke(frame, track);
            }
            finally
            {
                *pFrame = frame.Detach();
            }
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        unsafe
        {
            if (!_owner || _p == null) return;
            _p->currentMediaChanged(_p->@object, default);
            _p->setTimeout(_p->@object, 0, default);
            _p->onStateChanged(_p->@object, default);
            _p->setRenderCallback(_p->@object, default);
            _p->onSync(_p->@object, default, 0);
            _p->onSubtitleText(_p->@object, default, 1, null);
            _p->onVideo(_p->@object, default);
            _p->onAudio(_p->@object, default);

            lock (_currentCtx.Lock)
            {
                _currentCtx.Disposed = true;
                _currentCtx.Callback = null;
            }

            lock (_timeoutCtx.Lock)
            {
                _timeoutCtx.Disposed = true;
                _timeoutCtx.Callback = null;
            }

            lock (_stateCtx.Lock)
            {
                _stateCtx.Disposed = true;
                _stateCtx.Callback = null;
            }

            lock (_renderCtx.Lock)
            {
                _renderCtx.Disposed = true;
                _renderCtx.Callback = null;
            }

            lock (_syncCtx.Lock)
            {
                _syncCtx.Disposed = true;
                _syncCtx.Callback = null;
            }

            lock (_subtitleTextCtx.Lock)
            {
                _subtitleTextCtx.Disposed = true;
                _subtitleTextCtx.Callback = null;
            }

            lock (_videoCtx.Lock)
            {
                _videoCtx.Disposed = true;
                _videoCtx.Callback = null;
            }

            lock (_audioCtx.Lock)
            {
                _audioCtx.Disposed = true;
                _audioCtx.Callback = null;
            }

            lock (_switchCtx.Lock)
            {
                _switchCtx.Disposed = true;
                _switchCtx.Callback = null;
            }

            fixed (mdkPlayerAPI** pp = &_p)
            {
                Methods.mdkPlayerAPI_reset(pp, (byte)(_owner ? 1 : 0));
            }

            _currentCtxGcHandle?.Free();
            _currentCtxGcHandle = null;
            _timeoutCtxGcHandle?.Free();
            _timeoutCtxGcHandle = null;
            _stateCtxGcHandle?.Free();
            _stateCtxGcHandle = null;
            _renderCtxGcHandle?.Free();
            _renderCtxGcHandle = null;
            _audioCtxGcHandle?.Free();
            _audioCtxGcHandle = null;
            _videoCtxGcHandle?.Free();
            _videoCtxGcHandle = null;
            _syncCtxGcHandle?.Free();
            _syncCtxGcHandle = null;
            _subtitleTextCtxGcHandle?.Free();
            _subtitleTextCtxGcHandle = null;
            _switchCtxGcHandle?.Free();
            _switchCtxGcHandle = null;
        }
    }
}
