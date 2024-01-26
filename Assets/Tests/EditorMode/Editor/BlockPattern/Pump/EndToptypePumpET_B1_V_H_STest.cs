using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_V_H_S ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  public class EndTopTypePumpET_B1_V_H_STest
  {
    [Test]
    public void BlockCountTest()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var blockPattern = new EndTopTypePumpET_B1_V_H_S(curDoc).Create(null);

      Assert.AreEqual(blockPattern.Parent is BlockPatternArray, true);

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 1);// 作成直後はArrayCount = 1

      curDoc.MaintainEdgePlacement();

      Assert.AreEqual(blockArray.ArrayCount, 2);

      blockArray.GetProperty("BlockCount").Value = 3;
      Assert.AreEqual(blockArray.ArrayCount, 2);// 変更が反映されない

      curDoc.MaintainEdgePlacement();

      Assert.AreEqual(blockArray.ArrayCount, 3);

    }
    /// <summary>
    /// はじめの２つのBlockPattern でパイプ長の相違がないかテストする
    /// </summary>
    [Test]
    public void CheckLengthDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_B1_V_H_S(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        UnityEngine.Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (! ( bpe is Chiyoda.CAD.Topology.BlockPattern bp )) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if ( array[ importer.Info.DischargeIndex ] is IGroup group ) {
          dischargeGroups.Add( group ) ;
        }
        if ( array[ importer.Info.SuctionIndex ] is IGroup group2 ) {
          suctionGroups.Add( group2 ) ;
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      UnityEngine.Debug.Log("Discharge pipes");
      for (int i = 0; i < dischargeLeafEdges[0].Length; ++i) {
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.Length != pipe2.Length){
            UnityEngine.Debug.Log($"pipe{i} has different length");
            Assert.AreEqual(pipe1.Length, pipe2.Length);  //  
          }
          Assert.AreEqual(pipe1.Length, pipe2.Length);  //  
        } else {
          UnityEngine.Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      UnityEngine.Debug.Log("Discharge pipe ok!");
      UnityEngine.Debug.Log("Suction pipes");
      for (int i = 0; i < suctionLeafEdges[0].Length; ++i) {
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.Length != pipe2.Length){
            UnityEngine.Debug.Log($"pipe{i} has different length");
            Assert.AreEqual(pipe1.Length, pipe2.Length);  //  
          }
        } else {
          UnityEngine.Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      UnityEngine.Debug.Log("Suction pipe ok!");
    }
    /// <summary>
    /// はじめの２つのBlockPattern で最小溶接間距離の相違がないかテストする
    /// </summary>
    [Test]
    public void CheckMinLengthDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_B1_V_H_S(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        UnityEngine.Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (! ( bpe is Chiyoda.CAD.Topology.BlockPattern bp )) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if ( array[ importer.Info.DischargeIndex ] is IGroup group ) {
          dischargeGroups.Add( group ) ;
        }
        if ( array[ importer.Info.SuctionIndex ] is IGroup group2 ) {
          suctionGroups.Add( group2 ) ;
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      UnityEngine.Debug.Log("Discharge pipes");
      for (int i = 0; i < dischargeLeafEdges[0].Length; ++i) {
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.Length != pipe2.Length){
            UnityEngine.Debug.Log($"pipe{i} has different length");
            Assert.AreEqual(pipe1.Length, pipe2.Length);  //  
          }
          Assert.AreEqual(pipe1.Length, pipe2.Length);  //  
        } else {
          UnityEngine.Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      UnityEngine.Debug.Log("Discharge pipe ok!");
      UnityEngine.Debug.Log("Suction pipes");
      for (int i = 0; i < suctionLeafEdges[0].Length; ++i) {
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.Length != pipe2.Length){
            UnityEngine.Debug.Log($"pipe{i} has different length");
            Assert.AreEqual(pipe1.Length, pipe2.Length);  //  
          }
        } else {
          UnityEngine.Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      UnityEngine.Debug.Log("Suction pipe ok!");
    }
    /// <summary>
    /// 登録された通常パイプおよびOlet付きパイプのうち、MinLength = 0 のものがあれば表示する
    /// </summary>
    [Test]
    public void CheckMinLengthZeroPipe()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_B1_V_H_S(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);
      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (! ( bpe is Chiyoda.CAD.Topology.BlockPattern bp )) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if ( array[ importer.Info.DischargeIndex ] is IGroup group ) {
          dischargeGroups.Add( group ) ;
        }
        if ( array[ importer.Info.SuctionIndex ] is IGroup group2 ) {
          suctionGroups.Add( group2 ) ;
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);


      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      int error = 0;
      int numpipe = 0;
      for (int pumpix = 0; pumpix < 2; ++pumpix){
        UnityEngine.Debug.Log($"Pump({pumpix}) Discharge pipes");
        foreach (var ix in importer.Info.DischargeNormalPipes) {
          if (dischargeLeafEdges[pumpix][ix].PipingPiece is Pipe pipe) {
            if (pipe.MinimumLengthWithoutOletRadius == 0){
              ++error;
              UnityEngine.Debug.Log($"Pump ({pumpix}) MinimumLengthWithoutOletRadius of index {ix} is zero");
            }
            ++numpipe;
          }
        }
        foreach (var ix in importer.Info.DischargeOletPrePostPipes) {
          if (dischargeLeafEdges[pumpix][ix].PipingPiece is Pipe pipe) {
            if (pipe.MinimumLengthWithoutOletRadius == 0){
              ++error;
              UnityEngine.Debug.Log($"Pump ({pumpix}) MinimumLengthWithoutOletRadius of index {ix} is zero");
            }
            ++numpipe;
          }
        }
        UnityEngine.Debug.Log("Discharge pipe Done!");

        UnityEngine.Debug.Log($"Pump({pumpix}) Suction pipes");
        foreach (var ix in importer.Info.SuctionNormalPipes) {
          if (suctionLeafEdges[pumpix][ix].PipingPiece is Pipe pipe) {
            if (pipe.MinimumLengthWithoutOletRadius == 0){
              ++error;
              UnityEngine.Debug.Log($"Pump ({pumpix}) MinimumLengthWithoutOletRadius of index {ix} is zero");
            }
            ++numpipe;
          }
        }
        foreach (var ix in importer.Info.SuctionOletPrePostPipes) {
          if (suctionLeafEdges[pumpix][ix].PipingPiece is Pipe pipe) {
            if (pipe.MinimumLengthWithoutOletRadius == 0){
              ++error;
              UnityEngine.Debug.Log($"Pump ({pumpix}) MinimumLengthWithoutOletRadius of index {ix} is zero");
            }
            ++numpipe;
          }
        }
        UnityEngine.Debug.Log("Suction pipe Done!");
      }
      UnityEngine.Debug.Log($"{numpipe} pipes confirmed, {numpipe/2} each.");
      UnityEngine.Debug.Log($"{error} pipes has MinimumLengthWithoutOletRadius zero.");
      Assert.AreEqual(error, 0);  //   MinLength = 0 の通常PipeがあればAssert
    }
  }

}
