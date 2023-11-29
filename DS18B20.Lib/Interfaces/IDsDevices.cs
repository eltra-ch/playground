namespace DS18B20.Lib.Interfaces
{
    public interface IDsDevices
    {
        List<IDsDevice> ActiveDevices { get; set; }

        bool Serialize(SerializeMethod method, out string target);

        bool Deserialize(SerializeMethod method, string source, out DsDevices? devices);
        IDsDevice? FindDeviceByName(string? name);
    }
}
