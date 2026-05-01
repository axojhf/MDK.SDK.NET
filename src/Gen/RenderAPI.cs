using System.Runtime.CompilerServices;

namespace MDK.SDK.NET.Gen
{
    // Forward declarations for typed render API handles
    internal partial struct ID3D11DeviceContext { }
    internal partial struct ID3D12CommandQueue { }
    internal partial struct ID3D12Resource { }
    internal partial struct ID3D12GraphicsCommandList { }

    // D3D12_CPU_DESCRIPTOR_HANDLE: SIZE_T-sized struct (pointer-sized on each platform)
    internal partial struct D3D12_CPU_DESCRIPTOR_HANDLE
    {
        internal nuint ptr;
    }

    // DXGI format & resource state enums (uint-sized)
    internal enum DXGI_FORMAT : uint { }
    internal enum D3D12_RESOURCE_STATES : uint { }

    // Vulkan handle forward declarations
    internal partial struct VkInstance_T { }
    internal partial struct VkPhysicalDevice_T { }
    internal partial struct VkDevice_T { }
    internal partial struct VkQueue_T { }
    internal partial struct VkImage_T { }
    internal partial struct VkRenderPass_T { }
    internal partial struct VkImageView_T { }
    internal partial struct VkFramebuffer_T { }
    internal partial struct VkSemaphore_T { }
    internal partial struct VkCommandBuffer_T { }

    // Vulkan enum forward declarations
    internal enum VkFormat : int { }
    internal enum VkImageLayout : int { }

    internal enum MDK_RenderAPI
    {
        MDK_RenderAPI_Invalid,
        MDK_RenderAPI_OpenGL = 1,
        MDK_RenderAPI_Vulkan = 2,
        MDK_RenderAPI_Metal = 3,
        MDK_RenderAPI_D3D11 = 4,
        MDK_RenderAPI_D3D12 = 5,
    }

    internal partial struct mdkRenderAPI
    {
    }

    internal unsafe partial struct mdkGLRenderAPI
    {
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;

        internal int fbo;

        internal int unused;

        [NativeTypeName("void *(*)(const char *, void *)")]
        internal delegate* unmanaged[Cdecl]<sbyte*, void*, void*> getProcAddress;

        [NativeTypeName("void *(*)(void *)")]
        internal delegate* unmanaged[Cdecl]<void*, void*> getCurrentNativeContext;

        internal void* opaque;

        [NativeTypeName("bool")]
        internal byte debug;

        [NativeTypeName("int8_t")]
        internal sbyte egl;

        [NativeTypeName("int8_t")]
        internal sbyte opengl;

        [NativeTypeName("int8_t")]
        internal sbyte opengles;

        [NativeTypeName("uint8_t")]
        internal byte profile;

        internal float version;

        [NativeTypeName("int8_t")]
        internal sbyte depth;

        [NativeTypeName("int8_t[31]")]
        internal _reserved_e__FixedBuffer reserved;

        [InlineArray(31)]
        internal partial struct _reserved_e__FixedBuffer
        {
            internal sbyte e0;
        }
    }

    internal unsafe partial struct mdkMetalRenderAPI
    {
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;

        [NativeTypeName("const void *")]
        internal void* device;

        [NativeTypeName("const void *")]
        internal void* cmdQueue;

        [NativeTypeName("const void *")]
        internal void* texture;

        [NativeTypeName("const void *")]
        internal void* opaque;

        [NativeTypeName("const void *(*)(const void *)")]
        internal delegate* unmanaged[Cdecl]<void*, void*> currentRenderTarget;

        [NativeTypeName("const void *")]
        internal void* layer;

        [NativeTypeName("void (*)(const void **, const void **, const void *)")]
        internal delegate* unmanaged[Cdecl]<void**, void**, void*, void> currentCommand;

        internal int device_index;

        [NativeTypeName("unsigned int")]
        internal uint colorFormat;

        [NativeTypeName("unsigned int")]
        internal uint depthStencilFormat;
    }

    internal unsafe partial struct mdkD3D11RenderAPI
    {
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;

        internal ID3D11DeviceContext* context;

        internal ID3D11DeviceChild* rtv;

        [NativeTypeName("void *[2]")]
        internal _reserved_e__FixedBuffer reserved;

        [NativeTypeName("bool")]
        internal byte debug;

        internal int buffers;

        internal int adapter;

        internal float feature_level;

        [NativeTypeName("const char *")]
        internal sbyte* vendor;

        internal unsafe partial struct _reserved_e__FixedBuffer
        {
            internal void* e0;
            internal void* e1;

            internal ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    internal unsafe partial struct mdkD3D12RenderAPI
    {
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;

        internal ID3D12CommandQueue* cmdQueue;

        internal ID3D12Resource* rt;

        internal D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle;

        internal DXGI_FORMAT colorFormat;

        internal DXGI_FORMAT depthStencilFormat;

        [NativeTypeName("const void *")]
        internal void* opaque;

        [NativeTypeName("ID3D12Resource *(*)(const void *, UINT *, UINT *, D3D12_RESOURCE_STATES *)")]
        internal delegate* unmanaged[Cdecl]<void*, uint*, uint*, D3D12_RESOURCE_STATES*, ID3D12Resource*> currentRenderTarget;

        [NativeTypeName("ID3D12GraphicsCommandList *(*)(const void *)")]
        internal delegate* unmanaged[Cdecl]<void*, ID3D12GraphicsCommandList*> currentCommandList;

        [NativeTypeName("void *[1]")]
        internal _reserved2_e__FixedBuffer reserved2;

        [NativeTypeName("bool")]
        internal byte debug;

        internal int buffers;

        internal int adapter;

        internal float feature_level;

        [NativeTypeName("const char *")]
        internal sbyte* vendor;

        internal unsafe partial struct _reserved2_e__FixedBuffer
        {
            internal void* e0;

            internal ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    internal unsafe partial struct mdkVulkanRenderAPI
    {
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;

        [NativeTypeName("VkInstance")]
        internal VkInstance_T* instance;

        [NativeTypeName("VkPhysicalDevice")]
        internal VkPhysicalDevice_T* phy_device;

        [NativeTypeName("VkDevice")]
        internal VkDevice_T* device;

        [NativeTypeName("VkQueue")]
        internal VkQueue_T* graphics_queue;

        [NativeTypeName("VkImage")]
        internal VkImage_T* rt;

        [NativeTypeName("VkRenderPass")]
        internal VkRenderPass_T* render_pass;

        internal void* opaque;

        [NativeTypeName("int (*)(void *, int *, int *, VkFormat *, VkImageLayout *)")]
        internal delegate* unmanaged[Cdecl]<void*, int*, int*, VkFormat*, VkImageLayout*, int> renderTargetInfo;

        [NativeTypeName("int (*)(void *, VkImageView *, VkFramebuffer *, VkSemaphore *)")]
        internal delegate* unmanaged[Cdecl]<void*, VkImageView_T**, VkFramebuffer_T**, VkSemaphore_T**, int> beginFrame;

        [NativeTypeName("VkCommandBuffer (*)(void *)")]
        internal delegate* unmanaged[Cdecl]<void*, VkCommandBuffer_T*> currentCommandBuffer;

        [NativeTypeName("void (*)(void *, VkSemaphore *)")]
        internal delegate* unmanaged[Cdecl]<void*, VkSemaphore_T**, void> endFrame;

        [NativeTypeName("void *[2]")]
        internal _reserved_e__FixedBuffer reserved;

        internal int graphics_family;

        internal int compute_family;

        internal int transfer_family;

        internal int present_family;

        [NativeTypeName("bool")]
        internal byte debug;

        [NativeTypeName("uint8_t")]
        internal byte buffers;

        internal int device_index;

        [NativeTypeName("uint32_t")]
        internal uint max_version;

        internal int gfx_queue_index;

        internal int transfer_queue_index;

        internal int compute_queue_index;

        internal int depth;

        [NativeTypeName("uint8_t[32]")]
        internal _reserved_opt_e__FixedBuffer reserved_opt;

        internal unsafe partial struct _reserved_e__FixedBuffer
        {
            internal void* e0;
            internal void* e1;

            internal ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }

        [InlineArray(32)]
        internal partial struct _reserved_opt_e__FixedBuffer
        {
            internal byte e0;
        }
    }
}
