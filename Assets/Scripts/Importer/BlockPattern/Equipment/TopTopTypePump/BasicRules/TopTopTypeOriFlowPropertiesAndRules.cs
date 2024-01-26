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
  class TopTopTypeOriFlowTOPPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfoWithMinimumFlow info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("OriInMinLength", PropertyType.Length, 5.462);  //  上限下限は今は適当です  
      bp.RegisterUserDefinedProperty("OriOutMinLength", PropertyType.Length, 2.191);  //  上限下限は今は適当です

      rule = bp.RuleList.AddRule("#DischargeMinLengthPipe1.Length", "Max(.OriInMinLength,0.001*DiameterToPipeMinLength(#DischargeMinLengthPipe1.Diameter))");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule = bp.RuleList.AddRule("#DischargeMinLengthPipe2.Length", "Max(.OriOutMinLength,0.001*DiameterToPipeMinLength(#DischargeMinLengthPipe2.Diameter))");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");


    }
  }
}
