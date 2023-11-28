using DS18B20.Lib.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Unity;

namespace DS18B20
{
    [ExcludeFromCodeCoverage]
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
