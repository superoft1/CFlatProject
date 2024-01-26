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
#if false
  class EndTopTypePumpBasicHeaderIntervalPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      IRule rule;
      //  各JointEdge とBasePump との距離を設定
      bp.RegisterUserDefinedProperty("HeaderInterval", PropertyType.Length, 0.5);

      //  HeaderInterval
      bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Dischare パイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Suctionパイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準は0 Suction基準は1
      bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("SYS_Short_Y", PropertyType.TemporaryValue, 0.0);   // Y座標不足分
      bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      rule = bp.RuleList.AddRule(".HI_Offset", "DebugLog(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");

      //  System length      
      rule = bp.RuleList.AddRule(".D_MinSysLength", 
        "DebugLog(  MinHorzDistanceOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop) )  +(#DischargeSystemFlexStartPipe.MinLength + #DischargeSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule = bp.RuleList.AddRule(".S_MinSysLength", 
        "DebugLog( MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop) )  +(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("SuctionDiameter");

      //  Minimum Y coord of DischargeEnd
      rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeSystemFlexOrigin.MaxY + .D_MinSysLength + DiameterToElbow90Length(#DischargeEndPipe.Diameter)"); 

      //  Minimum Y coord of SuctionEnd
      rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)"); 

      //  Select origin position 
      rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, 1.0, 0.0)"); 

      //  基準座標設定
      rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))"); 
      rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.S_Base > 0, IF(.HeaderInterval >= 0, .S_MinPos_Y - .HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval), .D_MinPos_Y)");

      //  移動不可能判定
      rule = bp.RuleList.AddRule(".SYS_Short_Y", "Max(0,Max(.S_MinPos_Y-.S_EndPos_Y,.D_MinPos_Y-.D_EndPos_Y ))");

      //  LeafEdge に反映
      rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y + .SYS_Short_Y");
      rule = bp.RuleList.AddRule("#DischargeEnd.PosY", ".D_EndPos_Y + .SYS_Short_Y");

      { 
        var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedY ;
        }
        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedY ;
        }
      }

    }
  }
#else
  class EndTopTypePumpBasicHeaderIntervalPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      IRule rule;
      //  各JointEdge とBasePump との距離を設定
      bp.RegisterUserDefinedProperty("HeaderInterval", PropertyType.Length, 0.5);

      //  HeaderInterval
      bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Dischare パイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Suctionパイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準は0 Suction基準は1
      bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("SYS_Short_Y", PropertyType.TemporaryValue, 0.0);   // Y座標不足分
      bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      rule = bp.RuleList.AddRule(".HI_Offset", "DebugLog(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");

      //  System length      
      rule = bp.RuleList.AddRule(".D_MinSysLength", 
        "DebugLog(  MinHorzDistanceOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop) )  +(#DischargeSystemFlexStartPipe.MinLength + #DischargeSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      rule = bp.RuleList.AddRule(".S_MinSysLength", 
        "DebugLog( MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop) )  +(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("SuctionDiameter");

      //  Minimum Y coord of DischargeEnd
      rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeSystemFlexOrigin.MaxY + .D_MinSysLength + DiameterToElbow90Length(#DischargeEndPipe.Diameter)"); 

      //  Minimum Y coord of SuctionEnd
      rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)"); 

      //  Select origin position 
      rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, 1.0, 0.0)"); 

      //  基準座標設定
      rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))"); 
      rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.S_Base > 0, IF(.HeaderInterval >= 0, .S_MinPos_Y - .HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval), .D_MinPos_Y)");

      //  移動不可能判定
      rule = bp.RuleList.AddRule(".SYS_Short_Y", "Max(0,Max(.S_MinPos_Y-.S_EndPos_Y,.D_MinPos_Y-.D_EndPos_Y ))");

      //  LeafEdge に反映
      rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y + .SYS_Short_Y");
      rule = bp.RuleList.AddRule("#DischargeEnd.PosY", ".D_EndPos_Y + .SYS_Short_Y");

      { 
        var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedY ;
        }
        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null){
          edge.PositionMode = PositionMode.FixedY ;
        }
      }

    }
  }
#endif
}
