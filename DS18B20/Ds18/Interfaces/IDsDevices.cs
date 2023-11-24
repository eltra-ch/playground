namespace DS18B20.Ds18.Interfaces
{
    public interface IDsDevices
    {
        List<IDsDevice> ActiveDevices { get; }

        bool SerializeToJson();
    }
}
