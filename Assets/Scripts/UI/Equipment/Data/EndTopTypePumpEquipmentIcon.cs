using System ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;

namespace Chiyoda.UI
{
  public class EndTopTypePumpEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.EndTopTypePump );
      HorizontalPumpImporter.PumpImport( "24-P0704A", bp, onFinish ) ;
    }
  }
}