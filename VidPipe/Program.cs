using static System.Environment;
using System.CommandLine;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VidPipe;

class Program
{
    public static string AppDataPath => Path.Combine(GetFolderPath(SpecialFolder.ApplicationData),
        "VidPipe");

    private static string? VideoId;
    
    static int Main(string[] args)
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
        
        cmd.SetHandler((url, outDir) =>
        {
            const string prefix = "https://www.youtube.com/watch?v=";
            url = url.StartsWith(prefix) ? url : prefix + url;

            Console.WriteLine($"Downloading audio from {url}");
            
            YoutubeClient ytc = new();
            Video video = ytc.Videos.GetAsync(url).GetAwaiter().GetResult();
            StreamManifest manifest = ytc.Videos.Streams.GetManifestAsync(video.Id).GetAwaiter().GetResult();
            IStreamInfo si = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            Process ffmpeg = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFmpeg.InstallPath,
                    Arguments = $"-i pipe:0 -vn -c:a libmp3lame -b:a 192k {outDir}",
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            ffmpeg.Start();
            ytc.Videos.Streams.CopyToAsync(si, ffmpeg.StandardInput.BaseStream).GetAwaiter().GetResult();
            ffmpeg.StandardInput.Close();
            ffmpeg.WaitForExit();
            
            Console.WriteLine($"Saved output to {outDir}");
        }, videoOpt, outOpt);

        return cmd.Invoke(args);
    }
}