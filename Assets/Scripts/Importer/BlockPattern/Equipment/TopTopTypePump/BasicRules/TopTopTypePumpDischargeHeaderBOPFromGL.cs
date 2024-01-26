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
  class TopTopTypePumpDischargeHeaderBOPFromGL
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      bp.RegisterUserDefinedProperty( "DischargeHeaderBOP", PropertyType.Length, 2.180 ) ;

      //  Length of joint edge's legs. 
      string LengthFunc(string diameter) {
          return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )";
      }

      bp.RuleList.AddRule( "#DischargeEndPipe.Length",
          $"#BasePump.MinZ + .DischargeHeaderBOP - {LengthFunc( "#DischargeEndPipe.Diameter" )} - #NextOfDischargeEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "DischargeDiameter" ) ;
    }
  }
}
