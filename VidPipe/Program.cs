using static System.Environment;
using System.CommandLine;
using System.Diagnostics;
using VidPipe.FFmpeg;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VidPipe;

class Program
{
    public static string AppDataPath => Path.Combine(GetFolderPath(SpecialFolder.ApplicationData),
        "VidPipe");

    private static string? VideoId;
    
    static async Task Main(string[] args)
    {
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);

        RootCommand cmd = new("YouTube audio downloader");

        Option<string> videoOpt = new(aliases: ["--video", "-v"],
            description: "The id or link to a youtube video")
        {
            IsRequired = true
        };

        Option<string> outOpt = new(aliases: ["--output", "-o"],
            description: "The name and directory of the output file")
        {
            IsRequired = true
        };
        
        cmd.Add(videoOpt);
        cmd.Add(outOpt);

        string url = "", outDir = "";
        cmd.SetHandler((urlVar, outDirVar) =>
        {
            url = urlVar;
            outDir = outDirVar;
        }, videoOpt, outOpt);

        await cmd.InvokeAsync(args);
        
        const string prefix = "https://www.youtube.com/watch?v=";
        url = url.StartsWith(prefix) ? url : prefix + url;

        Console.WriteLine($"Downloading audio from {url}");
            
        YoutubeClient ytc = new();
        Video video = await ytc.Videos.GetAsync(url);
        StreamManifest manifest = await ytc.Videos.Streams.GetManifestAsync(video.Id);
        IStreamInfo si = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

        Stream videoStream = await ytc.Videos.Streams.GetAsync(si);
        IFfJob extractor = new AudioExtractor(videoStream, outDir, AudioCodec.Mp3, 192_000);

        await extractor.RunAsync();
            
        /*ytc.Videos.Streams.CopyToAsync(si, ffmpeg.StandardInput.BaseStream).GetAwaiter().GetResult();*/
            
        Console.WriteLine($"Saved output to {outDir}");
    }
}