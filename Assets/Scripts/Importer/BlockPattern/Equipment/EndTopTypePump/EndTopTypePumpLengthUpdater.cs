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
  delegate void RuleHook(IPropertiedElement owner, IUserDefinedNamedProperty property);
  delegate void PropertyInjectorHook(IPropertiedElement owner, IUserDefinedNamedProperty property, string targetPropertyName);
  public class EndTopTypePumpLengthUpdater
  {
    public int OletStrawIndex;  //  現時点ではOletから伸びる細管はすべて同じパイプと想定  変更あれば要対応
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
    private EndTopTypePumpLengthUpdater(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      BpOwner = bpa;
      BaseBp = bp;
      Info = info;
      _active = false;
    }
    public void Activate(bool active){
      _active = active;
    }
    public static EndTopTypePumpLengthUpdater Create(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      EndTopTypePumpLengthUpdater instance = new EndTopTypePumpLengthUpdater(bpa,bp,info);
      return instance;
    }
    /**
     *  指定されたGroup 内のLeafEdge を一次元配列として抽出する。
     */
    public static LeafEdge [] ExtractLeafEdgeArrayFromGroupHierarachy(IGroup group)
    {
      return ExtractLeafEdgeListFromGroupHierarachy(group)?.ToArray();
    }
    /**
     *  指定されたGroup 内のLeafEdge をインデックス付き辞書として抽出する
     */
    public static Dictionary<LeafEdge,int> ExtractLeafEdgeDictionaryFromGroupHierarachy(IGroup group)
    {
      List<LeafEdge> list = ExtractLeafEdgeListFromGroupHierarachy(group);
      if (list == null)
        return null;
      int index = 0;
      Dictionary<LeafEdge, int> dict = new Dictionary<LeafEdge, int>();
      
      foreach(var obj in list)
        dict.Add(obj, index++);

      return dict;
    }
    /**
     *  指定されたグループ内のLeafEdgie をリストとして抽出する
     */
    public static List<LeafEdge> ExtractLeafEdgeListFromGroupHierarachy(IGroup group){
      List<LeafEdge> result = new List<LeafEdge>();
      int num = group.EdgeCount;
      int diff = num; //  スタックが足りなくなったときの増分
      IGroup[] groupstack = new IGroup[num];
      int[] indexstack = new int[num];
      int stackindex = 0, edgeindex;
      int maxloop = 1000;
      groupstack[stackindex] = group;
      indexstack[stackindex++] = 0;

      while(stackindex > 0){
        if (0 == --maxloop)
          break;
        group = groupstack[--stackindex];
        edgeindex = indexstack[stackindex];

        Edge edge = group.EdgeList.ElementAtOrDefault(edgeindex++);
        if (edgeindex < group.EdgeCount){
          if (groupstack.Length <= stackindex){
            num += diff;
            Array.Resize(ref groupstack, num);
            Array.Resize(ref indexstack, num);
          }
          groupstack[stackindex] = group;
          indexstack[stackindex++] = edgeindex;
        }
        if (edge is LeafEdge le){
          result.Add(le);
        }else if (edge is IGroup gp){
          if (gp.EdgeCount > 0){
            if (groupstack.Length <= stackindex){
              num += diff;
              Array.Resize(ref groupstack, num);
              Array.Resize(ref indexstack, num);
            }
            groupstack[stackindex] = gp;
            indexstack[stackindex++] = 0;
          }
        }
      }
      return result;
    }

    /**
     *  Discharge パイプ順をLeafEdge配列で保存する（AdjustPipeOrdersで使用するデータとなる）
     *  現仕様ではSuction側は不要
     */
    public void KeepDischargePipeOrderForLaterAdjustment()
    {
      var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(Info.DischargeIndex) as IGroup;
      LeafEdge[] edges = ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroup);
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
    /**
     * Discharge パイプインデックスを再構築する 
     */
    public void AdjustDischargePipeIndexesAfterReconstructingGroups(){
      //  Preparation
      var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(Info.DischargeIndex) as IGroup;

      Dictionary<LeafEdge, int> edges = ExtractLeafEdgeDictionaryFromGroupHierarachy(dischargeGroup);
      
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
    /**
     *  全パイプの溶接間最小距離を設定しなおす
     */
    public void UpdateSuctionMinimumLengths(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (!_active)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        AssortSuctionMinimumLengths(ownerbp, Info);
      }else{
        foreach(var bpe in BpOwner.EdgeList){
          if (bpe.Name.Contains("EndTopType")){
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
          if (bpe.Name.Contains("EndTopType")){
            if (bpe is Chiyoda.CAD.Topology.BlockPattern bp){
              AssortDischargeMinimumLengths(bp, Info);
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
      LeafEdge [] leArray = ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroup);

      if (info.DischargeNormalPipes != null)
      {
        foreach (int i in info.DischargeNormalPipes)
        {
          le = leArray[i];
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
          le = leArray[i];

          if (le?.PipingPiece is Pipe aPipe)
          {
            double minLength = Chiyoda.CAD.Manager.PipeManager.WeldMinDistance(aPipe) * 0.001;
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
  /// <summary>
  /// Property injector
  /// </summary>
  public class InjectorHookedRule : IUserDefinedRule
  {
    PropertyInjectorHook _hook;
    string _targetPropertyName;
    internal InjectorHookedRule( PropertyInjectorHook hook , string targetName ){
      _hook = hook;
      _targetPropertyName = String.Copy(targetName);
    }
    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ){
      _hook(owner, property,_targetPropertyName);
    }
  }
  public class PropertyIncrementRule : IUserDefinedRule
  {
    string _propertyName;
    int _value, _diff;
    internal PropertyIncrementRule(string propertyname, int diff ){
      _propertyName = propertyname;
      _value = 0;
      _diff = diff;
    }
    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ){
      _value = (_value + _diff) & 0x7fffffff;
      var what = owner.Parent;
      //UnityEngine.Debug.Log($"{what.ToString()}");
      INamedProperty prop = owner.GetProperty(_propertyName);
      if (prop != null){
        prop.Value = (double)_value;
      }
    }
  }
}



