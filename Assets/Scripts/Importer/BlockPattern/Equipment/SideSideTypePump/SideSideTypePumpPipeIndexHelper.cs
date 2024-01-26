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
  public class SideSideTypePumpPipeIndexHelper
  {
    /**
     *  2つのEdge と group を指定すると、その２点をつなぐ経路を探索する。
     *  ただし、最短経路探索ではないので、複数の経路がある場合は、どちらに
     *  なるかは不定
     */
    private static (LeafEdge[], int) FindTheStraightPath(IGroup group, int startIndex, int endIndex) {
      LeafEdge goalEdge = group.EdgeList.ElementAtOrDefault(endIndex) as LeafEdge;
      if (goalEdge == null)
        return (null, 0);
      //  buffer to store the result
      LeafEdge[] resultbuffer = new LeafEdge[group.EdgeCount];
      int resultbuffersize = 0;

      //  stacks to recurse.
      LeafEdge[] prevstack = new LeafEdge[group.EdgeCount];
      LeafEdge[] readstack = new LeafEdge[group.EdgeCount];
      int[] writeindexstack = new int[group.EdgeCount];
      int stackindex = 0;

      int maxloop = group.EdgeCount;
      //  push the first edge
      readstack[stackindex] = group?.EdgeList.ElementAtOrDefault(startIndex) as LeafEdge;
      prevstack[stackindex] = null;
      writeindexstack[stackindex++] = 0;

      //  first prepare the straight line
      while (stackindex > 0)
      {
        if (0 == --maxloop)
          break;    //  watch dog
        LeafEdge lecurrent = readstack[--stackindex];
        int writeindex = writeindexstack[stackindex];
        LeafEdge prev = prevstack[stackindex];

        resultbuffer[writeindex++] = lecurrent;
        resultbuffersize = writeindex;

        if (lecurrent == goalEdge)
          break;

        //  store branch into stack
        for (int i = 0; i < lecurrent.VertexCount; ++i)
        {
          LeafEdge branch = lecurrent.GetVertex(i)?.Partner?.LeafEdge;
          if (branch != null && branch != prev)
          {
            if (branch.Group == lecurrent.Group){
              readstack[stackindex] = branch;
              writeindexstack[stackindex] = writeindex;
              prevstack[stackindex++] = lecurrent;
            }
          }
        }
      }
      return (resultbuffer, resultbuffersize);
    }
    /**
     *  SuctionFlex 用のインデックスを抽出する
     */
    public static List<int> GetSuctionFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges) {
      List<int> suctionResult = new List<int>();
      if (info.SuctionFlexHelper == null)
        return null;
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
      if (suctionGroup == null)
        return null;
      Dictionary<LeafEdge, int> leafedges = suctionLeafEdges;
      for (var row = 0; row < info.SuctionFlexHelper.GetLength(0); ++row) {
        if (info.SuctionFlexHelper.GetLength(1) == 2)
        {
          LeafEdge[] resultbuffer;
          int resultbuffersize;
          int startindex = info.SuctionFlexHelper[row, 0];
          int endindex = info.SuctionFlexHelper[row, 1];
          (resultbuffer, resultbuffersize) = FindTheStraightPath(suctionGroup, startindex, endindex);

          for (int i = 0; i < resultbuffersize; ++i) {
            if (resultbuffer[i].PipingPiece is Pipe aPipe) {
              int edgeindex = leafedges[resultbuffer[i]];
              suctionResult.Add(edgeindex);
            }
          }

        }
      }
      return suctionResult;
    }
    /**
     * DischargeFlex 用のインデックスを抽出する
     */
    public static List<int> GetDischargeFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges) {
      List<int> suctionResult = new List<int>();
      if (info.DischargeFlexHelper == null)
        return null;
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
      if (dischargeGroup == null)
        return null;
      Dictionary<LeafEdge, int> leafedges = dischargeLeafEdges;
      for (var row = 0; row < info.DischargeFlexHelper.GetLength(0); ++row) {
        if (info.SuctionFlexHelper.GetLength(1) == 2)
        {
          LeafEdge[] resultbuffer;
          int resultbuffersize;
          int startindex = info.DischargeFlexHelper[row, 0];
          int endindex = info.DischargeFlexHelper[row, 1];
          (resultbuffer, resultbuffersize) = FindTheStraightPath(dischargeGroup, startindex, endindex);
          for (int i = 0; i < resultbuffersize; ++i) {
            if (resultbuffer[i].PipingPiece is Pipe aPipe) {
              int edgeindex = leafedges[resultbuffer[i]];
              suctionResult.Add(edgeindex);
            }
          }
        }
      }
      return suctionResult;
    }
    /**
     *  SuctionPipe 用のインデックスを抽出する
     *  @return NormalPipeIndices , OletPipeIndices
     */
    public static (List<int>,List<int>) GetSuctionPipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges) {
      List<int> suctionNormalResult = new List<int>();
      List<int> suctionOletResult = new List<int>();
      if (info.SuctionPipeIndexRange == null)
        return (null,null);
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
      if (suctionGroup == null)
        return (null,null);
      Dictionary<LeafEdge, int> leafedges = suctionLeafEdges;
      for (var row = 0; row < info.SuctionPipeIndexRange.GetLength(0); ++row) {
        if (info.SuctionPipeIndexRange.GetLength(1) == 2)
        {
          LeafEdge[] resultbuffer;
          int resultbuffersize;
          int startindex = info.SuctionPipeIndexRange[row, 0];
          int endindex = info.SuctionPipeIndexRange[row, 1];
          (resultbuffer, resultbuffersize) = FindTheStraightPath(suctionGroup, startindex, endindex);
          for (int i = 0; i < resultbuffersize; ++i) {
            if (resultbuffer[i].PipingPiece is Pipe aPipe) {
              int edgeindex = leafedges[resultbuffer[i]];
              LeafEdge le0, le1;
              le0 = resultbuffer[i].GetVertex(0)?.Partner?.LeafEdge;
              le1 = resultbuffer[i].GetVertex(1)?.Partner?.LeafEdge;
              if ((le0 != null && le0.PipingPiece is WeldOlet) || (le1 != null && le1.PipingPiece is WeldOlet)
                ||(le0 != null && le0.PipingPiece is StubInReinforcingWeld) || (le1 != null && le1.PipingPiece is StubInReinforcingWeld)){
                suctionOletResult.Add(edgeindex);              
              }else
                suctionNormalResult.Add(edgeindex);
            }
          }
        }
      }
      return (suctionNormalResult,suctionOletResult);
    }
    /**
     *  DischargePipe 用のインデックスを抽出する
     *  @return NormalPipeIndices , OletPipeIndices
     */
    public static (List<int>,List<int>) GetDischargePipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges) {
      List<int> dischargeNormalResult = new List<int>();
      List<int> dischargeOletResult = new List<int>();
      if (info.DischargePipeIndexRange == null)
        return (null,null);
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
      if (dischargeGroup == null)
        return (null,null);
      Dictionary<LeafEdge, int> leafedges = dischargeLeafEdges;
      for (var row = 0; row < info.DischargePipeIndexRange.GetLength(0); ++row) {
        if (info.DischargePipeIndexRange.GetLength(1) == 2)
        {
          LeafEdge[] resultbuffer;
          int resultbuffersize;
          int startindex = info.DischargePipeIndexRange[row, 0];
          int endindex = info.DischargePipeIndexRange[row, 1];
          (resultbuffer, resultbuffersize) = FindTheStraightPath(dischargeGroup, startindex, endindex);
          for (int i = 0; i < resultbuffersize; ++i) {
            if (resultbuffer[i].PipingPiece is Pipe aPipe) {
              int edgeindex = leafedges[resultbuffer[i]];
              LeafEdge le0, le1;
              le0 = resultbuffer[i].GetVertex(0)?.Partner?.LeafEdge;
              le1 = resultbuffer[i].GetVertex(1)?.Partner?.LeafEdge;
              if ((le0 != null && le0.PipingPiece is WeldOlet) || (le1 != null && le1.PipingPiece is WeldOlet)){
                dischargeOletResult.Add(edgeindex);              
              }else
                dischargeNormalResult.Add(edgeindex);
            }
          }
        }
      }
      return (dischargeNormalResult,dischargeOletResult);
    }
    public static void BuildFlexList(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
      //  preparation
      List<int> indexList,oletIndexList;
      Dictionary<LeafEdge, int> suctionLeafEdges = new Dictionary<LeafEdge, int>();
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
      if (suctionGroup == null)
        return;
      for (int ix = 0; ix < suctionGroup.EdgeCount; ++ix)
      {
        LeafEdge le = suctionGroup?.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
        suctionLeafEdges.Add(le, ix);
      }
      Dictionary<LeafEdge, int> dischargeLeafEdges = new Dictionary<LeafEdge, int>();
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
      if (dischargeGroup == null)
        return;
      for (int ix = 0; ix < dischargeGroup.EdgeCount; ++ix)
      {
        LeafEdge le = dischargeGroup?.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
        dischargeLeafEdges.Add(le, ix);
      }

      //  Setups discharge flex index list
      if (null != (indexList = GetSuctionFlexIndices(bpa, bp, info, suctionLeafEdges)))
        info.SuctionFlexIndexList = indexList;
      //  Setups discharge flex index list
      if (null != (indexList = GetDischargeFlexIndices(bpa, bp, info, dischargeLeafEdges)))
        info.DischargeFlexIndexList = indexList;
      //  Suction pipes
      (indexList, oletIndexList) = GetSuctionPipeIndices(bpa, bp, info, suctionLeafEdges);
      if (indexList != null || oletIndexList != null){
        info.SuctionNormalPipes = indexList.ToArray() ;
        info.SuctionOletPrePostPipes = oletIndexList.ToArray(); ;
      }
      //  Discharge pipes
      (indexList, oletIndexList) = GetDischargePipeIndices(bpa, bp, info, dischargeLeafEdges);
      if (indexList != null || oletIndexList != null){
        info.DischargeNormalPipes = indexList.ToArray() ;
        info.DischargeOletPrePostPipes = oletIndexList.ToArray(); ;
      }
    }
  }
}
