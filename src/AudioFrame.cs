using MDK.SDK.NET.Gen;
using System.Data.Common;

namespace MDK.SDK.NET;

/// <summary>
/// Audio frame.
/// </summary>
public class AudioFrame : IDisposable
{
    private unsafe mdkAudioFrameAPI* _p;
    private bool _owner = true;

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
        _p = Methods.mdkAudioFrameAPI_ref(pp);
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
                return _p != null;
            }
        }
    }

    internal unsafe void Attach(mdkAudioFrameAPI* api)
    {
        if (_owner)
            fixed (mdkAudioFrameAPI** p = &_p)
                Methods.mdkAudioFrameAPI_delete(p);
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
            return _p->planeCount(_p->@object);
        }
    }

    /// <summary>
    /// Returns the sample format of the audio frame.
    /// </summary>
    public SampleFormat Format()
    {
        unsafe
        {
            return (SampleFormat)_p->sampleFormat(_p->@object);
        }
    }

    /// <summary>
    /// Returns the sample rate of the audio frame.
    /// </summary>
    public int SampleRate()
    {
        unsafe
        {
            return _p->sampleRate(_p->@object);
        }
    }

    /// <summary>
    /// Returns the number of channels in the audio frame.
    /// </summary>
    public int Channels()
    {
        unsafe
        {
            return _p->channels(_p->@object);
        }
    }

    /// <summary>
    /// Returns the channel mask of the audio frame.
    /// </summary>
    public ulong ChannelMask()
    {
        unsafe
        {
            return _p->channelMask(_p->@object);
        }
    }

    /// <summary>
    /// Returns the number of samples per channel in the audio frame.
    /// </summary>
    public int SamplesPerChannel()
    {
        unsafe
        {
            return _p->samplesPerChannel(_p->@object);
        }
    }

    /// <summary>
    /// Adds a buffer to the audio frame.
    /// </summary>
    /// <param name="data">Pointer to the data.</param>
    /// <param name="size">Size of the data.</param>
    /// <param name="plane">Plane index.</param>
    /// <param name="buf">Pointer to the buffer.</param>
    /// <param name="bufDeleter">Deleter function for the buffer.</param>
    public unsafe bool AddBuffer(IntPtr data, nuint size, int plane = -1, IntPtr buf = 0,
        delegate* unmanaged[Cdecl]<void**, void> bufDeleter = null)
    {
        return _p->addBuffer(_p->@object, (byte*)data, size, plane, (void*)buf, bufDeleter) != 0;
    }

    /// <summary>
    /// Sets the buffers for the audio frame.
    /// </summary>
    /// <param name="data">Pointer to the data.</param>
    /// <param name="bytesPerPlane">Bytes per plane.</param>
    public unsafe void SetBuffers(IntPtr data, int bytesPerPlane)
    {
        _p->setBuffers(_p->@object, (byte**)data, bytesPerPlane);
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
            return (IntPtr)_p->bufferData(_p->@object, plane);
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
            return _p->bytesPerPlane(_p->@object);
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
            _p->setTimestamp(_p->@object, t);
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
            return _p->timestamp(_p->@object);
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
            return _p->duration(_p->@object);
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
            return _p == null
                ? new AudioFrame(format, channels, sampleRate, 0)
                : new AudioFrame(_p->to(_p->@object, (MDK_SampleFormat)format, channels, sampleRate));
        }
    }

    /// <summary>
    /// Disposes the audio frame.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    /// <summary>
    /// Disposes the audio frame.
    /// </summary>
    /// <param name="disposing">Whether the audio frame is being disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            unsafe
            {
                if (_owner)
                    fixed (mdkAudioFrameAPI** p = &_p)
                        Methods.mdkAudioFrameAPI_delete(p);
            }
        }
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