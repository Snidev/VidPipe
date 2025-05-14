using static System.Environment;
using System.CommandLine;
using System.Diagnostics;
using VidPipe.FFmpeg;
using VidPipe.YoutubeExplode;

namespace VidPipe;

class Program
{
    public static string AppDataPath => Path.Combine(GetFolderPath(SpecialFolder.ApplicationData),
        "VidPipe");

    private static async Task GetAudioSingle(string link, string outFile)
    {
        Console.WriteLine($"Beginning download of {link}");
        AudioDownloadJob download = await AudioDownloadJob.CreateAudioStream(link);
        AudioExtractor audio = new(download.Stream, outFile, AudioCodec.Mp3, 193000);

        try
        {
            await Task.WhenAll(
                audio.RunAsync(),
                Task.Run(async () =>
                {
                    while (download.BytesWritten < download.TotalBytes)
                    {
                        Console.CursorLeft = 0;
                        Console.Write($"Download Progress: {download.BytesWritten} B / {download.TotalBytes} B");
                        await Task.Delay(100);
                    }

                    Console.WriteLine($"\nDownload Complete ({download.TotalBytes} B)");
                })
            );
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to download {link}");
            return;
        }
        finally
        {
            await download.DisposeAsync();
        }

        Console.WriteLine($"Successfully output to {outFile}");
    }

    private static void BulkDownload(string csv, string outFolder)
    {
        
    }

    private static RootCommand SetupCommands()
    {
        RootCommand root = new("A utility for downloading YouTube videos in various formats");
        
        Command audioCmd = new("audio", "Download the audio of a single video in mp3 format");
        Option<string> videoOpt = new(aliases: ["--video", "-v"], description: "The id or link to a youtube video")
        {
            IsRequired = true
        };

        Option<string> outOpt = new(aliases: ["--output", "-o"], description: "The name and directory of the output file")
        {
            IsRequired = true
        };
        
        audioCmd.Add(videoOpt);
        audioCmd.Add(outOpt);
        
        audioCmd.SetHandler(GetAudioSingle, videoOpt, outOpt);

        root.Add(audioCmd);
        return root;
    }
    
    static void Main(string[] args)
    {
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);

        RootCommand root = SetupCommands();
        root.Invoke(args);
    }
}