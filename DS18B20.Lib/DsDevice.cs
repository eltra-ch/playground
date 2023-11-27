using DS18B20.Lib.Interfaces;
using EltraCommon.Logger;
using System.Text.Json.Serialization;

namespace DS18B20.Lib
{
    public class DsDevice : IDsDevice
    {
        #region Private fields

        private readonly IDsBuilder? _builder;
        private readonly IDsDirectory? _directory;

        #endregion

        #region Constr

        public DsDevice()
        {
        }

        public DsDevice(IDsBuilder builder, IDsDirectory directory)
        {
            _builder = builder;
            _directory = directory;
        }

        #endregion

        #region Properties

        [JsonPropertyName("deviceName")]
        public string? Name {  get; set; }

        [JsonPropertyName("measures")]
        public List<IDsMeasure> Measures { get; set; } = new List<IDsMeasure>();

        #endregion

        #region Methods

        public bool GetMeasure(out IDsMeasure? measure)
        {
            const string method = "Read";

            bool result = false;

            measure = null;

            if (!string.IsNullOrEmpty(Name))
            {
                if (!ReadMeasure(out var mea))
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"Read temperature from device {Name} failed!");
                    result = false;
                }
                else if (mea != null)
                {
                    measure = mea;
                    MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{Name}', temperature = '{measure.Temperature} °C'");

                    result = AddMeasure(measure);
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"Device {Name} get measure failed!");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"Device name is empty!");
            }

            return result;
        }

        private bool AddMeasure(IDsMeasure? measure)
        {
            bool result = false;

            if (measure != null)
            {
                Measures.Add(measure);
                result = true;
            }

            return result;
        }

        private IDsMeasure? CreateMeasure()
        {
            IDsMeasure? result = null;

            if (_builder!= null)
            {
                result = _builder.BuildMeasure();
            }

            return result;
        }

        private bool ReadMeasure(out IDsMeasure? measure)
        {
            const string method = "ReadSlaveFile";
            const string slaveFileName = "w1_slave";

            bool result = false;

            measure = null;

            if (_directory == null)
                return false;

            if (!string.IsNullOrEmpty(Name))
            {
                string devicePath = Path.Combine(DsDefinitions.W1Path, Name);
                string slaveFilePath = Path.Combine(devicePath, slaveFileName);

                if (_directory.FileExists(slaveFilePath))
                {
                    var dsMeasure = CreateMeasure();

                    if (dsMeasure != null && !dsMeasure.GetMeasure(slaveFilePath))
                    {
                        MsgLogger.WriteError($"{GetType().Name} - {method}", $"Read temperature from '{slaveFilePath}' failed!");
                    }
                    else if (dsMeasure != null)
                    {
                        measure = dsMeasure;

                        MsgLogger.WriteDebug($"{GetType().Name} - {method}", $"device = '{Name}', temperature = '{dsMeasure.Temperature} °C'");

                        result = true;
                    }
                    else
                    {
                        MsgLogger.WriteError($"{GetType().Name} - {method}", $"{slaveFilePath} cannot be processed!");
                    }
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - {method}", $"{slaveFilePath} not found");
                }
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - {method}", $"device name is empty!");
            }

            return result;
        }

        #endregion
    }
}
