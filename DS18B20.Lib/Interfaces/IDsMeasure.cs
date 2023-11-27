namespace DS18B20.Lib.Interfaces
{
    public interface IDsMeasure
    {
        double Temperature { get; set; }

        string Unit { get; set; }

        DateTime Created { get; set; }

        bool GetMeasure(string source);
    }
}
