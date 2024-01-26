using System;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment;

namespace Chiyoda.UI
{
  public class SideSideTypePumpEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement(Document document, Action<Edge> onFinish)
    {
      var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.SideSideTypePump);
      HorizontalPumpImporter.PumpImport("SS-A1-H-H", bp, onFinish);
    }
  }
}