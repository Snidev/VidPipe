namespace VidPipe.FFmpeg;

public interface IFfJob
{
    public Task<int> RunAsync();
    public int Run() => RunAsync().GetAwaiter().GetResult();
}