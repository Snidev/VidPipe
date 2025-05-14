using static System.Environment;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using CsvHelper;
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

    record struct VideoRecord(string Link, string Start, string End);
    private static async Task BulkDownload(string csvPath, string outFolder)
    {
        VideoRecord[] records;
        using (StreamReader reader = new(csvPath))
        using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
        {
            records = csv.GetRecords<VideoRecord>().ToArray();
        }

        foreach (VideoRecord record in records)
        {
            AudioDownloadJob download = null;
            try
            {
                Console.WriteLine($"Beginning download of {record.Link}");
                download = await AudioDownloadJob.CreateAudioStream(record.Link);
                string fileName = Path.Combine(outFolder, download.Video.Id + ".mp3");
                AudioExtractor audio = new(download.Stream, fileName, AudioCodec.Mp3, 193000, null, record.Start, record.End);

                int code = await audio.RunAsync();
                
                Console.Write($"Job {record.Link} finished with code {code}");
                if (code == 0)
                    Console.WriteLine($" output to {fileName}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not save {record.Link} ({e.GetType()})");
            }
            finally
            {
                download?.Dispose();
                Thread.Sleep(1000);
            }
        }
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

        Command bulkAudioCmd = new("bulkaudio", "Download multiple audio samples as provided by a csv");
        Option<string> csvOpt = new(["--csv", "-c"], "The path to a csv with the videos and timestamps")
            { IsRequired = true };
        Option<string> outFolderOpt = new(["--output", "-o"], "The output folder for all videos") 
            { IsRequired = true };
        
        bulkAudioCmd.Add(csvOpt);
        bulkAudioCmd.Add(outFolderOpt);
        bulkAudioCmd.SetHandler(BulkDownload, csvOpt, outFolderOpt);
        root.Add(bulkAudioCmd);
        
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