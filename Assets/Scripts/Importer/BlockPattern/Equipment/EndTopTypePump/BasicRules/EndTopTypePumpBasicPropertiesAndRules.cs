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
  public class EndTopTypePumpBasicPropertiesAndRules
  {
   public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      IRule rule;
      string LengthFunc(string diameter) {
        return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )";
      }

      bp.RegisterUserDefinedProperty("SuctionJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("SuctionEnd"));
      bp.RegisterUserDefinedProperty("DischargeJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("DischargeEnd"));

      bp.RegisterUserDefinedProperty("DischargeHeaderBOP", PropertyType.Length, 2.3);
      bp.RuleList.AddRule("#DischargeEndPipe.Length",
          $"#BasePump.MinZ + .DischargeHeaderBOP - {LengthFunc("#DischargeEndPipe.Diameter")} - #NextOfDischargeEnd.MaxZ")
        .AddTriggerSourcePropertyName("DischargeDiameter");

      bp.RegisterUserDefinedProperty("SuctionHeaderBOP", PropertyType.Length, 2.3);
      bp.RuleList.AddRule("#SuctionEndPipe.Length",
          $"#BasePump.MinZ + .SuctionHeaderBOP - {LengthFunc("#SuctionEndPipe.Diameter")} - #NextOfSuctionEnd.MaxZ")
        .AddTriggerSourcePropertyName("SuctionDiameter");

      bp.RegisterUserDefinedProperty("NozzlePipeLength", PropertyType.Length, 4, "0", "10");
      rule = bp.RuleList.AddRule("#SuctionNozzlePipe.Length", "#SuctionNozzlePipe.Diameter * .NozzlePipeLength");
      rule.AddTriggerSourcePropertyName("SuctionDiameter");

   }
  }
}
