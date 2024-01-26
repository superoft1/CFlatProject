#define DEBUG_DUMP

using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.DB ;
using IDF ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  public class EndTopTypePumpUpDownBOPPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("BOP", PropertyType.Length, 0.5);
      bp.RegisterUserDefinedProperty("D_BOPMark1_Z", PropertyType.TemporaryValue, 0.0);  //  BOPMark のZ座標
      bp.RegisterUserDefinedProperty("D_BOPMark2_Z", PropertyType.TemporaryValue, 0.0);  //  BOPMark のZ座標
      bp.RegisterUserDefinedProperty("D_BOPMark3_Z", PropertyType.TemporaryValue, 0.0);  //  BOPMark のZ座標
      bp.RegisterUserDefinedProperty("D_BOP_Z", PropertyType.TemporaryValue, 0.0);  //  BOP のZ座標
      bp.RegisterUserDefinedProperty("D_BOPShimTop", PropertyType.TemporaryValue, 0.0);  //  Top of bop spacer pipe
      bp.RegisterUserDefinedProperty("D_BOPShimLen", PropertyType.TemporaryValue, 0.0);  //  Length of bop spacer pipe 
      bp.RegisterUserDefinedProperty("D_BOPOffset", PropertyType.TemporaryValue, 0.0);  //  Top of bop spacer pipe
      bp.RegisterUserDefinedProperty("D_NozzleLen", PropertyType.TemporaryValue, 0.0);  //  Length of discharge nozzle pipe 

      rule = bp.RuleList.AddRule(".D_BOP_Z", "#BasePump.MinZ+.BOP");
      rule = bp.RuleList.AddRule(".D_BOPOffset", "#DischargeEndPipe.Diameter*0.5+(DiameterToElbow90Length(#DischargeEndPipe.Diameter))"); 
      rule.AddTriggerSourcePropertyName("DischargeDiameter");

      rule = bp.RuleList.AddRule(".D_BOPMark1_Z", "#DischargeBOPMark1.MaxZ"); //  ポイントとなる基準点１ 下
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule = bp.RuleList.AddRule(".D_BOPMark2_Z", "#DischargeBOPMark2.MinZ"); //  ポイントとなる基準点２ 上
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule = bp.RuleList.AddRule(".D_BOPMark3_Z", "#DischargeBOPMark3.MinZ"); //  ポイントとなる基準点３
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      
      //  DischargeNozzle のパイプ長を最小値にした時のSpacerのTop
      rule = bp.RuleList.AddRule(".D_BOPShimTop", "(.D_BOPMark3_Z -.D_BOPMark2_Z) + (#DischargeBOPMark1.MaxZ + #DischargeNozzlePipe.MinLength)");

      bp.RuleList.AddRule(".D_BOPShimLen", ".D_BOPShimTop - .D_BOP_Z - .D_BOPOffset");
      rule = bp.RuleList.AddRule(".D_NozzleLen", "#DischargeNozzlePipe.MinLength + Max(0, #DischargeBOPSpacerPipe.MinLength-.D_BOPShimLen)");  //  Spacer がそのMinLength より下回ろうとしていたらNozzle を伸ばす
      
      bp.RuleList.AddRule("#DischargeBOPSpacerPipe.Length", ".D_BOPShimLen");
      bp.RuleList.AddRule("#DischargeNozzlePipe.Length", ".D_NozzleLen");
      bp.RuleList.AddRule("#DischargeSystemFlexStart.MinZ", ".D_BOP_Z");  //  Flexを使ってBOPパイプ移動

      { 
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedZ ;
        }
      }
    }
  }
}
