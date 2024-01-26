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
  delegate void RuleHook(IPropertiedElement owner, IUserDefinedNamedProperty property);
  public class SideSideTypePumpMinimumLengthUpdater
  {
    public int OletStrawIndex;  //  現時点ではOletから伸びる細管はすべて同じパイプと想定  変更あれば要対応
    SingleBlockPatternIndexInfo info;
    private Chiyoda.CAD.Topology.BlockPattern BaseBp;
    private CompositeBlockPattern BpOwner;
    //private const int defaultNumGroups = 2;
    //private int numGroups;
    //private Dictionary<LeafEdge,int> [] groupPipeIndices;

    public static SideSideTypePumpMinimumLengthUpdater Create(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      SideSideTypePumpMinimumLengthUpdater instance = new SideSideTypePumpMinimumLengthUpdater();
      instance.BaseBp = bp;
      instance.BpOwner = bpa;
      //  keep info to avoid GC
      instance.info = info;
      //instance.numGroups = defaultNumGroups;
      //instance.groupPipeIndices = new Dictionary<LeafEdge, int>[instance.numGroups];
      
      return instance;
    }
 
    /**
     *  全パイプの溶接間最小距離を設定しなおす
     */
    public void UpdateSuctionMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortSuctionMinimumLengths(ownerbp, info);
      }else{
        foreach (var bpe in BpOwner.EdgeList){
          if (bpe.Name.Contains("SideSideType")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortSuctionMinimumLengths(bp, info);
            }
          }
        }
      }
    }
    public void UpdateDischargeMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortDischargeMinimumLengths(ownerbp, info);
      }else{
        foreach(var bpe in BpOwner.EdgeList){
          if (bpe.Name.Contains("SideSideType")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortDischargeMinimumLengths(bp, info);
            }
          }
        }
      }
    }
    public static void AssortMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      AssortSuctionMinimumLengths(bp, info); 
      AssortDischargeMinimumLengths(bp, info); 
    }
    private static void AssortSuctionMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      LeafEdge le;
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
      if (info.SuctionNormalPipes != null)
      {
        foreach (int i in info.SuctionNormalPipes)
        {
          le = suctionGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            aPipe.PreferredLength = minLength;
          }
        }
      }
      if (info.SuctionOletPrePostPipes != null)
      {
        foreach (int i in info.SuctionOletPrePostPipes)
        {
          le = suctionGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
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
            aPipe.PreferredLength = minLength;
          }
        }
      }
    }

    private static void AssortDischargeMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      LeafEdge le;
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
      if (info.DischargeNormalPipes != null)
      {
        foreach (int i in info.DischargeNormalPipes)
        {
          le = dischargeGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            Chiyoda.CAD.Manager.PipeManager.SetMinimumLength(aPipe, minLength);
            aPipe.PreferredLength = minLength;
          }
        }
      }
      //  olet
      if (info.DischargeOletPrePostPipes != null)
      {
        foreach (int i in info.DischargeOletPrePostPipes)
        {
          le = dischargeGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
            //  ちょっと重いが、個別に支流の径を確認する
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
            aPipe.PreferredLength = minLength;
          }
        }
      }
    }
  }
  public class GenericHookedRule : IUserDefinedRule
  {
    RuleHook _hook;
    internal GenericHookedRule( RuleHook hook ){
      _hook = hook;
    }
    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ){
      _hook(owner, property);
    }
  }

}



