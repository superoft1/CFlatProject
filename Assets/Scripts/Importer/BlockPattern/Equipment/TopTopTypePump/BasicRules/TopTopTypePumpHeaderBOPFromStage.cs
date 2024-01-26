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
  class TopTopTypePumpSuctionHeaderBOPFromStage
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      //  Length of joint edge's legs. 
      string LengthFunc(string diameter) {
          return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )";
      }
      bp.RegisterUserDefinedProperty( "SuctionHeaderBOP", PropertyType.Length, 2.7 ) ;
      IRule rule = bp.RuleList.AddRule("#SuctionEndPipe.Length",
          $"#BasePump.MinZ + .SuctionHeaderBOP + .StageHeight - {LengthFunc("#SuctionEndPipe.Diameter")} - #NextOfSuctionEnd.MaxZ");
      rule.AddTriggerSourcePropertyName( "SuctionDiameter" ) ;
      rule.AddTriggerSourcePropertyName( "SuctionStageBOP" ) ;
    }
  }
  class TopTopTypePumpHeaderBOPFromStage
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      //  Length of joint edge's legs. 
      string LengthFunc(string diameter) {
          return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )";
      }
      bp.RegisterUserDefinedProperty( "SuctionHeaderBOP", PropertyType.Length, 2.7 ) ;
      IRule rule = bp.RuleList.AddRule("#SuctionEndPipe.Length",
          $"#BasePump.MinZ + .SuctionHeaderBOP + .StageHeight - {LengthFunc("#SuctionEndPipe.Diameter")} - #NextOfSuctionEnd.MaxZ");
      rule.AddTriggerSourcePropertyName( "SuctionDiameter" ) ;
      rule.AddTriggerSourcePropertyName( "SuctionStageBOP" ) ;
 
       bp.RegisterUserDefinedProperty( "DischargeHeaderBOP", PropertyType.Length, 2.180 ) ;

      bp.RuleList.AddRule( "#DischargeEndPipe.Length",
          $"#BasePump.MinZ + .DischargeHeaderBOP + .StageHeight - {LengthFunc( "#DischargeEndPipe.Diameter" )} - #NextOfDischargeEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "DischargeDiameter" ) ;
      rule.AddTriggerSourcePropertyName( "DischargeStageBOP" ) ;
    }
  }
  class TopTopTypePumpHeaderBOPFromStageWithMinimumFlow
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      TopTopTypePumpHeaderBOPFromStage.SetPropertiesAndRules(bpa, bp, info);
      //  Length of joint edge's legs. 
      bp.RegisterUserDefinedProperty( "MiniFlowHeaderBOP", PropertyType.Length, 2.7 ) ;
      IRule rule = bp.RuleList.AddRule("#MinimumFlowHeaderBOPSpacerPipe.Length",
          $"#BasePump.MinZ + .MiniFlowHeaderBOP + .StageHeight - (DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter)-(#MinimumFlowEndPipe.Diameter*0.5)) - #NextOfMinimumFlowHeaderBOPSpacer.MaxZ");
      rule.AddTriggerSourcePropertyName( "MiniFlowDiameter" ) ;
 
    }
  }
  class TopTopTypePumpHeaderBOPFromStageWithMinimumFlowOverTOP
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      Set(bpa, bp, info);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      TopTopTypePumpHeaderBOPFromStage.SetPropertiesAndRules(bpa, bp, info);
      //  Length of joint edge's legs. 
      string LengthFunc(string diameter) {
          return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )";
      }
      bp.RegisterUserDefinedProperty( "MiniFlowHeaderBOP", PropertyType.Length, 2.7 ) ;
      IRule rule = bp.RuleList.AddRule("#MinimumFlowEndPipe.Length",
          $"#BasePump.MinZ + .MiniFlowHeaderBOP + .StageHeight - {LengthFunc("#MinimumFlowEndPipe.Diameter")} -#NextOfMinimumFlowEnd.MaxZ");
      rule.AddTriggerSourcePropertyName( "MiniFlowDiameter" ) ;
    }
  }
}
