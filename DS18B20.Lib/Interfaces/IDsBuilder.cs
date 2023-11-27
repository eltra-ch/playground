namespace DS18B20.Lib.Interfaces
{
    public interface IDsBuilder
    {
        IDsDevice? BuildDevice();
        IDsMeasure? BuildMeasure();
    }
}
