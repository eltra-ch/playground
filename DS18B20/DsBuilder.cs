using DS18B20.Lib.Interfaces;
using Unity;

namespace DS18B20
{
    public class DsBuilder : IDsBuilder
    {
        private readonly IUnityContainer _unityContainer;

        public DsBuilder(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public IDsDevice? BuildDevice()
        {
            return _unityContainer.Resolve<IDsDevice>();
        }

        public IDsMeasure? BuildMeasure()
        {
            return _unityContainer.Resolve<IDsMeasure>();
        }
    }
}
