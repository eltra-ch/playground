using DS18B20.Ds18.Interfaces;
using EltraCommon.Logger;
using System.Text.Json.Serialization;

namespace DS18B20.Ds18
{
    public class DsDevice : IDsDevice
    {
        #region Constr

        public DsDevice()
        {
        }

        #endregion

        #region Properties

        [JsonPropertyName("deviceName")]
        public string? Name {  get; set; }

        [JsonPropertyName("measures")]
        public List<IDsMeasure> Measures { get; set; } = new List<IDsMeasure>();

        #endregion

        #region Methods

        private bool AddMeasure(IDsMeasure? measure)
        {
            bool result = false;

            if (measure != null)
            {
                Measures.Add(measure);
                result = true;
            }

            return result;
        }

        public bool Read(out IDsMeasure? measure)
        {
            const string method = "Read";

            bool result = false;

            measure = null;

            if (!string.IsNullOrEmpty(Name))
            {
                if (!GetMeasure(out var mea))
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"Read temperature from device {Name} failed!");
                    result = false;
                }
                else if (mea != null)
                {
                    measure = mea;
                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{Name}', temperature = '{measure.Temperature} °C'");

                    result = AddMeasure(measure);
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"Device {Name} get measure failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"Device name is empty!");
            }

            return result;
        }

        public bool GetMeasure(out IDsMeasure? measure)
        {
            const string method = "ReadSlaveFile";
            const string slaveFileName = "w1_slave";

            bool result = false;

            measure = null;

            if (!string.IsNullOrEmpty(Name))
            {
                string devicePath = Path.Combine(DsDefinitions.W1Path, Name);
                string slaveFilePath = Path.Combine(devicePath, slaveFileName);

                if (File.Exists(slaveFilePath))
                {
                    if (!GetMeasure(slaveFilePath, out var dsMeasure))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - {method}", $"Read temperature from '{slaveFilePath}' failed!");
                    }
                    else if (dsMeasure != null)
                    {
                        measure = dsMeasure;

                        MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{Name}', temperature = '{dsMeasure.Temperature} °C'");

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
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"device name is empty!");
            }

            return result;
        }

        public bool GetMeasure(string slaveFilePath, out IDsMeasure? measure)
        {
            const string method = "ReadTemperature";
            bool result = false;
            measure = null;

            MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Found device = {Name}, slave file = {slaveFilePath}");

            var content = File.ReadAllText(slaveFilePath);

            var prefix = "t=";
            int i = content.LastIndexOf(prefix, StringComparison.OrdinalIgnoreCase);

            if (i > 0)
            {
                var temp = content.Substring(i + prefix.Length);

                if (int.TryParse(temp, out int temperature))
                {
                    double tempCel = (double)Math.Round((decimal)temperature / 1000, 2);

                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{Name}', temperature = '{tempCel} °C'");

                    measure = new DsMeasure() { Temperature = tempCel };

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

        #endregion
    }
}
