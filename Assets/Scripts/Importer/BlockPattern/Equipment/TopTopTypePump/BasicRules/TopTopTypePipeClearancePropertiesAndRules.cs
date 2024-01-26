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
  /// <summary>
  /// Trombone パイプのある BlockPattern の Tee から伸びるminimum flow の Tee 部分のクリアランスを設定する
  /// </summary>
  class TopTopTypeMiniFlowDischargeDistancePropertiesAndRules
  {
    //  TODO: Set くらいの名前にした方が良い
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      Set(bpa, bp, info);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("MiniFlowDischargeDistance", PropertyType.Length, 0.2);
      bp.RegisterUserDefinedProperty("TMBS", PropertyType.TemporaryValue, 0.0); //  Tee main to branch leg-size.
      bp.RegisterUserDefinedProperty("SPACER", PropertyType.TemporaryValue, 0.0); //  Tee main to branch leg-size.
      rule = bp.RuleList.AddRule(".TMBS", "(#PostDischargeOriFlowAdjuster.MaxX-#PostDischargeOriFlowAdjuster.MinX) - #DischargeEndPipe.Diameter");
      rule = bp.RuleList.AddRule(".SPACER", ".MiniFlowDischargeDistance-.TMBS-DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter)-(#MinimumFlowEndPipe.Diameter*0.5)");
      rule = bp.RuleList.AddRule("#DischargeMinimumFlowSpacerPipe.Length", "Max(.SPACER, 0.001 * DiameterToPipeMinLength(#MinimumFlowEndPipe.Diameter))");
    }
  }
  /// <summary>
  /// Trombone パイプのない BlockPattern の Tee から伸びるminimum flow の Tee 部分のクリアランスを設定する
  /// </summary>
  class TopTopTypeMiniFlowStraitDischargeDistancePropertiesAndRules
  {
    //  TODO: Set くらいの名前にした方が良い
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      Set(bpa, bp, info);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      bp.RegisterUserDefinedProperty("MiniFlowDischargeDistanceW", PropertyType.Length, 0.2);
      bp.RegisterUserDefinedProperty("TMBS", PropertyType.TemporaryValue, 0.0); //  Tee main to branch leg-size.
      bp.RegisterUserDefinedProperty("SPACER", PropertyType.TemporaryValue, 0.0); //  Tee main to branch leg-size.
      rule = bp.RuleList.AddRule(".TMBS", "(#MinimumFlowBranchTee.MaxX-#MinimumFlowBranchTee.MinX) - #DischargeEndPipe.Diameter");
      rule = bp.RuleList.AddRule(".SPACER", ".MiniFlowDischargeDistanceW-.TMBS-DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter)-(#MinimumFlowEndPipe.Diameter*0.5)");
      rule = bp.RuleList.AddRule("#DischargeMinimumFlowSpacerPipe.Length", "Max(.SPACER, 0.001 * DiameterToPipeMinLength(#MinimumFlowEndPipe.Diameter))");

    }
  }
}
