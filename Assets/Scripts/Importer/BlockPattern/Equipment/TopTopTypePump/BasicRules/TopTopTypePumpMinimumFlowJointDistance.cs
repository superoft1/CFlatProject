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

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  class TopTopTypePumpMinimumFlowJointDistance
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("MiniFlowJointDistance", PropertyType.Length, 2.0);
      bp.RegisterUserDefinedProperty("MHJ_Offset", PropertyType.TemporaryValue, 0.0); //  offset for MiniFlowHeaderJointDistance
      bp.RuleList.AddRule(".MHJ_Offset", "2.0*(DiameterToElbow90Length( #MinimumFlowEndPipe.Diameter ) - #MinimumFlowEndPipe.Diameter*0.5)");
      rule = bp.RuleList.AddRule("#MinimumFlowEndPipe.Length", "#BasePump.MaxY+.MiniFlowJointDistance - .MHJ_Offset - #NextOfMinimumFlowEnd.MaxY");
      rule.AddTriggerSourcePropertyName("MiniFlowDiameter");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule.AddTriggerSourcePropertyName("HeaderInterval");
    }
  }

  class TopTopTypePumpMinimumFlowJointDistanceOverStage
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      Set(bpa, bp, info, 0.7);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, double defaultValue)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("MiniFlowJointDistance", PropertyType.Length, defaultValue);
      bp.RegisterUserDefinedProperty("MHJ_Offset", PropertyType.TemporaryValue, 0.0); //  offset for MiniFlowHeaderJointDistance
      bp.RuleList.AddRule(".MHJ_Offset", "#MinimumFlowEndPipe.Diameter*0.5");
      rule = bp.RuleList.AddRule("#MinimumFlowEnd.MinY", "#BasePump.MaxY + .MiniFlowJointDistance");
      rule.AddTriggerSourcePropertyName("MiniFlowDiameter");
      rule.AddTriggerSourcePropertyName("MinimumFlowStageBOPSpacer","PosY");
      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived){ 
        var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;
        var edge = minimumFlowGroup?.EdgeList.ElementAtOrDefault(infoDerived.MinimumFlowIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
      }
    }
  }
}
