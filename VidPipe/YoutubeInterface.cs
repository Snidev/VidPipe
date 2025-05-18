using FFMpegCore.Enums;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VidPipe;

public class YoutubeInterface : IDisposable
{
    private readonly YoutubeClient Client = new();
    private readonly Dictionary<string, VideoData> _cachedVideos = new();
    private readonly TempFs _temp = new();

    public Task<Stream> GetAudioStream(string vidId) => GetStream(vidId, ChannelType.Audio);

    public Task<Stream> GetVideoStream(string vidId) => GetStream(vidId, ChannelType.Video);

    private async Task<Stream> GetStream(string vidId, ChannelType ct)
    {
        VideoId id = VideoId.Parse(vidId);
        string fName = ct == ChannelType.Audio ? $"a_{id}" : $"v_{id}";

        if (_temp.Exists(fName))
            return _temp.Open(fName, FileMode.Open);
        
        StreamManifest manifest = await Client.Videos.Streams.GetManifestAsync(id);
        IStreamInfo streamInfo = ct == ChannelType.Audio ? manifest.GetAudioOnlyStreams().GetWithHighestBitrate() :
            manifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();

        Stream file = _temp.Open(fName, FileMode.OpenOrCreate);
        Stream vidStream = await Client.Videos.Streams.GetAsync(streamInfo);

        await vidStream.CopyToAsync(file);
        await file.FlushAsync();
        await vidStream.DisposeAsync();

        file.Position = 0;
        return file;
    }

    private readonly struct VideoData
    {
        public readonly ChannelType ChannelType;
        public readonly IStreamInfo StreamInfo;
        public readonly Video Video;
        public readonly StreamManifest Manifest;
        public readonly string FilePath;
    }

    private enum ChannelType
    {
        Audio,
        Video
    }

    public void Dispose()
    {
        _temp.Dispose();
    }
}