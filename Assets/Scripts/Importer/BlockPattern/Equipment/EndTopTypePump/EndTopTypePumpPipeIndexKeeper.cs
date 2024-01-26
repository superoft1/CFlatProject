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
  class EndTopTypePumpPipeIndexKeeper
  {
    private IGroup _group;
    private LeafEdge[] _kept;


    public EndTopTypePumpPipeIndexKeeper(IGroup group, int[] indices){
      LeafEdge[] all = EndTopTypeMinimumFlowLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(group);
      int num = 0;
      _kept = new LeafEdge[indices.Length];
      foreach(var index in indices){
        _kept[num++] = all[index];
      }
      _group = group;
    }
    

    public int[] ReassignIndices(int[] indices){
      int[] result = new int[_kept.Length];
      Dictionary<LeafEdge, int> dic = EndTopTypePumpLengthUpdater.ExtractLeafEdgeDictionaryFromGroupHierarachy(_group);
      for (int index = 0; index < _kept.Length; ++index){
        result[index] = dic[_kept[index]];
      }
      return result;
    }
  }
}
