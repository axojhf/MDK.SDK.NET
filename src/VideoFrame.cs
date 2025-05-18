using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;

namespace MDK.SDK.NET;

/// <summary>
/// Represents a video frame.
/// </summary>
public class VideoFrame : IDisposable
{
    private unsafe mdkVideoFrameAPI* _p;
    private bool _owner = true;

    /// <summary>
    /// Constructs a video frame for given format, size. If strides is not null, a single contiguous memory for all planes will be allocated.
    /// If data is not null, data is copied to allocated memory.
    /// </summary>
    /// <param name="width">Visual width.</param>
    /// <param name="height">Visual height.</param>
    /// <param name="format">Pixel format.</param>
    /// <param name="strides">Stride of data. If &lt;=0, it's the stride of current format at this plane.</param>
    /// <param name="data">External buffer data ptr.</param>
    public VideoFrame(int width, int height, PixelFormat format, IntPtr strides, IntPtr data)
    {
        unsafe
        {
            _p = Methods.mdkVideoFrameAPI_new(width, height, (MDK_PixelFormat)((int)format - 1));
            if (data != 0)
                _p->setBuffers(_p->@object, (byte**)data, (int*)strides);
        }
    }

    /// <summary>
    /// Constructs a video frame from an existing mdkVideoFrameAPI pointer.
    /// </summary>
    /// <param name="pp">mdkVideoFrameAPI pointer.</param>
    internal unsafe VideoFrame(mdkVideoFrameAPI* pp)
    {
        _p = pp;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="VideoFrame"/> class.
    /// </summary>
    ~VideoFrame()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets a value indicating whether this instance is valid.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
    /// </value>
    public bool IsValid { get { unsafe { return _p != null; } } }

    /// <summary>
    /// Gets or sets the timestamp of the video frame.
    /// </summary>
    public double Timestamp
    {
        get
        {
            if (!IsValid)
                return -1;
            unsafe { return _p->timestamp(_p->@object); }
        }
        set { unsafe { _p->setTimestamp(_p->@object, value); } }
    }

    /// <summary>
    /// Gets the number of planes in the video frame.
    /// </summary>
    public int PlaneCount
    {
        get { unsafe { return _p->planeCount(_p->@object); } }
    }

    /// <summary>
    /// Gets the width of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Width of the video frame.</returns>
    public int Width(int plane = -1)
    {
        unsafe { return _p->width(_p->@object, plane); }
    }

    /// <summary>
    /// Gets the height of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Height of the video frame.</returns>
    public int Height(int plane = -1)
    {
        unsafe { return _p->height(_p->@object, plane); }
    }

    /// <summary>
    /// Gets the pixel format of the video frame.
    /// </summary>
    /// <returns>Pixel format of the video frame.</returns>
    public PixelFormat Format()
    {
        unsafe
        {
            return (PixelFormat)(_p->format(_p->@object) + 1);
        }
    }

    /// <summary>
    /// Adds an external buffer to nth plane, store external buffer data ptr. The old buffer will be released.
    /// </summary>
    /// <param name="data">External buffer data ptr.</param>
    /// <param name="stride">Stride of data. If &lt;=0, it's the stride of current format at this plane.</param>
    /// <param name="buf">External buffer ptr. User should ensure the buffer is alive before frame is destroyed.</param>
    /// <param name="bufDeleter">To delete buf when frame is destroyed.</param>
    /// <param name="plane">Plane index.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public unsafe bool AddBuffer(IntPtr data, int stride, int plane = -1, IntPtr buf = 0, delegate* unmanaged[Cdecl]<void**, void> bufDeleter = null)
    {
        return Convert.ToBoolean(_p->addBuffer(_p->@object, (byte*)data, stride, (void*)buf, bufDeleter, plane));
    }

    /// <summary>
    /// Sets the buffers with data copied from given source. Unlike constructor, a single contiguous memory for all planes is always allocated.
    /// If data is not null, data is copied to allocated memory.
    /// </summary>
    /// <param name="data">Array of source data planes, array size MUST >= plane count of format if not null. Can be null and allocate memory without copying.</param>
    /// <param name="strides">Array of plane strides, size MUST >= plane count of format if not null. Can be null and strides[i] can be &lt;=0 indicating no padding bytes (for plane i).</param>
    public void SetBuffers(IntPtr data, IntPtr strides)
    {
        unsafe
        {
            _p->setBuffers(_p->@object, (byte**)data, (int*)strides);
        }
    }

    /// <summary>
    /// Gets the buffer data of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Buffer data of the video frame.</returns>
    public IntPtr BufferData(int plane = 0)
    {
        unsafe { return (nint)_p->bufferData(_p->@object, plane); }
    }

    /// <summary>
    /// Gets the bytes per line of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Bytes per line of the video frame.</returns>
    public int BytesPerLine(int plane = 0)
    {
        unsafe { return _p->bytesPerLine(_p->@object, plane); }
    }

    /// <summary>
    /// Converts the video frame to the specified format, width and height.
    /// </summary>
    /// <param name="format">Output format. If invalid, same as format().</param>
    /// <param name="width">Output width. If invalid(&lt;=0), same as width().</param>
    /// <param name="height">Output height. If invalid(&lt;=0), same as height().</param>
    /// <returns>Converted video frame.</returns>
    public VideoFrame To(PixelFormat format, int width = -1, int height = -1)
    {
        unsafe { return new VideoFrame(_p->to(_p->@object, (MDK_PixelFormat)((int)format - 1), width, height)); }
    }

    /// <summary>
    /// Saves the frame to the file with the given fileName, using the given image file format and quality factor.
    /// Save the original frame data if:
    /// - fileName extension is the same as format().name()
    /// - fileName has no extension, and format is null
    /// - format is the same as format().name()
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="format">Image file format. If null, guess the format by fileName's suffix.</param>
    /// <param name="quality">Quality factor. Must be in the range 0.0 to 1.0 or -1. Specify 0 to obtain small compressed files, 100 for large uncompressed files, and -1 (the default) to use the default settings.</param>
    /// <returns>True if the frame was successfully saved; otherwise returns false.</returns>
    public bool Save(string fileName, string? format = null, float quality = -1)
    {
        unsafe
        {
            IntPtr filename = Marshal.StringToCoTaskMemUTF8(fileName), formatTemp = Marshal.StringToCoTaskMemUTF8(format);
            var ret = Convert.ToBoolean(_p->save(_p->@object, (sbyte*)filename, (sbyte*)formatTemp, quality));
            Marshal.FreeCoTaskMem(filename);
            Marshal.FreeCoTaskMem(formatTemp);
            return ret;
        }
    }

    /// <summary>
    /// Attaches an existing mdkVideoFrameAPI pointer to the video frame.
    /// </summary>
    /// <param name="api">mdkVideoFrameAPI pointer.</param>
    internal unsafe void Attach(mdkVideoFrameAPI* api)
    {
        if (_owner)
            fixed (mdkVideoFrameAPI** p = &this._p)
                Methods.mdkVideoFrameAPI_delete(p);
        _p = api;
        _owner = false;
    }

    /// <summary>
    /// Detaches the mdkVideoFrameAPI pointer from the video frame.
    /// </summary>
    /// <returns>mdkVideoFrameAPI pointer.</returns>
    internal unsafe mdkVideoFrameAPI* Detach()
    {
        var ptr = _p;
        _p = null;
        return ptr;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="VideoFrame"/> object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="VideoFrame"/> object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose managed resources.
        }

        // Dispose unmanaged resources.
        unsafe
        {
            if (_owner)
                fixed (mdkVideoFrameAPI** p = &this._p)
                    Methods.mdkVideoFrameAPI_delete(p);
        }
    }
}

