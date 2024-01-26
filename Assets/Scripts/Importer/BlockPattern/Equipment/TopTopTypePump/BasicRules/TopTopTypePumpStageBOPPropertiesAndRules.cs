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
  class TopTopTypePumpStageBOPPropertiesAndRules
  {
    /// <summary>
    /// StageBOP Properties and rules for both suction pipes and discharge pipes
    /// </summary>
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty("DischargeStageBOP", PropertyType.Length, 0.5);
      IRule rule = bp.RuleList.AddRule("#DischargeSystemFlexStart.MinZ",
          $"#BasePump.MinZ + .StageHeight + .DischargeStageBOP");
      rule.AddTriggerSourcePropertyName("StageHeight");

      bp.RegisterUserDefinedProperty("SuctionStageBOP", PropertyType.Length, 0.5);
      rule = bp.RuleList.AddRule("#SuctionNozzlePipe.Length",
          $"#BasePump.MinZ + .StageHeight + .SuctionStageBOP - #SuctionNozzleFlange.MaxZ  - (DiameterToElbow90Length(#SuctionEndPipe.Diameter) - (0.5 * #SuctionEndPipe.Diameter)) - (#SuctionNozzleReducer.MaxZ - #SuctionNozzleReducer.MinZ)");

      rule.AddTriggerSourcePropertyName("StageHeight");
      {
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedZ;
        }
      }
    }
  }
  class TopTopTypePumpUnifiedStageBOPPropertiesAndRules
  {
    /// <summary>
    /// StageBOP Properties and rules for both suction pipes and discharge pipes
    /// </summary>
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("StageBOP", PropertyType.Length, 0.5);
      bp.RegisterUserDefinedProperty("DischargeStageBOP", PropertyType.TemporaryValue, 0.5);
      bp.RegisterUserDefinedProperty("SuctionStageBOP", PropertyType.TemporaryValue, 0.5);
      rule = bp.RuleList.AddRule(".DischargeStageBOP", ".StageBOP");
      rule = bp.RuleList.AddRule(".SuctionStageBOP", ".StageBOP");

      rule = bp.RuleList.AddRule("#DischargeSystemFlexStart.MinZ",
          $"#BasePump.MinZ + .StageHeight + .DischargeStageBOP");
      rule.AddTriggerSourcePropertyName("StageHeight");

      rule = bp.RuleList.AddRule("#SuctionNozzlePipe.Length",
          $"#BasePump.MinZ + .StageHeight + .SuctionStageBOP - #SuctionNozzleFlange.MaxZ  - (DiameterToElbow90Length(#SuctionEndPipe.Diameter) - (0.5 * #SuctionEndPipe.Diameter)) - (#SuctionNozzleReducer.MaxZ - #SuctionNozzleReducer.MinZ)");

      rule.AddTriggerSourcePropertyName("StageHeight");
      {
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedZ;
        }
      }
    }
  }
  /// <summary>
  /// StageBOP Properties and rules for suction pipes excluding discharge pipes
  /// </summary>
  class TopTopTypePumpSuctionStageBOPPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty("StageBOP", PropertyType.Length, 0.5);
      bp.RegisterUserDefinedProperty("SuctionStageBOP", PropertyType.TemporaryValue, 0.5);
      IRule rule = bp.RuleList.AddRule("#SuctionNozzlePipe.Length",
          $"#BasePump.MinZ + .StageHeight + .StageBOP - #SuctionNozzleFlange.MaxZ  - (DiameterToElbow90Length(#SuctionEndPipe.Diameter) - (0.5 * #SuctionEndPipe.Diameter)) - (#SuctionNozzleReducer.MaxZ - #SuctionNozzleReducer.MinZ)");
      rule.AddTriggerSourcePropertyName("StageHeight");
      bp.RuleList.AddRule(".SuctionStageBOP", ".StageBOP");
    }
  }
  /// <summary>
  /// StageBOP Properties and rules for suction pipes excluding discharge pipes
  /// </summary>
  class TopTopTypePumpSuctionOnlyStageBOPPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      Set(bpa, bp, info);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty("SuctionStageBOP", PropertyType.Length, 0.5);
      IRule rule = bp.RuleList.AddRule("#SuctionNozzlePipe.Length",
          $"#BasePump.MinZ + .StageHeight + .SuctionStageBOP - #SuctionNozzleFlange.MaxZ  - (DiameterToElbow90Length(#SuctionEndPipe.Diameter) - (0.5 * #SuctionEndPipe.Diameter)) - (#SuctionNozzleReducer.MaxZ - #SuctionNozzleReducer.MinZ)");
      rule.AddTriggerSourcePropertyName("StageHeight");
    }
  }

  class TopTopTypePumpDischargeStageBOPOverTOPPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      Set(bpa, bp, info);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty("DischargeStageBOP", PropertyType.Length, 0.5);
      IRule rule = bp.RuleList.AddRule("#DischargeBOPSpacerPipe.Length",
          $"#BasePump.MinZ + .StageHeight + .DischargeStageBOP - (DiameterToElbow90Length(#DischargeEndPipe.Diameter) - (0.5 * #DischargeEndPipe.Diameter)) - #NextOfDischargeBOPSpacer.MaxZ");

      bp.RegisterUserDefinedProperty( "TOP", PropertyType.Length, 0.7 ) ;

      rule = bp.RuleList.AddRule("#LowerDischargeBOP.MaxZ","#BasePump.MinZ + .StageHeight - .TOP");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");

      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived){ 
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.LowerDischargeBOP]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedZ;
        }
      }
    }
  }

  class TopTopTypePumpMinimumFlowStageBOPOverTOPPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty("MiniFlowStageBOP", PropertyType.Length, 0.5);
      IRule rule = bp.RuleList.AddRule("#MinimumFlowStageBOPSpacerPipe.Length",
          $"#BasePump.MinZ + .StageHeight + .MiniFlowStageBOP - (DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter) - (0.5 * #MinimumFlowEndPipe.Diameter)) - #NextOfMinimumFlowStageBOPSpacer.MaxZ");
      /*
      bp.RegisterUserDefinedProperty( "TOP", PropertyType.Length, 0.7 ) ;

      rule = bp.RuleList.AddRule("#LowerDischargeBOP.MaxZ","#BasePump.MinZ + .StageHeight - .TOP");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");

      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived){ 
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.LowerDischargeBOP]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedZ;
        }
      }
      */
    }
  }

}
