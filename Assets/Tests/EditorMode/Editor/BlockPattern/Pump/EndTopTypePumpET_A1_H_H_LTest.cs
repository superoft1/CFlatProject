using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_H_H_L ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  public class EndTopTypePumpET_A1_H_H_LTest
  {
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
    /// <summary>
    /// はじめの２つのBlockPattern のProperties を比較する
    /// </summary>
    [Test]
    public void CheckPropertyDifferenceOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      int index = 0;
      INamedProperty[][] props = new INamedProperty[2][];

      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        UnityEngine.Debug.Log($"BlockPattern{index} : {bp.Name}");

        var pl = new System.Collections.Generic.List<INamedProperty>();
        bp.GetProperties();
        foreach (var p in bp.GetProperties()) {
          pl.Add(p);
        }
        props[index++] = pl.ToArray();
      }
      Assert.AreEqual(props[0].Length, props[1].Length);
      index = 0;
      int error = 0;
      UnityEngine.Debug.Log("First checking on default header interval.");
      double[] intervals = { 0.0, 0.5, 1.0 };
      while (true) {
        for (int i = 0; i < props[0].Length; ++i) {
          var val0 = props[0][i].Value;
          var val1 = props[1][i].Value;
          if (val0 != val1) {
            ++error;
            UnityEngine.Debug.Log($"Difference found on {props[0][i].PropertyName} ; {val0} : {val1}!\n");
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
      UnityEngine.Debug.Log($"Check done, {error} difference(s) found");
      Assert.AreEqual(error, 0);
    }
    /// <summary>
    /// はじめの２つのBlockPattern のSuctionEnd 座標 を比較する
    /// </summary>
    [Test]
    public void CheckSuctionEndPositionYDifferenceAtDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");
      blockPattern.LocalCod = UnityEngine.LocalCodSys3d.Identity;

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);


      INamedProperty[][] props = new INamedProperty[2][];

      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.SuctionIndex] is IGroup group) {
          suctionGroups.Add(group);
        }
      }
      var suctionLeafEdges = new LeafEdge[2][];
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));
      double[] intervals = { 0.0, 0.5, 1.0 };
      UnityEngine.Debug.Log("Now checking at default interval");
      int index = 0;
      int error = 0;
      while (true) {
        var le0 = suctionLeafEdges[0][importer.Info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]];
        var le1 = suctionLeafEdges[1][importer.Info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]];
        var v0 = le0.LocalCod.Origin;
        var v1 = le1.LocalCod.Origin;
        if (v0.y != v1.y) {
          UnityEngine.Debug.Log($"SuctionEnd different {v0.y} / {v1.y}");
          ++error;
        } else {
          UnityEngine.Debug.Log($"SuctionEnds has same Y {v0.y}.");
        }
        if (index < intervals.Length) {
          double hi = intervals[index++];
          blockArray.GetProperty("HeaderInterval").Value = hi;
          curDoc.MaintainEdgePlacement();
          UnityEngine.Debug.Log($"Now checking at header-interval {hi} m");
        } else
          break;
      }
      UnityEngine.Debug.Log($"{error} differences found on SuctionEnd.PosY");
      Assert.AreEqual(error, 0);
      UnityEngine.Debug.Log("Done!");
    }

    /// <summary>
    /// はじめの２つのBlockPattern のProperties を比較する
    /// </summary>
    [Test]
    public void CheckAfterDiameterChangedPropertyDifferenceOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      blockArray.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(16).NpsMm;
      blockArray.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(16).NpsMm;
      curDoc.MaintainEdgePlacement();

      blockArray.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(12).NpsMm;
      blockArray.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(10).NpsMm;
      curDoc.MaintainEdgePlacement();

      int index = 0;
      INamedProperty[][] props = new INamedProperty[2][];

      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        UnityEngine.Debug.Log($"BlockPattern{index} : {bp.Name}");

        var pl = new System.Collections.Generic.List<INamedProperty>();
        bp.GetProperties();
        foreach (var p in bp.GetProperties()) {
          pl.Add(p);
        }
        props[index++] = pl.ToArray();
      }
      Assert.AreEqual(props[0].Length, props[1].Length);
      index = 0;
      int error = 0;
      UnityEngine.Debug.Log("First checking on default header interval.");
      double[] intervals = { 0.0, 0.5, 1.0 };
      while (true) {
        for (int i = 0; i < props[0].Length; ++i) {
          var val0 = props[0][i].Value;
          var val1 = props[1][i].Value;
          if (val0 != val1) {
            ++error;
            UnityEngine.Debug.Log($"Difference found on {props[0][i].PropertyName} ; {val0} : {val1}!\n");
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
      UnityEngine.Debug.Log($"Check done, {error} difference(s) found");
      Assert.AreEqual(error, 0);
    }
    /// <summary>
    /// 径を変更したあと始め２つのBlockPattern のSuctionEnd 座標 を比較する
    /// </summary>
    [Test]
    public void CheckAfterDiameterChangedSuctionEndPositionYDifferenceAtDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      blockArray.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(16).NpsMm;
      blockArray.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(16).NpsMm;
      curDoc.MaintainEdgePlacement();

      blockArray.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(12).NpsMm;
      blockArray.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(10).NpsMm;
      curDoc.MaintainEdgePlacement();

      INamedProperty[][] props = new INamedProperty[2][];

      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.SuctionIndex] is IGroup group) {
          suctionGroups.Add(group);
        }
      }
      var suctionLeafEdges = new LeafEdge[2][];
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));
      double[] intervals = { 0.0, 0.5, 1.0 };
      UnityEngine.Debug.Log("Now checking at default interval");
      int index = 0;
      int error = 0;
      while (true) {
        var le0 = suctionLeafEdges[0][importer.Info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]];
        var le1 = suctionLeafEdges[1][importer.Info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]];
        var v0 = le0.LocalCod.Origin;
        var v1 = le1.LocalCod.Origin;
        if (v0.y != v1.y) {
          UnityEngine.Debug.Log($"SuctionEnd different {v0.y} / {v1.y}");
        } else {
          UnityEngine.Debug.Log($"SuctionEnds has same Y {v0.y} / {v1.y}.");
        }
        if (index < intervals.Length) {
          double hi = intervals[index++];
          blockArray.GetProperty("HeaderInterval").Value = hi;
          curDoc.MaintainEdgePlacement();
          UnityEngine.Debug.Log($"Now checking at header-interval {hi} m");
        } else
          break;
      }
      UnityEngine.Debug.Log($"{error} differences found on SuctionEnd.PosY");
      Assert.AreEqual(error, 0);
      UnityEngine.Debug.Log("Done!");
    }

    /// <summary>
    /// 径を変更せず始め２つのBlockPattern のPipe長 を比較する
    /// </summary>
    [Test]
    public void CheckAllPipeLengthsOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      CheckPipeLengths(blockArray, importer);
      double[] intervals = { 0.0, 0.5, 1.0 };
      int err = 0;
      foreach(var hi in intervals){
        UnityEngine.Debug.Log($"HeaderInterval = {hi}");
        blockArray.GetProperty("HeaderInterval").Value = hi;
        curDoc.MaintainEdgePlacement();
        err += CheckPipeLengths(blockArray, importer);
      }
      UnityEngine.Debug.Log($"{err} errors found!");
      Assert.AreEqual(err, 0);
    }

    /// <summary>
    /// 径を変更せず始め２つのBlockPattern のPipe座標 を比較する
    /// </summary>
    [Test]
    public void CheckAllPipeCoordinatesOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      CheckPipeCoords(blockArray, importer);
      double[] intervals = { 0.0, 0.5, 1.0 };
      int err = 0;
      foreach(var hi in intervals){
        UnityEngine.Debug.Log($"HeaderInterval = {hi}");
        blockArray.GetProperty("HeaderInterval").Value = hi;
        curDoc.MaintainEdgePlacement();
        err += CheckPipeCoords(blockArray, importer);
      }
      UnityEngine.Debug.Log($"{err} errors found!");
      Assert.AreEqual(err, 0);
    }
    /// <summary>
    /// 径を変更せず始め２つのBlockPattern のDiameter を比較する
    /// </summary>
    [Test]
    public void CheckAllPipeDiametersOnDifferentHeaderIntervals()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_H_H_L(curDoc);
      var blockPattern = importer.Create(null);
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      CheckPipeDiameters(blockArray, importer);
      double[] intervals = { 0.0, 0.5, 1.0 };
      int err = 0;
      foreach(var hi in intervals){
        UnityEngine.Debug.Log($"HeaderInterval = {hi}");
        blockArray.GetProperty("HeaderInterval").Value = hi;
        curDoc.MaintainEdgePlacement();
        err += CheckPipeDiameters(blockArray, importer);
      }
      UnityEngine.Debug.Log($"{err} errors found!");
      Assert.AreEqual(err, 0);
    }

    /// <summary>
    /// Pipe 情報のチェック length 版
    /// </summary>
    /// <param name="blockArray"></param>
    /// <param name="importer"></param>
    private int CheckPipeLengths(BlockPatternArray blockArray, EndTopTypePumpET_A1_H_H_L importer){
      Assert.AreEqual(blockArray.ArrayCount, 2);
      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));
      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));

      var info = importer.Info;
      index = 0;
      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);
      int errCnt = 0;
      UnityEngine.Debug.Log("Discharge pipes!");
      foreach (int i in dischargeIndices) {
        var le1 = dischargeLeafEdges[0][i];
        var le2 = dischargeLeafEdges[1][i];
        if (le1.PipingPiece.GetType()!= le2.PipingPiece.GetType()){
          UnityEngine.Debug.Log("Different type");
          ++errCnt;
          continue;
        }else{
          if ((le1.PipingPiece is Pipe pipe1)&&(le2.PipingPiece is Pipe pipe2)){
            if (pipe1.Length == pipe2.Length && pipe1.MinimumLengthWithoutOletRadius == pipe2.MinimumLengthWithoutOletRadius && pipe1.PreferredLength == pipe2.PreferredLength)
              UnityEngine.Debug.Log($"Length ({i}):{pipe1.Length},{pipe1.MinimumLengthWithoutOletRadius},{pipe1.PreferredLength} / {pipe2.Length},{pipe2.MinimumLengthWithoutOletRadius},{pipe2.PreferredLength}");
            else{
              UnityEngine.Debug.Log($"Length*({i}):{pipe1.Length},{pipe1.MinimumLengthWithoutOletRadius},{pipe1.PreferredLength} / {pipe2.Length},{pipe2.MinimumLengthWithoutOletRadius},{pipe2.PreferredLength}");
              ++errCnt;
            }
          }
        }
      }
      UnityEngine.Debug.Log("Discharge done!");
      UnityEngine.Debug.Log("Suction pipes!");
      foreach (int i in suctionIndices) {
        var le1 = suctionLeafEdges[0][i];
        var le2 = suctionLeafEdges[1][i];
        if (le1.PipingPiece.GetType()!= le2.PipingPiece.GetType()){
          UnityEngine.Debug.Log("Different type");
          ++errCnt;
          continue;
        }else{
          if ((le1.PipingPiece is Pipe pipe1) && (le2.PipingPiece is Pipe pipe2)) {
            double diff1, diff2;
            var cp0 = pipe1.ConnectPoints.ElementAt(0).GlobalPoint;
            var cp1 = pipe1.ConnectPoints.ElementAt(1).GlobalPoint;
            diff1 = (cp1 - cp0).magnitude;
            cp0 = pipe2.ConnectPoints.ElementAt(0).GlobalPoint;
            cp1 = pipe2.ConnectPoints.ElementAt(1).GlobalPoint;
            diff2 = (cp1 - cp0).magnitude;
            if (pipe1.Length == pipe2.Length && pipe1.MinimumLengthWithoutOletRadius == pipe2.MinimumLengthWithoutOletRadius && pipe1.PreferredLength == pipe2.PreferredLength && diff1 == diff2){
              UnityEngine.Debug.Log($"Length ({i}):{pipe1.Length},{pipe1.MinimumLengthWithoutOletRadius},{pipe1.PreferredLength}:{diff1} / {pipe2.Length},{pipe2.MinimumLengthWithoutOletRadius},{pipe2.PreferredLength}:{diff2}");
            } else {
              UnityEngine.Debug.Log($"Length*({i}):{pipe1.Length},{pipe1.MinimumLengthWithoutOletRadius},{pipe1.PreferredLength}:{diff1} / {pipe2.Length},{pipe2.MinimumLengthWithoutOletRadius},{pipe2.PreferredLength}:{diff2}");
              ++errCnt;
            }
          }
        }
      }
      UnityEngine.Debug.Log("Suction done!");
      return errCnt;
    }
    /// <summary>
    /// Pipe 情報のチェック  座標 版
    /// </summary>
    /// <param name="blockArray"></param>
    /// <param name="importer"></param>
    private int CheckPipeCoords(BlockPatternArray blockArray, EndTopTypePumpET_A1_H_H_L importer){
      Assert.AreEqual(blockArray.ArrayCount, 2);
      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));
      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));

      var info = importer.Info;
      index = 0;
      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);
      int errCnt = 0;
      UnityEngine.Debug.Log("Discharge pipes!");
      foreach (int i in dischargeIndices) {
        var le1 = dischargeLeafEdges[0][i];
        var le2 = dischargeLeafEdges[1][i];
        if (le1.PipingPiece.GetType()!= le2.PipingPiece.GetType()){
          UnityEngine.Debug.Log("Different type");
          ++errCnt;
          continue;
        }else{
          if (le1.LocalCod.Origin.y == le2.LocalCod.Origin.y && le1.LocalCod.Origin.z == le2.LocalCod.Origin.z) {
            UnityEngine.Debug.Log($"Coord  {le1.LocalCod.Origin.x},{le1.LocalCod.Origin.y},{le1.LocalCod.Origin.z} / {le2.LocalCod.Origin.x},{le2.LocalCod.Origin.y},{le2.LocalCod.Origin.z} ");
          } else {
            UnityEngine.Debug.Log($"Coord* {le1.LocalCod.Origin.x},{le1.LocalCod.Origin.y},{le1.LocalCod.Origin.z} / {le2.LocalCod.Origin.x},{le2.LocalCod.Origin.y},{le2.LocalCod.Origin.z} ");
            ++errCnt;
          }
        }
      }
      UnityEngine.Debug.Log("Discharge done!");
      UnityEngine.Debug.Log("Suction pipes!");
      foreach (int i in suctionIndices) {
        var le1 = suctionLeafEdges[0][i];
        var le2 = suctionLeafEdges[1][i];
        if (le1.PipingPiece.GetType()!= le2.PipingPiece.GetType()){
          UnityEngine.Debug.Log("Different type");
          ++errCnt;
          continue;
        }else{
          if (le1.LocalCod.Origin.Equals(le2.LocalCod.Origin)) {
            UnityEngine.Debug.Log($"Coord  {le1.LocalCod.Origin.x},{le1.LocalCod.Origin.y},{le1.LocalCod.Origin.z} / {le2.LocalCod.Origin.x},{le2.LocalCod.Origin.y},{le2.LocalCod.Origin.z} ");
          } else {
            UnityEngine.Debug.Log($"Coord* {le1.LocalCod.Origin.x},{le1.LocalCod.Origin.y},{le1.LocalCod.Origin.z} / {le2.LocalCod.Origin.x},{le2.LocalCod.Origin.y},{le2.LocalCod.Origin.z} ");
            ++errCnt;
          }
        }
      }
      UnityEngine.Debug.Log("Suction done!");
      return errCnt;
    }

    /// <summary>
    /// Pipe 情報のチェック  Diameter 版
    /// </summary>
    /// <param name="blockArray"></param>
    /// <param name="importer"></param>
    private int CheckPipeDiameters(BlockPatternArray blockArray, EndTopTypePumpET_A1_H_H_L importer){
      Assert.AreEqual(blockArray.ArrayCount, 2);
      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));
      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));

      var info = importer.Info;
      index = 0;
      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);
      int errCnt = 0;
      UnityEngine.Debug.Log("Discharge pipes!");
      foreach (int i in dischargeIndices) {
        var le1 = dischargeLeafEdges[0][i];
        var le2 = dischargeLeafEdges[1][i];
        if (le1.PipingPiece.GetType()!= le2.PipingPiece.GetType()){
          UnityEngine.Debug.Log("Different type");
          ++errCnt;
          continue;
        }else{
          if ((le1.PipingPiece is Pipe pipe1) && (le2.PipingPiece is Pipe pipe2)){
            if (pipe1.Diameter == pipe2.Diameter){
              UnityEngine.Debug.Log($"Diameter  {pipe1.Diameter} / {pipe2.Diameter}");
            } else {
              UnityEngine.Debug.Log($"Diameter* {pipe1.Diameter} / {pipe2.Diameter}");
              ++errCnt;
            }
          }
        }
      }
      UnityEngine.Debug.Log("Discharge done!");
      UnityEngine.Debug.Log("Suction pipes!");
      foreach (int i in suctionIndices) {
        var le1 = suctionLeafEdges[0][i];
        var le2 = suctionLeafEdges[1][i];
        if (le1.PipingPiece.GetType()!= le2.PipingPiece.GetType()){
          UnityEngine.Debug.Log("Different type");
          ++errCnt;
          continue;
        }else{
          if ((le1.PipingPiece is Pipe pipe1) && (le2.PipingPiece is Pipe pipe2)) {
            if (pipe1.Diameter == pipe2.Diameter) {
              UnityEngine.Debug.Log($"Diameter  {pipe1.Diameter} / {pipe2.Diameter}");
            } else {
              UnityEngine.Debug.Log($"Diameter* {pipe1.Diameter} / {pipe2.Diameter}");
              ++errCnt;
            }
          }
        }
      }
      UnityEngine.Debug.Log("Suction done!");
      return errCnt;
    }

  }
}
