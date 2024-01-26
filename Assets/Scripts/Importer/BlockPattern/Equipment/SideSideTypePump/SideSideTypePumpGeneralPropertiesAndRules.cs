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

namespace Importer.BlockPattern.Equipment.SideSideTypePump
{
  public class SideSideTypePumpGeneralPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, int suctionMinNpsMm, int suctionMaxNpsMm, int dischargeMinNpsMm, int dischargeMaxNpsMm)
    {
      bp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 1.95 ) ;

#if false
      bp.RegisterUserDefinedProperty( "PipeMinLength", PropertyType.Length, 0.5 ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
#endif
      bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
      bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;

      IRule rule;
      rule = bp.RuleList.AddRule( ":Parent.ArrayOffsetX", "( #SuctionEnd.MaxX -#DischargeEnd.MinX ) + .AccessSpace"  ) ;
      rule.AddTriggerSourcePropertyName("SuctionNozzlePipeLength");
      rule.AddTriggerSourcePropertyName("DischargeNozzlePipeLength");

      bp.RegisterUserDefinedProperty( "SuctionJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "SuctionEnd" ) ) ;
      bp.RegisterUserDefinedProperty( "DischargeJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "DischargeEnd" ) ) ;

      string LengthFunc( string diameter ){
        return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeBranchLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )" ;
      }

      bp.RegisterUserDefinedProperty( "DischargeHeaderBOP", PropertyType.Length, 2.5 ) ;
      bp.RuleList.AddRule( "#DischargeEndPipe.Length",
          $"#BasePump.MinZ + .DischargeHeaderBOP - {LengthFunc( "#DischargeEndPipe.Diameter" )} - #NextOfDischargeEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "DischargeDiameter" ) ;
      
      bp.RegisterUserDefinedProperty( "SuctionHeaderBOP", PropertyType.Length, 2.5 ) ;
      bp.RuleList.AddRule( "#SuctionEndPipe.Length",
          $"#BasePump.MinZ + .SuctionHeaderBOP - {LengthFunc( "#SuctionEndPipe.Diameter" )} - #NextOfSuctionEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "SuctionDiameter" ) ;

      bp.RegisterUserDefinedProperty( "HeaderInterval", PropertyType.Length, 0.5 ) ;

      bp.RegisterUserDefinedProperty( "SuctionNozzlePipeLength", PropertyType.Length, 4.3, "0", "10" ) ;
      rule = bp.RuleList.AddRule( "#SuctionNozzlePipe.Length", "#SuctionNozzlePipe.Diameter * .SuctionNozzlePipeLength" )
              .AddTriggerSourcePropertyName( "SuctionDiameter" ) ;
      bp.RegisterUserDefinedProperty( "DischargeNozzlePipeLength", PropertyType.Length, 4.7, "0", "10" ) ;
      bp.RuleList.AddRule( "#DischargeNozzlePipe.Length", "#DischargeNozzlePipe.Diameter * .DischargeNozzlePipeLength" )
              .AddTriggerSourcePropertyName( "DischargeDiameter" ) ;

      //  各JointEdge とBasePump との距離を設定
      bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 3.0);  //  Dischare パイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 3.0);  //  Suctionパイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準は0 Suction基準は1
      bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("D_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("D_EndPos_Y3", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y3", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      #if USE_MINIMUM_LENGTH
      bp.RegisterUserDefinedProperty("FlexTest", PropertyType.Length, 0.0);  //  header interval offset
      #endif
      bp.RegisterUserDefinedProperty("DTest", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      bp.RegisterUserDefinedProperty("STest", PropertyType.TemporaryValue, 0.0);  //  header interval offset

      rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");

      //  System length  (ここのIndex選択は重要)
      rule = bp.RuleList.AddRule(".D_MinSysLength", "MinHorzDistanceOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop)+(#DischargeSystemFlexStartPipe.MinLength + #DischargeSystemFlexStopPipe.MinLength)*0.5");
      rule = bp.RuleList.AddRule(".DTest", "DistanceXYOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop)");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule = bp.RuleList.AddRule(".S_MinSysLength", "MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)+(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");
      #if USE_MINIMUM_LENGTH
      rule.AddTriggerSourcePropertyName("FlexTest");
      #endif
      rule = bp.RuleList.AddRule(".STest", "DistanceXYOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)");
      //rule.AddTriggerSourcePropertyName("SuctionDiameter");
      #if USE_MINIMUM_LENGTH
      rule.AddTriggerSourcePropertyName("FlexTest");
      #endif


      //  Minimum Y coord of DischargeEnd
      rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeSystemFlexOrigin.MaxY + .D_MinSysLength + DiameterToElbow90Length(#DischargeEndPipe.Diameter)"); 
      //  Minimum Y coord of SuctionEnd
      rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)"); 

      #if USE_MINIMUM_LENGTH
        rule = bp.RuleList.AddRule("#SuctionEnd.PosY", "0.0");
        rule.AddTriggerSourcePropertyName("FlexTest");

        rule = bp.RuleList.AddRule("#DischargeEnd.PosY", "0.0");
      #else
        //  Select origin position 
        rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, 1.0, 0.0)"); 

        //  基準座標設定
        rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))"); 
        rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.S_Base > 0, IF(.HeaderInterval >= 0, .S_MinPos_Y - .HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval), .D_MinPos_Y)");

        rule = bp.RuleList.AddRule(".S_EndPos_Y2", "Max(.S_EndPos_Y, .S_MinPos_Y)");
        rule = bp.RuleList.AddRule(".D_EndPos_Y2", "Max(.D_EndPos_Y, .D_MinPos_Y)");

        //  移動不可能時設定
        rule = bp.RuleList.AddRule(".S_EndPos_Y3", "IF(.S_Base > 0, IF (.HeaderInterval >= 0, .D_EndPos_Y2 + .HeaderInterval + .HI_Offset, .S_EndPos_Y2), .S_EndPos_Y2)"); 
        rule = bp.RuleList.AddRule(".D_EndPos_Y3", "IF(.S_Base > 0, .D_EndPos_Y2, IF (.HeaderInterval >= 0, .S_EndPos_Y2 + .HeaderInterval +.HI_Offset, .D_EndPos_Y2))"); 

        rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y3");
        rule = bp.RuleList.AddRule("#DischargeEnd.PosY", ".D_EndPos_Y3");
      #endif
      //SideSideTypePumpMinimumLengthUpdater updater = SideSideTypePumpMinimumLengthUpdater.Create(bpa, bp, info);
      double dinch = info.SuctionDiameterNPSInch;
      var suctionProp = bp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dinch>=8)?6.0:10.0).NpsMm, suctionMinNpsMm, suctionMaxNpsMm);
      suctionProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));
      //suctionProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateSuctionMinimumLengths) );

      var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dinch>=8)?6.0:10.0).NpsMm, dischargeMinNpsMm, dischargeMaxNpsMm);
      dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1)));
      //dischargeProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateDischargeMinimumLengths) );
    }
  }
}
