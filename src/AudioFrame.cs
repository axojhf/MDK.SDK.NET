using MDK.SDK.NET.Gen;
using System.Data.Common;

namespace MDK.SDK.NET;

public class AudioFrame : IDisposable
{
    private unsafe mdkAudioFrameAPI* _p;
    private bool _owner = true;

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

    public int PlaneCount()
    {
        unsafe
        {
            return _p->planeCount(_p->@object);
        }
    }

    public SampleFormat Format()
    {
        unsafe
        {
            return (SampleFormat)_p->sampleFormat(_p->@object);
        }
    }

    public int SampleRate()
    {
        unsafe
        {
            return _p->sampleRate(_p->@object);
        }
    }

    public int Channels()
    {
        unsafe
        {
            return _p->channels(_p->@object);
        }
    }

    public ulong ChannelMask()
    {
        unsafe
        {
            return _p->channelMask(_p->@object);
        }
    }

    public int SamplesPerChannel()
    {
        unsafe
        {
            return _p->samplesPerChannel(_p->@object);
        }
    }

    public unsafe bool AddBuffer(IntPtr data, nuint size, int plane = -1, IntPtr buf = 0,
        delegate* unmanaged[Cdecl]<void**, void> bufDeleter = null)
    {
        return _p->addBuffer(_p->@object, (byte*)data, size, plane, (void*)buf, bufDeleter) != 0;
    }

    public unsafe void SetBuffers(IntPtr data, int bytesPerPlane)
    {
        _p->setBuffers(_p->@object, (byte**)data, bytesPerPlane);
    }

    public IntPtr BufferData(int plane = 0)
    {
        unsafe
        {
            return (IntPtr)_p->bufferData(_p->@object, plane);
        }
    }

    public int BytesPerPlane()
    {
        unsafe
        {
            return _p->bytesPerPlane(_p->@object);
        }
    }

    public void SetTimestamp(double t)
    {
        unsafe
        {
            _p->setTimestamp(_p->@object, t);
        }
    }

    public double Timestamp()
    {
        unsafe
        {
            return _p->timestamp(_p->@object);
        }
    }

    public double Duration()
    {
        unsafe
        {
            return _p->duration(_p->@object);
        }
    }

    public AudioFrame To(SampleFormat format, int channels, int sampleRate)
    {
        unsafe
        {
            return _p == null
                ? new AudioFrame(format, channels, sampleRate, 0)
                : new AudioFrame(_p->to(_p->@object, (MDK_SampleFormat)format, channels, sampleRate));
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

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