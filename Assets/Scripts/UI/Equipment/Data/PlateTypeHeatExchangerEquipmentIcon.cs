using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;

namespace Chiyoda.UI
{
  public class PlateTypeHeatExchangerEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      PlateTypeHeatExchangerBlockPatternImporter.Import( onFinish ) ;
    }
  }
}