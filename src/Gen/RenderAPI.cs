namespace MDK.SDK.NET.Gen
{
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
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;
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

        [NativeTypeName("int8_t[32]")]
        internal fixed sbyte reserved[32];
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

        [NativeTypeName("const void *[1]")]
        internal _reserved_e__FixedBuffer reserved;

        internal int device_index;

        internal unsafe partial struct _reserved_e__FixedBuffer
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
        public MDK_RenderAPI type;

        [NativeTypeName("VkInstance")]
        public void* instance;

        [NativeTypeName("VkPhysicalDevice")]
        public void* phy_device;

        [NativeTypeName("VkDevice")]
        public void* device;

        [NativeTypeName("VkQueue")]
        public void* graphics_queue;

        [NativeTypeName("VkImage")]
        public void* rt;

        [NativeTypeName("VkRenderPass")]
        public void* render_pass;

        public void* opaque;

        [NativeTypeName("int (*)(void *, int *, int *, VkFormat *, VkImageLayout *)")]
        public delegate* unmanaged[Cdecl]<void*, int*, int*, void*, void*, int> renderTargetInfo;

        [NativeTypeName("int (*)(void *, VkImageView *, VkFramebuffer *, VkSemaphore *)")]
        public delegate* unmanaged[Cdecl]<void*, void*, void*, void*, int> beginFrame;

        [NativeTypeName("VkCommandBuffer (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void*> currentCommandBuffer;

        [NativeTypeName("void (*)(void *, VkSemaphore *)")]
        public delegate* unmanaged[Cdecl]<void*, void*, void> endFrame;

        [NativeTypeName("void *[2]")]
        public _reserved_e__FixedBuffer reserved;

        public int graphics_family;

        public int compute_family;

        public int transfer_family;

        public int present_family;

        [NativeTypeName("bool")]
        public byte debug;

        [NativeTypeName("uint8_t")]
        public byte buffers;

        public int device_index;

        [NativeTypeName("uint32_t")]
        public uint max_version;

        public int gfx_queue_index;

        public int transfer_queue_index;

        public int compute_queue_index;

        public int depth;

        [NativeTypeName("uint8_t[32]")]
        public fixed byte reserved_opt[32];

        public unsafe partial struct _reserved_e__FixedBuffer
        {
            public void* e0;
            public void* e1;

            public ref void* this[int index]
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

    internal unsafe partial struct mdkD3D11RenderAPI
    {
        [NativeTypeName("enum MDK_RenderAPI")]
        internal MDK_RenderAPI type;

        internal void* context;

        internal void* rtv;

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

        [NativeTypeName("ID3D12CommandQueue*")]
        internal void* cmdQueue;

        [NativeTypeName("ID3D12Resource*")]
        internal void* rt;

        [NativeTypeName("CpuDescriptorHandle")]
        internal void* rtvHandle;

        [NativeTypeName("void *[2]")]
        internal _reserved_e__FixedBuffer reserved;

        [NativeTypeName("const void *")]
        internal void* opaque;

        [NativeTypeName("ID3D12Resource *(*)(const void *, UINT *, UINT *, D3D12_RESOURCE_STATES *)")]
        internal delegate* unmanaged[Cdecl]<void*, uint*, uint*, void*, void*> currentRenderTarget;

        [NativeTypeName("void *[2]")]
        internal _reserved2_e__FixedBuffer reserved2;

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

        internal unsafe partial struct _reserved2_e__FixedBuffer
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
}