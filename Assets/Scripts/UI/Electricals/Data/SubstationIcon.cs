using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Electricals ;
using UnityEngine ;

namespace Chiyoda.UI
{
  public class SubstationIcon : ElectoricalIcon
  {
    protected override void CreateInitialElement( Document document, Action<ElectricalDevices> onFinish )
    {
      var entity = ElectricalsFactory.Create( document, ElectricalDeviceType.Substation ) ;
      entity.Size = new Vector3d(20, 10, 3);
      onFinish( entity ) ;
    }
  }
}