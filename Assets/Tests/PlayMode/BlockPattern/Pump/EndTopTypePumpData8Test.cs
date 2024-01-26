using System.Diagnostics ;
using System.Linq;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.PlayMode.BlockPattern.Pump
{
  public class EndTopTypePumpData8Test
  {
    private UnityEngine.GameObject _gameObject = null ;
    // クラス内の最初のテストが実行される前に一度だけ実行される
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _gameObject = new UnityEngine.GameObject();
      _gameObject.AddComponent<ImportManager>();
      Assert.IsNotNull( ImportManager.Instance() );
    }
    
    // クラス内の最後のテストが実行された後に一度だけ実行される
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
      foreach ( Transform child in _gameObject.transform ) {
        Object.DestroyImmediate(child.gameObject);
        child.parent = null ;
      }
      Object.DestroyImmediate(_gameObject);
      Assert.IsNull( ImportManager.Instance() );
    }
#if false
    /// <summary>
    /// はじめの２つのBlockPattern のRuleList を比較する
    /// Core を変更しないと使えないためいったんクローズ
    /// </summary>
    [Test]
    public void CheckRuleListDifferenceOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      blockPattern.LocalCod = UnityEngine.LocalCodSys3d.Identity;
      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);
      int index = 0;
      IRule[][] rules = new IRule[2][];
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        UnityEngine.Debug.Log($"BlockPattern{index} : {bp.Name}");
        var rl = new System.Collections.Generic.List<IRule>();
        foreach (var r in bp.RuleList.Rules) {
          rl.Add(r);
        }
        rules[index++] = rl.ToArray();
      }
      Assert.AreEqual(rules[0].Length, rules[1].Length);
      UnityEngine.Debug.Log("checking rules started.");
      int error = 0;
      UnityEngine.Debug.Log("First checking on default header interval.");
      double[] intervals = { 0.0, 0.5, 1.0 };
      index = 0;
      while (true) {
        for (int i = 0; i < rules[0].Length; ++i) {
          string rule0 = rules[0][i].ToString();
          string rule1 = rules[1][i].ToString();
          if (rules[0][i] is ObjectPropertyRule opr) {
            rule0 = opr.Expression.ToString();
          }
          if (rules[1][i] is ObjectPropertyRule opr2) {
            rule1 = opr2.Expression.ToString();
          }
          if (!rule0.Equals(rule1)) {
            ++error;
            UnityEngine.Debug.Log("Difference found!\n");
            UnityEngine.Debug.Log($"{rule0}");
            UnityEngine.Debug.Log($"{rule1}");
          }
        }
        if (index < intervals.Length) {
          double hi = intervals[index++];
          blockArray.GetProperty("HeaderInterval").Value = hi;
          curDoc.MaintainEdgePlacement();
          UnityEngine.Debug.Log($"Now checking at header-interval {hi} m");
        } else
          break;
      }
      UnityEngine.Debug.Log($"Check done, {error} Differnt Rule(s) found");
      Assert.AreEqual(error, 0);
    }
#endif
    static bool dblDiff(double d1, double d2){
      if (System.Math.Abs(d1 - d2) < 0.000001)  //  1ミクロンまで誤差を許容
        return false;
      return true;
    }
    /// <summary>
    /// はじめの２つのBlockPattern のProperties を比較する
    /// </summary>
    [Test]
    public void CheckPropertyDifferenceOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      if (_gameObject == null){
        _gameObject = new UnityEngine.GameObject();
        _gameObject.AddComponent<ImportManager>();
        Assert.IsNotNull( ImportManager.Instance() );
      }
      
      var blockPattern = CreateEndTopTypePumpData8.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);
      int error = 0;
      int index = 0;
      var dischargePipes = new System.Collections.Generic.List<LeafEdge>[2];
      var suctionPipes = new System.Collections.Generic.List<LeafEdge>[2];
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        dischargePipes[index] = new System.Collections.Generic.List<LeafEdge>();
        suctionPipes[index] = new System.Collections.Generic.List<LeafEdge>();
        foreach (var gpe in bp.EdgeList){
          if (!(gpe is Group group)) continue;
          if (group.Name.Contains("Discharge")) {
            foreach (var le in group.GetAllLeafEdges()){
              if (le.PipingPiece is Pipe)
                dischargePipes[index].Add(le);
            }
          }else if (group.Name.Contains("Suction")){
            foreach (var le in group.GetAllLeafEdges()){
              if (le.PipingPiece is Pipe)
                suctionPipes[index].Add(le);
            }
          }
        }
        ++index;
      }
      Assert.AreEqual(index, 2);
      UnityEngine.Debug.Log("Discharge pipes");

      int disErr = 0;
      Assert.AreEqual(dischargePipes[0].Count, dischargePipes[1].Count);
      for (index = 0; index < dischargePipes[0].Count; ++index){
        double diff0, diff1;
        var pipe0 = (Pipe)dischargePipes[0].ElementAt(index).PipingPiece;
        var pipe1 = (Pipe)dischargePipes[1].ElementAt(index).PipingPiece;
        var cp0 = pipe0.ConnectPoints.ElementAt(0).GlobalPoint;
        var cp1 = pipe0.ConnectPoints.ElementAt(1).GlobalPoint;
        string mark = "";
        diff0 = (cp1 - cp0).magnitude;
        cp0 = pipe1.ConnectPoints.ElementAt(0).GlobalPoint;
        cp1 = pipe1.ConnectPoints.ElementAt(1).GlobalPoint;
        diff1 = (cp1 - cp0).magnitude;
        if (dblDiff(diff0,diff1) || dblDiff(pipe0.Length,pipe1.Length) || dblDiff(pipe0.MinimumLengthWithoutOletRadius,pipe1.MinimumLengthWithoutOletRadius)){ 
          mark = "*";
          disErr++;
        }
        UnityEngine.Debug.Log($"{pipe0.ObjectName}{mark} : {pipe0.Length},{pipe0.MinimumLengthWithoutOletRadius},{diff0} / {pipe1.Length},{pipe1.MinimumLengthWithoutOletRadius},{diff1}");
      }
      UnityEngine.Debug.Log("Discharge done");
      UnityEngine.Debug.Log("Suction pipes");
      Assert.AreEqual(suctionPipes[0].Count, suctionPipes[1].Count);
      int sucErr = 0;
      for (index = 0; index < suctionPipes[0].Count; ++index){
        double diff0, diff1;
        var pipe0 = (Pipe)suctionPipes[0].ElementAt(index).PipingPiece;
        var pipe1 = (Pipe)suctionPipes[1].ElementAt(index).PipingPiece;
        var cp0 = pipe0.ConnectPoints.ElementAt(0).GlobalPoint;
        var cp1 = pipe0.ConnectPoints.ElementAt(1).GlobalPoint;
        string mark = "";
        diff0 = (cp1 - cp0).magnitude;
        cp0 = pipe1.ConnectPoints.ElementAt(0).GlobalPoint;
        cp1 = pipe1.ConnectPoints.ElementAt(1).GlobalPoint;
        diff1 = (cp1 - cp0).magnitude;
        if (diff0 != diff1 || pipe0.Length != pipe1.Length || pipe0.MinimumLengthWithoutOletRadius != pipe1.MinimumLengthWithoutOletRadius){ 
          mark = "*";
          sucErr++;
        }
        UnityEngine.Debug.Log($"{pipe0.ObjectName}{mark} : {pipe0.Length},{pipe0.MinimumLengthWithoutOletRadius},{diff0} / {pipe1.Length},{pipe1.MinimumLengthWithoutOletRadius},{diff1}");
      }
      UnityEngine.Debug.Log("Suction done");

      UnityEngine.Debug.Log($"Check done, dis:{disErr} suc:{sucErr} difference(s) found");
      Assert.AreEqual(disErr+sucErr, 0,$"dis:{disErr} suc:{sucErr} difference(s) found");
    }
  }
}
