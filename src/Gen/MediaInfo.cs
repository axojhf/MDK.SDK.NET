using System.Runtime.InteropServices;

namespace MDK.SDK.NET.Gen
{
    internal unsafe partial struct mdkAudioCodecParameters
    {
        [NativeTypeName("const char *")]
        internal sbyte* codec;

        [NativeTypeName("uint32_t")]
        internal uint codec_tag;

        [NativeTypeName("const uint8_t *")]
        internal byte* extra_data;

        internal int extra_data_size;

        [NativeTypeName("int64_t")]
        internal long bit_rate;

        internal int profile;

        internal int level;

        internal float frame_rate;

        [NativeTypeName("bool")]
        internal byte is_float;

        [NativeTypeName("bool")]
        internal byte is_unsigned;

        [NativeTypeName("bool")]
        internal byte is_planar;

        internal int raw_sample_size;

        internal int channels;

        internal int sample_rate;

        internal int block_align;

        internal int frame_size;

        [NativeTypeName("char[128]")]
        internal fixed sbyte reserved[128];
    }

    internal unsafe partial struct mdkAudioStreamInfo
    {
        internal int index;

        [NativeTypeName("int64_t")]
        internal long start_time;

        [NativeTypeName("int64_t")]
        internal long duration;

        [NativeTypeName("int64_t")]
        internal long frames;

        [NativeTypeName("const void *")]
        internal void* priv;
    }

    internal unsafe partial struct mdkVideoCodecParameters
    {
        [NativeTypeName("const char *")]
        internal sbyte* codec;

        [NativeTypeName("uint32_t")]
        internal uint codec_tag;

        [NativeTypeName("const uint8_t *")]
        internal byte* extra_data;

        internal int extra_data_size;

        [NativeTypeName("int64_t")]
        internal long bit_rate;

        internal int profile;

        internal int level;

        internal float frame_rate;

        internal int format;

        [NativeTypeName("const char *")]
        internal sbyte* format_name;

        internal int width;

        internal int height;

        internal int b_frames;

        internal float par;

        [NativeTypeName("char[128]")]
        internal fixed sbyte reserved[128];
    }

    internal unsafe partial struct mdkVideoStreamInfo
    {
        internal int index;

        [NativeTypeName("int64_t")]
        internal long start_time;

        [NativeTypeName("int64_t")]
        internal long duration;

        [NativeTypeName("int64_t")]
        internal long frames;

        internal int rotation;

        [NativeTypeName("const void *")]
        internal void* priv;
    }

    internal unsafe partial struct mdkSubtitleCodecParameters
    {
        [NativeTypeName("const char *")]
        internal sbyte* codec;

        [NativeTypeName("uint32_t")]
        internal uint codec_tag;

        [NativeTypeName("const uint8_t *")]
        internal byte* extra_data;

        internal int extra_data_size;

        internal int width;

        internal int height;
    }

    internal unsafe partial struct mdkSubtitleStreamInfo
    {
        internal int index;

        [NativeTypeName("int64_t")]
        internal long start_time;

        [NativeTypeName("int64_t")]
        internal long duration;

        [NativeTypeName("const void *")]
        internal void* priv;
    }

    internal unsafe partial struct mdkChapterInfo
    {
        [NativeTypeName("int64_t")]
        internal long start_time;

        [NativeTypeName("int64_t")]
        internal long end_time;

        [NativeTypeName("const char *")]
        internal IntPtr title;

        [NativeTypeName("const void *")]
        internal void* priv;
    }

    internal unsafe partial struct mdkProgramInfo
    {
        internal int id;

        [NativeTypeName("const int *")]
        internal int* stream;

        internal int nb_stream;

        [NativeTypeName("const void *")]
        internal void* priv;
    }

    internal unsafe partial struct mdkMediaInfo
    {
        [NativeTypeName("int64_t")]
        internal long start_time;

        [NativeTypeName("int64_t")]
        internal long duration;

        [NativeTypeName("int64_t")]
        internal long bit_rate;

        [NativeTypeName("int64_t")]
        internal long size;

        [NativeTypeName("const char *")]
        internal IntPtr format;

        internal int streams;

        internal mdkAudioStreamInfo* audio;

        internal int nb_audio;

        internal mdkVideoStreamInfo* video;

        internal int nb_video;

        internal mdkSubtitleStreamInfo* subtitle;

        internal int nb_subtitle;

        [NativeTypeName("const void *")]
        internal void* priv;

        internal mdkChapterInfo* chapters;

        internal int nb_chapters;

        internal mdkProgramInfo* programs;

        internal int nb_programs;
    }

    internal static unsafe partial class Methods
    {
        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_AudioStreamCodecParameters([NativeTypeName("const mdkAudioStreamInfo *")] mdkAudioStreamInfo* param0, mdkAudioCodecParameters* p);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_AudioStreamMetadata([NativeTypeName("const mdkAudioStreamInfo *")] mdkAudioStreamInfo* param0, mdkStringMapEntry* entry);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_VideoStreamCodecParameters([NativeTypeName("const mdkVideoStreamInfo *")] mdkVideoStreamInfo* param0, mdkVideoCodecParameters* p);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_VideoStreamMetadata([NativeTypeName("const mdkVideoStreamInfo *")] mdkVideoStreamInfo* param0, mdkStringMapEntry* entry);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("const uint8_t *")]
        internal static partial byte* MDK_VideoStreamData([NativeTypeName("const mdkVideoStreamInfo *")] mdkVideoStreamInfo* param0, int* len, int flags);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void MDK_SubtitleStreamCodecParameters([NativeTypeName("const mdkSubtitleStreamInfo *")] mdkSubtitleStreamInfo* param0, mdkSubtitleCodecParameters* p);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_SubtitleStreamMetadata([NativeTypeName("const mdkSubtitleStreamInfo *")] mdkSubtitleStreamInfo* param0, mdkStringMapEntry* entry);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_ProgramMetadata([NativeTypeName("const mdkProgramInfo *")] mdkProgramInfo* param0, mdkStringMapEntry* entry);

        [LibraryImport("mdk")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: NativeTypeName("bool")]
        internal static partial byte MDK_MediaMetadata([NativeTypeName("const mdkMediaInfo *")] mdkMediaInfo* param0, mdkStringMapEntry* entry);
    }
}
