using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;

namespace MDK.SDK.NET;

/// <summary>
/// The State enum
/// <para>Current playback state. Set/Get by user</para>
/// </summary>
public enum State : sbyte
{
    NotRunning,
    Stopped = NotRunning,
    Running,
    Playing = Running,
    Paused
}

public delegate bool PrepareCallback(long position, ref bool boost);

[Flags]
public enum SeekFlag
{
    /// <summary>
    /// relative to time 0. TODO: remove from api
    /// </summary>
    From0 = 1,
    /// <summary>
    /// relative to media start position
    /// </summary>
    FromStart = 1 << 1,
    /// <summary>
    /// relative to current position, the seek position can be negative
    /// </summary>
    FromNow = 1 << 2,
    /// <summary>
    /// Seek by frame. Seek target is frame count instead of milliseconds. Currently only FromNow|Frame is supported. BUG: avsync
    /// </summary>
    Frame = 1 << 6,
    /// <summary>
    /// fast key-frame seek, forward if Backward is not set. It's accurate seek without this flag. Accurate seek is slow and implies backward seek internally.
    /// </summary>
    KeyFrame = 1 << 8,
    Fast = KeyFrame,
    AnyFrame = 1 << 9,
    /// <summary>
    /// try to seek in memory cache first. useful for speeding up network stream seeking.  Target position must be in range (position(), position() + Player.buffered()]
    /// </summary>
    InCache = 1 << 10,
    Default = KeyFrame | FromStart | InCache
}

/// <summary>
/// The MediaStatus enum
/// <para>Defines the io status of a media stream,<br/>
/// Use flags_added/removed() to check the change, for example buffering after seek is Loaded|Prepared|Buffering, and changes to Loaded|Prepared|Buffered when seek completed</para>
/// </summary>
public enum MediaStatus
{
    /// <summary>
    /// initial status, not invalid. // what if set an empty url and closed?
    /// </summary>
    NoMedia = 0,
    /// <summary>
    /// unloaded // (TODO: or when a source(url) is set?)
    /// </summary>
    Unloaded = 1,
    /// <summary>
    /// opening and parsing the media
    /// </summary>
    Loading = 1 << 1,
    /// <summary>
    /// media is loaded and parsed. player is stopped state. mediaInfo() is available now
    /// </summary>
    Loaded = 1 << 2,
    /// <summary>
    /// all tracks are buffered and ready to decode frames. tracks failed to open decoder are ignored
    /// </summary>
    Prepared = 1 << 8,
    /// <summary>
    /// insufficient buffering or other interruptions (timeout, user interrupt)
    /// </summary>
    Stalled = 1 << 3,
    /// <summary>
    /// when buffering starts
    /// </summary>
    Buffering = 1 << 4,
    /// <summary>
    /// when buffering ends
    /// </summary>
    Buffered = 1 << 5,
    /// <summary>
    /// reached the end of the current media, no more data to read
    /// </summary>
    End = 1 << 6,
    Seeking = 1 << 7,
    /// <summary>
    /// failed to load media because of unsupported format or invalid media source
    /// </summary>
    Invalid = 1 << 31,
}

public enum PlaybackState : sbyte
{
    NotRunning,
    Stopped = NotRunning,
    Running,
    Playing = Running,
    Paused
}

public enum SurfaceType
{
    Auto, // platform default type
    X11,
    GBM,
    Wayland,
};

public struct SnapshotRequest
{
    /// <summary>
    /// rgba or bgra data. Created internally or provided by user.<br/>
    /// <para>
    /// If data is provided by user, stride, height and width MUST be also set, and data MUST be valid until snapshot callback is finished.
    /// </para>
    /// </summary>
    public IntPtr data = 0;
    /// <summary>
    /// result width of snapshot image set by user, or the same as current frame width if 0. no renderer transform.<br/>
    /// if both requested width and height are &lt; 0, then result image is scaled image of current frame with ratio=width/height. no renderer transform.<br/>
    /// if only one of width and height &lt; 0, then the result size is video renderer viewport size, and all transforms will be applied.<br/>
    /// if both width and height == 0, then result size is region of interest size of video frame set by setPointMap(), or video frame size<br/>
    /// </summary>
    public int width = 0;
    public int height = 0;
    public int stride = 0;
    /// <summary>
    /// not supported yet
    /// </summary>
    public bool subtitle = false;

    public SnapshotRequest()
    {
    }
}

public enum MapDirection
{
    /// <summary>
    /// left-hand
    /// </summary>
    FrameToViewport,
    /// <summary>
    /// left-hand
    /// </summary>
    ViewportToFrame,
};

/// <summary>
/// per video renderer effect. set via Player.
/// Only one(the last call) of ScaleChannels or ShiftChannels will be applied
/// </summary>
public enum VideoEffect
{
    /// <summary>
    /// [-1.0f, 1.0f], default 0
    /// </summary>
    Brightness,
    /// <summary>
    /// [-1.0f, 1.0f], default 0
    /// </summary>
    Contrast,
    /// <summary>
    /// [-1.0f, 1.0f], default 0
    /// </summary>
    Hue,
    /// <summary>
    /// [-1.0f, 1.0f], default 0
    /// </summary>
    Saturation,
    /// <summary>
    /// {Sr, Sg, Sb}, Sx: [0, 1.0f]. no scale: {1.0f, 1.0f, 1.0f}
    /// </summary>
    ScaleChannels,
    /// <summary>
    /// {Sr, Sg, Sb}, Sx: [-1.0f, 1.0f]. no shift: {.0f, .0f, .0f}
    /// </summary>
    ShiftChannels,
};

public enum ColorSpace
{
    ColorSpaceUnknown,
    ColorSpaceBT709,
    ColorSpaceBT2100_PQ,
    /// <summary>
    /// scRGB, linear sRGB in extended component range. Scene-referred white level, D65 is 80nit. Used on windows
    /// </summary>
    ColorSpaceSCRGB,
    ColorSpaceExtendedLinearDisplayP3,
    /// <summary>
    /// sRGB in extended component range, sRGB transfer function. Available for macOS displays
    /// </summary>
    ColorSpaceExtendedSRGB,
    /// <summary>
    /// linear sRGB in extended component range. Display-referred white level
    /// </summary>
    ColorSpaceExtendedLinearSRGB,
    ColorSpaceBT2100_HLG,
};

public enum PixelFormat
{
    Unknown = 0,
    YUV420P,
    NV12,
    YUV422P,
    YUV444P,
    P010LE,
    P016LE,
    YUV420P10LE,
    UYVY422,
    RGB24,
    /// <summary>
    /// same as QImage::Format_RGBA8888
    /// </summary>
    RGBA,
    /// <summary>
    /// same as QImage::Format_RGBX8888
    /// </summary>
    RGBX,
    /// <summary>
    /// same as QImage::Format_ARGB32
    /// </summary>
    BGRA,
    /// <summary>
    /// same as QImage::Format_RGB32
    /// </summary>
    BGRX,
    RGB565LE,
    RGB48LE,
    RGB48 = RGB48LE,
    GBRP,
    GBRP10LE,
    XYZ12LE,
    YUVA420P,
    BC1,
    BC3,
    /// <summary>
    /// name: "rgba64le"
    /// </summary>
    RGBA64,
    /// <summary>
    /// name: "bgra64le"
    /// </summary>
    BGRA64,
    /// <summary>
    /// name: "rgbp16le"
    /// </summary>
    RGBP16,
    /// <summary>
    /// name: "rgbpf32le"
    /// </summary>
    RGBPF32,
    /// <summary>
    /// name: "bgraf32le"
    /// </summary>
    BGRAF32,
};

public partial struct TimeRange
{
    [NativeTypeName("int64_t")]
    public long start;

    [NativeTypeName("int64_t")]
    public long end;
}

/// <summary>
///events:
///<para>
///{timestamp(ms), "render.video", "1st_frame"}: when the first frame is rendered. requires setVideoSurface() called with a valid size<br/>
///{error, "decoder.audio/video/subtitle", "open", stream}: decoder of a stream is open, or failed to open if error != 0. TODO: do not use "open" ?<br/>
///{ 0, "decoder.video", decoderName, stream}: decoder of a stream is open or changed<br/>
///{0, "decoder.audio", decoderName, stream}: decoder of a stream is open or changed<br/>
///{0, "decoder.subtitle", decoderName, stream}: decoder of a stream is open or changed<br/>
///{track, "decoder.video", "size", {width, height}}: video decoder output size change. MediaInfo.video[track].codec.width / height also changes.<br/>
///{ track, "video", "size", { width, height} }: video frame size change before rendering, e.g. change by a filter. MediaInfo.video[track].codec.width / height does not change.<br/>
///{progress 0~100, "reader.buffering"}: error is buffering progress<br/>
///{ 0 / 1, "thread.audio/video/subtitle", stream}: decoder thread is started (error = 1) and about to exit(error = 0)<br/>
///{error, "snapshot", saved_file if no error and error string if error &lt; 0}<br/>
///{ 0, "cc"}: the 1st closed caption data is decoded. can be used in ui to show CC button.<br/>
///{0, "metadata"}: metadata update. new metadata can be read from Player.mediaInfo().metadata<br/>
///{count, "cache.ranges"}: buffered time ranges added, dropped or merged. count is ranges count<br/>
///TODO: video.thread, video.decoder, video.render<br/>
///</para>
/// </summary>
public class MediaEvent
{
    /// <summary>
    /// result &lt; 0: error code(fourcc?). >=0: special value depending on event
    /// </summary>
    public long Error { get; set; }

    /// <summary>
    /// The category of the event.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// if error, detail can be error string
    /// </summary>
    public string Detail { get; set; } = "";

    public int Stream { get; set; }

    public int VideoWidth { get; set; }

    public int VideoHeight { get; set; }
}

public enum MediaType : sbyte
{
    Unknown = -1,
    Video = 0,
    Audio = 1,
    Subtitle = 3,
};

public enum LogLevel
{
    Off,
    Error,
    Warning,
    Info,
    Debug,
    All,
}

public class Global
{
    private static LogHandler? logHandler;

    /// <summary>
    /// A callback function to process received log from MDK
    /// </summary>
    /// <param name="logLevel">Log Level</param>
    /// <param name="log">log string</param>
    public delegate void LogHandler(LogLevel logLevel, string log);

    /// <summary>
    /// If log handler is not set, i.e. setLogHandler() was not called, log is disabled.<br/>
    /// Set environment var `MDK_LOG=1` to enable log to std err.<br/>
    /// If set to non-null handler, logs that >= logLevel() will be passed to the handler.<br/>
    /// If previous handler is set by user and not null, then call setLogHandler(nullptr) will print to std err, and call setLogHandler(nullptr) again to silence the log<br/>
    /// To disable log, setLogHandler(nullptr) twice is better than simply setLogLevel(LogLevel::Off)
    /// </summary>
    /// <param name="handler">A callback function to process received log from MDK</param>
    public static void SetLogHandler(LogHandler? handler)
    {
        logHandler = handler;
        unsafe
        {
            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static void temp(MDK_LogLevel logLevel, sbyte* log, void* opaque)
            {
                var s_log = Marshal.PtrToStringUTF8((nint)log);
                if (s_log == null)
                    return;
                Marshal.GetDelegateForFunctionPointer<LogHandler>((nint)opaque)((LogLevel)logLevel, s_log);
            }
            mdkLogHandler callback = new()
            {
                cb = &temp,
                opaque = logHandler != null ? (void*)Marshal.GetFunctionPointerForDelegate(logHandler) : (void*)0,
            };
            Methods.MDK_setLogHandler(callback);
        }
    }

    /// <summary>
    /// use SetGlobalOption("log", LogLevel or name) instead
    /// </summary>
    /// <param name="logLevel"></param>
    [Obsolete("use SetGlobalOption(\"log\", LogLevel/*or name*/) instead")]
    public static void SetLogLevel(LogLevel logLevel = LogLevel.All)
    {
        Methods.MDK_setLogLevel((MDK_LogLevel)logLevel);
    }

    /// <summary>
    /// https://github.com/wang-bin/mdk-sdk/wiki/Global-Options
    /// </summary>
    /// <param name="key">
    /// - "avutil_lib", "avcodec_lib", "avformat_lib", "swresample_lib", "avfilter_lib": path to ffmpeg runtime libraries<br/>
    /// - "plugins_dir", "plugins.dir": plugins directory.MUST set before "plugins" if not in default dirs<br/>
    /// - "plugins": plugin filenames or paths in pattern "p1:p2:p3"<br/>
    /// - "MDK_KEY": license key for your product<br/>
    /// - "MDK_KEY_CODE_PAGE": license key code page used internally(windows only)<br/>
    /// - "ffmpeg.loglevel" or "ffmpeg.log": ffmpeg log level names, "trace", "debug", "verbose", "info", "warning", "error", "fatal", "panic", "quiet", or "${level}=${avclass}" to set AVClass log level(can be multiple times), e.g. "debug=http"<br/>
    /// - "ffmpeg.cpuflags": cpu flags for ffmpeg<br/>
    /// - "logLevel" or "log": can be "Off", "Error", "Warning", "Info", "Debug", "All". same as SetGlobalOption("logLevel", int(LogLevel))<br/>
    /// - "profiler.gpu": "0" or "1"<br/>
    /// - "R3DSDK_DIR": R3D dlls dir. default dir is working dir<br/>
    /// - "subtitle.fonts.dir": extra fonts dir for subtitle renderer<br/>
    /// - "subtitle.fonts.file": default subtitle font as fallback.can be an asset path for android<br/>
    /// - "subtitle.fonts.family": default subtitle font family as fallback
    /// </param>
    /// <param name="value"></param>
    public static void SetGlobalOption(string key, string value)
    {
        Methods.MDK_setGlobalOptionString(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">
    /// - "videoout.clear_on_stop": 0/1. clear renderer using background color if playback stops<br/>
    /// - "videoout.buffer_frames": N.max buffered frames to in the renderer<br/>
    /// - "videoout.hdr": auto send hdr metadata to display.overrides Player.set(ColorSpace)<br/>
    /// - "logLevel" or "log": raw int value of LogLevel<br/>
    /// - "profiler.gpu": true/false, 0/1<br/>
    /// - "demuxer.io": use io module for demuxer<br/>
    ///       - 0: use demuxer's internal io<br/>
    ///       - 1: default. prefer io module<br/>
    ///       - 2: always use io module for all protocols<br/>
    /// - "demuxer.live_eos_timeout": read error if no data for the given milliseconds for a live stream. default is 5000
    /// </param>
    /// <param name="value"></param>
    public static void SetGlobalOption(string key, int value)
    {
        Methods.MDK_setGlobalOptionInt32(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">"sdr.white": sdr white level. usually it's 203, but some obs-studio default value is 300, so let user set the value</param>
    /// <param name="value"></param>
    public static void SetGlobalOption(string key, float value)
    {
        Methods.MDK_setGlobalOptionFloat(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">
    /// "jvm", "JavaVM": JavaVM*. android only. Required if not loaded by System.loadLibrary()<br/>
    /// "android.app.Application" or "android.content.Context": jobject. android only. automatically set when setting JavaVM.<br/>
    /// "X11Display": Display*<br/>
    /// "DRMDevice": drm device path, for vaapi<br/>
    /// "DRMFd": drm fd, for vaapi<br/>
    /// "d3d11.device": ID3D11Device*, global d3d11 device used by decoders and renderers. if value is 1, create an internal device as global device(same decoder and renderer device may results in lower fps, e.g. amd gpu)
    /// </param>
    /// <param name="value"></param>
    public static void SetGlobalOption(string key, IntPtr value)
    {
        unsafe
        {
            Methods.MDK_setGlobalOptionPtr(key, value.ToPointer());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">"ffmpeg.configuration": ffmpeg major version. return false if no ffmpeg api was invoked internally.</param>
    /// <param name="value"></param>
    /// <returns>false if no such key</returns>
    public static bool GetGlobalOption(string key, out string value)
    {
        unsafe
        {
            sbyte* ptr = null;
            var ret = Methods.MDK_getGlobalOptionString(key, &ptr);
            value = Marshal.PtrToStringUTF8((nint)ptr) ?? "";
            return *(bool*)ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">"ffmpeg.version": ffmpeg major version. return false if no ffmpeg api was invoked internally.</param>
    /// <param name="value"></param>
    /// <returns>false if no such key</returns>
    public static bool GetGlobalOption(string key, out int value)
    {
        unsafe
        {
            fixed (int* ptr = &value)
            {
                var ret = Methods.MDK_getGlobalOptionInt32(key, ptr);
                return *(bool*)ret;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetGlobalOption(string key, out IntPtr value)
    {
        unsafe
        {
            fixed (void* ptr = &value)
            {
                var ret = Methods.MDK_getGlobalOptionPtr(key, &ptr);
                return *(bool*)ret;
            }
        }
    }

    /// <summary>
    /// Get MDK Version Int Number
    /// </summary>
    public static int MDK_VERSION_INT
    {
        get
        { return (Methods.MDK_MAJOR << 16) | (Methods.MDK_MINOR << 8) | Methods.MDK_MICRO; }
    }
}