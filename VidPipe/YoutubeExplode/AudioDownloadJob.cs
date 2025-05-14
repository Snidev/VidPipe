using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VidPipe.YoutubeExplode;

public class AudioDownloadJob : IDisposable, IAsyncDisposable
{
    public readonly Stream Stream;
    public readonly Video Video;
    public long BytesWritten => Stream.Position;
    public readonly long TotalBytes;

public static async Task<AudioDownloadJob> CreateAudioStream(string link)
    {
        YoutubeClient client = new();
        Video video = await client.Videos.GetAsync(link);
        StreamManifest manifest = await client.Videos.Streams.GetManifestAsync(video.Id);
        IStreamInfo si = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        Stream stream = await client.Videos.Streams.GetAsync(si);

        return new AudioDownloadJob(si, video, stream);
    }

    private AudioDownloadJob(IStreamInfo streamInfo, Video video, Stream stream)
    {
        Stream = stream;
        Video = video;
        TotalBytes = streamInfo.Size.Bytes;
    }

    public void Dispose()
    {
        Stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Stream.DisposeAsync();
    }
}