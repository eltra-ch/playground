using DS18B20.Lib;
using DS18B20.Lib.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Unity;

namespace DS18B20.Tests
{
    [ExcludeFromCodeCoverage]
    public class DsDevicesUnitTest
    {
        [Fact]
        public void GetActiveDevicesShouldShowTwoActiveDevices()
        {
            var unityContainer = new UnityContainer();
            int expectedCount = 2;

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectoryMock>();
            unityContainer.RegisterType<IDsDevices, DsDevices>();
            unityContainer.RegisterType<IDsDevice, DsDevice>();
            unityContainer.RegisterType<IDsMeasure, DsMeasure>();

            var devices = unityContainer.Resolve<IDsDevices>();

            Assert.NotNull(devices);

            Assert.True(devices.ActiveDevices.Count == expectedCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ActiveDeviceShouldHaveMeasures(int deviceIndex)
        {
            var unityContainer = new UnityContainer();
            int expectedCount = 2;

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectoryMock>();
            unityContainer.RegisterType<IDsDevices, DsDevices>();
            unityContainer.RegisterType<IDsDevice, DsDevice>();
            unityContainer.RegisterType<IDsMeasure, DsMeasure>();

            var devices = unityContainer.Resolve<IDsDevices>();

            Assert.NotNull(devices);

            Assert.True(devices.ActiveDevices.Count == expectedCount);

            var device = devices.ActiveDevices[deviceIndex];

            device.GetMeasure(out var _);
            device.GetMeasure(out var _);

            Assert.True(device.Measures.Count == expectedCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void GetFirstActiveDeviceShouldHaveMeasure(int deviceIndex)
        {
            var unityContainer = new UnityContainer();
            const int expectedDeviceCount = 2;
            const int expectedMeasureCount = 1;

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectoryMock>();
            unityContainer.RegisterType<IDsDevices, DsDevices>();
            unityContainer.RegisterType<IDsDevice, DsDevice>();
            unityContainer.RegisterType<IDsMeasure, DsMeasure>();

            var devices = unityContainer.Resolve<IDsDevices>();

            Assert.NotNull(devices);

            Assert.True(devices.ActiveDevices.Count == expectedDeviceCount);

            var device = devices.ActiveDevices[deviceIndex];

            bool result = device.GetMeasure(out var measure);

            Assert.True(result);

            Assert.True(device.Measures.Count == expectedMeasureCount);

            Assert.NotNull(measure);

            Assert.True(measure.Temperature > 0);
        }
    }
}