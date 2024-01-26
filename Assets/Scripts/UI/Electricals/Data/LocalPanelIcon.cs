using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Electricals ;
using UnityEngine ;

namespace Chiyoda.UI
{
  public class LocalPanelIcon : ElectoricalIcon
  {
    protected override void CreateInitialElement( Document document, Action<ElectricalDevices> onFinish )
    {
      var entity = ElectricalsFactory.Create( document, ElectricalDeviceType.LocalPanel ) ;
      entity.Size = new Vector3d(1, 0.5, 1.8);
      onFinish( entity ) ;
    }
  }
}