using System.Text.Json.Serialization;

namespace DS18B20.Ds18
{
    public class DsDeviceMeasures
    {
        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; } = string.Empty;

        [JsonPropertyName("measures")]
        public List<DsDeviceMeasure> Measures { get; set; } = new List<DsDeviceMeasure>();

        internal void AddMeasure(DsDeviceMeasure measure)
        {
            Measures.Add(measure);
        }
    }
}
