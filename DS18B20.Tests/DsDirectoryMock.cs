using DS18B20.Lib.Interfaces;

namespace DS18B20.Tests
{
    internal class DsDirectoryMock : IDsDirectory
    {
        public bool DirectoryExists(string? path)
        {
            return true;
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return new[] { "/sys/devices/w1", "/sys/devices/w2" };
        }

        public bool FileExists(string? path)
        {
            return true;
        }

        public string GetDirectoryName(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return "w0";

            if(path.Contains("w1"))
            {
                return "w1";
            }else if (path.Contains("w2"))
            {
                return "w2";
            }

            return "w0";
        }

        public string ReadAllTextFromFile(string path)
        {
            Random rnd = new Random();
            
            var t = rnd.Next(10000, 20000);

            if (path.Contains("w1"))
            {
                return $"abcd \r\nabcd t={t}";
            }
            else if (path.Contains("w2"))
            {
                return $"abcd \r\nabcd t={t}";
            }

            return "abcd \r\nabcd t=00000";
        }
    }
}
