using System.Diagnostics;
using System.IO.Pipelines;

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
                Arguments = $"-i pipe:0 -vn -c:a {Codec.GetCodecName()} -b:a {bitrate} -f {Codec.GetFormat()} pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        ffmpeg.Start();
        Stream outFile = File.Open(outputFile, FileMode.OpenOrCreate);
        
        await Task.WhenAll(
            Task.Run(async () =>
            {
                await input.CopyToAsync(ffmpeg.StandardInput.BaseStream);
                ffmpeg.StandardInput.Close();
            }),
            Task.Run(async () =>
            {
                await ffmpeg.StandardOutput.BaseStream.CopyToAsync(outFile);
            }),
            Task.Run(async () =>
            {
                if (err is not null)
                    await ffmpeg.StandardError.BaseStream.CopyToAsync(err);
            })
        );

        await ffmpeg.WaitForExitAsync();
    }
}