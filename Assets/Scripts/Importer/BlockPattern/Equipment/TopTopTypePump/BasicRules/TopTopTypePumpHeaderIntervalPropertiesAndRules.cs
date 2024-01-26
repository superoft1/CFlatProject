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
  public class TopTopTypePumpBasicHeaderIntervalPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      Set(bpa, bp, info, false);
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, bool dischargeMinlength = false)
    {
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

      rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");

      //  System length      
      rule = bp.RuleList.AddRule(".D_MinSysLength", "SystemMinHorzDistanceOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop)+(#DischargeSystemFlexStartPipe.MinLength + #DischargeSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      if (dischargeMinlength) {
        rule.AddTriggerSourcePropertyName("ConOriInMinLength");
        rule.AddTriggerSourcePropertyName("ConOriOutMinLength");
      }
      rule = bp.RuleList.AddRule(".S_MinSysLength", "SystemMinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)+(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");
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
      rule = bp.RuleList.AddRule(".SYS_Short_Y", "Max(0,Max(.S_MinPos_Y-.S_EndPos_Y,.D_MinPos_Y-.D_EndPos_Y))");

      //  LeafEdge に反映
      rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y + .SYS_Short_Y");
      rule = bp.RuleList.AddRule("#DischargeEnd.PosY", ".D_EndPos_Y + .SYS_Short_Y");

      {
        var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
      }

    }
  }
  /// <summary>
  /// MiniFlowSystemFlexOriginが暴れていると思われるケースへの対応
  /// </summary>
  class TopToptypePumpFixSuctionSystemFlexOriginOnYCoord
  {
    public static void Stop(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
        var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
    }
  }

  /// <summary>
  /// MiniFlowEnd が水平方向のPump でのHeaderInterval 設定
  /// </summary>
  public class     TopTopTypePumpMiniFlowHorizonalEndHeaderIntervalPropertiesAndRules
  {
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info,bool dischargeMinlength = false)
    {
      IRule rule;
      //  各JointEdge とBasePump との距離を設定
      bp.RegisterUserDefinedProperty("HeaderInterval", PropertyType.Length, 0.5);
      bp.RegisterUserDefinedProperty("MiniFlowHeaderInterval", PropertyType.Length, 2.0);

      //  HeaderInterval
      bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Dischare パイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Suctionパイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("M_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  MiniFlow パイプの一番短くできる長さ

      bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("M_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  MinimumFlow の一番近くできる座標

      bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Suction基準のとき1
      bp.RegisterUserDefinedProperty("D_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準のとき1
      bp.RegisterUserDefinedProperty("M_Base", PropertyType.TemporaryValue, 0.0);  // MinimumFlow基準のとき1
 
      bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 制約設定前の座標
      bp.RegisterUserDefinedProperty("M_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 

      bp.RegisterUserDefinedProperty("SYS_Short_Y", PropertyType.TemporaryValue, 0.0);  //  Y座標の不足分

      bp.RegisterUserDefinedProperty("D_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 制約設定後の座標
      bp.RegisterUserDefinedProperty("M_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 

      bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      bp.RegisterUserDefinedProperty("MS_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      bp.RegisterUserDefinedProperty("MD_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset

      rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");
      rule = bp.RuleList.AddRule(".MS_Offset", "(#MinimumFlowEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");
      rule = bp.RuleList.AddRule(".MD_Offset", "(#MinimumFlowEndPipe.Diameter + #DischargeEndPipe.Diameter)*0.5");

      //  System length      
      rule = bp.RuleList.AddRule(".D_MinSysLength", "MinHorzDistanceOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop)+(#DischargeSystemFlexStartPipe.MinLength + #DischargeSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");
      if (dischargeMinlength) {
        rule.AddTriggerSourcePropertyName("ConOriInMinLength");
        rule.AddTriggerSourcePropertyName("ConOriOutMinLength");
      }
      rule = bp.RuleList.AddRule(".S_MinSysLength", "MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)+(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("SuctionDiameter");

      rule = bp.RuleList.AddRule(".M_MinSysLength", "#MinimumFlowEndPipe.MinLength");
      rule.AddTriggerSourcePropertyName("MiniFlowDiameter");

      //  Minimum Y coord of DischargeEnd
      rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeSystemFlexOrigin.MaxY + .D_MinSysLength + DiameterToElbow90Length(#DischargeEndPipe.Diameter)");

      //  Minimum Y coord of SuctionEnd
      rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)");

      //  Minimum Y coord of MinimumFlowEnd
      rule = bp.RuleList.AddRule(".M_MinPos_Y", "#NextOfMinimumFlowEnd.MaxY + .M_MinSysLength  + DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter)");
      rule.AddTriggerSourcePropertyName("HeaderInterval");

      //  Select origin position 
      rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, IF(.S_MinPos_Y>=.M_MinPos_Y,1.0,0.0),0.0)");
      rule = bp.RuleList.AddRule(".D_Base", "IF(.S_Base == 0, 1.0, 0.0)");

      //  基準座標設定
 
      rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))");
      rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.D_Base > 0, .D_MinPos_Y, IF(.HeaderInterval >= 0, .S_MinPos_Y-.HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval ))");
      rule = bp.RuleList.AddRule(".M_EndPos_Y", 
        "IF(.S_Base > 0, IF(.MiniFlowHeaderInterval >= 0, .S_MinPos_Y-.MS_Offset - .MiniFlowHeaderInterval, .S_MinPos_Y + .MS_Offset - .MiniFlowHeaderInterval),IF(.MiniFlowHeaderInterval >= 0, .D_MinPos_Y-.MD_Offset - .MiniFlowHeaderInterval, .D_MinPos_Y + .MD_Offset - .MiniFlowHeaderInterval ))");

      //  移動不可能判定
      rule = bp.RuleList.AddRule(".SYS_Short_Y", "Max(0,Max(.S_MinPos_Y-.S_EndPos_Y,Max(.M_MinPos_Y-.M_EndPos_Y,.D_MinPos_Y-.D_EndPos_Y)))");

      //  LeafEdge に反映
      bp.RegisterUserDefinedProperty("MinEnd_Len", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      bp.RegisterUserDefinedProperty("Result_Len", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y + .SYS_Short_Y");
      rule = bp.RuleList.AddRule("#DischargeEnd.PosY", ".D_EndPos_Y + .SYS_Short_Y");
      rule = bp.RuleList.AddRule("#MinimumFlowEndPipe.Length", ".M_EndPos_Y + .SYS_Short_Y - #NextOfMinimumFlowEnd.MaxY - DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter)");


      rule.AddTriggerSourcePropertyName("SYS_Short_Y");
      rule.AddTriggerSourcePropertyName("HeaderInterval");

      {
        var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
      }
    }

  }

  /// <summary>
  /// MiniFlowEnd が垂直方向のPump でのHeaderInterval 設定
  /// </summary>
  public class     TopTopTypePumpMiniFlowVerticalEndHeaderIntervalPropertiesAndRules
  {
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      IRule rule;
      //  各JointEdge とBasePump との距離を設定
      bp.RegisterUserDefinedProperty("HeaderInterval", PropertyType.Length, 0.5);
      bp.RegisterUserDefinedProperty("MiniFlowHeaderInterval", PropertyType.Length, 2.0);

      //  HeaderInterval
      bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Dischare パイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Suctionパイプの一番短くできる長さ
      bp.RegisterUserDefinedProperty("M_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  MiniFlow パイプの一番短くできる長さ

      bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
      bp.RegisterUserDefinedProperty("M_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  MinimumFlow の一番近くできる座標

      bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Suction基準のとき1
      bp.RegisterUserDefinedProperty("D_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準のとき1
      bp.RegisterUserDefinedProperty("M_Base", PropertyType.TemporaryValue, 0.0);  // MinimumFlow基準のとき1

      bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 制約設定前の座標
      bp.RegisterUserDefinedProperty("M_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 

      bp.RegisterUserDefinedProperty("SYS_Short_Y", PropertyType.TemporaryValue, 0.0);  //  Y座標の不足分

      bp.RegisterUserDefinedProperty("D_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("S_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 制約設定後の座標
      bp.RegisterUserDefinedProperty("M_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 

      bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      bp.RegisterUserDefinedProperty("MS_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
      bp.RegisterUserDefinedProperty("MD_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset

      rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");
      rule = bp.RuleList.AddRule(".MS_Offset", "(#MinimumFlowEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");
      rule = bp.RuleList.AddRule(".MD_Offset", "(#MinimumFlowEndPipe.Diameter + #DischargeEndPipe.Diameter)*0.5");

      //
      //  動作機序　Flexで伸び縮みする一連パイプの最短長を求める
      //
      rule = bp.RuleList.AddRule(".D_MinSysLength", "MinHorzDistanceOf(#DischargeSystemFlexStart, #DischargeSystemFlexStop)+(#DischargeSystemFlexStartPipe.MinLength + #DischargeSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");

      rule = bp.RuleList.AddRule(".S_MinSysLength", "MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)+(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("SuctionDiameter");

      rule = bp.RuleList.AddRule(".M_MinSysLength", "MinHorzDistanceOf(#MinimumFlowSystemFlexStart, #MinimumFlowSystemFlexStop)+(#MinimumFlowSystemFlexStartPipe.MinLength + #MinimumFlowSystemFlexStopPipe.MinLength)*0.5");
      rule.AddTriggerSourcePropertyName("MiniFlowDiameter");

      //
      //  FixedY で設定したHeader を動かすパイプが一番Pump に近づける座標を算出する
      //
      //  Minimum Y coord of DischargeEnd
      rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeSystemFlexOrigin.MaxY + .D_MinSysLength + DiameterToElbow90Length(#DischargeEndPipe.Diameter)");

      //  Minimum Y coord of SuctionEnd
      rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)");

      //  Minimum Y coord of MinimumFlowEnd
      rule = bp.RuleList.AddRule(".M_MinPos_Y", "#MinimumFlowSystemFlexOrigin.MaxY + .M_MinSysLength + DiameterToElbow90Length(#MinimumFlowEndPipe.Diameter)");

      //  Select origin position 
      rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, IF(.S_MinPos_Y>=.M_MinPos_Y,1.0,0.0),0.0)");
      rule = bp.RuleList.AddRule(".D_Base", "IF(.S_Base == 0, 1.0, 0.0)");

      //  基準座標設定
      rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))");
      rule.AddTriggerSourcePropertyName("MiniFlowHeaderInterval");

      rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.D_Base > 0, .D_MinPos_Y, IF(.HeaderInterval >= 0, .S_MinPos_Y-.HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval ))");
      rule.AddTriggerSourcePropertyName("MiniFlowHeaderInterval");

      rule = bp.RuleList.AddRule(".M_EndPos_Y",
        "IF(.S_Base > 0, IF(.MiniFlowHeaderInterval >= 0, .S_MinPos_Y-.MS_Offset - .MiniFlowHeaderInterval, .S_MinPos_Y + .MS_Offset - .MiniFlowHeaderInterval),IF(.MiniFlowHeaderInterval >= 0, .D_MinPos_Y-.MD_Offset - .MiniFlowHeaderInterval, .D_MinPos_Y + .MD_Offset - .MiniFlowHeaderInterval ))");
      rule.AddTriggerSourcePropertyName("HeaderInterval");

      //  移動不可能があるときその不足分を算出
      rule = bp.RuleList.AddRule(".SYS_Short_Y", "Max(0,Max(.S_MinPos_Y-.S_EndPos_Y,Max(.M_MinPos_Y-.M_EndPos_Y,.D_MinPos_Y-.D_EndPos_Y)))");

      //  不足分を３本ともにプラスして、全体に遠方にずらせる
      rule = bp.RuleList.AddRule(".S_EndPos_Y2", ".S_EndPos_Y + .SYS_Short_Y");
      rule.AddTriggerSourcePropertyName("D_EndPos_Y");
      rule.AddTriggerSourcePropertyName("M_EndPos_Y");
      rule = bp.RuleList.AddRule(".D_EndPos_Y2", ".D_EndPos_Y + .SYS_Short_Y");
      rule.AddTriggerSourcePropertyName("S_EndPos_Y");
      rule.AddTriggerSourcePropertyName("M_EndPos_Y");
      rule = bp.RuleList.AddRule(".M_EndPos_Y2", ".M_EndPos_Y + .SYS_Short_Y");
      rule.AddTriggerSourcePropertyName("S_EndPos_Y");
      rule.AddTriggerSourcePropertyName("D_EndPos_Y");

      //  LeafEdge に反映
      rule = bp.RuleList.AddRule("#SuctionEnd.PosY", "DebugLog(.S_EndPos_Y2)");
      rule = bp.RuleList.AddRule("#DischargeEnd.PosY", "DebugLog(.D_EndPos_Y2)");
      rule = bp.RuleList.AddRule("#MinimumFlowEnd.PosY", "DebugLog(.M_EndPos_Y2)");

      bp.RegisterUserDefinedProperty("D_RESULT", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("S_RESULT", PropertyType.TemporaryValue, 0.0);  // 制約設定後の座標
      bp.RegisterUserDefinedProperty("M_RESULT", PropertyType.TemporaryValue, 0.0);  // 
      bp.RegisterUserDefinedProperty("M_ORIGIN_Y", PropertyType.TemporaryValue, 0.0);  // 
      rule = bp.RuleList.AddRule(".D_RESULT", "DebugLog(#DischargeEnd.PosY)");
      rule = bp.RuleList.AddRule(".S_RESULT", "DebugLog(#SuctionEnd.PosY)");
      rule = bp.RuleList.AddRule(".M_RESULT", "DebugLog(#MinimumFlowEnd.PosY)");
      rule = bp.RuleList.AddRule(".M_ORIGIN_Y", "DebugLog(#MinimumFlowSystemFlexOrigin.MaxY)");

      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
        edge = minimumFlowGroup?.EdgeList.ElementAtOrDefault(infoDerived.MinimumFlowIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowEnd]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedY;
        }
      }
    }
  }
}
