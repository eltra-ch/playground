using EltraCommon.Logger;

namespace DS18B20.Ds18
{
    internal class DsDevice
    {
        public bool GetMeasure(string slaveFilePath, string deviceName, out DsDeviceMeasure? measure)
        {
            const string method = "ReadTemperature";
            bool result = false;

            measure = null;

            MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Found device = {deviceName}, slave file = {slaveFilePath}");

            var content = File.ReadAllText(slaveFilePath);

            var prefix = "t=";
            int i = content.LastIndexOf(prefix, StringComparison.OrdinalIgnoreCase);

            if (i > 0)
            {
                var temp = content.Substring(i + prefix.Length);

                if (int.TryParse(temp, out int temperature))
                {
                    double tempCel = (double)Math.Round((decimal)temperature / 1000, 2);

                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{deviceName}', temperature = '{tempCel} °C'");

                    measure = new DsDeviceMeasure() { Temperature = tempCel };

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"cannot parse temperature from text = {temp}");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"temp not found in file {slaveFilePath}");
            }
     
            return result;
        }
    }
}
