using System.Text.Json;
using System.Text.Json.Serialization;

namespace DS18B20.Lib.Converters
{
    public class DsDevicesConverter : JsonConverter<DsDevices>
    {
        public override DsDevices? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, DsDevices value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
