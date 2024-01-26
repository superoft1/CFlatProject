using System;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment;

namespace Chiyoda.UI
{
  public class HorizontalHeatExchangerEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.HorizontalHeatExchanger);
      HorizontalPumpImporter.PumpImport("95-HBG62311", bp, onFinish);
    }
  }
}