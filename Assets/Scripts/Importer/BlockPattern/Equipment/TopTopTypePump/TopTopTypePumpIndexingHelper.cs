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
  class TopTopTypePumpIndexingHelper
  {
    ///
    /// <summary>BlockPattern を頂点とする Hierarchy 内を巡回して、インデックスをPipe と Olet付き Pipe に整理する</summary>
    /// <param name="bpa">BasePump が所属するCompositeBlockPattern</param>
    /// <param name="BlockPattern">LeafEdge が所属する BlockPattern(Pump や Filter 本体)</param>
    /// <param name="info">Index が格納されたSingleBlockPatternInfo</param>
    /// 
    public static void BuildIndexList(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info){
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

    ///
    /// <summary>SuctionFlex 用のインデックスを抽出する</summary>
    /// <param name="bpa">BasePump が所属するCompositeBlockPattern</param>
    /// <param name="BlockPattern">LeafEdge が所属する BlockPattern(Pump や Filter 本体)</param>
    /// <param name="info">Index が格納されたSingleBlockPatternInfo</param>
    /// <param name="suctionLeafEdges">suctionGroup のLeafEdge を格納したDictionaryオブジェクト</param>
    /// <remarks>使い方は、BuildIndexListを参照のこと</remarks>
    /// 
    public static List<int> GetSuctionFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> suctionLeafEdges) {
      List<int> suctionResult = new List<int>();
      if (info.SuctionFlexHelper == null)
        return null;
      var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
      if (suctionGroup == null)
        return null;
      Dictionary<LeafEdge, int> leafedges = suctionLeafEdges;
      for (var row = 0; row < info.
      SuctionFlexHelper.GetLength(0); ++row) {
        if (info.SuctionFlexHelper.GetLength(1) == 2)
        {
          LeafEdge[] resultbuffer;
          int resultbuffersize;
          int startindex = info.SuctionFlexHelper[row, 0];
          int endindex = info.SuctionFlexHelper[row, 1];
          (resultbuffer, resultbuffersize) = TopTopTypePumpGroupLeafEdgeExtractor.FindTheStraightPath(suctionGroup, startindex, endindex);
          for (int i = 0; i < resultbuffersize; ++i) {
            if (resultbuffer[i].PipingPiece is Pipe aPipe) {
              int edgeindex = leafedges[resultbuffer[i]];
              suctionResult.Add(edgeindex);
            }
          }
        }
      }
      #if DEBUG_DUMP
      string result = "suction Flex ";
      foreach(var ix in suctionResult){
        result += $"{ix} ";
      }
      UnityEngine.Debug.Log(result);
      #endif

      return suctionResult;
    }

    ///
    /// <summary>DischargeFlex 用のインデックスを抽出する</summary>
    /// <param name="bpa">BasePump が所属するCompositeBlockPattern</param>
    /// <param name="BlockPattern">LeafEdge が所属する BlockPattern(Pump や Filter 本体)</param>
    /// <param name="info">Index が格納されたSingleBlockPatternInfo</param>
    /// <param name="dischargeLeafEdges">suctionGroup のLeafEdge を格納したDictionaryオブジェクト</param>
    /// <remarks>使い方は、BuildIndexListを参照のこと</remarks>
    /// 
    public static List<int> GetDischargeFlexIndices(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, Dictionary<LeafEdge, int> dischargeLeafEdges) {
      List<int> dischargeResult = new List<int>();
      if (info.DischargeFlexHelper == null)
        return null;
      var dischargeGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
      if (dischargeGroup == null)
        return null;
      Dictionary<LeafEdge, int> leafedges = dischargeLeafEdges;
      for (var row = 0; row < info.DischargeFlexHelper.GetLength(0); ++row) {
        if (info.DischargeFlexHelper.GetLength(1) == 2)
        {
          LeafEdge[] resultbuffer;
          int resultbuffersize;
          int startindex = info.DischargeFlexHelper[row, 0];
          int endindex = info.DischargeFlexHelper[row, 1];
          (resultbuffer, resultbuffersize) = TopTopTypePumpGroupLeafEdgeExtractor.FindTheStraightPath(dischargeGroup, startindex, endindex);
          for (int i = 0; i < resultbuffersize; ++i) {
            if (resultbuffer[i].PipingPiece is Pipe aPipe) {
              int edgeindex = leafedges[resultbuffer[i]];
              dischargeResult.Add(edgeindex);
            }
          }
        }
      }
      #if DEBUG_DUMP
      string result = "discharge Flex ";
      foreach(var ix in dischargeResult){
        result += $"{ix} ";
      }
      UnityEngine.Debug.Log(result);
      #endif
      return dischargeResult;
    }
    ///
    /// <summary>SuctionPipe 用のインデックスを抽出する</summary>
    /// <param name="bpa">BasePump が所属するCompositeBlockPattern</param>
    /// <param name="BlockPattern">LeafEdge が所属する BlockPattern(Pump や Filter 本体)</param>
    /// <param name="info">Index が格納されたSingleBlockPatternInfo</param>
    /// <param name="dischargeLeafEdges">suctionGroup のLeafEdge を格納したDictionaryオブジェクト</param>
    /// <remarks>MinLength の設定に使用する Index(DischargeNormalPipes/DischargeOletPrePostPipes)を用意する。
    /// 使い方は、BuildIndexListを参照のこと</remarks>
    /// 
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
          (resultbuffer, resultbuffersize) = TopTopTypePumpGroupLeafEdgeExtractor.FindTheStraightPath(suctionGroup, startindex, endindex);
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
          #if DEBUG_DUMP
          string result = "suction normal ";
          foreach(var ix in suctionNormalResult){
            result += $"{ix} ";
          }
          UnityEngine.Debug.Log(result);
          result = "suction olet ";
          foreach(var ix in suctionOletResult){
            result += $"{ix} ";
          }
          UnityEngine.Debug.Log(result);
          #endif
        }
      }
      return (suctionNormalResult,suctionOletResult);
    }

    ///
    /// <summary>DischargePipe 用のインデックスを抽出する</summary>
    /// <param name="bpa">BasePump が所属するCompositeBlockPattern</param>
    /// <param name="BlockPattern">LeafEdge が所属する BlockPattern(Pump や Filter 本体)</param>
    /// <param name="info">Index が格納されたSingleBlockPatternInfo</param>
    /// <param name="dischargeLeafEdges">suctionGroup のLeafEdge を格納したDictionaryオブジェクト</param>
    /// <remarks>MinLength の設定に使用する Index(SuctionNormalPipes/SuctionOletPrePostPipes)を用意する。
    /// 使い方は、BuildIndexListを参照のこと</remarks>
    /// 
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
          (resultbuffer, resultbuffersize) = TopTopTypePumpGroupLeafEdgeExtractor.FindTheStraightPath(dischargeGroup, startindex, endindex);
          
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
          #if DEBUG_DUMP
          string result = "discharge normal ";
          foreach(var ix in dischargeNormalResult){
            result += $"{ix} ";
          }
          UnityEngine.Debug.Log(result);
          result = "discharge olet ";
          foreach(var ix in dischargeOletResult){
            result += $"{ix} ";
          }
          UnityEngine.Debug.Log(result);
          #endif
        }
      }
      return (dischargeNormalResult,dischargeOletResult);
    }
    ///
    ///<summary>グループを指定して、指定されたインデックスから、指定されたインデックスまでの(支流を含まない)LeafEdge全てを示すIndex配列 を返す</summary>
    ///<param name="group">探索するグループ</param>
    ///<param name="startIndex">開始Index</param>
    ///<param name="endIndex">終了Index</param>
    ///
    public static int[]  ExtractGroupIndices(IGroup group, int startIndex, int endIndex){
      LeafEdge[] leafedgearray;
      int num;
      Dictionary<LeafEdge, int> dicLeafEdges = new Dictionary<LeafEdge, int>();
      List<int> result = new List<int>();
      for (int ix = 0; ix < group.EdgeCount; ++ix) {
        LeafEdge le = group.EdgeList.ElementAtOrDefault(ix) as LeafEdge;
        dicLeafEdges.Add(le, ix);
      }
      (leafedgearray, num) = TopTopTypePumpGroupLeafEdgeExtractor.FindTheStraightPath(group, startIndex, endIndex);
      for (int index = 0; index < num; ++index){
        var le = leafedgearray[index];
        result.Add(dicLeafEdges[le]);
      }
      return result.ToArray();
    }

  }
}
