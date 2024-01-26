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
  class EndTopTypePumpMinimumFlowBOPandOriFlowPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfoWithMinimumFlow info)
    {

      string LengthFunc( string diameter )
      {
        return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )" ;
      }
      IRule rule;

      bp.RegisterUserDefinedProperty( "DischargeHeaderBOP", PropertyType.Length, 4.5  ) ;
      bp.RegisterUserDefinedProperty( "SuctionHeaderBOP", PropertyType.Length, 2.3 ) ;
      bp.RegisterUserDefinedProperty( "MiniFlowHeaderBOP", PropertyType.Length, 5.5 ) ;
      bp.RegisterUserDefinedProperty( "OriHeaderBOP", PropertyType.Length, 4.2 ) ;

      bp.RuleList.AddRule( "#SuctionEndPipe.Length",
          $"#BasePump.MinZ + .SuctionHeaderBOP - {LengthFunc( "#SuctionEndPipe.Diameter" )} - #NextOfSuctionEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "SuctionDiameter" ) ;

      bp.RegisterUserDefinedProperty( "DH_BOP_Offset", PropertyType.TemporaryValue, 0.0  ) ;
      bp.RuleList.AddRule(".DH_BOP_Offset", "DiameterToElbow90Length( #DischargeEndPipe.Diameter ) - #DischargeEndPipe.Diameter*0.5");
      
      rule = bp.RuleList.AddRule("#DischargeHeaderBOPSpacerPipe.Length", ".DischargeHeaderBOP + #BasePump.MinZ - #NextOfDischargeHeaderBOPSpacer.MaxZ - .DH_BOP_Offset");
      rule.AddTriggerSourcePropertyName("NextOfDischargeHeaderBOPSpacer","PosZ");

      bp.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.5 ) ;
      bp.RegisterUserDefinedProperty( "BOP_Z", PropertyType.TemporaryValue, 0.0 ) ;
      rule = bp.RuleList.AddRule(".BOP_Z", "#BasePump.MinZ + .BOP");
      rule.AddTriggerSourcePropertyName( "DischargeDiameter" ) ;

      rule = bp.RuleList.AddRule("#DischargeBOP.MinZ", ".BOP_Z");
      rule.AddTriggerSourcePropertyName("DischargeOriFlowHeaderBOPGoal", "PosZ");

      bp.RegisterUserDefinedProperty( "OriInMinLength", PropertyType.Length, 5.462) ;  //  上限下限は今は適当です  
      bp.RegisterUserDefinedProperty( "OriOutMinLength", PropertyType.Length, 2.191) ;  //  上限下限は今は適当です
      if ( info.SuctionFlexHelper != null ){
        rule = bp.RuleList.AddRule( "#DischargeMinLengthPipe1.MinLength", "Max(.OriInMinLength,0.001*DiameterToPipeMinLength(#DischargeMinLengthPipe1.Diameter))" ) ;
        rule.AddTriggerSourcePropertyName("DischargeDiameter");
        rule = bp.RuleList.AddRule( "#DischargeMinLengthPipe2.MinLength", "Max(.OriOutMinLength,0.001*DiameterToPipeMinLength(#DischargeMinLengthPipe2.Diameter))" ) ;
        rule.AddTriggerSourcePropertyName("DischargeDiameter");
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfDischargeOriFlowPath]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedY ;
        }
      }else{
        rule = bp.RuleList.AddRule( "#DischargeMinLengthPipe1.Length", "Max(.OriInMinLength,0.001*DiameterToPipeMinLength(#DischargeMinLengthPipe1.Diameter))" ) ;
        rule.AddTriggerSourcePropertyName("DischargeDiameter");
        rule = bp.RuleList.AddRule( "#DischargeMinLengthPipe2.Length", "Max(.OriOutMinLength,0.001*DiameterToPipeMinLength(#DischargeMinLengthPipe2.Diameter))" ) ;
        rule.AddTriggerSourcePropertyName("DischargeDiameter");
      }

      bp.RegisterUserDefinedProperty( "OH_BOP_Z", PropertyType.TemporaryValue, 4.2 ) ;
      bp.RegisterUserDefinedProperty( "OH_BOP_Offset", PropertyType.TemporaryValue, 4.2 ) ;
      bp.RuleList.AddRule(".OH_BOP_Offset", "DiameterToElbow90Length( #DischargeMinLengthPipe2.Diameter ) - #DischargeMinLengthPipe2.Diameter*0.5");
      bp.RuleList.AddRule(".OH_BOP_Z", "#BasePump.MinZ + .OriHeaderBOP - .OH_BOP_Offset");
      bp.RuleList.AddRule( "#DischargeOriFlowHeaderBOPGoal.MinZ", ".OH_BOP_Z" ).AddTriggerSourcePropertyName("DischargeOriFlowHeaderBOPGoal","PosZ") ;
      { 
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeOriFlowHeaderBOPGoal]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedZ ;
        }
      }

      bp.RegisterUserDefinedProperty( "MH_BOP_Offset", PropertyType.TemporaryValue, 0.0  ) ;  //  offset for MiniFlowHeaderBOP
      bp.RuleList.AddRule(".MH_BOP_Offset", "DiameterToElbow90Length( #MinimumFlowEndPipe.Diameter ) - #MinimumFlowEndPipe.Diameter*0.5");
      rule = bp.RuleList.AddRule("#MinimumFlowHeaderBOPSpacerPipe.Length", ".MiniFlowHeaderBOP + #BasePump.MinZ - #NextOfMinimumFlowHeaderBOPSpacer.MaxZ - .MH_BOP_Offset");
      rule.AddTriggerSourcePropertyName("NextOfDischargeHeaderBOPSpacer","PosZ");

    }
  }
}
