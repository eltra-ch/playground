namespace DS18B20.Lib.Interfaces
{
    public interface IDsMeasure
    {
        string? Id { get; set; }
        double Temperature { get; set; }
        string Unit { get; set; }
        DateTime Created { get; set; }

        bool GetMeasure(string source);
        void CopyTo(IDsMeasure? measure);
    }
}
