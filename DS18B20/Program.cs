// See https://aka.ms/new-console-template for more information

using DS18B20;
using DS18B20.Lib;
using DS18B20.Lib.Interfaces;
using EltraCommon.Logger;
using System.Reflection;
using Unity;

MsgLogger.LogLevels = "Error;Warning;Exception;Info";
MsgLogger.LogOutputs = MsgLogger.SupportedLogOutputs;

var name = Assembly.GetExecutingAssembly().GetName().Name;
var version = Assembly.GetExecutingAssembly().GetName().Version;

MsgLogger.WriteLine($"{name}", $"{name}, {version}");

IUnityContainer unityContainer = new UnityContainer();

unityContainer.RegisterType<IDsBuilder, DsBuilder>();
unityContainer.RegisterType<IDsDirectory, DsDirectory>();
unityContainer.RegisterType<IDsDevices, DsDevices>();
unityContainer.RegisterType<IDsDevice, DsDevice>();
unityContainer.RegisterType<IDsMeasure, DsMeasure>();

var devices = unityContainer.Resolve<IDsDevices>();

foreach(var device in devices.ActiveDevices)
{
    MsgLogger.WriteLine($"{name}", $"read device = '{device}'...");

    if (device.GetMeasure(out var measure) && measure != null)
    {
        MsgLogger.WriteLine($"{name}", $"device = '{device.Name}', temperature = '{measure.Temperature} {measure.Unit}', time = {measure.Created}");
    }
}

devices.SerializeToJson();