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
  class EndTopTypePumpMinimumFlowJointsPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty("SuctionJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("SuctionEnd"));
      ;
      bp.RegisterUserDefinedProperty("DischargeJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("DischargeEnd"));
      ;
      bp.RegisterUserDefinedProperty("MinimumFlowJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("MinimumFlowEnd"));
    }
  }
}
