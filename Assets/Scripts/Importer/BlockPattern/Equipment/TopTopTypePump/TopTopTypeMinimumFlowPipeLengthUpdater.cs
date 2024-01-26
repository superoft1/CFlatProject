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
  class TopTopTypeMinimumFlowPipeLengthUpdater
  {
    TopTopTypePumpPipeLengthUpdater _target;
    bool _active;
    private TopTopTypeMinimumFlowPipeLengthUpdater(TopTopTypePumpPipeLengthUpdater target){
      _target = target;
      _active = false;
    }
    public static TopTopTypeMinimumFlowPipeLengthUpdater Create(TopTopTypePumpPipeLengthUpdater target){
      if (target == null)
        return null;
      TopTopTypeMinimumFlowPipeLengthUpdater instance = new TopTopTypeMinimumFlowPipeLengthUpdater(target);
      return instance;
    }
    public void Activate(bool active){
      _target.Activate(active);
      _active = active;
    }
    /**
     *  Discharge パイプ順をLeafEdge配列で保存する（AdjustPipeOrdersで使用するデータとなる）
     *  現仕様ではSuction側は不要
     */
    public void KeepDischargePipeOrderForLaterAdjustment(){
      _target.KeepDischargePipeOrderForLaterAdjustment();
    }
        
    /**
     * Discharge パイプインデックスを再構築する 
     */
    public void AdjustDischargePipeIndexesAfterReconstructingGroups(){
      _target.AdjustDischargePipeIndexesAfterReconstructingGroups();
    }
        /**
     *  全パイプの溶接間最小距離を設定しなおす
     */
    public void UpdateSuctionMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property){
      _target.UpdateSuctionMinimumLengths(owner, property);
    }
    public void UpdateDischargeMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property){
      _target.UpdateDischargeMinimumLengths(owner, property);
    }

    public void UpdateMinimumFlowPreMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property){
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortMinimumFlowPreMinimumLengths(ownerbp, _target.Info);
      }else{
        foreach(var bpe in _target.BpOwner.EdgeList){
          if (bpe.Name.Contains("EndTopType")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortMinimumFlowPreMinimumLengths(bp, _target.Info);
            }
          }
        }
      }
    }
    public void UpdateMinimumFlowMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property){
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortMinimumFlowInDischargeGroupMinimumLengths(ownerbp, _target.Info);
        AssortMinimumFlowMinimumLengths(ownerbp, _target.Info);
        AssortMinimumFlowPreMinimumLengths(ownerbp, _target.Info);
      }else{
        foreach(var bpe in _target.BpOwner.EdgeList){
          if (bpe.Name.Contains("EndTopType")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortMinimumFlowInDischargeGroupMinimumLengths(bp, _target.Info);
              AssortMinimumFlowMinimumLengths(bp, _target.Info);
              AssortMinimumFlowPreMinimumLengths(bp, _target.Info);
            }
          }
        }
      }
    }

    public static void AssortMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      TopTopTypePumpPipeLengthUpdater.AssortMinimumLengths(bp, info);
      AssortMinimumFlowInDischargeGroupMinimumLengths(bp, info);
      AssortMinimumFlowPreMinimumLengths(bp, info);
      AssortMinimumFlowMinimumLengths(bp, info);
    }

    private static void AssortMinimumFlowInDischargeGroupMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      LeafEdge le;
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived))
        return;
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.DischargeIndex) as IGroup;
      if (infoDerived.MinimumFlowPreNormalPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowInDischargeGroupNormalPipes)
        {
          le = dischargeGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            aPipe.Length = minLength;
          }
        }
      }
      if (infoDerived.MinimumFlowPreOletPrePostPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowInDischargeGroupOletPrePostPipes)
        {
          le = dischargeGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            double oletdiameter = 0;
            var le0 = le.GetVertex(0)?.Partner?.LeafEdge;
            if (le0 != null){
              if (le0.PipingPiece is WeldOlet olet){
                oletdiameter = olet.Diameter;
              }else if (le0.PipingPiece is StubInReinforcingWeld srweld){
                oletdiameter = srweld.Diameter;
              }
            }
            var le1 = le.GetVertex(1)?.Partner?.LeafEdge;
            if (le1 != null){
              if (le1.PipingPiece is WeldOlet olet){
                oletdiameter += olet.Diameter;
              }else if (le1.PipingPiece is StubInReinforcingWeld srweld){
                oletdiameter += srweld.Diameter;
              }
            }
            minLength += oletdiameter * 0.5;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            aPipe.Length = minLength;
          }
        }
      }
    }
    private static void AssortMinimumFlowPreMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      LeafEdge le;
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived))
        return;
      var miniFlowPreGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowPreIndex) as IGroup;
      if (infoDerived.MinimumFlowPreNormalPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowPreNormalPipes)
        {
          le = miniFlowPreGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            aPipe.Length = minLength;
          }
        }
      }
      if (infoDerived.MinimumFlowPreOletPrePostPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowPreOletPrePostPipes)
        {
          le = miniFlowPreGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            double oletdiameter = 0;
            var le0 = le.GetVertex(0)?.Partner?.LeafEdge;
            if (le0 != null){
              if (le0.PipingPiece is WeldOlet olet){
                oletdiameter = olet.Diameter;
              }else if (le0.PipingPiece is StubInReinforcingWeld srweld){
                oletdiameter = srweld.Diameter;
              }
            }
            var le1 = le.GetVertex(1)?.Partner?.LeafEdge;
            if (le1 != null){
              if (le1.PipingPiece is WeldOlet olet){
                oletdiameter += olet.Diameter;
              }else if (le1.PipingPiece is StubInReinforcingWeld srweld){
                oletdiameter += srweld.Diameter;
              }
            }
            minLength += oletdiameter * 0.5;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            aPipe.Length = minLength;
          }
        }
      }
    }
    private static void AssortMinimumFlowMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      LeafEdge le;
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived))
        return;
      var miniFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;
      if (infoDerived.MinimumFlowNormalPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowNormalPipes)
        {
          le = miniFlowGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            //aPipe.PreferredLength = minLength;
            aPipe.Length = minLength;
          }
        }
      }
      if (infoDerived.MinimumFlowOletPrePostPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowOletPrePostPipes)
        {
          le = miniFlowGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            double oletdiameter = 0;
            var le0 = le.GetVertex(0)?.Partner?.LeafEdge;
            if (le0 != null){
              if (le0.PipingPiece is WeldOlet olet){
                oletdiameter = olet.Diameter;
              }else if (le0.PipingPiece is StubInReinforcingWeld srweld){
                oletdiameter = srweld.Diameter;
              }
            }
            var le1 = le.GetVertex(1)?.Partner?.LeafEdge;
            if (le1 != null){
              if (le1.PipingPiece is WeldOlet olet){
                oletdiameter += olet.Diameter;
              }else if (le1.PipingPiece is StubInReinforcingWeld srweld){
                oletdiameter += srweld.Diameter;
              }
            }
            minLength += oletdiameter * 0.5;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            //aPipe.PreferredLength = minLength;
            aPipe.Length = minLength;
          }
        }
      }
    }
  }
}
