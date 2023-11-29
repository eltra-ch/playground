namespace DS18B20.Lib.Interfaces
{
    public interface IDsDevice
    {
        string? Name { get; set; }

        List<IDsMeasure> Measures { get; set; }

        void CopyTo(IDsDevice? device);
        IDsMeasure? FindMeasureById(string? id);
        bool GetMeasure(out IDsMeasure? measure);
    }
}
