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
  class EndTopTypePumpMinimumFlowJointDistancePropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
    
      bool dischargeFlexSystem = false;
      int index;
      if (info.DischargeIndexTypeValue.TryGetValue(SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,out index)){
        dischargeFlexSystem = true;
      }
      bp.RegisterUserDefinedProperty( "MiniFlowJointDistance", PropertyType.Length, 0.7 ) ;
      bp.RegisterUserDefinedProperty( "MHJ_Offset", PropertyType.TemporaryValue, 0.0  ) ; //  offset for MiniFlowHeaderJointDistance
      bp.RuleList.AddRule(".MHJ_Offset", "2.0*(DiameterToElbow90Length( #MinimumFlowEndPipe.Diameter ) - #MinimumFlowEndPipe.Diameter*0.5)");
      rule = bp.RuleList.AddRule("#MinimumFlowEndPipe.Length", ".MiniFlowJointDistance - .MHJ_Offset");
      rule.AddTriggerSourcePropertyName( "MiniFlowDiameter" ) ;
      if (dischargeFlexSystem)
        rule.AddTriggerSourcePropertyName( "DischargeSystemFlexStart","PosY" ) ;
      
      bp.RegisterUserDefinedProperty( "DischargeHeaderJointDistance", PropertyType.Length, 0.7 ) ;
      bp.RegisterUserDefinedProperty( "DHJ_Offset", PropertyType.TemporaryValue, 0.0  ) ; //  offset for MiniFlowHeaderJointDistance
      bp.RuleList.AddRule(".DHJ_Offset", "2.0*(DiameterToElbow90Length( #DischargeEndPipe.Diameter ) - #DischargeEndPipe.Diameter*0.5)");

      rule = bp.RuleList.AddRule("#DischargeEndPipe.Length", ".DischargeHeaderJointDistance - .DHJ_Offset");
      rule.AddTriggerSourcePropertyName( "DischargeDiameter" ) ;
      if (dischargeFlexSystem)
        rule.AddTriggerSourcePropertyName( "DischargeSystemFlexStart","PosY" ) ;

      if (info.SuctionFlexHelper != null){
        //  HeaderIntereval
        bp.RegisterUserDefinedProperty("HeaderInterval", PropertyType.Length, 4.5);

        //  各JointEdge とBasePump との距離を設定
        bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Dischare パイプの一番短くできる長さ
        bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Suctionパイプの一番短くできる長さ
        bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
        bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
        bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準は0 Suction基準は1
        bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
        bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
        bp.RegisterUserDefinedProperty("D_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("S_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("D_EndPos_Y3", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("S_EndPos_Y3", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("D_EndPos_Offset", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
        rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");
        rule = bp.RuleList.AddRule(".D_EndPos_Offset", "3.0*DiameterToElbow90Length(#DischargeEndPipe.Diameter)+#DischargeEndPipe.Length");

        //  System length      
        rule = bp.RuleList.AddRule(".D_MinSysLength", "MinHorzDistanceOf(#DischargeMinLength1, #DischargeMinLength2)+(#DischargeMinLengthPipe1.MinLength + #DischargeMinLengthPipe2.MinLength)*0.5");
        rule = bp.RuleList.AddRule(".S_MinSysLength", "MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)+(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");

        //  Minimum Y coord of DischargeEnd
        rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeOriFlowHeaderBOPGoal.MaxY + .D_MinSysLength + .D_EndPos_Offset"); 

        //  Minimum Y coord of SuctionEnd
        rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)"); 

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

        //  LeafEdge に反映
        rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y3");
        rule = bp.RuleList.AddRule("#NextOfDischargeOriFlowPath.MinY", ".D_EndPos_Y3 - .D_EndPos_Offset");

        if (info.SuctionFlexHelper != null){ 
          var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
          var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
          if (edge != null){
            edge.PositionMode = PositionMode.FixedY ;
          }
        }
      }

    }
  }
}
