namespace VidPipe;

public readonly record struct DownloadJob(JobType JobType, string Id, TimeSpan? Start, TimeSpan? Duration);

public enum JobType
{
    Audio,
    Video,
    Mux
}