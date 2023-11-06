using EltraCommon.Logger;

namespace DS18B20.Ds18
{
    internal class DsDevices
    {
        public List<string> DeviceNames
        {
            get
            {
                var result = new List<string>();

                if (ReadAllDevices(out var deviceNames))
                {
                    result = deviceNames;
                }

                return result;
            }
        }

        public bool Read(string deviceName, out DsDeviceMeasure? measure)
        {
            const string method = "Read";

            bool result = false;
            var slaveFile = new DsSlaveFile();

            measure = null;

            if (!slaveFile.GetMeasure(deviceName, out var mea))
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"Read temperature from device {deviceName} failed!");
                result = false;
            }
            else if (mea != null)
            {
                measure = mea;
                MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{deviceName}', temperature = '{measure.Temperature} °C'");
                result = true;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"Device {deviceName} get measure failed!");
            }

            return result;
        }

        private bool ReadAllDevices(out List<string> deviceNames)
        {
            const string method = "ReadAllDevices";
            bool result = false;

            deviceNames = new List<string>();

            try
            {
                var devicesPath = Directory.EnumerateDirectories(DsDefinitions.w1Path, "28*");

                if (devicesPath.Any())
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Search in {DsDefinitions.w1Path}, found = {devicesPath.Count()}");

                    foreach (var devicePath in devicesPath)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Search in {devicePath}");

                        if (Directory.Exists(devicePath))
                        {
                            var deviceDirInfo = new DirectoryInfo(devicePath);
                            string deviceName = deviceDirInfo.Name;

                            deviceNames.Add(deviceName);

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
    }
}
