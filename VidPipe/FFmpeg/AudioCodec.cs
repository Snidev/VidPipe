using System.ComponentModel;

namespace VidPipe.FFmpeg;

public enum AudioCodec
{
    Mp3,
    Aac,
    Opus,
    Vorbis,
    Flac,
    Wav,
    Pcm,
    Alac
}

public static class AudioCodecExtensions
{
    public static string GetCodecName(this AudioCodec codec) => codec switch
    {
        AudioCodec.Mp3 => "libmp3lame",
        AudioCodec.Aac => "aac",
        AudioCodec.Opus => "libopus",
        AudioCodec.Vorbis => "libvorbis",
        AudioCodec.Pcm or AudioCodec.Wav => "pcm_s16le",
        AudioCodec.Alac => "alac",
        AudioCodec.Flac => "flac",
        _ => throw new InvalidEnumArgumentException()
    };
    
    public static string GetFormat(this AudioCodec codec) => codec switch
    {
        AudioCodec.Mp3 => "mp3",
        AudioCodec.Aac => "adts",
        AudioCodec.Opus => "opus",
        AudioCodec.Vorbis => "ogg",
        AudioCodec.Pcm => "s16le",
        AudioCodec.Wav => "wav",
        AudioCodec.Alac => "ipod",
        AudioCodec.Flac => "flac",
        _ => throw new InvalidEnumArgumentException()
    };
}