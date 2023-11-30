using OxyPlot.Series;
using OxyPlot;
using System.ComponentModel;
using OxyPlot.Axes;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using DS18B20.Lib.Interfaces;
using DS18B20.Lib;
using Unity;
using DS18B20.UI;
using System.Collections.Generic;

namespace DS18UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private PlotModel? _model;
        private IDsDevices? _devices;
        private DateTime _selectedDateFrom;
        private DateTime _selectedDateTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            _selectedDateFrom = DateTime.Now;
            _selectedDateTo = _selectedDateFrom;

            CreateDevices();
            CreateModel();
        }

        /// <summary>
        /// Gets the plot model.
        /// </summary>
        public PlotModel? Model 
        { 
            get => _model;
            private set
            {
                _model = value;
                OnPropertyChanged("Model");
            } 
        }

        public DateTime SelectedDateFrom
        {
            get => _selectedDateFrom;
            set
            {
                _selectedDateFrom = value;
                OnPropertyChanged("SelectedDateFrom");
            }
        }

        public DateTime SelectedDateTo
        {
            get => _selectedDateTo;
            set
            {
                _selectedDateTo = value;
                OnPropertyChanged("SelectedDateTo");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task<DsDevices?> GetAsync()
        {
            DsDevices? result = null;
                        
            HttpClient httpClient = new()
            {
                BaseAddress = new Uri(Settings1.Default.Url),
            };

            using HttpResponseMessage response = await httpClient.GetAsync("/ds18");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if(_devices != null && _devices.Deserialize(SerializeMethod.Json, jsonResponse, out var devices))
            {
                result = devices;
            }

            return result;
        }

        private void CreateDevices()
        {
            IUnityContainer unityContainer = new UnityContainer();

            unityContainer.RegisterType<IDsBuilder, DsBuilder>();
            unityContainer.RegisterType<IDsDirectory, DsDirectory>();
            unityContainer.RegisterType<IDsDevices, DsDevices>();
            unityContainer.RegisterType<IDsDevice, DsDevice>();
            unityContainer.RegisterType<IDsMeasure, DsMeasure>();

            _devices = unityContainer.Resolve<IDsDevices>();
        }

        private void CreateModel()
        {
            Task.Run(async () =>
            {
                DsDevices? devices = await GetAsync();

                var model = new PlotModel { Title = "DS18B20", Subtitle = Settings1.Default.Url };

                BuildSeries(model, devices);
                
                BuildAxes(model);

                this.Model = model;
            });
        }

        private void BuildAxes(PlotModel tmp)
        {
            if (tmp.Series.Count > 0)
            {
                tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "temperature" });

                tmp.Axes.Add(new DateTimeAxis()
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    Angle = 90,
                    StringFormat = "HH:mm",
                    MajorStep = 1.0 / 24 / 1, // 1/24 = 1 hour, 1/24/2 = 30 minutes
                    IsZoomEnabled = true,
                    MaximumPadding = 0,
                    MinimumPadding = 0,
                    TickStyle = TickStyle.None
                });
            }
        }

        private void BuildSeries(PlotModel tmp, DsDevices? devices)
        {
            if (devices != null)
            {
                foreach (var device in devices.ActiveDevices)
                {
                    var ls = new LineSeries()
                    {
                        Title = device.Name,
                        MarkerType = MarkerType.Circle,
                        DataFieldX = "Created",
                        DataFieldY = "Temperature"
                    };
                    var bls = new LineSeries()
                    {
                        MarkerType = MarkerType.None,
                        DataFieldX = "Created",
                        DataFieldY = "Temperature"
                    };

                    ls.ItemsSource = new List<IDsMeasure>();
                    bls.ItemsSource = new List<IDsMeasure>();

                    if (ls.ItemsSource is List<IDsMeasure> lst &&
                        bls.ItemsSource is List<IDsMeasure> lst2)
                    {
                        foreach (var measure in device.Measures)
                        {
                            if (measure.Created.Date >= SelectedDateFrom.Date &&
                                measure.Created.Date <= SelectedDateTo.Date)
                            {
                                lst.Add(measure);
                                lst2.Add(new DsMeasure() { Created = measure.Created, Temperature = 0 });
                            }
                        }
                        if (lst.Count > 0)
                        {
                            tmp.Series.Add(ls);
                            tmp.Series.Add(bls);
                        }
                    }
                }
            }
        }

        internal void Refresh()
        {
            CreateModel();
        }
    }
}
