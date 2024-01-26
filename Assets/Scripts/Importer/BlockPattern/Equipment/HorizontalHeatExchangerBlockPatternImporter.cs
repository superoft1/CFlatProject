using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.HorizontalHeatExchanger ;
using Importer.Equipment ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment
{
  public class HorizontalHeatExchangerBlockPatternImporter
  {
    [Flags]
    public enum HeatExchangerType
    {
      None = 0,
      HE_A1_1_G = 1 << 1,
      HE_A1_1_S = 1 << 2,
      HE_B1_1_G = 1 << 3,
      HE_B1_1_S = 1 << 4,
      HE_C1_1_G = 1 << 5,
      HE_C1_1_S = 1 << 6,
      HE_C2_1_G = 1 << 7,
      HE_C2_1_S = 1 << 8,
      HE_D1_1_G = 1 << 9,
      HE_D2_1_G = 1 << 10,
      HE_D2_2_G = 1 << 11,
      HE_E1_1_G = 1 << 12,
      HE_E1_2_G = 1 << 13,
    }

    private static Dictionary<String, String> typeIdDic = new Dictionary<String, String>()
    {
      { "HE-A1-1-G", "95-HBG62311" },
      { "HE-A1-1-S", "95-HBG62311" },
      { "HE-B1-1-G", "95-HBG62311" },
      { "HE-B1-1-S", "95-HBG62311" },
      { "HE-C1-1-G", "95-HBG62311" },
      { "HE-C1-1-S", "95-HBG62311" },
      { "HE-C2-1-G", "95-HBC63122" },
      { "HE-C2-1-S", "95-HBG62311" },
      { "HE-D1-1-G", "95-HBG62311" },
      { "HE-D2-1-G", "95-HBG62311" },
      { "HE-D2-2-G", "95-HBG62311" },
      { "HE-E1-1-G", "95-HBG62311" },
      { "HE-E1-2-G", "95-HBG62311" },
    };

    public static void Import(HeatExchangerType type, Action<Edge> onFinish)
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      if (type.HasFlag(HeatExchangerType.HE_A1_1_G))
      {
        new HorizontalHeatExchangerBase(curDoc, "HE-A1-1-G").Create(onFinish);
        Debug.Log("HE-A1-1-G");
      }
    }

    public static Chiyoda.CAD.Model.Equipment HorizontalHeatExchangerImport(
      string id,
      Chiyoda.CAD.Topology.BlockPattern bp,
      Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      var table = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
      var (heatExchanger, origin, rot) = table.Generate( bp.Document, typeIdDic[id], createNozzle: true ) ;
      var equip = heatExchanger as Chiyoda.CAD.Model.Equipment ;
      BlockPatternFactory.CreateInstrumentEdgeVertex( bp, equip, origin, rot ) ;
      onFinish?.Invoke(bp);
      return equip;
    }
  }
}
