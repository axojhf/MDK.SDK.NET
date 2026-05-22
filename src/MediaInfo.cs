using System.Runtime.InteropServices;
using MDK.SDK.NET.Gen;

namespace MDK.SDK.NET;

public partial struct AudioCodecParameters
{
    public string codec;

    public uint codec_tag;

    public byte[] extra_data;

    public int extra_data_size;

    public long bit_rate;

    public int profile = -99;

    public int level = -99;

    public float frame_rate;

    public byte is_float;

    public byte is_unsigned;

    public byte is_planar;

    public int raw_sample_size;

    public int channels;

    public int sample_rate;

    public int block_align;

    public int frame_size;

    public AudioCodecParameters()
    {
        codec = "";
        extra_data = [];
    }
}

public partial struct AudioStreamInfo
{
    public int index;

    public long start_time;

    public long duration;

    public long frames;

    public Dictionary<string, string> metadata;

    public AudioCodecParameters codec;

    public AudioStreamInfo()
    {
        metadata = [];
        codec = new();
    }
}

public partial struct VideoCodecParameters
{
    public string codec;

    public uint codec_tag;

    /// <summary>
    /// without padding data
    /// </summary>
    public byte[] extra_data;

    public int extra_data_size;

    public long bit_rate;

    public int profile = -99;

    public int level = -99;

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

    public VideoCodecParameters()
    {
        codec = "";
        extra_data = [];
        format_name = "";
    }
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

    public VideoStreamInfo()
    {
        metadata = [];
        codec = new();
        image_data = [];
    }
}

public partial struct SubtitleCodecParameters
{
    public string codec;

    public uint codec_tag;

    /// <summary>
    /// without padding data
    /// </summary>
    public byte[] extra_data;

    public int extra_data_size;

    /// <summary>
    /// display width. bitmap subtitles only
    /// </summary>
    public int width;

    /// <summary>
    /// display height. bitmap subtitles only
    /// </summary>
    public int height;

    public SubtitleCodecParameters()
    {
        codec = "";
        extra_data = [];
    }
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

    public SubtitleStreamInfo()
    {
        metadata = [];
        codec = new();
    }
}

public partial struct ChapterInfo
{
    public long start_time;

    public long end_time;

    public string title;

    public ChapterInfo()
    {
        title = "";
    }
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

    public ProgramInfo()
    {
        stream = [];
        metadata = [];
    }
}

public partial struct MediaInfo
{
    /// <summary>
    /// ms
    /// </summary>
    public long start_time;

    /// <summary>
    /// ms. 0 for live streams. may change when playing a stream being recorded
    /// </summary>
    public long duration;

    /// <summary>
    /// when loaded, e.g. in prepare callback, it's the value from container. when running, it's updated to the realtime value
    /// </summary>
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

    public MediaInfo()
    {
        format = "";
        chapters = [];
        metadata = [];
        audio = [];
        video = [];
        subtitle = [];
        program = [];
    }

    internal static unsafe void From_c(mdkMediaInfo* cinfo, ref MediaInfo mediaInfo)
    {
        mediaInfo = new MediaInfo();
        if (cinfo == null)
            return;

        mediaInfo.start_time = cinfo->start_time;
        mediaInfo.duration = cinfo->duration;
        mediaInfo.bit_rate = cinfo->bit_rate;
        mediaInfo.size = cinfo->size;
        mediaInfo.format = PtrToString(cinfo->format);
        mediaInfo.streams = cinfo->streams;
        mediaInfo.metadata = ReadMetadata(cinfo);

        var chapterCount = NativeCount(cinfo->chapters, cinfo->nb_chapters);
        mediaInfo.chapters = new List<ChapterInfo>(chapterCount);
        for (var i = 0; i < chapterCount; ++i)
        {
            var cci = &cinfo->chapters[i];
            ChapterInfo ci = new()
            {
                start_time = cci->start_time,
                end_time = cci->end_time,
                title = PtrToString(cci->title)
            };
            mediaInfo.chapters.Add(ci);
        }

        var audioCount = NativeCount(cinfo->audio, cinfo->nb_audio);
        mediaInfo.audio = new List<AudioStreamInfo>(audioCount);
        for (var i = 0; i < audioCount; ++i)
        {
            AudioStreamInfo si = new();
            var csi = &cinfo->audio[i];
            si.index = csi->index;
            si.start_time = csi->start_time;
            si.duration = csi->duration;
            si.frames = csi->frames;

            mdkAudioCodecParameters codec = new();
            Methods.MDK_AudioStreamCodecParameters(csi, &codec);
            var extraData = CopyNativeBytes(codec.extra_data, codec.extra_data_size);
            si.codec = new AudioCodecParameters
            {
                codec = PtrToString(codec.codec),
                codec_tag = codec.codec_tag,
                extra_data = extraData,
                extra_data_size = extraData.Length,
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
            si.metadata = ReadMetadata(csi);
            mediaInfo.audio.Add(si);
        }

        var videoCount = NativeCount(cinfo->video, cinfo->nb_video);
        mediaInfo.video = new List<VideoStreamInfo>(videoCount);
        for (var i = 0; i < videoCount; ++i)
        {
            VideoStreamInfo si = new();
            var csi = &cinfo->video[i];
            si.index = csi->index;
            si.start_time = csi->start_time;
            si.duration = csi->duration;
            si.frames = csi->frames;
            si.rotation = csi->rotation;

            mdkVideoCodecParameters codec = default;
            Methods.MDK_VideoStreamCodecParameters(csi, &codec);
            var extraData = CopyNativeBytes(codec.extra_data, codec.extra_data_size);
            si.codec = new VideoCodecParameters
            {
                codec = PtrToString(codec.codec),
                codec_tag = codec.codec_tag,
                extra_data = extraData,
                extra_data_size = extraData.Length,
                bit_rate = codec.bit_rate,
                profile = codec.profile,
                level = codec.level,
                frame_rate = codec.frame_rate,
                format = codec.format,
                format_name = PtrToString(codec.format_name),
                width = codec.width,
                height = codec.height,
                b_frames = codec.b_frames,
                par = codec.par,
                color_space = (ColorSpace)codec.color_space,
                dovi_profile = codec.dovi_profile
            };
            si.metadata = ReadMetadata(csi);
            var imageSize = 0;
            si.image_data = CopyNativeBytes(Methods.MDK_VideoStreamData(csi, &imageSize, 0), imageSize);
            si.image_size = si.image_data.Length;
            mediaInfo.video.Add(si);
        }

        var subtitleCount = NativeCount(cinfo->subtitle, cinfo->nb_subtitle);
        mediaInfo.subtitle = new List<SubtitleStreamInfo>(subtitleCount);
        for (var i = 0; i < subtitleCount; ++i)
        {
            SubtitleStreamInfo si = new();
            var csi = &cinfo->subtitle[i];
            si.index = csi->index;
            si.start_time = csi->start_time;
            si.duration = csi->duration;

            mdkSubtitleCodecParameters codec = default;
            Methods.MDK_SubtitleStreamCodecParameters(csi, &codec);
            var extraData = CopyNativeBytes(codec.extra_data, codec.extra_data_size);
            si.codec = new SubtitleCodecParameters
            {
                codec = PtrToString(codec.codec),
                codec_tag = codec.codec_tag,
                extra_data = extraData,
                extra_data_size = extraData.Length,
                width = codec.width,
                height = codec.height,
            };
            si.metadata = ReadMetadata(csi);
            mediaInfo.subtitle.Add(si);
        }

        var programCount = NativeCount(cinfo->programs, cinfo->nb_programs);
        mediaInfo.program = new List<ProgramInfo>(programCount);
        for (var i = 0; i < programCount; ++i)
        {
            ProgramInfo pi = new();
            var cpi = &cinfo->programs[i];
            pi.id = cpi->id;
            var streamCount = NativeCount(cpi->stream, cpi->nb_stream);
            pi.stream = new List<int>(streamCount);
            for (var j = 0; j < streamCount; ++j)
            {
                pi.stream.Add(cpi->stream[j]);
            }
            pi.metadata = ReadMetadata(cpi);
            mediaInfo.program.Add(pi);
        }
    }

    private static unsafe string PtrToString(sbyte* value)
    {
        return value == null ? "" : Marshal.PtrToStringUTF8((nint)value) ?? "";
    }

    private static unsafe byte[] CopyNativeBytes(byte* data, int size)
    {
        return data == null || size <= 0 ? [] : new ReadOnlySpan<byte>(data, size).ToArray();
    }

    private static unsafe int NativeCount<T>(T* data, int count) where T : unmanaged
    {
        return data == null || count <= 0 ? 0 : count;
    }

    private static unsafe Dictionary<string, string> ReadMetadata(mdkMediaInfo* info)
    {
        Dictionary<string, string> metadata = [];
        if (info == null)
            return metadata;

        mdkStringMapEntry entry = default;
        while (Methods.MDK_MediaMetadata(info, &entry) != 0)
            AddMetadata(metadata, &entry);

        return metadata;
    }

    private static unsafe Dictionary<string, string> ReadMetadata(mdkAudioStreamInfo* info)
    {
        Dictionary<string, string> metadata = [];
        if (info == null)
            return metadata;

        mdkStringMapEntry entry = default;
        while (Methods.MDK_AudioStreamMetadata(info, &entry) != 0)
            AddMetadata(metadata, &entry);

        return metadata;
    }

    private static unsafe Dictionary<string, string> ReadMetadata(mdkVideoStreamInfo* info)
    {
        Dictionary<string, string> metadata = [];
        if (info == null)
            return metadata;

        mdkStringMapEntry entry = default;
        while (Methods.MDK_VideoStreamMetadata(info, &entry) != 0)
            AddMetadata(metadata, &entry);

        return metadata;
    }

    private static unsafe Dictionary<string, string> ReadMetadata(mdkSubtitleStreamInfo* info)
    {
        Dictionary<string, string> metadata = [];
        if (info == null)
            return metadata;

        mdkStringMapEntry entry = default;
        while (Methods.MDK_SubtitleStreamMetadata(info, &entry) != 0)
            AddMetadata(metadata, &entry);

        return metadata;
    }

    private static unsafe Dictionary<string, string> ReadMetadata(mdkProgramInfo* info)
    {
        Dictionary<string, string> metadata = [];
        if (info == null)
            return metadata;

        mdkStringMapEntry entry = default;
        while (Methods.MDK_ProgramMetadata(info, &entry) != 0)
            AddMetadata(metadata, &entry);

        return metadata;
    }

    private static unsafe void AddMetadata(Dictionary<string, string> metadata, mdkStringMapEntry* entry)
    {
        if (entry == null || entry->key == null)
            return;

        metadata[PtrToString(entry->key)] = PtrToString(entry->value);
    }
}


