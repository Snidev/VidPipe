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
    Pcm = Wav,
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
        AudioCodec.Pcm => "pcm_s16le",
        AudioCodec.Alac => "alac",
        _ => throw new InvalidEnumArgumentException()
    };
}