using EltraCommon.Logger;

namespace DS18B20.Ds18
{
    internal class DsSlaveFile
    {
        public bool GetMeasure(string deviceName, out DsDeviceMeasure? measure)
        {
            const string method = "ReadSlaveFile";
            bool result = false;
            string slaveFileName = "w1_slave";
            string devicePath = Path.Combine(DsDefinitions.w1Path, deviceName);
            string slaveFilePath = Path.Combine(devicePath, slaveFileName);

            measure = null;

            if (File.Exists(slaveFilePath))
            {
                var dsDevice = new DsDevice();

                if (!dsDevice.GetMeasure(slaveFilePath, deviceName, out var dsMeasure))
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"Read temperature from '{slaveFilePath}' failed!");
                }
                else if (dsMeasure != null)
                {
                    measure = dsMeasure;

                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{deviceName}', temperature = '{dsMeasure.Temperature} °C'");

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"{slaveFilePath} cannot be processed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"{slaveFilePath} not found");
            }

            return result;
        }
    }
}
