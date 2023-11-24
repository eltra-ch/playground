namespace DS18B20.Ds18.Interfaces
{
    public interface IDsMeasure
    {
        double Temperature { get; set; }

        string Unit { get; set; }

        DateTime Created { get; set; }
    }
}
