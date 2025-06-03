using MDK.SDK.NET.Gen;
using System.Runtime.InteropServices;

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

/// <summary>
/// OpenGL RenderAPI
/// </summary>
public struct GLRenderAPI : IRenderAPI
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
    /// optional. can be null and then standard gl libraries will be searched.<br/>
    /// if not null, it's used to load gl functions<br/>
    /// void* (*getProcAddress)(const char* name, void* opaque);
    /// </summary>
    public IntPtr GetProcAddress
    {
        get { unsafe { return (nint)internalAPI.getProcAddress; } }
        set { unsafe { internalAPI.getProcAddress = (delegate* unmanaged[Cdecl]<sbyte*, void*, void*>)value; } }
    }

    /// <summary>
    /// optional. getProcAddress user data, e.g. a gl context handle.<br/>
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

    /// <summary>
    /// optional. can be null and then standard gl libraries will be searched.<br/>
    /// if not null, it's used to load gl functions<br/>
    /// </summary>
    /// <param name="name">gl function name</param>
    /// <param name="opaque">user data, e.g. gl context handle</param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint GetProcAddressCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr opaque);

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

    /// <summary>
    /// Get RenderAPI Ptr For Player.SetRenderAPI()
    /// </summary>
    /// <returns></returns>
    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }

    /// <summary>
    /// Pins the internal API and returns a disposable GCHandle.
    /// </summary>
    /// <returns>A disposable GCHandle.</returns>
    public readonly DisposableGCHandle Pin()
    {
        return new DisposableGCHandle(internalAPI, GCHandleType.Pinned);
    }
}

/// <summary>
/// D3D11 RenderAPI
/// </summary>
public struct D3D11RenderAPI : IRenderAPI
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
            internalAPI.type = MDK_RenderAPI.MDK_RenderAPI_D3D11;
            internalAPI.context = (void*)0;
            internalAPI.rtv = (void*)0;
            internalAPI.debug = 0;
            internalAPI.buffers = 2;
            internalAPI.adapter = 0;
            internalAPI.feature_level = 0;
            internalAPI.vendor = (sbyte*)0;
        }
    }

    /// <summary>
    /// Get RenderAPI Ptr For Player.SetRenderAPI()
    /// </summary>
    /// <returns></returns>
    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }

    /// <summary>
    /// Pins the internal API and returns a disposable GCHandle.
    /// </summary>
    /// <returns>A disposable GCHandle.</returns>
    public readonly DisposableGCHandle Pin()
    {
        return new DisposableGCHandle(internalAPI, GCHandleType.Pinned);
    }
}

/// <summary>
/// Metal RenderAPI
/// </summary>
public struct MetalRenderAPI : IRenderAPI
{
    public RenderType Type { get { return (RenderType)internalAPI.type; } }

    // Render Context Resources. Foreign context (provided by user) only

    /// <summary>
    /// MUST set if metal is provided by user
    /// </summary>
    public IntPtr Device
    {
        get { unsafe { return (nint)internalAPI.device; } }
        set { unsafe { internalAPI.device = (void*)value; } }
    }

    /// <summary>
    /// optional. if not null, device can be null. currentQueue callback to share the same command buffer?
    /// </summary>
    public IntPtr CmdQueue
    {
        get { unsafe { return (nint)internalAPI.cmdQueue; } }
        set { unsafe { internalAPI.cmdQueue = (void*)value; } }
    }

    // one of texture and currentRenderTarget MUST be set if metal is provided by user

    /// <summary>
    /// optional. id &lt; MTLTexture &gt; . if not null, device can be null. usually for offscreen rendering. render target for MTLRenderPassDescriptor if encoder is not provided by user. set once for offscreen rendering
    /// </summary>
    public IntPtr Texture
    {
        get { unsafe { return (nint)internalAPI.texture; } }
        set { unsafe { internalAPI.texture = (void*)value; } }
    }

    /// <summary>
    /// optional. callback opaque
    /// </summary>
    public IntPtr Opaque
    {
        get { unsafe { return (nint)internalAPI.opaque; } }
        set { unsafe { internalAPI.opaque = (void*)value; } }
    }

    /// <summary>
    /// optional. usually for on screen rendering. return id MTLTexture .
    /// </summary>
    public IntPtr CurrentRenderTarget
    {
        get { unsafe { return (nint)internalAPI.currentRenderTarget; } }
        set { unsafe { internalAPI.currentRenderTarget = (delegate* unmanaged[Cdecl]<void*, void*>)value; } }
    }

    // no encoder because we need own render pass

    /// <summary>
    /// optional. CAMetalLayer only used for appling colorspace parameters for hdr/sdr videos.
    /// </summary>
    public IntPtr Layer
    {
        get { unsafe { return (nint)internalAPI.layer; } }
        set { unsafe { internalAPI.layer = (void*)value; } }
    }

    /// <summary>
    /// Get current MTLRenderCommandEncoder and MTLCommandBuffer. required if texture and currentRenderTarget are null. useful in an foreign render pass
    /// </summary>
    public IntPtr CurrentCommand
    {
        get { unsafe { return (nint)internalAPI.currentCommand; } }
        set { unsafe { internalAPI.currentCommand = (delegate* unmanaged[Cdecl]<void**, void**, void*, void>)value; } }
    }

    /// <summary>
    ///  -1 will use system default device. callback with index+name?
    /// </summary>
    public int DeviceIndex
    {
        get { unsafe { return internalAPI.device_index; } }
        set { unsafe { internalAPI.device_index = value; } }
    }

    uint ColorFormat
    {
        get { unsafe { return internalAPI.colorFormat; } }
        set { unsafe { internalAPI.colorFormat = value; } }
    }

    uint DepthStencilFormat
    {
        get { unsafe { return internalAPI.depthStencilFormat; } }
        set { unsafe { internalAPI.depthStencilFormat = value; } }
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
            internalAPI.currentCommand = (delegate* unmanaged[Cdecl]<void**, void**, void*, void>)0;
            internalAPI.device_index = -1;
            internalAPI.colorFormat = 0;
            internalAPI.depthStencilFormat = 0;
        }
    }

    /// <summary>
    /// Get RenderAPI Ptr For Player.SetRenderAPI()
    /// </summary>
    /// <returns></returns>
    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }

    /// <summary>
    /// Pins the internal API and returns a disposable GCHandle.
    /// </summary>
    /// <returns>A disposable GCHandle.</returns>
    public readonly DisposableGCHandle Pin()
    {
        return new DisposableGCHandle(internalAPI, GCHandleType.Pinned);
    }
}

/// <summary>
/// Vulkan RenderAPI
/// </summary>
public struct VulkanRenderAPI : IRenderAPI
{
    private mdkVulkanRenderAPI internalAPI;
    public RenderType Type { get { return (RenderType)internalAPI.type; } }

    // Set by user and used internally even if device is provided by user

    /// <summary>
    /// OPTIONAL. shared instance. for internal created context but not foreign context, to load instance extensions
    /// </summary>
    public IntPtr Instance
    {
        get { unsafe { return (nint)internalAPI.instance; } }
        set { unsafe { internalAPI.instance = (void*)value; } }
    }

    /// <summary>
    /// Optional to create internal context. MUST not null for foreign context. Must set if logical device is provided to create internal context.
    /// </summary>
    public IntPtr PhyDevice
    {
        get { unsafe { return (nint)internalAPI.phy_device; } }
        set { unsafe { internalAPI.phy_device = (void*)value; } }
    }

    /// <summary>
    /// Optional to create internal context as shared device. Required for foreign context.
    /// </summary>
    public IntPtr Device
    {
        get { unsafe { return (nint)internalAPI.device; } }
        set { unsafe { internalAPI.device = (void*)value; } }
    }

    /// <summary>
    /// OPTIONAL. If null, will use gfx_queue_index. NOT required if vk is create internally
    /// </summary>
    public IntPtr GraphicsQueue
    {
        get { unsafe { return (nint)internalAPI.graphics_queue; } }
        set { unsafe { internalAPI.graphics_queue = (void*)value; } }
    }

    /// <summary>
    /// VkImage? so can use qrhitexture.nativeTexture().object
    /// </summary>
    public IntPtr Rt
    {
        get { unsafe { return (nint)internalAPI.rt; } }
        set { unsafe { internalAPI.rt = (void*)value; } }
    }

    /// <summary>
    /// optional. If null(usually for offscreen rendering), final image layout is VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL/VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL_KHR
    /// </summary>
    public IntPtr RenderPass
    {
        get { unsafe { return (nint)internalAPI.render_pass; } }
        set { unsafe { internalAPI.render_pass = (void*)value; } }
    }

    public IntPtr Opaque
    {
        get { unsafe { return (nint)internalAPI.opaque; } }
        set { unsafe { internalAPI.opaque = (void*)value; } }
    }

    /// <summary>
    /// Get render target image size
    /// </summary>
    public IntPtr RenderTargetInfo
    {
        get { unsafe { return (nint)internalAPI.renderTargetInfo; } }
        set { unsafe { internalAPI.renderTargetInfo = (delegate* unmanaged[Cdecl]<void*, int*, int*, void*, void*, int>)value; } }
    }

    /// <summary>
    /// Optional. Can be null(or not) for offscreen rendering if rt is not null.
    /// MUST be paired with endFrame()
    /// </summary>
    public IntPtr BeginFrame
    {
        get { unsafe { return (nint)internalAPI.beginFrame; } }
        set { unsafe { internalAPI.beginFrame = (delegate* unmanaged[Cdecl]<void*, void*, void*, void*, int>)value; } }
    }

    /// <summary>
    /// if null, create pool internally(RTT)
    /// </summary>
    public IntPtr CurrentCommandBuffer
    {
        get { unsafe { return (nint)internalAPI.currentCommandBuffer; } }
        set { unsafe { internalAPI.currentCommandBuffer = (delegate* unmanaged[Cdecl]<void*, void*>)value; } }
    }

    /// <summary>
    /// Optional. If null, frame is guaranteed to be rendered to image before executing the next command buffer in user code.
    /// If not null, user can wait for drawSem before using the image.
    /// MUST be paired with beginFrame()
    /// </summary>
    public IntPtr EndFrame
    {
        get { unsafe { return (nint)internalAPI.endFrame; } }
        set { unsafe { internalAPI.endFrame = (delegate* unmanaged[Cdecl]<void*, void*, void>)value; } }
    }

    /// <summary>
    /// MUST if graphics and transfer queue family are different
    /// </summary>
    public int GraphicsFamily
    {
        get { unsafe { return internalAPI.graphics_family; } }
        set { unsafe { internalAPI.graphics_family = value; } }
    }

    /// <summary>
    /// optional. it's graphics_family if not set
    /// </summary>
    public int ComputeFamily
    {
        get { unsafe { return internalAPI.compute_family; } }
        set { unsafe { internalAPI.compute_family = value; } }
    }

    /// <summary>
    /// optional. it's graphics_family if not set
    /// </summary>
    public int TransferFamily
    {
        get { unsafe { return internalAPI.transfer_family; } }
        set { unsafe { internalAPI.transfer_family = value; } }
    }

    /// <summary>
    /// optional. Must set if logical device is provided to create internal context
    /// </summary>
    public int PresentFamily
    {
        get { unsafe { return internalAPI.present_family; } }
        set { unsafe { internalAPI.present_family = value; } }
    }
    /// <summary>
    /// Render Context Creation Options.
    /// as input, they are desired values to create an internal context(ignored if context is provided by user). as output, they are result values(if context is not provided by user)
    /// </summary>
    public bool Debug
    {
        get
        {
            unsafe
            {
                fixed (byte* p = &internalAPI.debug)
                    return *(bool*)p;
            }
        }
        set { unsafe { internalAPI.debug = *(byte*)&value; } }
    }

    /// <summary>
    /// 2 for double-buffering
    /// </summary>
    public byte Buffers
    {
        get { unsafe { return internalAPI.buffers; } }
        set { unsafe { internalAPI.buffers = value; } }
    }

    /// <summary>
    /// -1: dGPU &gt; iGPU &gt; vGPU &gt; software &gt; others. &gt;= 0: index
    /// </summary>
    public int DeviceIndex
    {
        get { unsafe { return internalAPI.device_index; } }
        set { unsafe { internalAPI.device_index = value; } }
    }

    /// <summary>
    /// requires vulkan 1.1
    /// </summary>
    public uint MaxVersion
    {
        get { unsafe { return internalAPI.max_version; } }
        set { unsafe { internalAPI.max_version = value; } }
    }

    /// <summary>
    /// OPTIONAL
    /// </summary>
    public int GfxQueueIndex
    {
        get { unsafe { return internalAPI.gfx_queue_index; } }
        set { unsafe { internalAPI.gfx_queue_index = value; } }
    }

    /// <summary>
    /// OPTIONAL. if not set, will use gfx queue
    /// </summary>
    public int TransferQueueIndex
    {
        get { unsafe { return internalAPI.transfer_queue_index; } }
        set { unsafe { internalAPI.transfer_queue_index = value; } }
    }

    /// <summary>
    /// OPTIONAL. if not set, will use gfx queue
    /// </summary>
    public int ComputeQueueIndex
    {
        get { unsafe { return internalAPI.compute_queue_index; } }
        set { unsafe { internalAPI.compute_queue_index = value; } }
    }

    /// <summary>
    /// Default 8
    /// </summary>
    public int Depth
    {
        get { unsafe { return internalAPI.depth; } }
        set { unsafe { internalAPI.depth = value; } }
    }

    public VulkanRenderAPI()
    {
        unsafe
        {
            internalAPI.type = MDK_RenderAPI.MDK_RenderAPI_Vulkan;
            internalAPI.instance = (void*)0;
            internalAPI.phy_device = (void*)0;
            internalAPI.device = (void*)0;
            internalAPI.graphics_queue = (void*)0;
            internalAPI.rt = (void*)0;
            internalAPI.render_pass = (void*)0;
            internalAPI.opaque = (void*)0;
            internalAPI.renderTargetInfo = (delegate* unmanaged[Cdecl]<void*, int*, int*, void*, void*, int>)0;
            internalAPI.beginFrame = (delegate* unmanaged[Cdecl]<void*, void*, void*, void*, int>)0;
            internalAPI.currentCommandBuffer = (delegate* unmanaged[Cdecl]<void*, void*>)0;
            internalAPI.endFrame = (delegate* unmanaged[Cdecl]<void*, void*, void>)0;
            internalAPI.graphics_family = -1;
            internalAPI.compute_family = -1;
            internalAPI.transfer_family = -1;
            internalAPI.present_family = -1;
            internalAPI.debug = 0;
            internalAPI.buffers = 2;
            internalAPI.device_index = 0;
            internalAPI.max_version = 0;
            internalAPI.gfx_queue_index = 0;
            internalAPI.transfer_queue_index = -1;
            internalAPI.compute_queue_index = -1;
            internalAPI.depth = 8;
        }
    }

    /// <summary>
    /// Get RenderAPI Ptr For Player.SetRenderAPI()
    /// </summary>
    /// <returns></returns>
    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }

    /// <summary>
    /// Pins the internal API and returns a disposable GCHandle.
    /// </summary>
    /// <returns>A disposable GCHandle.</returns>
    public readonly DisposableGCHandle Pin()
    {
        return new DisposableGCHandle(internalAPI, GCHandleType.Pinned);
    }
}

/// <summary>
/// D3D12 RenderAPI
/// </summary>
public struct D3D12RenderAPI : IRenderAPI
{
    public RenderType Type { get { return (RenderType)internalAPI.type; } }

    private mdkD3D12RenderAPI internalAPI;

    /// <summary>
    /// optional. will create an internal queue if null.
    /// </summary>
    public IntPtr CmdQueue
    {
        get { unsafe { return (nint)internalAPI.cmdQueue; } }
        set { unsafe { internalAPI.cmdQueue = (void*)value; } }
    }

    /// <summary>
    /// optional. the render target
    /// </summary>
    public IntPtr Rt
    {
        get { unsafe { return (nint)internalAPI.rt; } }
        set { unsafe { internalAPI.rt = (void*)value; } }
    }

    /// <summary>
    /// optional
    /// </summary>
    public IntPtr RtvHandle
    {
        get { unsafe { return (nint)internalAPI.rtvHandle; } }
        set { unsafe { internalAPI.rtvHandle = (void*)value; } }
    }

    public uint ColorFormat
    {
        get { unsafe { return internalAPI.colorFormat; } }
        set { unsafe { internalAPI.colorFormat = value; } }
    }

    public uint DepthStencilFormat
    {
        get { unsafe { return internalAPI.depthStencilFormat; } }
        set { unsafe { internalAPI.depthStencilFormat = value; } }
    }

    /// <summary>
    /// optional. callback opaque
    /// </summary>
    public IntPtr Opaque
    {
        get { unsafe { return (nint)internalAPI.opaque; } }
        set { unsafe { internalAPI.opaque = (void*)value; } }
    }

    /// <summary>
    /// optional. usually for on screen rendering.
    /// </summary>
    public IntPtr CurrentRenderTarget
    {
        get { unsafe { return (nint)internalAPI.currentRenderTarget; } }
        set { unsafe { internalAPI.currentRenderTarget = (delegate* unmanaged[Cdecl]<void*, uint*, uint*, void*, void*>)value; } }
    }

    /// <summary>
    /// optional. will use an internal command list if null. if not null, can be used by on screen rendering
    /// </summary>
    public IntPtr CurrentCommandList
    {
        get { unsafe { return (nint)internalAPI.currentCommandList; } }
        set { unsafe { internalAPI.currentCommandList = (delegate* unmanaged[Cdecl]<void*, void*>)value; } }
    }


    public bool Debug
    {
        get
        {
            unsafe
            {
                fixed (byte* p = &internalAPI.debug)
                    return *(bool*)p;
            }
        }
        set { unsafe { internalAPI.debug = *(byte*)&value; } }
    }

    /// <summary>
    /// must >= 2.
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

    public D3D12RenderAPI()
    {
        unsafe
        {
            internalAPI.type = MDK_RenderAPI.MDK_RenderAPI_D3D12;
            internalAPI.cmdQueue = (void*)0;
            internalAPI.rt = (void*)0;
            internalAPI.rtvHandle = (void*)0;
            internalAPI.colorFormat = 0; //DXGI_FORMAT_UNKNOWN
            internalAPI.depthStencilFormat = 0; //DXGI_FORMAT_UNKNOWN
            internalAPI.opaque = (void*)0;
            internalAPI.currentRenderTarget = (delegate* unmanaged[Cdecl]<void*, uint*, uint*, void*, void*>)0;
            internalAPI.currentCommandList = (delegate* unmanaged[Cdecl]<void*, void*>)0;
            internalAPI.debug = 0;
            internalAPI.buffers = 2;
            internalAPI.adapter = 0;
            internalAPI.feature_level = 0;
            internalAPI.vendor = (sbyte*)0;
        }
    }

    /// <summary>
    /// Get RenderAPI Ptr For Player.SetRenderAPI()
    /// </summary>
    /// <returns></returns>
    public IntPtr GetPtr()
    {
        unsafe
        {
            fixed (void* ptr = &internalAPI)
                return (nint)ptr;
        }
    }

    /// <summary>
    /// Pins the internal API and returns a disposable GCHandle.
    /// </summary>
    /// <returns>A disposable GCHandle.</returns>
    public readonly DisposableGCHandle Pin()
    {
        return new DisposableGCHandle(internalAPI, GCHandleType.Pinned);
    }
}

/// <summary>
/// Disposable GCHandle.
/// </summary>
public sealed class DisposableGCHandle : IDisposable
{
    private GCHandle _handle;

    /// <summary>
    /// Constructs a disposable GCHandle for the given target object.
    /// </summary>
    /// <param name="target">The object to pin.</param>
    /// <param name="type">The type of handle to create.</param>
    public DisposableGCHandle(object target, GCHandleType type = GCHandleType.Normal)
    {
        _handle = GCHandle.Alloc(target, type);
    }

    /// <summary>
    /// Returns the address of the pinned object.
    /// </summary>
    /// <returns>The address of the pinned object.</returns>
    public IntPtr AddrOfPinnedObject()
    {
        return _handle.AddrOfPinnedObject();
    }

    /// <summary>
    /// Gets the target object.
    /// </summary>
    /// <returns>The target object.</returns>
    public object? Target => _handle.Target;

    /// <summary>
    /// Disposes the GCHandle.
    /// </summary>
    public void Dispose()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
        GC.SuppressFinalize(this);
    }
}


/// <summary>
/// Interface for RenderAPI structures that can be pinned in memory
/// </summary>
public interface IRenderAPI
{
    /// <summary>
    /// Get RenderAPI Ptr For Player.SetRenderAPI()
    /// </summary>
    /// <returns></returns>
    [Obsolete("Use Pin() instead")]
    IntPtr GetPtr();

    /// <summary>
    /// Pin the structure in memory and return a GCHandle
    /// </summary>
    /// <returns>GCHandle that can be used to get the pinned object address</returns>
    DisposableGCHandle Pin();
}

