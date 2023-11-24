﻿using DS18B20.Ds18.Interfaces;
using System.Text.Json.Serialization;

namespace DS18B20.Ds18
{
    public class DsMeasure : IDsMeasure
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "°C";
        [JsonPropertyName("created")]
        public DateTime Created { get; set; } = DateTime.Now;
    }
}