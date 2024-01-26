using System;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment;

namespace Chiyoda.UI
{
  public class TopTopTypePumpEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement(Document document, Action<Edge> onFinish)
    {
      var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.TopTopTypePump);
      HorizontalPumpImporter.PumpImport("TT-A1-S-G-N", bp, onFinish);
    }
  }
}