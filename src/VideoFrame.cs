using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MDK.SDK.NET.Gen;

// ReSharper disable UnusedMember.Global

namespace MDK.SDK.NET;

/// <summary>
/// Represents a video frame.
/// </summary>
public class VideoFrame : IDisposable
{
    private unsafe mdkVideoFrameAPI* _p;
    private bool _owner = true;
    private int _disposed;

    private sealed class BufferDeleterContext
    {
        public required BufferDeleter Callback { get; init; }
        public IntPtr Buffer { get; init; }
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
    /// A managed callback that releases the external buffer owner passed to <see cref="AddBuffer(IntPtr, int, int, IntPtr, BufferDeleter?)"/>.
    /// </summary>
    /// <param name="buffer">External buffer owner pointer.</param>
    public delegate void BufferDeleter(IntPtr buffer);

    /// <summary>
    /// Constructs an invalid video frame.
    /// </summary>
    public VideoFrame()
    {
    }

    /// <summary>
    /// Constructs a video frame for the given format and size.
    /// </summary>
    /// <param name="width">Visual width.</param>
    /// <param name="height">Visual height.</param>
    /// <param name="format">Pixel format.</param>
    public VideoFrame(int width, int height, PixelFormat format)
    {
        unsafe
        {
            _p = Methods.mdkVideoFrameAPI_new(width, height, ToNativeFormat(format));
        }
    }

    /// <summary>
    /// Constructs a video frame for the given format and size, then initializes buffers from native pointer arrays.
    /// </summary>
    /// <param name="width">Visual width.</param>
    /// <param name="height">Visual height.</param>
    /// <param name="format">Pixel format.</param>
    /// <param name="strides">Pointer to an array of per-plane strides.</param>
    /// <param name="data">Pointer to an array of per-plane data pointers.</param>
    public VideoFrame(int width, int height, PixelFormat format, IntPtr strides, IntPtr data)
        : this(width, height, format)
    {
        if (data != IntPtr.Zero)
            SetBuffers(data, strides);
    }

    /// <summary>
    /// Constructs a video frame for the given format and size, then initializes buffers from managed pointer and stride spans.
    /// </summary>
    /// <param name="width">Visual width.</param>
    /// <param name="height">Visual height.</param>
    /// <param name="format">Pixel format.</param>
    /// <param name="strides">Per-plane strides. The span must be empty or contain at least <see cref="PlaneCount"/> items.</param>
    /// <param name="data">Per-plane source data pointers. The span must be empty or contain at least <see cref="PlaneCount"/> items.</param>
    public VideoFrame(int width, int height, PixelFormat format, Span<int> strides, Span<IntPtr> data)
        : this(width, height, format)
    {
        if (!data.IsEmpty)
            SetBuffers(data, strides);
    }

    /// <summary>
    /// Constructs a video frame from an existing mdkVideoFrameAPI pointer and takes ownership of that pointer.
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
    /// Gets a value indicating whether this instance currently wraps a native video frame.
    /// </summary>
    public bool IsValid
    {
        get
        {
            unsafe
            {
                return Volatile.Read(ref _disposed) == 0 && _p != null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the timestamp of the video frame.
    /// </summary>
    public double Timestamp
    {
        get
        {
            unsafe
            {
                ThrowIfDisposed();
                var p = _p;
                return p == null ? -1 : p->timestamp(p->@object);
            }
        }
        set
        {
            unsafe
            {
                var p = GetRequiredHandle();
                p->setTimestamp(p->@object, value);
            }
        }
    }

    /// <summary>
    /// Gets the clockwise rotation of the video frame in degrees.
    /// </summary>
    public int Rotation
    {
        get
        {
            unsafe
            {
                ThrowIfDisposed();
                var p = _p;
                return p == null ? -1 : p->rotation(p->@object);
            }
        }
    }

    /// <summary>
    /// Returns metadata bytes for the specified key.
    /// </summary>
    /// <param name="key">Metadata key.</param>
    /// <returns>Metadata bytes, or an empty array when the key is absent.</returns>
    public byte[] Metadata(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        unsafe
        {
            ThrowIfDisposed();
            var p = _p;
            if (p == null)
                return [];

            var keyBytes = Encoding.UTF8.GetBytes(key + '\0');
            var size = 0;
            fixed (byte* keyPtr = keyBytes)
            {
                var ret = p->metadata(p->@object, (sbyte*)keyPtr, &size);
                if (ret == null || size <= 0)
                    return [];

                var data = new byte[size];
                Marshal.Copy((IntPtr)ret, data, 0, size);
                return data;
            }
        }
    }

    /// <summary>
    /// Gets the number of planes in the video frame.
    /// </summary>
    public int PlaneCount
    {
        get
        {
            unsafe
            {
                var p = GetRequiredHandle();
                return p->planeCount(p->@object);
            }
        }
    }

    /// <summary>
    /// Gets the width of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Width of the video frame.</returns>
    public int Width(int plane = -1)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->width(p->@object, plane);
        }
    }

    /// <summary>
    /// Gets the height of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Height of the video frame.</returns>
    public int Height(int plane = -1)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->height(p->@object, plane);
        }
    }

    /// <summary>
    /// Gets the pixel format of the video frame.
    /// </summary>
    /// <returns>Pixel format of the video frame.</returns>
    public PixelFormat Format()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return FromNativeFormat(p->format(p->@object));
        }
    }

    /// <summary>
    /// Adds an external buffer to the given plane. The native frame stores <paramref name="data"/> and releases
    /// <paramref name="buffer"/> through <paramref name="bufferDeleter"/> when the buffer is replaced or the frame is destroyed.
    /// </summary>
    /// <param name="data">External buffer data pointer. It must remain valid until the native frame releases the buffer.</param>
    /// <param name="stride">Stride of data. If &lt;= 0, the native library uses the current format stride for this plane.</param>
    /// <param name="plane">Plane index.</param>
    /// <param name="buffer">External buffer owner pointer passed back to <paramref name="bufferDeleter"/>.</param>
    /// <param name="bufferDeleter">Callback invoked when the native frame releases <paramref name="buffer"/>.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool AddBuffer(IntPtr data, int stride, int plane = -1, IntPtr buffer = default,
        BufferDeleter? bufferDeleter = null)
    {
        unsafe
        {
            if (bufferDeleter == null)
                return AddBufferCore(data, stride, plane, buffer, null);

            var ctx = new BufferDeleterContext
            {
                Callback = bufferDeleter,
                Buffer = buffer,
            };
            var handle = GCHandle.Alloc(ctx);
            ctx.SetHandle(handle);

            try
            {
                var added = AddBufferCore(data, stride, plane, GCHandle.ToIntPtr(handle), &ReleaseManagedBuffer);
                if (!added)
                    ctx.ReleaseHandle();

                return added;
            }
            catch
            {
                ctx.ReleaseHandle();
                throw;
            }
        }
    }

    /// <summary>
    /// Adds a buffer by copying from managed memory.
    /// </summary>
    /// <param name="data">Plane data to copy. The span must contain at least the bytes required by the plane stride and height.</param>
    /// <param name="stride">Stride of data. If &lt;= 0, the native library uses the current format stride for this plane.</param>
    /// <param name="plane">Plane index. This overload requires a concrete plane index.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool AddBuffer(ReadOnlySpan<byte> data, int stride, int plane = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(plane);
        EnsurePlaneSpanIsLargeEnough(data, stride, plane);

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                return AddBufferCore((IntPtr)dataPtr, stride, plane, default, null);
            }
        }
    }

    /// <summary>
    /// Sets the buffers with data copied from the given native pointer arrays.
    /// </summary>
    /// <param name="data">Pointer to an array of source data planes, or zero to allocate without copying.</param>
    /// <param name="strides">Pointer to an array of plane strides, or zero to use native defaults.</param>
    public void SetBuffers(IntPtr data, IntPtr strides)
    {
        unsafe
        {
            SetBuffersCore((byte**)data, (int*)strides);
        }
    }

    /// <summary>
    /// Sets the buffers with data copied from the given source plane pointers.
    /// </summary>
    /// <param name="data">Per-plane source data pointers. The span must be empty or contain at least <see cref="PlaneCount"/> items.</param>
    public void SetBuffers(Span<IntPtr> data)
    {
        SetBuffers(data, Span<int>.Empty);
    }

    /// <summary>
    /// Sets the buffers with data copied from the given source plane pointers.
    /// </summary>
    /// <param name="data">Per-plane source data pointers. The span must be empty or contain at least <see cref="PlaneCount"/> items.</param>
    /// <param name="strides">Per-plane strides. The span must be empty or contain at least <see cref="PlaneCount"/> items.</param>
    public void SetBuffers(Span<IntPtr> data, Span<int> strides)
    {
        ValidatePlaneArrayLengths(data.Length, strides.Length);

        unsafe
        {
            fixed (IntPtr* dataPtr = data)
            fixed (int* stridePtr = strides)
            {
                SetBuffersCore((byte**)dataPtr, stridePtr);
            }
        }
    }

    /// <summary>
    /// Gets the buffer data pointer of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Buffer data pointer.</returns>
    public IntPtr BufferData(int plane = 0)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return (IntPtr)p->bufferData(p->@object, plane);
        }
    }

    /// <summary>
    /// Gets a read-only span over the native plane data.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>A span whose lifetime is bound to this frame and its current buffers.</returns>
    public ReadOnlySpan<byte> GetPlaneSpan(int plane = 0)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            var data = p->bufferData(p->@object, plane);
            var stride = p->bytesPerLine(p->@object, plane);
            var height = p->height(p->@object, plane);
            if (data == null || stride <= 0 || height <= 0)
                return [];

            var length = checked(stride * height);
            return new ReadOnlySpan<byte>(data, length);
        }
    }

    /// <summary>
    /// Gets the bytes per line of the video frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Bytes per line.</returns>
    public int BytesPerLine(int plane = 0)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->bytesPerLine(p->@object, plane);
        }
    }

    /// <summary>
    /// Converts the video frame to the specified format, width and height.
    /// </summary>
    /// <param name="format">Output format. If invalid, same as format().</param>
    /// <param name="width">Output width. If invalid(&lt;= 0), same as width().</param>
    /// <param name="height">Output height. If invalid(&lt;= 0), same as height().</param>
    /// <returns>Converted video frame, or an invalid frame when conversion fails.</returns>
    public VideoFrame To(PixelFormat format, int width = -1, int height = -1)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return new VideoFrame(p->to(p->@object, ToNativeFormat(format), width, height));
        }
    }

    /// <summary>
    /// Creates a new managed wrapper that references the same native video frame.
    /// </summary>
    /// <returns>A referenced video frame wrapper.</returns>
    public VideoFrame Clone()
    {
        unsafe
        {
            ThrowIfDisposed();
            var p = _p;
            return new VideoFrame(p == null ? null : Methods.mdkVideoFrameAPI_ref(p));
        }
    }

    /// <summary>
    /// Saves the frame to the file with the given fileName, using the given image file format and quality factor.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="format">Image file format. If null, guess the format by fileName's suffix.</param>
    /// <param name="quality">Quality factor in the range 0.0 to 1.0, or -1 to use native defaults.</param>
    /// <returns>True if the frame was successfully saved; otherwise false.</returns>
    public bool Save(string fileName, string? format = null, float quality = -1)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        unsafe
        {
            var p = GetRequiredHandle();
            var fileNameBytes = Encoding.UTF8.GetBytes(fileName + '\0');
            fixed (byte* fileNamePtr = fileNameBytes)
            {
                if (format == null)
                    return p->save(p->@object, (sbyte*)fileNamePtr, null, quality) != 0;

                var formatBytes = Encoding.UTF8.GetBytes(format + '\0');
                fixed (byte* formatPtr = formatBytes)
                {
                    return p->save(p->@object, (sbyte*)fileNamePtr, (sbyte*)formatPtr, quality) != 0;
                }
            }
        }
    }

    /// <summary>
    /// Attaches an existing mdkVideoFrameAPI pointer to the video frame.
    /// </summary>
    /// <param name="api">mdkVideoFrameAPI pointer.</param>
    internal unsafe void Attach(mdkVideoFrameAPI* api)
    {
        ThrowIfDisposed();
        if (_owner && _p != null)
            DeleteOwned();
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
    /// Releases the unmanaged resources used by the <see cref="VideoFrame"/> object and optionally releases managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        unsafe
        {
            if (_owner && _p != null)
                DeleteOwned();

            _p = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe bool AddBufferCore(IntPtr data, int stride, int plane, IntPtr buffer,
        delegate* unmanaged[Cdecl]<void**, void> bufferDeleter)
    {
        var p = GetRequiredHandle();
        return p->addBuffer(p->@object, (byte*)data, stride, (void*)buffer, bufferDeleter, plane) != 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void ReleaseManagedBuffer(void** nativeBuffer)
    {
        if (nativeBuffer == null || *nativeBuffer == null)
            return;

        BufferDeleterContext? ctx = null;
        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)(*nativeBuffer));
            ctx = handle.Target as BufferDeleterContext;
            ctx?.Callback(ctx.Buffer);
        }
        catch
        {
            // Exceptions cannot cross the unmanaged callback boundary.
        }
        finally
        {
            ctx?.ReleaseHandle();
            *nativeBuffer = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void SetBuffersCore(byte** data, int* strides)
    {
        var p = GetRequiredHandle();
        p->setBuffers(p->@object, data, strides);
    }

    private void ValidatePlaneArrayLengths(int dataLength, int stridesLength)
    {
        var planeCount = PlaneCount;
        if (dataLength != 0 && dataLength < planeCount)
            throw new ArgumentException("The data span must be empty or contain at least the frame plane count.",
                nameof(dataLength));
        if (stridesLength != 0 && stridesLength < planeCount)
            throw new ArgumentException("The strides span must be empty or contain at least the frame plane count.",
                nameof(stridesLength));
    }

    private void EnsurePlaneSpanIsLargeEnough(ReadOnlySpan<byte> data, int stride, int plane)
    {
        var requiredStride = stride > 0 ? stride : BytesPerLine(plane);
        var height = Height(plane);
        if (requiredStride <= 0 || height <= 0)
            throw new InvalidOperationException("The required plane byte count cannot be determined.");

        var requiredLength = checked(requiredStride * height);
        if (data.Length < requiredLength)
            throw new ArgumentException("The data span is too small for the requested plane.", nameof(data));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe mdkVideoFrameAPI* GetRequiredHandle()
    {
        ThrowIfDisposed();
        var p = _p;
        return p == null ? ThrowInvalidFrame() : p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void DeleteOwned()
    {
        fixed (mdkVideoFrameAPI** p = &_p)
        {
            Methods.mdkVideoFrameAPI_delete(p);
        }
    }

    [DoesNotReturn]
    private static unsafe mdkVideoFrameAPI* ThrowInvalidFrame()
    {
        throw new InvalidOperationException("Video frame is invalid.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
            ThrowDisposed();
    }

    [DoesNotReturn]
    private static void ThrowDisposed()
    {
        throw new ObjectDisposedException(nameof(VideoFrame));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MDK_PixelFormat ToNativeFormat(PixelFormat format)
    {
        return (MDK_PixelFormat)((int)format - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PixelFormat FromNativeFormat(MDK_PixelFormat format)
    {
        return (PixelFormat)((int)format + 1);
    }
}

/// <summary>
/// Direct3D 11 video resource description.
/// </summary>
public struct DX11Resource
{
    /// <summary>
    /// ID3D11Texture2D or ID3D11VideoDecoderOutputView as input, ID3D11Texture2D as output.
    /// </summary>
    public IntPtr resource = IntPtr.Zero;

    /// <summary>
    /// Subresource index for texture array, or 0 otherwise.
    /// </summary>
    public int subResource = 0;

    /// <summary>
    /// ID3D11Texture2D for each plane. The array should contain 4 items when used with native DX11 APIs.
    /// </summary>
    public IntPtr[] plane = [];

    /// <summary>
    /// Plane count.
    /// </summary>
    public int planeCount = 0;

    /// <summary>
    /// Initializes a new Direct3D 11 resource description.
    /// </summary>
    public DX11Resource()
    {
    }
}
