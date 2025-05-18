namespace VidPipe;

public class TempFs : IDisposable
{
    public readonly string FilePath;

    public TempFs()
    {
        FilePath =  Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(FilePath);
    }

    public Stream Open(string file, FileMode mode) => File.Open(Path.Combine(FilePath, file), mode);

    public bool Exists(string file) => File.Exists(Path.Combine(FilePath, file));

    public void Dispose()
    {
        Directory.Delete(FilePath, true);
    }
}