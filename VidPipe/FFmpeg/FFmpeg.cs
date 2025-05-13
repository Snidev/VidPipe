using System.Diagnostics;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;

namespace VidPipe.FFmpeg;

public static class FFmpeg
{
    private static readonly string _installPath;
    public static string InstallPath => _installPath;
    private const string _ffInstall = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z"; 

    private static bool TestForFFmpeg(string path = "")
    {
        Process proc = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(path, "ffmpeg.exe"),
                Arguments = "-version",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            proc.Start();
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private static bool InstallFFmpeg(string path)
    {
        using HttpClient http = new();
        HttpResponseMessage res = http.GetAsync(_ffInstall).GetAwaiter().GetResult();
        res.EnsureSuccessStatusCode();

        string tempPath = Path.GetTempPath();

        string zip = Path.Combine(tempPath, "ffmpeg.zip");
        
        using IArchive archive = SevenZipArchive.Open(res.Content.ReadAsStream());
        IArchiveEntry? entry = archive.Entries
            .FirstOrDefault(e => e.Key != null && e.Key.EndsWith("ffmpeg.exe", StringComparison.OrdinalIgnoreCase));
        
        entry?.WriteToDirectory(Program.AppDataPath);

        return TestForFFmpeg(Program.AppDataPath);
    }
    
    

    static FFmpeg()
    {
        if (TestForFFmpeg())
            _installPath = "ffmpeg";
        else if (TestForFFmpeg(Program.AppDataPath))
            _installPath = Path.Combine(Program.AppDataPath, "ffmpeg");
        else
        {
            Console.WriteLine($"ffmpeg was not found on the PATH or in app data. Installing to {Program.AppDataPath}");
            if (!InstallFFmpeg(Program.AppDataPath))
                throw new InstallationException("ffmpeg could not be installed!");
            
            _installPath = Path.Combine(Program.AppDataPath, "ffmpeg");
        }
    }

    public class InstallationException : Exception
    {
        public InstallationException()
        {
        }

        public InstallationException(string message) : base(message)
        {
        }

        public InstallationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}