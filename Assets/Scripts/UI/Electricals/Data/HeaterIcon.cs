using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Electricals ;

namespace Chiyoda.UI
{
  public class HeaterIcon : ElectoricalIcon
  {
    protected override void CreateInitialElement( Document document, Action<ElectricalDevices> onFinish )
    {
      var entity = ElectricalsFactory.Create( document, ElectricalDeviceType.Heater ) ;
      onFinish( entity ) ;
    }
  }
}