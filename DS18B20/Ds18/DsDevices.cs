using DS18B20.Ds18.Interfaces;
using EltraCommon.Logger;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DS18B20.Ds18
{
    public class DsDevices : IDsDevices
    {
        private readonly IDsDevice? _device;
        private List<IDsDevice>? _activeDevices;

        public DsDevices()
        {
        }

        public DsDevices(IDsDevice device)
        {
            _device = device;
        }

        [JsonPropertyName("devices")]
        public List<IDsDevice> ActiveDevices
        {
            get
            {
                return _activeDevices ?? (_activeDevices = CreateDevicesLists());
            }
        }

        private List<IDsDevice> CreateDevicesLists()
        {
            var result = new List<IDsDevice>();

            if (ReadAllDevices(out var deviceNames))
            {
                result = deviceNames;
            }

            return result;
        }

        public bool SerializeToJson()
        {
            const string method = "SerializeToJson";

            bool result = false;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(this, options);

                MsgLogger.WriteDebug($"{GetType().Name} - {method}", json);

                result = !string.IsNullOrEmpty(json);
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {method}", e);
            }

            return result;
        }

        private bool ReadAllDevices(out List<IDsDevice> deviceNames)
        {
            const string method = "ReadAllDevices";
            bool result = false;

            deviceNames = new List<IDsDevice>();

            try
            {
                var devicesPath = Directory.EnumerateDirectories(DsDefinitions.W1Path, "28*");

                if (devicesPath.Any())
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Search in {DsDefinitions.W1Path}, found = {devicesPath.Count()}");

                    foreach (var devicePath in devicesPath)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Search in {devicePath}");

                        if (Directory.Exists(devicePath))
                        {
                            var deviceDirInfo = new DirectoryInfo(devicePath);
                            string deviceName = deviceDirInfo.Name;

                            AddDevice(deviceNames, deviceName);

                            result = true;
                        }
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"no devices found");
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {method}", e);
                result = false;
            }

            return result;
        }

        private void AddDevice(List<IDsDevice> deviceNames, string deviceName)
        {
            if (_device != null && !string.IsNullOrEmpty(deviceName))
            {
                IDsDevice? device = CreateDevice(_device.GetType(), deviceName);

                if (device != null)
                {
                    deviceNames.Add(device);
                }
            }
        }

        private static IDsDevice? CreateDevice(Type deviceType, string deviceName)
        {
            IDsDevice? result = null;
            
            if(Activator.CreateInstance(deviceType) is IDsDevice device)
            {   
                device.Name = deviceName;

                result = device;
            }

            return result;
        }
    }
}
