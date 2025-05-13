namespace VidPipe.FFmpeg;

public interface IFfJob
{
    public Task RunAsync();
    public void Run() => RunAsync().GetAwaiter().GetResult();
}