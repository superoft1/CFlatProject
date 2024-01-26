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
using Chiyoda.DB ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  /// <summary>
  /// TopTopTypePump に、ミニフローの機能を付けるためのstatic メソッドをまとめたクラス
  /// </summary>
  public class TopTopTypeMinimumFlowExtention
  {
    /// <summary>
    /// TopTopTypePump のミニフロー付きを対象にEdgeName を追加する
    /// </summary>
    /// <remarks>
    /// TopTopTypePumpBase.SetEdgeNames を呼び出してからこのメソッドを呼び出すこと
    /// </remarks>
    /// <param name="target">ミニフロー付きBasePump オブジェクト</param>
    /// <param name="info">インデックス情報を格納したクラス</param>
    public static void AppendEdgeNames( TopTopTypePumpBase target, SingleBlockPatternIndexInfo info )
    {
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow)) {
        return; //  error
      }
      var infoDerived = (SingleBlockPatternIndexInfoWithMinimumFlow)info;

      Chiyoda.CAD.Topology.BlockPattern bp = target.BaseBp;
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
      foreach (SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType value in Enum.GetValues(typeof(SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType))) {
        if ( ! infoDerived.SuctionAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
        if (edge == null) {
          continue;
        }
        TopTopTypePumpBase.SetEdgeNameStatic(edge, Enum.GetName(typeof(SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType), value));
      }
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;

      foreach (SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType value in Enum.GetValues(typeof(SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType))) {
        if ( ! infoDerived.DischargeAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
        if (edge == null) {
          continue;
        }
        TopTopTypePumpBase.SetEdgeNameStatic(edge, Enum.GetName(typeof(SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType), value));
      }

      var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;

      foreach (SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType value
            in Enum.GetValues(typeof(SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType))
      )
      {
        if ( ! infoDerived.MinimumFlowIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        //  get minimumflow-end and next of minimumflow-end
        var edge = minimumFlowGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
        if (edge == null) {
          continue;
        }

        TopTopTypePumpBase.SetEdgeNameStatic(edge, Enum.GetName(
              typeof(SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType), value));
      }

      var minimumFlowPreGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowPreIndex) as IGroup;

      foreach (SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowPreIndexType value
            in Enum.GetValues(typeof(SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowPreIndexType))
      )
      {
        if ( ! infoDerived.MinimumFlowPreIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        //  get minimumflow-end and next of minimumflow-end
        var edge = minimumFlowPreGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
        if (edge == null)
        {
          continue;
        }
        TopTopTypePumpBase.SetEdgeNameStatic(edge, Enum.GetName(
              typeof(SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowPreIndexType), value));
      }
    }

    /// <summary>
    /// TopTopTypePump のミニフロー付きを対象にインデックス情報からFlex と Origin を設定する
    /// </summary>
    /// <remarks>
    /// TopTopTypePumpBase.SetBlockPatternInfo と置き換える。両方呼んではならない
    /// </remarks>
    /// <param name="target">ターゲットとなるBasePump</param>
    /// <param name="info">インデックス情報を格納したクラス</param>
    public static void SetBlockPatternInfoMinimumFlow(TopTopTypePumpBase target, SingleBlockPatternIndexInfo info)
    {
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow)) {
        return; //  error
      }
      var infoDerived = (SingleBlockPatternIndexInfoWithMinimumFlow)info;
      var bp = target.BaseBp;

      var groupList = bp.NonEquipmentEdges.ToList();
      var dischargeGroup = groupList[info.DischargeIndex] as Group;
      dischargeGroup.Name = "DischargePipes";
      var suctionGroup = groupList[info.SuctionIndex] as Group;
      suctionGroup.Name = "SuctionPipes";

      if (!TopTopTypePumpBase.INDEXING_NOW)
        target.SetPropertyAndRule(info);

      if (info.DischargeFlexIndexList != null){ 
        foreach (var flex in info.DischargeFlexIndexList) {
          target.SetFlexRatio(dischargeGroup, flex, 1);
        }
      }

      if (info.SuctionFlexIndexList != null) {
        foreach (var flex in info.SuctionFlexIndexList) {
          target.SetFlexRatio(suctionGroup, flex, 1);
        }
      }

      var basePump = bp.EquipmentEdges.ElementAtOrDefault( info.BasePumpIndex ) ;
      basePump.ObjectName = "BasePump";
      basePump.ConnectionMaintenanceOrigin = basePump;
      foreach (var value in info.DischargeIndexTypeValue.Values.Where(v => v >= 0)) {
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump;
      }
      foreach ( var value in infoDerived.DischargeAdditionalIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      

      foreach (var value in info.SuctionIndexTypeValue.Values.Where(v => v >= 0)) {
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump;
      }

      target.SetEdgeNames(info);

      if (!TopTopTypePumpBase.INDEXING_NOW){
        if (info.DischargeAngleGroupIndexList != null){
          var edges1 = info.DischargeAngleGroupIndexList.Select(v => dischargeGroup?.EdgeList.ElementAtOrDefault(v) as LeafEdge);
          var group1 = Group.CreateContinuousGroup(edges1.WithOlets().ToArray());
          group1.Name = "DischargeGroup";
          group1.ObjectName = "DischargeGroup";
          group1.ConnectionMaintenanceOrigin = basePump;
        }
        target.OnGroupChanged();
      }
      
      groupList = bp.NonEquipmentEdges.ToList();
      var minimumFlowGroup = groupList[infoDerived.MinimumFlowIndex] as Group;
      minimumFlowGroup.Name = "MinimumFlowPipes";

      if (infoDerived.MinimumFlowFlexIndexList != null) {
        foreach (var flex in infoDerived.MinimumFlowFlexIndexList) {
          target.SetFlexRatio(minimumFlowGroup, flex, 1);
        }
      }

      foreach (var value in infoDerived.MinimumFlowIndexTypeValue.Values.Where(v => v >= 0)) {
        var edge = minimumFlowGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump;
      }
      var minimumFlowPreGroup = groupList[infoDerived.MinimumFlowPreIndex] as Group;

      minimumFlowPreGroup.Name = "MinimumFlowPrePipes";
      foreach ( var value in infoDerived.MinimumFlowPreIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = minimumFlowPreGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      
      HorizontalPumpImporter.AlignAllLeafEdges( bp, basePump ) ;
    
      bp.RuleList.BindChangeEvents( true ) ;
      if ( null != target.BpOwner ) {
        target.BpOwner.BaseBlockPattern = bp ;
        target.BpOwner.RuleList.BindChangeEvents(true);
      }
    }

    /// <summary>
    /// TopTopTypePump のミニフロー付きを対象とした、PostProcess メソッドへの追加メソッド
    /// </summary>
    /// <remarks>
    /// TopTopTypePumpBase.PostProcess を呼び出してからこのメソッドを呼び出すこと
    /// </remarks>
    /// <param name="target">ターゲットとなるBasePump</param>
    /// <param name="info">インデックス情報を格納したクラス</param>

    public static void PostProcessMinimumFlow(TopTopTypePumpBase target)
    {
      if (!TopTopTypePumpBase.INDEXING_NOW){
        var bpa = target.BpOwner ;
        bpa.GetProperty( "MinimumFlowJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      }
    }
  }
}
