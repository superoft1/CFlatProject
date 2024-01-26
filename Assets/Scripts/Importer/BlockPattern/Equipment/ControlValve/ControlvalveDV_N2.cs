using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text.RegularExpressions ;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.ControlValve
{
  public class ControlValveDV_N2 : ControlValveBase
  {
    public ControlValveDV_N2(Document doc) : base(doc, "Valve Set", "DV-N2")
    {
      Info = new SingleBlockPatternIndexInfo {
        MainGroupIndices = new int[] { 0 },
        OriginLeafEdgeIndices = new int[][] { new int[] { 2, 12 } }, //  ControlValveA と ControlValveB の中間を基準とする
        BasicIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.BasicIndexType, int>{
          { SingleBlockPatternIndexInfo.BasicIndexType.InletTube, 0},
          { SingleBlockPatternIndexInfo.BasicIndexType.ControlValveA, 2},
          { SingleBlockPatternIndexInfo.BasicIndexType.ControlValveB, 12},
          { SingleBlockPatternIndexInfo.BasicIndexType.OutletTube, 14},
        }
      };
    }
    public override Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish){
      return base.Create(onFinish);
    }
  }
}
