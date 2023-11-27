using DS18B20.Lib.Interfaces;

namespace DS18B20.Lib
{
    public class DsDirectory : IDsDirectory
    {
        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return Directory.EnumerateDirectories(path, searchPattern);
        }

        public bool DirectoryExists(string? path)
        {
            return Directory.Exists(path);
        }

        public string GetDirectoryName(string? path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var deviceDirInfo = new DirectoryInfo(path);

            return deviceDirInfo.Name;
        }

        public bool FileExists(string? path)
        {
            return File.Exists(path);
        }

        public string ReadAllTextFromFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
