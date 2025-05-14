using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VidPipe.YoutubeExplode;

public class AudioDownloadJob : IDisposable, IAsyncDisposable
{
    public readonly Stream Stream;
    public long BytesWritten => Stream.Position;
    public readonly long TotalBytes;

public static async Task<AudioDownloadJob> CreateAudioStream(string link)
    {
        YoutubeClient client = new();
        Video video = await client.Videos.GetAsync(link);
        StreamManifest manifest = await client.Videos.Streams.GetManifestAsync(video.Id);
        IStreamInfo si = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        Stream stream = await client.Videos.Streams.GetAsync(si);

        return new AudioDownloadJob(si, stream);
    }

    private AudioDownloadJob(IStreamInfo streamInfo, Stream stream, int progressUpdateFreq = 1000)
    {
        Stream = stream;
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