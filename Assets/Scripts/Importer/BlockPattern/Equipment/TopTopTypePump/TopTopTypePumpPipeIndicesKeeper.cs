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
  class TopTopTypePumpPipeIndicesKeeper
  {
    private IGroup _group;
    private LeafEdge [][]_kept;


    public TopTopTypePumpPipeIndicesKeeper(IGroup group, params int[][] indices){
      LeafEdge[] all = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(group);
      _kept = new LeafEdge[indices.Length][];
      for (int row = 0; row < indices.Length; ++row){
        _kept[row] = new LeafEdge[indices[row].Length];
        int num = 0;
        foreach(var index in indices[row]){
          _kept[row][num++] = all[index];
        }
      }
      _group = group;
    }
    /// <summary>
    /// グループ替え後のインデックスを返す。
    /// </summary>
    /// <remarks>コンストラクタで渡した配列の数だけの配列を返すので注意。一つだけでよいときは少しわかりにくい</remarks>
    /// <returns></returns>
    public int[][] ReassignIndices(){
      Dictionary<LeafEdge, int> dic = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeDictionaryFromGroupHierarachy(_group);
      int[][] result = new int[_kept.Length][];
      for (int row = 0; row < _kept.Length; ++row){
        result[row] = new int[_kept[row].Length];
        for (int index = 0; index < _kept[row].Length; ++index){
          result[row][index] = dic[_kept[row][index]];
        }
      }      
      return result;
    }
  }
}
