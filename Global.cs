using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;

namespace MDK.SDK.NET;

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
    From0 = 1,          // relative to time 0. TODO: remove from api
    FromStart = 1 << 1, // relative to media start position
    FromNow = 1 << 2,   // relative to current position, the seek position can be negative
    Frame = 1 << 6,     // Seek by frame. Seek target is frame count instead of milliseconds. Currently only FromNow|Frame is supported. BUG: avsync
    KeyFrame = 1 << 8,  // fast key-frame seek, forward if Backward is not set. It's accurate seek without this flag. Accurate seek is slow and implies backward seek internally.
    Fast = KeyFrame,
    InCache = 1 << 10,  // try to seek in memory cache first. useful for speeding up network stream seeking.  Target position must be in range (position(), position() + Player.buffered()]
    Default = KeyFrame | FromStart | InCache
}

public enum MediaStatus
{
    NoMedia = 0, // initial status, not invalid. // what if set an empty url and closed?
    Unloaded = 1, // unloaded // (TODO: or when a source(url) is set?)
    Loading = 1 << 1, // opening and parsing the media
    Loaded = 1 << 2, // media is loaded and parsed. player is stopped state. mediaInfo() is available now
    Prepared = 1 << 8, // all tracks are buffered and ready to decode frames. tracks failed to open decoder are ignored
    Stalled = 1 << 3, // insufficient buffering or other interruptions (timeout, user interrupt)
    Buffering = 1 << 4, // when buffering starts
    Buffered = 1 << 5, // when buffering ends
    End = 1 << 6, // reached the end of the current media, no more data to read
    Seeking = 1 << 7,
    Invalid = 1 << 31, // failed to load media because of unsupport format or invalid media source
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
    // data: rgba or bgra data. Created internally or provided by user.
    // If data is provided by user, stride, height and width MUST be also set, and data MUST be valid until snapshot callback is finished.
    public IntPtr data = 0;
    // result width of snapshot image set by user, or the same as current frame width if 0. no renderer transform.
    // if both requested width and height are < 0, then result image is scaled image of current frame with ratio=width/height. no renderer transform.
    // if only one of width and height < 0, then the result size is video renderer viewport size, and all transforms will be applied.
    // if both width and height == 0, then result size is region of interest size of video frame set by setPointMap(), or video frame size
    public int width = 0;
    public int height = 0;
    public int stride = 0;
    public bool subtitle = false; // not supported yet

    public SnapshotRequest()
    {
    }
}

public enum MapDirection
{
    FrameToViewport, // left-hand
    ViewportToFrame, // left-hand
};

public enum VideoEffect
{
    Brightness,   /* [-1.0f, 1.0f], default 0 */
    Contrast,     /* [-1.0f, 1.0f], default 0 */
    Hue,          /* [-1.0f, 1.0f], default 0 */
    Saturation,   /* [-1.0f, 1.0f], default 0 */
};

public enum ColorSpace
{
    ColorSpaceUnknown,
    ColorSpaceBT709,
    ColorSpaceBT2100_PQ,
    ColorSpaceSCRGB,        // scRGB, linear sRGB in extended component range
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
    RGBA,
    RGBX,
    BGRA,
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
    RGBA64,         // name: "rgba64le"
    BGRA64,         // name: "bgra64le"
    RGBP16,         // name: "rgbp16le"
    RGBPF32,        // name: "rgbpf32le"
    BGRAF32,        // name: "bgraf32le"
};

/// <summary>
/// The MediaEvent class represents an event that can occur during media playback.
/// </summary>
public class MediaEvent
{
    /// <summary>
    /// The error code associated with the event. If the value is less than 0, it represents an error code (fourcc?). If the value is greater than or equal to 0, it represents a special value depending on the event.
    /// </summary>
    public long Error { get; set; }

    /// <summary>
    /// The category of the event.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// The detail of the event. If there is an error, the detail can be an error string.
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
    private LogHandler? logHandler;

    public delegate void LogHandler(LogLevel logLevel, string log);

    public void SetLogHandler(LogHandler handler)
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
                opaque = (void*)Marshal.GetFunctionPointerForDelegate(logHandler),
            };
            Methods.MDK_setLogHandler(callback);
        }
    }

    public static void SetLogLevel(LogLevel logLevel = LogLevel.All)
    {
        Methods.MDK_setLogLevel((MDK_LogLevel)logLevel);
    }

    public static void SetGlobalOption(string key, string value)
    {
        Methods.MDK_setGlobalOptionString(key, value);
    }

    public static void SetGlobalOption(string key, int value)
    {
        Methods.MDK_setGlobalOptionInt32(key, value);
    }

    public static void SetGlobalOption(string key, float value)
    {
        Methods.MDK_setGlobalOptionFloat(key, value);
    }

    public static void SetGlobalOption(string key, IntPtr value)
    {
        unsafe
        {
            Methods.MDK_setGlobalOptionPtr(key, value.ToPointer());
        }
    }
}