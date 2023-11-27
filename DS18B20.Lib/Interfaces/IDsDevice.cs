namespace DS18B20.Lib.Interfaces
{
    public interface IDsDevice
    {
        string? Name { get; set; }

        List<IDsMeasure> Measures { get; set; }

        bool GetMeasure(out IDsMeasure? measure);
    }
}
