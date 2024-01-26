using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.TopTopTypePump ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_G_N ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  public class TopTopTypePumpTT_A1_S_G_NTest
  {

    /// <summary>
    /// はじめの２つのBlockPattern で最小溶接間距離の相違がないかテストする
    /// </summary>
    [Test]
    public void CheckMinLengthDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new TopTopTypePumpTT_A1_S_G_N(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      int errCnt = 0;
      Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      var dischargeGroups = new List<IGroup> { };
      var suctionGroups = new List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      Debug.Log("Discharge pipes");
      var info = importer.Info;

      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);

      foreach (int i in dischargeIndices){
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.MinimumLengthWithoutOletRadius != pipe2.MinimumLengthWithoutOletRadius) {
            UnityEngine.Debug.Log($"pipe{i} has different min-length-without-olet-radius one is {pipe1.MinimumLengthWithoutOletRadius} the other has {pipe2.MinimumLengthWithoutOletRadius}");
            ++errCnt;
          } else{
            UnityEngine.Debug.Log($"pipe{i} has same min-length-without-olet-radius one is {pipe1.MinimumLengthWithoutOletRadius} the other has {pipe2.MinimumLengthWithoutOletRadius}");
          }

        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Discharge pipe ok!");
      Debug.Log("Suction pipes");
      foreach (int i in suctionIndices){
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.MinimumLengthWithoutOletRadius != pipe2.MinimumLengthWithoutOletRadius) {
            UnityEngine.Debug.Log($"pipe{i} has different min-length-without-olet-radius one is {pipe1.MinimumLengthWithoutOletRadius} the other has {pipe2.MinimumLengthWithoutOletRadius}");
            ++errCnt;
          } else{
            UnityEngine.Debug.Log($"pipe{i} has same min-length-without-olet-radius one is {pipe1.MinimumLengthWithoutOletRadius} the other has {pipe2.MinimumLengthWithoutOletRadius}");
          }
        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Suction pipe ok!");
      if (errCnt > 0){
        Debug.Log($"{errCnt} pipes has different minimum length.");
      }
      Assert.AreEqual(errCnt, 0);
    }

    /// <summary>
    /// はじめの２つのBlockPattern でパイプ長の相違がないかテストする
    /// </summary>
    [Test]
    public void CheckLengthDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new TopTopTypePumpTT_A1_S_G_N(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      int errCnt = 0;

      Debug.Log($"{blockPattern.Type.ToString()}");
      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      var dischargeGroups = new List<IGroup> { };
      var suctionGroups = new List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      Debug.Log("Discharge pipes");
      var info = importer.Info;

      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);

      foreach (int i in dischargeIndices){
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.Length != pipe2.Length) {
            Debug.Log($"pipe{i} has different length one is {pipe1.Length} the other has {pipe2.Length}");
            ++errCnt;
          } else{
            Debug.Log($"pipe{i} has same length one is {pipe1.Length} the other has {pipe2.Length}");
          }

        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Discharge pipe ok!");
      Debug.Log("Suction pipes");
      foreach (int i in suctionIndices){
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.Length != pipe2.Length) {
            Debug.Log($"pipe{i} has different length one is {pipe1.Length} the other has {pipe2.Length}");
            ++errCnt;
          } else{
            Debug.Log($"pipe{i} has same length one is {pipe1.Length} the other has {pipe2.Length}");
          }
        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Suction pipe ok!");
      if (errCnt > 0){
        Debug.Log($"{errCnt} pipes has different length.");
      }
      Assert.AreEqual(errCnt, 0);

    }
    /// <summary>
    /// はじめの２つのBlockPattern でPreferredLength の相違がないかテストする
    /// </summary>
    [Test]
    public void CheckPreferredLengthDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new TopTopTypePumpTT_A1_S_G_N(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      int errCnt = 0;

      var dischargeGroups = new List<IGroup> { };
      var suctionGroups = new List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      Debug.Log("Discharge pipes");
      var info = importer.Info;

      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);

      foreach (int i in dischargeIndices) {
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.PreferredLength != pipe2.PreferredLength) {
            Debug.Log($"pipe{i} has different pref-length one is {pipe1.PreferredLength} the other has {pipe2.PreferredLength}");
            ++errCnt;
          } else {
            Debug.Log($"pipe{i} has same pref-length one is {pipe1.PreferredLength} the other has {pipe2.PreferredLength}");
          }

        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Discharge pipe ok!");
      Debug.Log("Suction pipes");
      foreach (int i in suctionIndices) {
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.PreferredLength != pipe2.PreferredLength) {
            Debug.Log($"pipe{i} has different pref-length one is {pipe1.PreferredLength} the other has {pipe2.PreferredLength}");
            ++errCnt;
          } else {
            Debug.Log($"pipe{i} has same pref-length one is {pipe1.PreferredLength} the other has {pipe2.PreferredLength}");
          }
        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Suction pipe ok!");
      if (errCnt > 0){
        Debug.Log($"{errCnt} pipes has different pref-length.");
      }
      Assert.AreEqual(errCnt, 0);
    }

    /// <summary>
    /// はじめの２つのBlockPattern でFlexRatio の相違がないかテストする
    /// </summary>
    [Test]
    public void CheckでFlexRatioDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new TopTopTypePumpTT_A1_S_G_N(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray)blockPattern.Parent;
      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      var dischargeGroups = new List<IGroup> { };
      var suctionGroups = new List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (!(bpe is Chiyoda.CAD.Topology.BlockPattern bp)) continue;
        Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if (array[importer.Info.DischargeIndex] is IGroup group) {
          dischargeGroups.Add(group);
        }
        if (array[importer.Info.SuctionIndex] is IGroup group2) {
          suctionGroups.Add(group2);
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      int errCnt = 0;
      Debug.Log("Discharge pipes");
      var info = importer.Info;

      int[] dischargeIndices = new int[info.DischargeNormalPipes.Length + info.DischargeOletPrePostPipes.Length];
      info.DischargeNormalPipes.CopyTo(dischargeIndices, 0);
      info.DischargeOletPrePostPipes.CopyTo(dischargeIndices, info.DischargeNormalPipes.Length);

      int[] suctionIndices = new int[info.SuctionNormalPipes.Length + info.SuctionOletPrePostPipes.Length];
      info.SuctionNormalPipes.CopyTo(suctionIndices, 0);
      info.SuctionOletPrePostPipes.CopyTo(suctionIndices, info.SuctionNormalPipes.Length);

      foreach (int i in dischargeIndices){
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.FlexRatio != pipe2.FlexRatio) {
            Debug.Log($"pipe{i} has different flex-ratio one is {pipe1.FlexRatio} the other has {pipe2.FlexRatio}");
            ++errCnt;
          } else{
            Debug.Log($"pipe{i} has same flex-ratio one is {pipe1.FlexRatio} the other has {pipe2.FlexRatio}");
          }

        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Discharge pipe Done!");
      Debug.Log("Suction pipes");
      foreach (int i in suctionIndices){
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.FlexRatio != pipe2.FlexRatio) {
            Debug.Log($"pipe{i} has different flex-ratio one is {pipe1.FlexRatio} the other has {pipe2.FlexRatio}");
            ++errCnt;
          } else{
            Debug.Log($"pipe{i} has same flex-ratio one is {pipe1.FlexRatio} the other has {pipe2.FlexRatio}");
          }
        } else {
          Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      Debug.Log("Suction pipe Done!");
      if (errCnt != 0){
        Debug.Log($"{errCnt} pipes have different flex-ratio!");

      }
      Assert.AreEqual(errCnt,0);
    }
  }

}
