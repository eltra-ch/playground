using DS18B20.Lib;
using DS18B20.Lib.Interfaces;
using Unity;

namespace DS18B20.Tests
{
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

        [Fact]
        public void GetFirstActiveDeviceShouldHaveMeasures()
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

            var device = devices.ActiveDevices[0];

            device.GetMeasure(out var _);
            device.GetMeasure(out var _);

            Assert.True(device.Measures.Count == expectedCount);
        }
    }
}