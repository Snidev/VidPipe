using System.Diagnostics;
using System.IO.Pipelines;

namespace VidPipe.FFmpeg;

public class AudioExtractor(Stream input, string outputFile, AudioCodec codec, ulong bitrate, Stream? err = null, string? startTs = null, string? endTs = null) : IFfJob
{
    public readonly AudioCodec Codec = codec;
    
    public async Task<int> RunAsync()
    {
        Process ffmpeg = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Program.AppDataPath, "ffmpeg.exe"),
                //Arguments = $"{(startTs is not null ? $"-ss {startTs} " : "")}{(endTs is not null ? $"-ss {endTs} " : "")}-i pipe:0 -vn -c:a {Codec.GetCodecName()} -b:a {bitrate} -f {Codec.GetFormat()} pipe:1",
                Arguments = $"{(startTs is not null ? $"-ss {startTs} " : "")}{(endTs is not null ? $"-t {endTs} " : "")}-i pipe:0 -vn -c:a {Codec.GetCodecName()} -b:a {bitrate} -f {Codec.GetFormat()} pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = err is not null,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        ffmpeg.Start();
        Stream outFile = File.Open(outputFile, FileMode.OpenOrCreate);

        try
        {
            await Task.WhenAll(
                Task.Run(async () =>
                {
                    await input.CopyToAsync(ffmpeg.StandardInput.BaseStream);
                    ffmpeg.StandardInput.Close();
                }),
                Task.Run(async () =>
                {
                    await ffmpeg.StandardOutput.BaseStream.CopyToAsync(outFile);
                    await outFile.FlushAsync();
                }),
                Task.Run(async () =>
                {
                    if (err is not null)
                        await ffmpeg.StandardError.BaseStream.CopyToAsync(err);
                })
            );
        }
        catch (Exception e)
        {
            // ignored
        }

        await ffmpeg.WaitForExitAsync();
        return ffmpeg.ExitCode;
    }
}