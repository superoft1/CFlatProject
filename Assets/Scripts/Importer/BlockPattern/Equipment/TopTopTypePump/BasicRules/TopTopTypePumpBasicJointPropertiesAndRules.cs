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

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  class TopTopTypePumpBasicJointPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      bp.RegisterUserDefinedProperty( "SuctionJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "SuctionEnd" ) ) ;
      bp.RegisterUserDefinedProperty( "DischargeJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "DischargeEnd" ) ) ;

    }
  }
}
