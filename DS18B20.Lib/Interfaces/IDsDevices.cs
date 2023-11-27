namespace DS18B20.Lib.Interfaces
{
    public interface IDsDevices
    {
        List<IDsDevice> ActiveDevices { get; }

        bool SerializeToJson();
    }
}
