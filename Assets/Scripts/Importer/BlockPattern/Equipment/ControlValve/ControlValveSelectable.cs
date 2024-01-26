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
  public class ControlValveSelectable : ControlValveBase
  {
    public ControlValveSelectable(Document doc) : base(doc, "Valve Set", "SV-N1", "DV-N2")
    {
      Info = new SingleBlockPatternIndexInfo {
        MainGroupIndices = new int[] { 0, 0 },  //  念のため、個々のパターンごとに、ベースグループのインデックスを指定できるようにする
        OriginLeafEdgeIndices = new int[][] { new int[]{ 2 }, new int[]{ 2,12 } },  //  座標基準 index を指定、単数の時は指定LeafEdgeの中央、複数のときはその中間
        BasicIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.BasicIndexType, int>{
          { SingleBlockPatternIndexInfo.BasicIndexType.InletTube, 0},
          { SingleBlockPatternIndexInfo.BasicIndexType.ControlValveA, 2},
          { SingleBlockPatternIndexInfo.BasicIndexType.OutletTube, 4},
        }
      };
    }
    public override Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish){
      return base.Create(onFinish);
    }
    protected override void SetPropertyAndRule(SingleBlockPatternIndexInfo info)
    {
      var prop = BaseBp.RegisterUserDefinedProperty("SelectValve", ValveSelectItems.Items.SV_N1);
      var rule = new ValveSelectorRule(info,BaseBp,ControlValveBase.ExtractChildBlockPatterns(BaseBp));
      prop.AddUserDefinedRule(rule);
    }
  }
}
