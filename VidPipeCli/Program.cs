using VidPipe;

namespace VidPipeCli;

class Program
{
    static async Task Main(string[] args)
    {
        /*YoutubeInterface yt = new();
        Stream video = await yt.GetAudioStream("https://www.youtube.com/watch?v=zhKB78Cm_44");

        Stream file = File.Open(@"C:\Users\Joe\Desktop\vid", FileMode.OpenOrCreate);
        await video.CopyToAsync(file);
        video.Close();
        file.Close();
        yt.Dispose();*/

        await MainProgram.ProcessJobBatchFromCsv(@"C:\Users\Joe\Desktop\suite1.csv", @"C:\Users\Joe\Desktop\files");
    }
}