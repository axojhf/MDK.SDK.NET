using MDK.SDK.NET.Gen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MDK.SDK.NET;

/// <summary>
/// Audio frame.
/// </summary>
public sealed class AudioFrame : IDisposable
{
    private unsafe mdkAudioFrameAPI* _p;
    private bool _owner = true;
    private bool _disposed;

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
    /// A managed callback that releases the external buffer owner passed to <see cref="AddBuffer(IntPtr, nuint, int, IntPtr, BufferDeleter?)"/>.
    /// </summary>
    /// <param name="buffer">External buffer owner pointer.</param>
    public delegate void BufferDeleter(IntPtr buffer);

    /// <summary>
    /// Constructs a audio frame for given format, channels, sample rate, samples per channel.
    /// </summary>
    /// <param name="format">Sample format.</param>
    /// <param name="channels">Number of channels.</param>
    /// <param name="sampleRate">Sample rate.</param>
    /// <param name="samplesPerChannel">Samples per channel.</param>
    public AudioFrame(SampleFormat format, int channels, int sampleRate, int samplesPerChannel)
    {
        unsafe
        {
            _p = Methods.mdkAudioFrameAPI_new((MDK_SampleFormat)format, channels, sampleRate, samplesPerChannel);
        }
    }

    internal unsafe AudioFrame(mdkAudioFrameAPI* pp)
    {
        _p = pp;
    }

    /// <summary>
    /// isValid() is true for EOS frame, but no data and timestamp() is TimestampEOS.
    /// </summary>
    public bool IsValid
    {
        get
        {
            unsafe
            {
                return !_disposed && _p != null;
            }
        }
    }

    internal unsafe void Attach(mdkAudioFrameAPI* api)
    {
        ThrowIfDisposed();
        if (_owner && _p != null)
            DeleteOwned();
        _p = api;
        _owner = false;
    }

    internal unsafe mdkAudioFrameAPI* Detach()
    {
        var ptr = _p;
        _p = null;
        return ptr;
    }

    /// <summary>
    /// Returns the number of planes in the audio frame.
    /// </summary>
    public int PlaneCount()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->planeCount(p->@object);
        }
    }

    /// <summary>
    /// Returns the sample format of the audio frame.
    /// </summary>
    public SampleFormat Format()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return (SampleFormat)p->sampleFormat(p->@object);
        }
    }

    /// <summary>
    /// Returns the sample rate of the audio frame.
    /// </summary>
    public int SampleRate()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->sampleRate(p->@object);
        }
    }

    /// <summary>
    /// Returns the number of channels in the audio frame.
    /// </summary>
    public int Channels()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->channels(p->@object);
        }
    }

    /// <summary>
    /// Returns the channel mask of the audio frame.
    /// </summary>
    public ulong ChannelMask()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->channelMask(p->@object);
        }
    }

    /// <summary>
    /// Returns the number of samples per channel in the audio frame.
    /// </summary>
    public int SamplesPerChannel()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->samplesPerChannel(p->@object);
        }
    }

    /// <summary>
    /// Sets the number of samples per channel in the audio frame.
    /// </summary>
    /// <param name="samples">Samples per channel.</param>
    public void SetSamplesPerChannel(int samples)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            p->setSamplesPerChannel(p->@object, samples);
        }
    }

    /// <summary>
    /// Adds a buffer to the audio frame.
    /// </summary>
    /// <param name="data">Pointer to the data.</param>
    /// <param name="size">Size of the data.</param>
    /// <param name="plane">Plane index.</param>
    /// <param name="buffer">External buffer owner pointer passed back to <paramref name="bufferDeleter"/>.</param>
    /// <param name="bufferDeleter">Callback invoked when the native frame releases <paramref name="buffer"/>.</param>
    public bool AddBuffer(IntPtr data, nuint size, int plane = -1, IntPtr buffer = default, BufferDeleter? bufferDeleter = null)
    {
        unsafe
        {
            if (bufferDeleter == null)
                return AddBufferCore(data, size, plane, buffer, null);

            var ctx = new BufferDeleterContext
            {
                Callback = bufferDeleter,
                Buffer = buffer,
            };
            var handle = GCHandle.Alloc(ctx);
            ctx.SetHandle(handle);

            try
            {
                var added = AddBufferCore(data, size, plane, GCHandle.ToIntPtr(handle), &ReleaseManagedBuffer);
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
    /// Adds a buffer to the audio frame by copying from managed memory.
    /// </summary>
    /// <param name="data">Audio data to copy.</param>
    /// <param name="plane">Plane index.</param>
    public bool AddBuffer(ReadOnlySpan<byte> data, int plane = -1)
    {
        unsafe
        {
            fixed (byte* pData = data)
            {
                return AddBufferCore((IntPtr)pData, (nuint)data.Length, plane, default, null);
            }
        }
    }

    /// <summary>
    /// Sets the buffers for the audio frame.
    /// </summary>
    /// <param name="data">Pointer to an array of plane data pointers.</param>
    /// <param name="bytesPerPlane">Bytes per plane.</param>
    public void SetBuffers(IntPtr data, int bytesPerPlane)
    {
        unsafe
        {
            SetBuffersCore((byte**)data, bytesPerPlane);
        }
    }

    /// <summary>
    /// Sets the buffers for the audio frame.
    /// </summary>
    /// <param name="data">Plane data pointers. The span length must be at least the plane count when it is not empty.</param>
    /// <param name="bytesPerPlane">Bytes per plane. If zero, the native library computes it from the frame format.</param>
    public void SetBuffers(Span<IntPtr> data, int bytesPerPlane)
    {
        unsafe
        {
            fixed (IntPtr* pData = data)
            {
                SetBuffersCore((byte**)pData, bytesPerPlane);
            }
        }
    }

    /// <summary>
    /// Returns the buffer data for the audio frame.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>Pointer to the buffer data.</returns>
    public IntPtr BufferData(int plane = 0)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return (IntPtr)p->bufferData(p->@object, plane);
        }
    }

    /// <summary>
    /// Returns a read-only span over the native plane data.
    /// </summary>
    /// <param name="plane">Plane index.</param>
    /// <returns>A span whose lifetime is bound to this frame and its current buffers.</returns>
    public ReadOnlySpan<byte> GetPlaneSpan(int plane = 0)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            var data = p->bufferData(p->@object, plane);
            var length = p->bytesPerPlane(p->@object);
            return data == null || length <= 0 ? [] : new ReadOnlySpan<byte>(data, length);
        }
    }

    /// <summary>
    /// Returns the bytes per plane for the audio frame.
    /// </summary>
    /// <returns>Bytes per plane.</returns>
    public int BytesPerPlane()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->bytesPerPlane(p->@object);
        }
    }

    /// <summary>
    /// Sets the timestamp for the audio frame.
    /// </summary>
    /// <param name="t">Timestamp.</param>
    public void SetTimestamp(double t)
    {
        unsafe
        {
            var p = GetRequiredHandle();
            p->setTimestamp(p->@object, t);
        }
    }

    /// <summary>
    /// Returns the timestamp for the audio frame.
    /// </summary>
    /// <returns>Timestamp.</returns>
    public double Timestamp()
    {
        unsafe
        {
            ThrowIfDisposed();
            var p = _p;
            return p == null ? -1 : p->timestamp(p->@object);
        }
    }

    /// <summary>
    /// Returns the duration of the audio frame.
    /// </summary>
    /// <returns>Duration.</returns>
    public double Duration()
    {
        unsafe
        {
            var p = GetRequiredHandle();
            return p->duration(p->@object);
        }
    }

    /// <summary>
    /// Converts the audio frame to a new format.
    /// </summary>
    /// <param name="format">New sample format.</param>
    /// <param name="channels">Number of channels.</param>
    /// <param name="sampleRate">Sample rate.</param>
    /// <returns>Converted audio frame.</returns>
    public AudioFrame To(SampleFormat format, int channels, int sampleRate)
    {
        unsafe
        {
            ThrowIfDisposed();
            var p = _p;
            return p == null
                ? new AudioFrame(format, channels, sampleRate, 0)
                : new AudioFrame(p->to(p->@object, (MDK_SampleFormat)format, channels, sampleRate));
        }
    }

    /// <summary>
    /// Creates a new managed wrapper that references the same native audio frame.
    /// </summary>
    /// <returns>A referenced audio frame wrapper.</returns>
    public AudioFrame Clone()
    {
        unsafe
        {
            ThrowIfDisposed();
            var p = _p;
            return new AudioFrame(p == null ? null : Methods.mdkAudioFrameAPI_ref(p));
        }
    }

    /// <summary>
    /// Disposes the audio frame.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        unsafe
        {
            if (_owner && _p != null)
                DeleteOwned();

            _p = null;
        }

        _disposed = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe mdkAudioFrameAPI* GetRequiredHandle()
    {
        ThrowIfDisposed();
        var p = _p;
        return p == null ? ThrowInvalidFrame() : p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe bool AddBufferCore(IntPtr data, nuint size, int plane, IntPtr buffer,
        delegate* unmanaged[Cdecl]<void**, void> bufferDeleter)
    {
        var p = GetRequiredHandle();
        return p->addBuffer(p->@object, (byte*)data, size, plane, (void*)buffer, bufferDeleter) != 0;
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
    private unsafe void SetBuffersCore(byte** data, int bytesPerPlane)
    {
        var p = GetRequiredHandle();
        p->setBuffers(p->@object, data, bytesPerPlane);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void DeleteOwned()
    {
        fixed (mdkAudioFrameAPI** p = &_p)
        {
            Methods.mdkAudioFrameAPI_delete(p);
        }
    }

    [DoesNotReturn]
    private static unsafe mdkAudioFrameAPI* ThrowInvalidFrame()
    {
        throw new InvalidOperationException("Audio frame is invalid.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            ThrowDisposed();
    }

    [DoesNotReturn]
    private static void ThrowDisposed()
    {
        throw new ObjectDisposedException(nameof(AudioFrame));
    }
}

/// <summary>
/// Sample format.
/// </summary>
public enum SampleFormat
{
    Unknown = 0,
    U8,
    U8P,
    S16,
    S16P,
    S32,
    S32P,
    F32,
    F32P,
    F64,
    F64P,
};
