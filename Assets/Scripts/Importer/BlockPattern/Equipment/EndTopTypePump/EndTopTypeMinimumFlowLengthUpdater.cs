//#define DEBUG_DUMP

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
  class EndTopTypeMinimumFlowLengthUpdater
  {
    EndTopTypePumpLengthUpdater _target;
    bool _active;
    public static EndTopTypeMinimumFlowLengthUpdater Create(EndTopTypePumpLengthUpdater target)
    {
      if (target == null)
        return null;
      EndTopTypeMinimumFlowLengthUpdater instance = new EndTopTypeMinimumFlowLengthUpdater { _target = target };
      return instance;
    }
    public void Activate(bool active){
      _target.Activate(active);
      _active = active;
    }
    /**
     *  指定されたGroup 内のLeafEdge を一次元配列として抽出する。
     */
    public static LeafEdge [] ExtractLeafEdgeArrayFromGroupHierarachy(IGroup group)
    {
      return EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(group);
    }

    /**
     *  指定されたGroup 内のLeafEdge をインデックス付き辞書として抽出する
     */
    public static Dictionary<LeafEdge,int> ExtractLeafEdgeDictionaryFromGroupHierarachy(IGroup group){
      return EndTopTypePumpLengthUpdater.ExtractLeafEdgeDictionaryFromGroupHierarachy(group);
    }


    /**
     *  指定されたグループ内のLeafEdgie をリストとして抽出する
     */
    public static List<LeafEdge> ExtractLeafEdgeListFromGroupHierarachy(IGroup group){
      return EndTopTypePumpLengthUpdater.ExtractLeafEdgeListFromGroupHierarachy(group);
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
    public void UpdateMinimumFlowMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property){
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortMinimumFlowMinimumLengths(ownerbp, _target.Info);
      }else{
        foreach(var bpe in _target.BpOwner.EdgeList){
          if (bpe.Name.Contains("EndTopType")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortMinimumFlowMinimumLengths(bp, _target.Info);
            }
          }
        }
      }
    }

    public static void AssortMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      EndTopTypePumpLengthUpdater.AssortMinimumLengths(bp, info);
      AssortMinimumFlowMinimumLengths(bp, info);
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

          if (!(le.PipingPiece is Pipe aPipe)) continue;

          double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
          Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
          //aPipe.PreferredLength = minLength;
          aPipe.Length = minLength;
        }
      }
      if (infoDerived.MinimumFlowOletPrePostPipes != null)
      {
        foreach (int i in infoDerived.MinimumFlowOletPrePostPipes)
        {
          le = miniFlowGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;

          if (!(le.PipingPiece is Pipe aPipe)) continue;

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
