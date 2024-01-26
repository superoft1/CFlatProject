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

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  /*
   *  <Summary>SetEdgeNames する際に、MinimumFlow付きのEndTopTypePump で共通する処理をまとめた</Summary>
   */
  public class EndTopTypeMinimumFlowExtension
  {
    public static void AppendEdgeNames( EndTopTypePumpBase target, SingleBlockPatternIndexInfo info )
    {
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow)){
        return; //  error
      }
      Chiyoda.CAD.Topology.BlockPattern bp = target.BaseBp;
      var infoDerived = (SingleBlockPatternIndexInfoWithMinimumFlow)info;
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;

      if (infoDerived.SuctionAdditionalIndexTypeValue != null){
        foreach ( SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType ) ) ) {
          if ( ! infoDerived.SuctionAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }
          var edge = suctionGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
          if ( edge == null ) {
            continue ;
          }
          EndTopTypePumpBase.SetEdgeNameStatic( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType ), value ) ) ;
        }
      }
      
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;

      if (infoDerived.DischargeAdditionalIndexTypeValue != null){
        foreach ( SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType ) ) ) {
          if ( ! infoDerived.DischargeAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }
          var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
          if ( edge == null ) {
            continue ;
          }
          EndTopTypePumpBase.SetEdgeNameStatic( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType ), value ) ) ;
        }
      }

      var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;

      if (infoDerived.MinimumFlowIndexTypeValue != null){
        foreach ( SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType value 
              in Enum.GetValues( typeof( SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType ) ) 
        )
        {
          //  get minimumflow-end and next of minimumflow-end
          if ( ! infoDerived.MinimumFlowIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }
          var edge = minimumFlowGroup?.EdgeList.ElementAtOrDefault(index) as LeafEdge;
          if ( edge == null ) {
            continue ;
          }
          EndTopTypePumpBase.SetEdgeNameStatic( edge, Enum.GetName( 
                typeof( SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType ), value ) ) ;

          if (value == SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowInletReducer){
            var reducer = edge.PipingPiece as EccentricPipingReducerCombination;
            if (reducer != null){
              reducer.EnableChangeSizeLevelPropagation = false;
            }
          }
        }
      }
    }
    
  /*
   *  <Summary>SetBlockPatternInfo する再に、MinimumFlow 付きのEndToptypePumpで共通する処理をまとめた</Summary>
   */
    internal static void SetBlockPatternInfoWithMinimumFlow(EndTopTypePumpBase target, SingleBlockPatternIndexInfo info)
    {
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow)){
        return; //  error
      }
      var infoDerived = (SingleBlockPatternIndexInfoWithMinimumFlow)info;
      Chiyoda.CAD.Topology.BlockPattern bp = target.BaseBp;
      var groupList = bp.NonEquipmentEdges.ToList() ;
      var dischargeGroup = groupList[ info.DischargeIndex ] as Group ;
      dischargeGroup.Name = "DischargePipes" ;
      var suctionGroup = groupList[ info.SuctionIndex ] as Group ;
      suctionGroup.Name = "SuctionPipes" ;

      target.SetPropertyAndRule( info ) ;

      if (null != info.DischargeFlexIndexList)
        foreach ( var flex in info.DischargeFlexIndexList ) {
          target.SetFlexRatio( dischargeGroup, flex, 1 ) ;
        }

      if (null != info.SuctionFlexIndexList)
        foreach ( var flex in info.SuctionFlexIndexList ) {
          target.SetFlexRatio( suctionGroup, flex, 1 ) ;
        }

      var basePump = bp.EquipmentEdges.ElementAtOrDefault( info.BasePumpIndex ) ;
      basePump.ObjectName = "BasePump" ;
      basePump.ConnectionMaintenanceOrigin = basePump ;
      foreach ( var value in info.DischargeIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      foreach ( var value in infoDerived.DischargeAdditionalIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }

      foreach ( var value in info.SuctionIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      foreach ( var value in infoDerived.SuctionAdditionalIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = suctionGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }

      target.SetEdgeNames( info ) ;

      if (EndTopTypePumpBase.INDEXING_NOW == false){ 
        var edges1 = info.DischargeAngleGroupIndexList.Select( v => target.GetEdge( dischargeGroup, v ) ) ;
        var group1 = Group.CreateContinuousGroup( edges1.WithOlets().ToArray() ) ;
        group1.Name = "DischargeGroup" ;
        group1.ObjectName = "DischargeGroup" ;
        group1.ConnectionMaintenanceOrigin = basePump ;
        target.OnGroupChanged();
      }

      groupList = bp.NonEquipmentEdges.ToList() ;
      var minimumFlowGroup = groupList[ infoDerived.MinimumFlowIndex ] as Group ;
      minimumFlowGroup.Name = "MinimumFlowPipes" ;

      if (null != infoDerived.MinimumFlowFlexIndexList)
        foreach ( var flex in infoDerived.MinimumFlowFlexIndexList ) {
          target.SetFlexRatio( minimumFlowGroup, flex, 1 ) ;
        }

      foreach ( var value in infoDerived.MinimumFlowIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = minimumFlowGroup?.EdgeList.ElementAtOrDefault(value) as LeafEdge;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }

      HorizontalPumpImporter.AlignAllLeafEdges( bp, basePump ) ;

      bp.RuleList.BindChangeEvents( true ) ;
      if ( null != target.BpOwner ) {
        target.BpOwner.BaseBlockPattern = bp ;
        target.BpOwner.RuleList.BindChangeEvents(true);
      }
    }

  /*
   *  <Summary>PostProcess する再に、MinimumFlow 付きのEndToptypePumpで共通する処理を取り出してまとめた</Summary>
   */
    internal static void PostPostProcessWithMinimumFlow(EndTopTypePumpBase target, SingleBlockPatternIndexInfo info)
    {
      var cbp = target.BpOwner ;
      if (EndTopTypePumpBase.INDEXING_NOW == false){ 
        cbp.GetProperty("MinimumFlowJoinType").Value = (int)CompositeBlockPattern.JoinType.MiddleTeeInnerDir;
        cbp.GetProperty("MiniFlowHeaderBOP").Value = 5.5;
      }
    }
  }
}
