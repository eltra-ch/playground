using DS18B20.Lib.Interfaces;
using EltraCommon.Logger;
using System.IO;
using System.Text.Json.Serialization;

namespace DS18B20.Lib
{
    public class DsMeasure : IDsMeasure
    {
        private readonly IDsDirectory? _directory;

        public DsMeasure()
        {
        }

        public DsMeasure(IDsDirectory directory)
        {
            _directory = directory;
        }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "°C";
        [JsonPropertyName("created")]
        public DateTime Created { get; set; } = DateTime.Now;

        public bool GetMeasure(string source)
        {
            const string method = "GetMeasure";
            bool result = false;
            
            MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"Found device, source = {source}");

            if (_directory == null)
                return false;

            var content = _directory.ReadAllTextFromFile(source);

            var prefix = "t=";
            int i = content.LastIndexOf(prefix, StringComparison.OrdinalIgnoreCase);

            if (i > 0)
            {
                var temp = content.Substring(i + prefix.Length);

                if (int.TryParse(temp, out int temperature))
                {
                    double tempCel = (double)Math.Round((decimal)temperature / 1000, 2);

                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"source = '{source}', temperature = '{tempCel} °C'");

                    Temperature = tempCel;

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"cannot parse temperature from text = {temp}");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"temp not found in file {source}");
            }

            return result;
        }
    }
}
