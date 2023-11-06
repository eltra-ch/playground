using System.Text.Json.Serialization;

namespace DS18B20.Ds18
{
    public class DsMeasures
    {
        [JsonPropertyName("deviceMeasures")]
        public List<DsDeviceMeasures> DeviceMeasures { get; set; } = new List<DsDeviceMeasures>();

        public DsDeviceMeasures? GetDeviceMeasures(string deviceName)
        {
            DsDeviceMeasures? result = null;

            foreach (var device in DeviceMeasures)
            {
                if(device.DeviceName == deviceName)
                {
                    result = device;
                    break;
                }
            }

            return result;
        }

        public void AddMeasure(string deviceName, DsDeviceMeasure measure)
        {
            var deviceMeasurements = GetSetDeviceMeasures(deviceName);

            deviceMeasurements.AddMeasure(measure);
        }

        private DsDeviceMeasures GetSetDeviceMeasures(string deviceName)
        {
            var result = GetDeviceMeasures(deviceName);

            if (result == null)
            {
                result = new DsDeviceMeasures() { DeviceName = deviceName };

                DeviceMeasures.Add(result);
            }

            return result;
        }
    }
}
