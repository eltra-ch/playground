// See https://aka.ms/new-console-template for more information

using DS18B20;
using DS18B20.Ds18;
using EltraCommon.Logger;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

MsgLogger.LogLevels = "Error;Warning;Exception;Info";
MsgLogger.LogOutputs = MsgLogger.SupportedLogOutputs;

var name = Assembly.GetExecutingAssembly().GetName().Name;
var version = Assembly.GetExecutingAssembly().GetName().Version;

MsgLogger.WriteLine($"{name}", $"{name}, {version}");

var devices = new DsDevices();
var measures = new DsMeasures();

foreach(var deviceName in devices.DeviceNames)
{
    MsgLogger.WriteLine($"{name}", $"read device = '{deviceName}'...");

    if (devices.Read(deviceName, out var measure) && measure != null)
    {
        measures.AddMeasure(deviceName, measure);

        MsgLogger.WriteLine($"{name}", $"device = '{deviceName}', temperature = '{measure.Temperature} {measure.Unit}', time = {measure.Created}");
    }
}

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var json = JsonSerializer.Serialize(measures, options);

MsgLogger.WriteLine(json);


