using MDK.SDK.NET.Gen;

namespace MDK.SDK.NET;

public enum RenderType
{
    Invalid,
    OpenGL = 1,
    Vulkan = 2,
    Metal = 3,
    D3D11 = 4,
    D3D12 = 5,
}

public class RenderAPI
{
    public RenderType Type { get { return (RenderType)internalAPI.type; } }
    private mdkRenderAPI internalAPI;
    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }
}

public class GLRenderAPI : RenderAPI
{
    /// <summary>
    /// if >=0, will draw in given fbo. no need to bind in user code
    /// </summary>
    public int Fbo
    {
        get { unsafe { return internalAPI.fbo; } }
        set { unsafe { internalAPI.fbo = value; } }
    }

    /// <summary>
    /// NOT IMPLENETED<br/>
    /// void* (*getProcAddress)(const char* name, void* opaque);
    /// </summary>
    public IntPtr GetProcAddress
    {
        get { unsafe { return (nint)internalAPI.getProcAddress; } }
        set { unsafe { internalAPI.getProcAddress = (delegate* unmanaged[Cdecl]<sbyte*, void*, void*>)value; } }
    }

    /// <summary>
    /// NOT IMPLENETED<br/>
    /// void* (*getCurrentNativeContext)(void* opaque);
    /// </summary>
    public IntPtr GetCurrentNativeContext
    {
        get { unsafe { return (nint)internalAPI.getCurrentNativeContext; } }
        set { unsafe { internalAPI.getCurrentNativeContext = (delegate* unmanaged[Cdecl]<void*, void*>)value; } }
    }

    /// <summary>
    /// NOT IMPLENETED
    /// </summary>
    public IntPtr Opaque
    {
        get { unsafe { return (nint)internalAPI.opaque; } }
        set { unsafe { internalAPI.opaque = (void*)value; } }
    }

    /// <summary>
    /// default false. NOT IMPLENETED
    /// </summary>
    public byte Debug
    {
        get { unsafe { return internalAPI.debug; } }
        set { unsafe { internalAPI.debug = value; } }
    }

    /// <summary>
    /// default -1. -1: auto. 0: no, 1: yes
    /// </summary>
    public sbyte Egl
    {
        get { unsafe { return internalAPI.egl; } }
        set { unsafe { internalAPI.egl = value; } }
    }

    /// <summary>
    /// default -1. -1: auto. 0: no, 1: yes
    /// </summary>
    public sbyte Opengl
    {
        get { unsafe { return internalAPI.opengl; } }
        set { unsafe { internalAPI.opengl = value; } }
    }

    /// <summary>
    /// default -1. -1: auto. 0: no, 1: yes
    /// </summary>
    public sbyte Opengles
    {
        get { unsafe { return internalAPI.opengles; } }
        set { unsafe { internalAPI.opengles = value; } }
    }

    /// <summary>
    /// default 3. 0: no profile, 1: core profile, 2: compatibility profile
    /// </summary>
    public byte Profile
    {
        get { unsafe { return internalAPI.profile; } }
        set { unsafe { internalAPI.profile = value; } }
    }

    /// <summary>
    /// default 0, ignored if &lt; 2.0. requested version major.minor. result version may &lt; requested version if not supported
    /// </summary>
    public float Version
    {
        get { unsafe { return internalAPI.version; } }
        set { unsafe { internalAPI.version = value; } }
    }

    private mdkGLRenderAPI internalAPI = default;
    public GLRenderAPI()
    {
        unsafe
        {
            internalAPI.type = MDK_RenderAPI.MDK_RenderAPI_OpenGL;
            internalAPI.fbo = -1;
            internalAPI.getProcAddress = (delegate* unmanaged[Cdecl]<sbyte*, void*, void*>)0;
            internalAPI.getCurrentNativeContext = (delegate* unmanaged[Cdecl]<void*, void*>)0;
            internalAPI.opaque = (void*)0;
            internalAPI.debug = 0;
            internalAPI.egl = -1;
            internalAPI.opengl = -1;
            internalAPI.opengles = -1;
            internalAPI.profile = 3;
            internalAPI.version = 0;
        }
    }
}

struct D3D11RenderAPI
{
    public readonly RenderType Type { get { return (RenderType)internalAPI.type; } }

    /// <summary>
    /// context and rtv can be set by user if user can provide. then rendering becomes foreign context mode.<br/>
    /// if rtv is not null, no need to set context
    /// </summary>
    public IntPtr Context
    {
        get { unsafe { return (nint)internalAPI.context; } }
        set { unsafe { internalAPI.context = (void*)value; } }
    }

    /// <summary>
    /// rtv or texture. usually user can provide a texture from gui easly, no d3d code to create a view.<br/>
    /// optional. the render target(view). ID3D11RenderTargetView or ID3D11Texture2D. <br/>
    /// can be null if context is not null. if not null, no need to set context
    /// </summary>
    public IntPtr Rtv
    {
        get { unsafe { return (nint)internalAPI.rtv; } }
        set { unsafe { internalAPI.rtv = (void*)value; } }
    }

    /// <summary>
    /// Render Context Creation Options.<br/>
    /// as input, they are desired values to create an internal context(ignored if context is provided by user). <br/>
    /// as output, they are result values(if context is not provided by user)
    /// </summary>
    public byte Debug
    {
        get { unsafe { return internalAPI.debug; } }
        set { unsafe { internalAPI.debug = value; } }
    }

    /// <summary>
    /// UWP must >= 2.
    /// </summary>
    public int Buffers
    {
        get { unsafe { return internalAPI.buffers; } }
        set { unsafe { internalAPI.buffers = value; } }
    }

    /// <summary>
    /// adapter index
    /// </summary>
    public int Adapter
    {
        get { unsafe { return internalAPI.adapter; } }
        set { unsafe { internalAPI.adapter = value; } }
    }

    /// <summary>
    /// 0 is the highest
    /// </summary>
    public float FeatureLevel
    {
        get { unsafe { return internalAPI.feature_level; } }
        set { unsafe { internalAPI.feature_level = value; } }
    }

    /// <summary>
    /// gpu vendor name
    /// </summary>
    public IntPtr Vendor
    {
        get { unsafe { return (nint)internalAPI.vendor; } }
        set { unsafe { internalAPI.vendor = (sbyte*)value; } }
    }

    private mdkD3D11RenderAPI internalAPI;

    public D3D11RenderAPI()
    {
        unsafe
        {
            internalAPI.type = (MDK_RenderAPI)Type;
            internalAPI.context = (void*)0;
            internalAPI.rtv = (void*)0;
            internalAPI.debug = 0;
            internalAPI.buffers = 2;
            internalAPI.adapter = 0;
            internalAPI.feature_level = 0;
            internalAPI.vendor = (sbyte*)0;
        }
    }

    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }
}


struct MetalRenderAPI
{
    public RenderType Type { get { return (RenderType)internalAPI.type; } }

    public IntPtr Device
    {
        get { unsafe { return (nint)internalAPI.device; } }
        set { unsafe { internalAPI.device = (void*)value; } }
    }

    public IntPtr CmdQueue
    {
        get { unsafe { return (nint)internalAPI.cmdQueue; } }
        set { unsafe { internalAPI.cmdQueue = (void*)value; } }
    }

    public IntPtr Texture
    {
        get { unsafe { return (nint)internalAPI.texture; } }
        set { unsafe { internalAPI.texture = (void*)value; } }
    }

    public IntPtr Opaque
    {
        get { unsafe { return (nint)internalAPI.opaque; } }
        set { unsafe { internalAPI.opaque = (void*)value; } }
    }

    public IntPtr CurrentRenderTarget
    {
        get { unsafe { return (nint)internalAPI.currentRenderTarget; } }
        set { unsafe { internalAPI.currentRenderTarget = (delegate* unmanaged[Cdecl]<void*, void*>)value; } }
    }

    public IntPtr Layer
    {
        get { unsafe { return (nint)internalAPI.layer; } }
        set { unsafe { internalAPI.layer = (void*)value; } }
    }

    public int DeviceIndex
    {
        get { unsafe { return internalAPI.device_index; } }
        set { unsafe { internalAPI.device_index = value; } }
    }

    private mdkMetalRenderAPI internalAPI;

    public MetalRenderAPI()
    {
        unsafe
        {
            internalAPI.type = MDK_RenderAPI.MDK_RenderAPI_Metal;
            internalAPI.device = (void*)0;
            internalAPI.cmdQueue = (void*)0;
            internalAPI.texture = (void*)0;
            internalAPI.opaque = (void*)0;
            internalAPI.currentRenderTarget = (delegate* unmanaged[Cdecl]<void*, void*>)0;
            internalAPI.layer = (void*)0;
            internalAPI.device_index = -1;
        }
    }

    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }
}

