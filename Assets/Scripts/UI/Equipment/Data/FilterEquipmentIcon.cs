using System;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment;

namespace Chiyoda.UI
{
  public class FilterEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.Filter);
      HorizontalPumpImporter.PumpImport("124-S001", bp, onFinish);
    }
  }
}