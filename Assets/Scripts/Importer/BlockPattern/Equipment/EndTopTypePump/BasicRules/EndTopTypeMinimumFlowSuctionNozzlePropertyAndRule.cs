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
  class EndTopTypeMinimumFlowSuctionNozzlePropertyAndRule
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty( "SuctionNozzlePipeLength", PropertyType.Length, 4, "0", "10" ) ;
      rule = bp.RuleList.AddRule( "#SuctionNozzlePipe.Length", "#SuctionNozzlePipe.Diameter * .SuctionNozzlePipeLength" ) ;
      rule.AddTriggerSourcePropertyName("SuctionSystemFlexOrigin", "MaxY");
    }
  }
}
