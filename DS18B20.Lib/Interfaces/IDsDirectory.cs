namespace DS18B20.Lib.Interfaces
{
    public interface IDsDirectory
    {
        IEnumerable<string> EnumerateDirectories(string path, string searchPattern);
        bool DirectoryExists(string? path);

        string GetDirectoryName(string? path);
        bool FileExists(string? path);
        string ReadAllTextFromFile(string path);
    }
}
