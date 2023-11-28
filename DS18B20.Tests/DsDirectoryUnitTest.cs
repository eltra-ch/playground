using DS18B20.Lib;
using DS18B20.Lib.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Unity;

namespace DS18B20.Tests
{
    [ExcludeFromCodeCoverage]
    public class DsDirectoryUnitTest
    {
        [Fact]
        public void DirectoryExistsShouldCompleteSuccessfully()
        {
            var unityContainer = new UnityContainer();
            
            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            string path = Path.GetTempPath();
            
            bool result = directory.DirectoryExists(path);

            Assert.True(result);
        }

        [Fact]
        public void DirectoryExistsShouldFail()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            bool result = directory.DirectoryExists(path);

            Assert.False(result);
        }

        [Fact]
        public void FileExistsShouldFail()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            bool result = directory.FileExists(path);

            Assert.False(result);
        }

        [Fact]
        public void FileExistsShouldSucceed()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            var sys32path = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string path = Path.Combine(sys32path, "drivers\\etc\\hosts");

            bool result = directory.FileExists(path);

            Assert.True(result);
        }

        [Fact]
        public void GetDirectoryNameShouldSucceed()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            var sys32path = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string path = Path.Combine(sys32path, "drivers\\etc\\hosts");

            var result = directory.GetDirectoryName(path);

            Assert.True(!string.IsNullOrEmpty(result));
        }

        [Fact]
        public void GetDirectoryNameShouldReturnNullString()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            string path = string.Empty;

            var result = directory.GetDirectoryName(path);

            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void ReadAllTextFromFileShouldReturnString()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            var sys32path = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string path = Path.Combine(sys32path, "drivers\\etc\\hosts");

            var result = directory.ReadAllTextFromFile(path);

            Assert.True(!string.IsNullOrEmpty(result));
        }

        [Fact]
        public void EnumerateDirectoriesShouldReturnListOfDirectories()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();

            var directory = unityContainer.Resolve<IDsDirectory>();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.System);
            
            var result = directory.EnumerateDirectories(path, "*");

            Assert.True(result.Any());
        }
    }
}
