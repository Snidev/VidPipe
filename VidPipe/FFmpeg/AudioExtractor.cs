using System.Diagnostics;

namespace VidPipe.FFmpeg;

public class AudioExtractor(Stream input, string outputFile, AudioCodec codec, ulong bitrate, Stream? err = null) : IFfJob
{
    public readonly AudioCodec Codec = codec;
    
    public async Task RunAsync()
    {
        Process ffmpeg = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Program.AppDataPath, "ffmpeg.exe"),
                Arguments = $"-i pipe:0 -vn -c:a {codec.GetCodecName()} -b:a {bitrate} {outputFile}",
                RedirectStandardInput = true,
                //RedirectStandardOutput = true,
                RedirectStandardError = err is not null,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        ffmpeg.Start();

        await input.CopyToAsync(ffmpeg.StandardInput.BaseStream);
        ffmpeg.StandardInput.Close();
        if (err is not null)
            await ffmpeg.StandardError.BaseStream.CopyToAsync(err);

        await ffmpeg.WaitForExitAsync();
    }
}