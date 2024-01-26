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

namespace Importer.BlockPattern.Equipment.Filter
{
  class FilterGroupLeafEdgeExtractor
  {
    ///
    /// <summary>指定されたGroup 内のLeafEdge を一次元配列として抽出する。</summary>
    ///
    public static LeafEdge [] ExtractLeafEdgeArrayFromGroupHierarachy(IGroup group)
    {
      return ExtractLeafEdgeListFromGroupHierarachy(group)?.ToArray();
    }
    ///<summary>
    /// 指定されたGroupHierarchy 内のLeafEdge をインデックス付き辞書として抽出する
    ///</summary>
    ///<remarks>group は group を内包することができるため、それらを全て巡回して一次元のデータとする。深さ優先で再インデックスする</remarks>
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
    ///<summary>
    /// 指定されたグループ内のLeafEdge をリストとして抽出する
    ///</summary>
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

    ///
    /// <summary>2つのEdge と group を指定すると、その２点をつなぐ（支流に入り込まない）経路を探索する。</summary>
    /// <remarks>最短経路探索ではないので、複数の経路がある場合は、どちらになるかは不定、group の外側へは出ない</remarks>
    /// <param name="group">探索する group </param>
    /// <param name="startIndex">探索開始Index</param>
    /// <param name="endIndex">探索終了Index</param>
    /// 
    public static (LeafEdge[], int) FindTheStraightPath(IGroup group, int startIndex, int endIndex) {
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

  }
}
