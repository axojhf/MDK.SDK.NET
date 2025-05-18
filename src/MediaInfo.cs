using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;

namespace MDK.SDK.NET;

public partial struct AudioCodecParameters
{
    public string codec;

    public uint codec_tag;

    public IntPtr extra_data;

    public int extra_data_size;

    public long bit_rate;

    public int profile;

    public int level;

    public float frame_rate;

    public byte is_float;

    public byte is_unsigned;

    public byte is_planar;

    public int raw_sample_size;

    public int channels;

    public int sample_rate;

    public int block_align;

    public int frame_size;
}

public partial struct AudioStreamInfo
{
    public int index;

    public long start_time;

    public long duration;

    public long frames;

    public Dictionary<string, string> metadata;

    public AudioCodecParameters codec;
}

public partial struct VideoCodecParameters
{
    public string codec;

    public uint codec_tag;

    /// <summary>
    /// without padding data
    /// </summary>
    public IntPtr extra_data;

    public int extra_data_size;

    public long bit_rate;

    public int profile;

    public int level;

    public float frame_rate;

    /// <summary>
    /// pixel format
    /// </summary>
    public int format;

    /// <summary>
    /// pixel format name
    /// </summary>
    public string format_name;

    public int width;

    public int height;

    public int b_frames;

    public float par;

    public ColorSpace color_space;

    public byte dovi_profile;
}

public partial struct VideoStreamInfo
{
    public int index;

    public long start_time;

    public long duration;

    public long frames;

    /// <summary>
    /// degree need to rotate clockwise
    /// </summary>
    public int rotation;

    /// <summary>
    /// stream language is metadata["language"]
    /// </summary>
    public Dictionary<string, string> metadata;

    public VideoCodecParameters codec;

    /// <summary>
    /// audio cover art image data, can be jpeg, png etc.
    /// </summary>
    public byte[] image_data;

    public int image_size;
}

public unsafe partial struct SubtitleCodecParameters
{
    public string codec;

    public uint codec_tag;

    /// <summary>
    /// without padding data
    /// </summary>
    public IntPtr extra_data;

    public int extra_data_size;

    /// <summary>
    /// display width. bitmap subtitles only
    /// </summary>
    public int width;

    /// <summary>
    /// display height. bitmap subtitles only
    /// </summary>
    public int height;
}

public partial struct SubtitleStreamInfo
{
    public int index;

    public long start_time;

    public long duration;

    /// <summary>
    /// stream language is metadata["language"]
    /// </summary>
    public Dictionary<string, string> metadata;

    public SubtitleCodecParameters codec;
}

public partial struct ChapterInfo
{
    public long start_time;

    public long end_time;

    public string title;
}

public partial struct ProgramInfo
{
    public int id;

    /// <summary>
    /// stream index
    /// </summary>
    public List<int> stream;

    /// <summary>
    /// "service_name", "service_provider" etc.
    /// </summary>
    public Dictionary<string, string> metadata;
}

public partial struct MediaInfo
{
    /// <summary>
    /// ms
    /// </summary>
    public long start_time;

    public long duration;

    public long bit_rate;

    /// <summary>
    /// file size. IGNORE ME!
    /// </summary>
    public long size;

    public string format;

    public int streams;

    public List<ChapterInfo> chapters;

    public Dictionary<string, string> metadata;

    public List<AudioStreamInfo> audio;

    public List<VideoStreamInfo> video;

    public List<SubtitleStreamInfo> subtitle;

    public List<ProgramInfo> program;

    internal static unsafe void From_c(mdkMediaInfo* cinfo, ref MediaInfo mediaInfo)
    {
        mediaInfo.start_time = cinfo->start_time;
        mediaInfo.duration = cinfo->duration;
        mediaInfo.bit_rate = cinfo->bit_rate;
        mediaInfo.size = cinfo->size;
        mediaInfo.format = Marshal.PtrToStringUTF8(cinfo->format) ?? "";
        mediaInfo.metadata = [];
        mediaInfo.chapters = [];
        mediaInfo.audio = [];
        mediaInfo.video = [];
        mediaInfo.subtitle = [];
        mediaInfo.program = [];

        mdkStringMapEntry e = default;
        while (0 != Methods.MDK_MediaMetadata(cinfo, &e))
        {
            var key = Marshal.PtrToStringUTF8((nint)e.key) ?? "";
            var value = Marshal.PtrToStringUTF8((nint)e.value) ?? "";
            mediaInfo.metadata.TryAdd(key, value);
        }

        for (var i = 0; i < cinfo->nb_chapters; ++i)
        {
            var cci = &cinfo->chapters[i];
            ChapterInfo ci = new()
            {
                start_time = cci->start_time,
                end_time = cci->end_time
            };
            if (cci->title != IntPtr.Zero)
            {
                ci.title = Marshal.PtrToStringUTF8(cci->title) ?? "";
            }
            mediaInfo.chapters.Add(ci);
        }

        for (var i = 0; i < cinfo->nb_audio; ++i)
        {
            AudioStreamInfo si = new();
            var csi = &cinfo->audio[i];
            si.index = csi->index;
            si.start_time = csi->start_time;
            si.duration = csi->duration;
            si.frames = csi->frames;

            mdkAudioCodecParameters codec = new();
            Methods.MDK_AudioStreamCodecParameters(csi, &codec);
            si.codec = new AudioCodecParameters
            {
                codec = Marshal.PtrToStringUTF8((nint)codec.codec) ?? "",
                codec_tag = codec.codec_tag,
                extra_data = (nint)codec.extra_data,
                extra_data_size = codec.extra_data_size,
                bit_rate = codec.bit_rate,
                profile = codec.profile,
                level = codec.level,
                frame_rate = codec.frame_rate,
                is_float = codec.is_float,
                is_unsigned = codec.is_unsigned,
                is_planar = codec.is_planar,
                raw_sample_size = codec.raw_sample_size,
                channels = codec.channels,
                sample_rate = codec.sample_rate,
                block_align = codec.block_align,
                frame_size = codec.frame_size,
            };
            si.metadata = [];
            e.next = null;
            while (Methods.MDK_AudioStreamMetadata(csi, &e) != 0)
            {
                var key = Marshal.PtrToStringUTF8((nint)e.key) ?? "";
                var value = Marshal.PtrToStringUTF8((nint)e.value) ?? "";
                si.metadata.TryAdd(key, value);
            }
            mediaInfo.audio.Add(si);
        }

        for (var i = 0; i < cinfo->nb_video; ++i)
        {
            VideoStreamInfo si = new();
            var csi = &cinfo->video[i];
            si.index = csi->index;
            si.start_time = csi->start_time;
            si.duration = csi->duration;
            si.frames = csi->frames;
            si.rotation = csi->rotation;

            mdkVideoCodecParameters codec;
            Methods.MDK_VideoStreamCodecParameters(csi, &codec);
            si.codec = new VideoCodecParameters
            {
                codec = Marshal.PtrToStringUTF8((nint)codec.codec) ?? "",
                codec_tag = codec.codec_tag,
                extra_data = (nint)codec.extra_data,
                extra_data_size = codec.extra_data_size,
                bit_rate = codec.bit_rate,
                profile = codec.profile,
                level = codec.level,
                frame_rate = codec.frame_rate,
                format = codec.format,
                format_name = Marshal.PtrToStringUTF8((nint)codec.format_name) ?? "",
                width = codec.width,
                height = codec.height,
                b_frames = codec.b_frames,
                par = codec.par,
                color_space = (ColorSpace)codec.color_space,
                dovi_profile = codec.dovi_profile
            };
            si.metadata = [];
            e.next = null;
            while (Methods.MDK_VideoStreamMetadata(csi, &e) != 0)
            {
                var key = Marshal.PtrToStringUTF8((nint)e.key) ?? "";
                var value = Marshal.PtrToStringUTF8((nint)e.value) ?? "";
                si.metadata.TryAdd(key, value);
            }
            mediaInfo.video.Add(si);
        }

        for (var i = 0; i < cinfo->nb_subtitle; ++i)
        {
            SubtitleStreamInfo si = new();
            var csi = &cinfo->subtitle[i];
            si.index = csi->index;
            si.start_time = csi->start_time;
            si.duration = csi->duration;

            mdkSubtitleCodecParameters codec;
            Methods.MDK_SubtitleStreamCodecParameters(csi, &codec);
            si.codec = new SubtitleCodecParameters
            {
                codec = Marshal.PtrToStringUTF8((nint)codec.codec) ?? "",
                codec_tag = codec.codec_tag,
                extra_data = (nint)codec.extra_data,
                extra_data_size = codec.extra_data_size,
                width = codec.width,
                height = codec.height,
            };
            si.metadata = [];
            //mdkStringMapEntry entry = default;//必须要new一个，不然会出现野指针
            e.next = null;
            while (Methods.MDK_SubtitleStreamMetadata(csi, &e) != 0)
            {
                var key = Marshal.PtrToStringUTF8((nint)e.key) ?? "";
                var value = Marshal.PtrToStringUTF8((nint)e.value) ?? "";
                si.metadata.TryAdd(key, value);
            }
            mediaInfo.subtitle.Add(si);
        }

        for (var i = 0; i < cinfo->nb_programs; ++i)
        {
            ProgramInfo pi = new();
            var cpi = cinfo->programs[i];
            pi.id = cpi.id;
            pi.stream = [];
            for (var j = 0; j < cpi.nb_stream; ++j)
            {
                pi.stream.Add(cpi.stream[j]);
            }
            pi.metadata = [];
            e.next = null;
            while (Methods.MDK_ProgramMetadata(&cpi, &e) != 0)
            {
                var key = Marshal.PtrToStringUTF8((nint)e.key) ?? "";
                var value = Marshal.PtrToStringUTF8((nint)e.value) ?? "";
                pi.metadata.TryAdd(key, value);
            }
            mediaInfo.program.Add(pi);
        }
    }
}


