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
  class EndTopTypeMinimumFlowPipeIndexHelper
  {
    /**
     *  2つのEdge と group を指定すると、その２点をつなぐ経路を探索する。
     *  ただし、最短経路探索ではないので、複数の経路がある場合は、どちらに
     *  なるかは不定
     */
    public static (LeafEdge[], int) FindTheStraightPath(IGroup group, int startIndex, int endIndex)
    {
      return EndTopTypePumpPipeIndexHelper.FindTheStraightPath(group, startIndex, endIndex);
    }
    /**
      *  SuctionFlex 用のインデックスを抽出する
      */
    public static List<int> GetSuctionFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges)
    {
      return EndTopTypePumpPipeIndexHelper.GetSuctionFlexIndices(bpa, bp, info, suctionLeafEdges);
    }
    /**
     * DischargeFlex 用のインデックスを抽出する
     */
    public static List<int> GetDischargeFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges)
    {
      return EndTopTypePumpPipeIndexHelper.GetSuctionFlexIndices(bpa, bp, info, dischargeLeafEdges);
    }

   /**
     *  SuctionPipe 用のインデックスを抽出する
     *  @return NormalPipeIndices , OletPipeIndices
     */
    public static (List<int>,List<int>) GetSuctionPipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges) 
    {
      return EndTopTypePumpPipeIndexHelper.GetSuctionPipeIndices(bpa, bp, info, suctionLeafEdges);
    }
    /**
     *  DischargePipe 用のインデックスを抽出する
     *  @return NormalPipeIndices , OletPipeIndices
     */
    public static (List<int>,List<int>) GetDischargePipeIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges) 
    {
      return EndTopTypePumpPipeIndexHelper.GetDischargePipeIndices(bpa, bp, info, dischargeLeafEdges);
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


    /**
     *  Range 指定されたindex から個別 Pipe のindex を抽出する
     */
    public static void BuildIndexList(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info)
    {
      EndTopTypePumpPipeIndexHelper.BuildIndexList(bpa, bp, info);
      List<int> indexList,oletIndexList;

      Dictionary<LeafEdge, int> minimumFlowLeafEdges = new Dictionary<LeafEdge, int>();
      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        var minimumFlowGroup = bp.NonEquipmentEdges.ElementAtOrDefault(infoDerived.MinimumFlowIndex) as IGroup;
        if (minimumFlowGroup == null)
          return;
        /*
        for (int ix = 0; ix < minimumFlowGroup.EdgeCount; ++ix)
        {
          LeafEdge le = minimumFlowGroup?.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
          minimumFlowLeafEdges.Add(le, ix);
        }
        */
        //  list のシーケンシャルアクセスには、ElementAt より foreachの方が若干パフォーマンスが良いと期待
        if (minimumFlowGroup != null){
          int ix = 0;
          foreach (var edge in minimumFlowGroup.EdgeList){
            if (edge is LeafEdge le){
              minimumFlowLeafEdges.Add(le,ix);
            }
            ++ix;
          }
        }
        (indexList, oletIndexList) = GetMinimumFlowPipeIndices(bpa, bp, info, minimumFlowLeafEdges);
        if (indexList != null || oletIndexList != null){
          infoDerived.MinimumFlowNormalPipes = indexList.ToArray() ;
          infoDerived.MinimumFlowOletPrePostPipes = oletIndexList.ToArray(); ;
        }
      }
    }
  }
}
