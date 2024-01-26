using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;

namespace Chiyoda.UI
{
  public class HorizontalVesselEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      HorizontalVesselBlockPatternImporter.Import( onFinish ) ;
    }
  }
}