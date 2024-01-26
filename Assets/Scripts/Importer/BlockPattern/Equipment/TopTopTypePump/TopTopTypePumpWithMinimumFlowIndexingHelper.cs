//#define DEBUG_DUMP

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
  class TopTopTypePumpWithMinimumFlowIndexingHelper
  {
    /**
     *  Range 指定されたindex から個別 Pipe のindex を抽出する
     */
    public static void BuildIndexList(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      TopTopTypePumpIndexingHelper.BuildIndexList(bpa, bp, info);
      List<int> indexList,oletIndexList;

      Dictionary<LeafEdge, int> dischargeLeafEdges = new Dictionary<LeafEdge, int>();
      Dictionary<LeafEdge, int> minimumFlowPreLeafEdges = new Dictionary<LeafEdge, int>();
      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {

        //  Minimum flow in discharge group pipes
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.DischargeIndex) as IGroup;
        if (dischargeGroup != null){
          for (int ix = 0; ix < dischargeGroup.EdgeCount; ++ix)
          {
            LeafEdge le = dischargeGroup?.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
            dischargeLeafEdges.Add(le, ix);
          }
          (indexList, oletIndexList) = GetMinimumFlowInDischargeGroupPipeIndices(bpa, bp, info, dischargeLeafEdges);
          if (indexList != null || oletIndexList != null){
            infoDerived.MinimumFlowInDischargeGroupNormalPipes = indexList.ToArray() ;
            infoDerived.MinimumFlowInDischargeGroupOletPrePostPipes = oletIndexList.ToArray(); ;
          }
        }


        //  Minimum flow pre pipes
        var minimumFlowPreGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowPreIndex) as IGroup;
        if (minimumFlowPreGroup == null)
          return;
        for (int ix = 0; ix < minimumFlowPreGroup.EdgeCount; ++ix)
        {
          LeafEdge le = minimumFlowPreGroup?.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
          minimumFlowPreLeafEdges.Add(le, ix);
        }
        (indexList, oletIndexList) = GetMinimumFlowPrePipeIndices(bpa, bp, info, minimumFlowPreLeafEdges);
        if (indexList != null || oletIndexList != null){
          infoDerived.MinimumFlowPreNormalPipes = indexList.ToArray() ;
          infoDerived.MinimumFlowPreOletPrePostPipes = oletIndexList.ToArray(); ;
        }

        //  minimum flow pipes
        Dictionary<LeafEdge, int> minimumFlowLeafEdges = new Dictionary<LeafEdge, int>();
        var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;
        if (minimumFlowGroup == null)
          return;
        for (int ix = 0; ix < minimumFlowGroup.EdgeCount; ++ix)
        {
          LeafEdge le = minimumFlowGroup?.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
          minimumFlowLeafEdges.Add(le, ix);
        }
        (indexList, oletIndexList) = GetMinimumFlowPipeIndices(bpa, bp, info, minimumFlowLeafEdges);
        if (indexList != null || oletIndexList != null){
          infoDerived.MinimumFlowNormalPipes = indexList.ToArray() ;
          infoDerived.MinimumFlowOletPrePostPipes = oletIndexList.ToArray(); ;
        }
      }
    }

    /**
     *  2つのEdge と group を指定すると、その２点をつなぐ経路を探索する。
     *  ただし、最短経路探索ではないので、複数の経路がある場合は、どちらに
     *  なるかは不定
     */
    public static (LeafEdge[], int) FindTheStraightPath(IGroup group, int startIndex, int endIndex)
    {
      return TopTopTypePumpGroupLeafEdgeExtractor.FindTheStraightPath(group, startIndex, endIndex);
    }
    /**
      *  SuctionFlex 用のインデックスを抽出する
      */
    public static List<int> GetSuctionFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges)
    {
      return TopTopTypePumpIndexingHelper.GetSuctionFlexIndices(bpa, bp, info, suctionLeafEdges);
    }
    /**
     * DischargeFlex 用のインデックスを抽出する
     */
    public static List<int> GetDischargeFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges)
    {
      return TopTopTypePumpIndexingHelper.GetSuctionFlexIndices(bpa, bp, info, dischargeLeafEdges);
    }

   /**
     *  SuctionPipe 用のインデックスを抽出する
     *  @return NormalPipeIndices , OletPipeIndices
     */
    public static (List<int>,List<int>) GetSuctionPipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges) 
    {
      return TopTopTypePumpIndexingHelper.GetSuctionPipeIndices(bpa, bp, info, suctionLeafEdges);
    }
    /**
     *  DischargePipe 用のインデックスを抽出する
     *  @return NormalPipeIndices , OletPipeIndices
     */
    public static (List<int>,List<int>) GetDischargePipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges) 
    {
      return TopTopTypePumpIndexingHelper.GetDischargePipeIndices(bpa, bp, info, dischargeLeafEdges);
    }

    /**
     * Discharge group 内の MinimumFlow Pipe 用のインデックスを抽出する
    */
    public static (List<int>, List<int>) GetMinimumFlowInDischargeGroupPipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> minimumFlowPreLeafEdges)
    {
      List<int> minimumFlowInDischargeGroupNormalResult = new List<int>();
      List<int> minimumFlowInDischargeGroupOletResult = new List<int>();
      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        if (infoDerived.MinimumFlowInDischargeGroupPipeIndexRange == null)
          return (null, null);
        var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.DischargeIndex) as IGroup;
        if (dischargeGroup == null)
          return (null, null);
        Dictionary<LeafEdge, int> leafedges = minimumFlowPreLeafEdges;
        for (var row = 0; row < infoDerived.MinimumFlowInDischargeGroupPipeIndexRange.GetLength(0); ++row) {

          if (infoDerived.MinimumFlowInDischargeGroupPipeIndexRange.GetLength(1) == 2) {
            LeafEdge[] resultbuffer;
            int resultbuffersize;
            int startindex = infoDerived.MinimumFlowInDischargeGroupPipeIndexRange[row, 0];
            int endindex = infoDerived.MinimumFlowInDischargeGroupPipeIndexRange[row, 1];
            (resultbuffer, resultbuffersize) = FindTheStraightPath(dischargeGroup, startindex, endindex);

            for (int i = 0; i < resultbuffersize; ++i) {
              if (resultbuffer[i].PipingPiece is Pipe aPipe) {
                int edgeindex = leafedges[resultbuffer[i]];
                LeafEdge le0, le1;
                le0 = resultbuffer[i].GetVertex(0)?.Partner?.LeafEdge;
                le1 = resultbuffer[i].GetVertex(1)?.Partner?.LeafEdge;
                if ((le0 != null && le0.PipingPiece is WeldOlet) || (le1 != null && le1.PipingPiece is WeldOlet)) {
                  minimumFlowInDischargeGroupOletResult.Add(edgeindex);
                } else
                  minimumFlowInDischargeGroupNormalResult.Add(edgeindex);
              }
            }
#if DEBUG_DUMP
            string result = "mini-flow in discharge group normal pipe";
            foreach (var ix in minimumFlowInDischargeGroupNormalResult) {
              result += $"{ix} ";
            }
            UnityEngine.Debug.Log(result);
            result = "mini-flow pre pipe with olet ";
            foreach (var ix in minimumFlowInDischargeGroupOletResult) {
              result += $"{ix} ";
            }
            UnityEngine.Debug.Log(result);
#endif
          }
        }
      }
      return (minimumFlowInDischargeGroupNormalResult,minimumFlowInDischargeGroupOletResult);    
    }

    /**
     * MinimumFlow Pre Pipe 用のインデックスを抽出する
    */
    public static (List<int>,List<int>) GetMinimumFlowPrePipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> minimumFlowPreLeafEdges) 
    {
      List<int> minimumFlowPreNormalResult = new List<int>();
      List<int> minimumFlowPreOletResult = new List<int>();
      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        if (infoDerived.MinimumFlowPrePipeIndexRange == null)
          return (null,null);
        var minimumFlowPreGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowPreIndex) as IGroup;
        if (minimumFlowPreGroup == null)
          return (null,null);
        Dictionary<LeafEdge, int> leafedges = minimumFlowPreLeafEdges;
        for (var row = 0; row < infoDerived.MinimumFlowPrePipeIndexRange.GetLength(0); ++row) {

          if (infoDerived.MinimumFlowPrePipeIndexRange.GetLength(1) == 2)
          {
            LeafEdge[] resultbuffer;
            int resultbuffersize;
            int startindex = infoDerived.MinimumFlowPrePipeIndexRange[row, 0];
            int endindex = infoDerived.MinimumFlowPrePipeIndexRange[row, 1];
            (resultbuffer, resultbuffersize) = FindTheStraightPath(minimumFlowPreGroup, startindex, endindex);
          
            for (int i = 0; i < resultbuffersize; ++i) {
              if (resultbuffer[i].PipingPiece is Pipe aPipe) {
                int edgeindex = leafedges[resultbuffer[i]];
                LeafEdge le0, le1;
                le0 = resultbuffer[i].GetVertex(0)?.Partner?.LeafEdge;
                le1 = resultbuffer[i].GetVertex(1)?.Partner?.LeafEdge;
                if ((le0 != null && le0.PipingPiece is WeldOlet) || (le1 != null && le1.PipingPiece is WeldOlet)){
                  minimumFlowPreOletResult.Add(edgeindex);              
                }else
                  minimumFlowPreNormalResult.Add(edgeindex);
              }
            }
            #if DEBUG_DUMP
            string result = "mini-flow pre normal pipe";
            foreach(var ix in minimumFlowPreNormalResult){
              result += $"{ix} ";
            }
            UnityEngine.Debug.Log(result);
            result = "mini-flow pre pipe with olet ";
            foreach(var ix in minimumFlowPreOletResult){
              result += $"{ix} ";
            }
            UnityEngine.Debug.Log(result);
            #endif
          }
        }
        return (minimumFlowPreNormalResult,minimumFlowPreOletResult);    

      } else
        return (null, null);
    }

    /**
     * MinimumFlow Pipe 用のインデックスを抽出する
    */
    public static (List<int>,List<int>) GetMinimumFlowPipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> minimumFlowLeafEdges) 
    {
      List<int> minimumFlowNormalResult = new List<int>();
      List<int> minimumFlowOletResult = new List<int>();
      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        if (infoDerived.MinimumFlowPipeIndexRange == null)
          return (null,null);
        var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;
        if (minimumFlowGroup == null)
          return (null,null);
        Dictionary<LeafEdge, int> leafedges = minimumFlowLeafEdges;
        for (var row = 0; row < infoDerived.MinimumFlowPipeIndexRange.GetLength(0); ++row) {

          if (infoDerived.MinimumFlowPipeIndexRange.GetLength(1) == 2)
          {
            LeafEdge[] resultbuffer;
            int resultbuffersize;
            int startindex = infoDerived.MinimumFlowPipeIndexRange[row, 0];
            int endindex = infoDerived.MinimumFlowPipeIndexRange[row, 1];
            (resultbuffer, resultbuffersize) = FindTheStraightPath(minimumFlowGroup, startindex, endindex);
          
            for (int i = 0; i < resultbuffersize; ++i) {
              if (resultbuffer[i].PipingPiece is Pipe aPipe) {
                int edgeindex = leafedges[resultbuffer[i]];
                LeafEdge le0, le1;
                le0 = resultbuffer[i].GetVertex(0)?.Partner?.LeafEdge;
                le1 = resultbuffer[i].GetVertex(1)?.Partner?.LeafEdge;
                if ((le0 != null && le0.PipingPiece is WeldOlet) || (le1 != null && le1.PipingPiece is WeldOlet)){
                  minimumFlowOletResult.Add(edgeindex);              
                }else
                  minimumFlowNormalResult.Add(edgeindex);
              }
            }
            #if DEBUG_DUMP
            string result = "mini-flow normal ";
            foreach(var ix in minimumFlowNormalResult){
              result += $"{ix} ";
            }
            UnityEngine.Debug.Log(result);
            result = "mini-flow olet ";
            foreach(var ix in minimumFlowOletResult){
              result += $"{ix} ";
            }
            UnityEngine.Debug.Log(result);
            #endif
          }
        }
        return (minimumFlowNormalResult,minimumFlowOletResult);    

      } else
        return (null, null);
    }

  }
}
