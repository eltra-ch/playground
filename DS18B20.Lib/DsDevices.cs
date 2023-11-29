using DS18B20.Lib.Interfaces;
using EltraCommon.Logger;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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

        public DsDevices(List<DsDevice> devices)
        {
            _activeDevices = new List<IDsDevice>();

            foreach (var device in devices)
            {
                _activeDevices.Add(device);
            }
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
            set 
            {
                _activeDevices = value;
            }

        }

        #endregion

        #region Methods

        public bool Serialize(SerializeMethod method, out string target)
        {
            const string methodName = "Serialize";

            bool result = false;
            target = string.Empty;

            switch (method)
            {
                case SerializeMethod.Json:
                    result = SerializeToJson(out target);
                    break;
                default:
                    MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"not supported serialize method = '{method}'");
                    break;
            }

            return result;
        }

        public bool Deserialize(SerializeMethod method, string source, out DsDevices? devices)
        {
            const string methodName = "Deserialize";

            bool result = false;

            devices = null;

            switch (method)
            {
                case SerializeMethod.Json:
                    result = DeserializeFromJson(source, out devices);
                    break;
                default:
                    MsgLogger.WriteError($"{GetType().Name} - {methodName}", $"not supported serialize method = '{method}'");
                    break;
            }

            return result;
        }

        private bool SerializeToJson(out string json)
        {
            const string methodName = "SerializeToJson";

            bool result = false;
            json = string.Empty;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                json = JsonSerializer.Serialize(this, options);

                MsgLogger.WriteDebug($"{GetType().Name} - {methodName}", json);

                result = !string.IsNullOrEmpty(json);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {methodName}", e);
            }

            return result;
        }

        private bool DeserializeFromJson(string json, out DsDevices? devices)
        {
            const string methodName = "DeserializeFromJson";

            bool result = false;

            devices = null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver
                    {
                        Modifiers =
                           {
                                 static typeInfo =>
                                 {
                                       if (typeInfo.Type == typeof(IDsDevice))
                                       {
                                             typeInfo.CreateObject = () => new DsDevice();
                                       }
                                       else if (typeInfo.Type == typeof(IDsMeasure))
                                       {
                                             typeInfo.CreateObject = () => new DsMeasure();
                                       }
                                 }
                           }
                    }
                };

                devices = JsonSerializer.Deserialize<DsDevices>(json, options);

                MsgLogger.WriteDebug($"{GetType().Name} - {methodName}", json);

                result = devices != null;
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - {methodName}", e);
            }

            return result;
        }

        public IDsDevice? FindDeviceByName(string? name)
        {
            IDsDevice? result = null;

            if(string.IsNullOrEmpty(name))
                return null;

            foreach (var device in ActiveDevices)
            {
                if(device.Name == name)
                {
                    result = device;
                    break;
                }
            }

            return result;
        }

        public void CopyTo(IDsDevices? target)
        {
            if(target == null)
                return;

            foreach(var device in ActiveDevices)
            {
                IDsDevice? d = target.FindDeviceByName(device.Name);

                device.CopyTo(d);
            }
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
