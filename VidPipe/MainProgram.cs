using System.ComponentModel;
using System.Globalization;
using CsvHelper;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using YoutubeExplode.Videos;

namespace VidPipe;

public static class MainProgram
{
    public static Task ProcessJobBatchFromCsv(string csvFile, string destination)
    {
        using StreamReader sr = new(csvFile);
        using CsvReader csv = new(sr, CultureInfo.InvariantCulture);

        return ProcessJobBatch(destination, csv.GetRecords<DownloadJob>().ToArray());
    }
    
    public static async Task ProcessJobBatch(string destination, DownloadJob[] jobs)
    {
        using YoutubeInterface youtube = new();
        Dictionary<string, int> vidInstances = [];

        foreach (DownloadJob job in jobs)
        {
            string? idStr = VideoId.TryParse(job.Id);
            if (idStr is null)
                continue;
            vidInstances.TryAdd(idStr, 0);

            Stream file = File.Open(
                Path.Combine(destination, $"{GetPrefix(job.JobType)}_{idStr}_{vidInstances[idStr]++}.mp3"), 
                FileMode.OpenOrCreate);

            await ProcessJob(file, job);
        }

        return;

        string GetPrefix(JobType job) => job switch
        {
            JobType.Audio => "a",
            JobType.Video => "v",
            JobType.Mux => "m",
            _ => throw new InvalidEnumArgumentException()
        };
    }
    
    public static async Task ProcessJob(Stream outStream, DownloadJob job)
    {
        using YoutubeInterface youtube = new();
        await using Stream stream = job.JobType switch
        {
            JobType.Audio => await youtube.GetAudioStream(job.Id),
            JobType.Video => await youtube.GetVideoStream(job.Id),
            _ => throw new NotImplementedException()
        };

        // For reasons beyond my understanding, FFMpeg breaks the input pipe if you specify a duration.
        // We no longer need the input pipe, however the CLR halts anyway.
        // The output pipe works just fine however, so we're gonna pretend nothing is wrong :)
        try
        {
            await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream), o =>
                {
                    if (job.Start is not null)
                        o.Seek(job.Start);
                })
                .OutputToPipe(new StreamPipeSink(outStream), o =>
                {
                    if (job.Duration is not null)
                        o.WithDuration(job.Duration);

                    if (job.JobType == JobType.Audio)
                        o.DisableChannel(Channel.Video)
                            .ForceFormat("mp3")
                            .WithAudioCodec("libmp3lame");
                }).ProcessAsynchronously();
        }
        catch (IOException e) { }
        await outStream.FlushAsync();
    }
}