namespace DS18B20.Ds18.Interfaces
{
    public interface IDsDevice
    {
        string? Name { get; set; }

        List<IDsMeasure> Measures { get; set; }

        bool Read(out IDsMeasure? measure);
    }
}
