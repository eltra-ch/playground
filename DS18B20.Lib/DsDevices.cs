using DS18B20.Lib.Interfaces;
using EltraCommon.Logger;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DS18B20.Lib
{
    public class DsDevices : IDsDevices
    {
        #region Private fields

        private readonly IDsBuilder? _builder;
        private readonly IDsDirectory? _directory;

        private List<IDsDevice>? _activeDevices;
        
        #endregion

        #region Constructors

        public DsDevices()
        {
        }

        public DsDevices(IDsBuilder builder, IDsDirectory directory)
        {
            _builder = builder;
            _directory = directory;
        }

        #endregion

        #region Properties

        [JsonPropertyName("devices")]
        public List<IDsDevice> ActiveDevices
        {
            get
            {
                return _activeDevices ?? (_activeDevices = CreateDevicesLists());
            }
        }

        #endregion

        #region Methods

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

        #region Private methods

        private List<IDsDevice> CreateDevicesLists()
        {
            var result = new List<IDsDevice>();

            if (ReadAllDevices(out var deviceNames))
            {
                result = deviceNames;
            }

            return result;
        }

        private bool ReadAllDevices(out List<IDsDevice> deviceNames)
        {
            const string method = "ReadAllDevices";
            bool result = false;

            deviceNames = new List<IDsDevice>();

            if(_directory == null)
                return false;

            try
            {
                var devicesPath = _directory.EnumerateDirectories(DsDefinitions.W1Path, "28*");

                if (devicesPath.Any())
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Search in {DsDefinitions.W1Path}, found = {devicesPath.Count()}");

                    foreach (var devicePath in devicesPath)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Search in {devicePath}");

                        if (_directory.DirectoryExists(devicePath))
                        {
                            string deviceName = _directory.GetDirectoryName(devicePath);

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
            if (!string.IsNullOrEmpty(deviceName))
            {
                IDsDevice? device = CreateDevice(deviceName);

                if (device != null)
                {
                    deviceNames.Add(device);
                }
            }
        }

        private IDsDevice? CreateDevice(string deviceName)
        {
            IDsDevice? result = null;

            if (_builder != null)
            {
                result = _builder.BuildDevice();

                if (result != null)
                {
                    result.Name = deviceName;
                }
            }
            
            return result;
        }

        #endregion

        #endregion
    }
}
