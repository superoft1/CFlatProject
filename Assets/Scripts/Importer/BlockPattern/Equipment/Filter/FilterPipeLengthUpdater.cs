//#define DEBUG_DUMP

using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Manager ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;

namespace Importer.BlockPattern.Equipment.Filter
{
  delegate void RuleHook(IPropertiedElement owner, IUserDefinedNamedProperty property);
  class FilterPipeLengthUpdater
  {
    public SingleBlockPatternIndexInfo Info { get; }
    public Chiyoda.CAD.Topology.BlockPattern BaseBp { get; }
    public CompositeBlockPattern BpOwner { get; }
    //private const int defaultNumGroups = 2;
    //private int numGroups;
    //private Dictionary<LeafEdge,int> [] groupPipeIndices;
    private LeafEdge [] _dischargeGroupNormalPipes;
    private LeafEdge [] _dischargeGroupOletPrePostPipes;
    private LeafEdge [] _suctionGroupNormalPipes;
    private LeafEdge [] _suctionGroupOletPrePostPipes;
    private bool _active;
    private FilterPipeLengthUpdater(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      BpOwner = bpa;
      BaseBp = bp;
      Info = info;
      _active = true;
    }
    public static FilterPipeLengthUpdater Create(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      FilterPipeLengthUpdater instance = new FilterPipeLengthUpdater(bpa,bp,info);
      return instance;
    }
    public void Activate(bool active){
      _active = active;
    }
    ///
    /// <summary>Discharge パイプ順をLeafEdge配列で保存する（AdjustPipeOrdersで使用するデータとなる）</summary>
    /// <remarks>現仕様ではSuction側は不要</remarks>
    ///
    public void KeepDischargePipeOrderForLaterAdjustment()
    {
      var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(Info.DischargeIndex) as IGroup;
      LeafEdge[] edges = FilterGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroup);
      int[] order;
      if (null != (order = Info.DischargeNormalPipes))
      {
        _dischargeGroupNormalPipes = new LeafEdge[order.Length];
        for (int ix = 0; ix < order.Length; ++ix)
          _dischargeGroupNormalPipes[ix] = edges[order[ix]];
      }
      if (null != (order = Info.DischargeOletPrePostPipes)) {
        _dischargeGroupOletPrePostPipes = new LeafEdge[order.Length];
        for (int ix = 0; ix < order.Length; ++ix)
          _dischargeGroupOletPrePostPipes[ix] = edges[order[ix]];
      }
    }
    ///
    /// <summary>Discharge パイプインデックスを再構築する </summary>
    /// <remarks>EndTopTypePump および TopTopTypePump で必要 なお、現仕様ではSuction側は不要</remarks>
    ///
    public void AdjustDischargePipeIndexesAfterReconstructingGroups(){
      //  Preparation
      var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(Info.DischargeIndex) as IGroup;

      Dictionary<LeafEdge, int> edges = FilterGroupLeafEdgeExtractor.ExtractLeafEdgeDictionaryFromGroupHierarachy(dischargeGroup);
      
      if (_dischargeGroupNormalPipes != null){
        int[] swap = new int[Info.DischargeNormalPipes.Length];
        for (int i = 0 ; i < swap.Length ; ++i){
          var obj = _dischargeGroupNormalPipes[i];
          swap[i] = edges[obj];
        }
        Info.DischargeNormalPipes = swap;
      }
      if (_dischargeGroupOletPrePostPipes != null){
        int[] swap = new int[Info.DischargeOletPrePostPipes.Length];
        for (int i = 0 ; i < swap.Length ; ++i){
          var obj = _dischargeGroupOletPrePostPipes[i];
          swap[i] = edges[obj];
        }
        Info.DischargeOletPrePostPipes = swap;
      }

      #if DEBUG_DUMP
      string result = "discharge normal assigned ";
      foreach (var index in Info.DischargeNormalPipes){
        result += $"{index} ";
      }
      UnityEngine.Debug.Log(result);
      result = "discharge oletprepost assigned ";
      foreach (var index in Info.DischargeOletPrePostPipes){
        result += $"{index} ";
      }
      UnityEngine.Debug.Log(result);
      #endif

    }
    ///
    /// <summary>全パイプの溶接間最小距離を設定しなおす</summary>
    ///
    public void UpdateAllMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property){
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortSuctionMinimumLengths(ownerbp, Info);
        AssortDischargeMinimumLengths(ownerbp, Info);
      }

    }

    ///
    /// <summary>全パイプの溶接間最小距離を設定しなおす</summary>
    ///
    public void UpdateSuctionMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortSuctionMinimumLengths(ownerbp, Info);
      }else{
        foreach(var bpe in BpOwner.EdgeList){
          if (bpe.Name.Contains("Filter")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortSuctionMinimumLengths(bp, Info);
            }
          }
        }
      }
    }
    public void UpdateDischargeMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortDischargeMinimumLengths(ownerbp, Info);
      }else{
        foreach(var bpe in BpOwner.EdgeList){
          if (bpe.Name.Contains("Filter")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortDischargeMinimumLengths(bp, Info);
            }
          }
        }
      }
    }
    
    ///
    /// <summary>info で登録された全パイプの溶接間最小距離を設定する</summary>
    /// <param name="bp">BasePump</param>
    /// <param name="info">インデックス情報</param>
    ///
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
          if ( ! ( le?.PipingPiece is Pipe aPipe ) ) continue ;
          double minLength = aPipe.WeldMinDistance().Millimeters();
          aPipe.SetMinimumLength( minLength);
          aPipe.PreferredLength = minLength;
        }
      }
      if (info.SuctionOletPrePostPipes != null)
      {
        foreach (int i in info.SuctionOletPrePostPipes)
        {
          le = suctionGroup?.EdgeList.ElementAtOrDefault(i) as LeafEdge;
          if (le.PipingPiece is Pipe aPipe) {
            double minLength = aPipe.WeldMinDistance().Millimeters() ;
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
            aPipe.SetMinimumLength( minLength);
            aPipe.PreferredLength = minLength;
          }
        }
      }
    }

    private static void AssortDischargeMinimumLengths(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      LeafEdge le;
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
      LeafEdge [] leArray = FilterGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroup);

      if (info.DischargeNormalPipes != null)
      {
        foreach (int i in info.DischargeNormalPipes)
        {
          le = leArray[i];
          if (le.PipingPiece is Pipe aPipe)
          {
            double minLength = aPipe.WeldMinDistance().Millimeters();
            aPipe.SetMinimumLength( minLength);
            aPipe.PreferredLength = minLength;
          }
        }
      }
      //  olet
      if (info.DischargeOletPrePostPipes != null)
      {
        foreach (int i in info.DischargeOletPrePostPipes)
        {
          le = leArray[i];

          if (le?.PipingPiece is Pipe aPipe)
          {
            double minLength = PipeManager.WeldMinDistance(aPipe) * 0.001;
            double oletdiameter = 0;
            //  ちょっと重いが、個別に支流の径を確認する
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
            aPipe.SetMinimumLength( minLength);
            aPipe.PreferredLength = minLength;
          }
        }
      }
    }
  }
  public class GenericHookedRule : IUserDefinedRule
  {
    RuleHook _hook;
    IUserDefinedRule _completion;
    internal GenericHookedRule( RuleHook hook , IUserDefinedRule nextRule = null){
      _hook = hook;
      _completion = nextRule;
    }
    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ){
      _hook(owner, property);
      if (_completion!=null)
        _completion.Run(owner, property);
    }
  }
}
