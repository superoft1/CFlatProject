//#define DEBUG_DUMP
//#define USE_PREFERRED_LENGTH
using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Manager ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.DB ;
using IDF ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  /// <summary>
  /// 先頭と末尾の二つのPipeを指定すると支流を含まないパイプをまとめた経路を用意して、
  /// 間のPipe以外の長さも含めたトータルな長さを指定できるようにする
  /// </summary>
  class EndTopTypePumpSystemLengthUpdater
  {
    int _groupIndex;
    int[] _indices;
    bool _activated;
    public EndTopTypePumpSystemLengthUpdater(int groupindex, int[] indices){
      _groupIndex = groupindex;
      _indices = indices;
      _activated = false;
    }

    public void ReplaceIndices(int[] indices){
      _indices = indices;
    }

    public void Activate(bool active){
      _activated = active;
    }

    private static double GetLengthSum(LeafEdge[] all, int [] indices){
      double sum = 0, length;
      for (int index = 0; index < indices.Length; ++index){
        var le = all[indices[index]] as LeafEdge;
        if (le != null){
          length = 0;
          if (le.PipingPiece is Pipe pipe) {
            length = pipe.Length;
          } else if (le.PipingPiece is WeldNeckFlange flange) {
            length = flange.Length;
          } else if (le.PipingPiece is GateValve valve) {
            length = valve.Length;
          } else if (le.PipingPiece is CheckValve cv) {
            length = cv.Length;
          } else if (le.PipingPiece is WeldOlet) {
            length = 0;
          } else if (le.PipingPiece is StubInReinforcingWeld) {
            length = 0;
          } else if (le.VertexCount < 2) {
            length = 0;
          } else if (le.VertexCount == 2) {
            Vector3d v0 = le.GetVertex(0).GlobalPoint;
            Vector3d v1 = le.GetVertex(1).GlobalPoint;
            length = (v1 - v0).magnitude;
          } else {
            UnityEngine.Debug.LogError("Unimplemented leaf edge");
          }
          sum += length;
        }
      }
      return sum;
    }

#if USE_PREFERRED_LENGTH
    /// <summary>
    /// パイプのList とPreferredLength の 合計長を返すメソッド
    /// </summary>
    /// <param name="group"></param>
    /// <returns>Pipe list, sum of length, sum of preferred length</returns>
    private static (List<LeafEdge>,double,double) CreatePipeList(LeafEdge[] all, int[] indices){
      List<LeafEdge> resultList = new List<LeafEdge>();
      double resultLength = 0;
      double resultPrefLength = 0;
      for (int index = 0; index < indices.Length; ++index) {
        var le = all[indices[index]];
        if (le.PipingPiece is Pipe pipe){
          resultList.Add(le);
          resultLength += pipe.Length;
          resultPrefLength += pipe.PreferredLength;
        }
      }
      return (resultList, resultLength,resultPrefLength);
    }
    private static void SetSystemLength(List<LeafEdge>pipes, double target, double ratioDenom){
      foreach (var le in pipes){
        if (le.PipingPiece is Pipe pipe){
          pipe.Length = target * pipe.PreferredLength / ratioDenom;
        }
      }
    }
#else
    /// <summary>
    /// パイプのList とPreferredLength の 合計長を返すメソッド
    /// </summary>
    /// <param name="group"></param>
    /// <returns>Pipe list, sum of length, sum of preferred length</returns>
    private static (List<LeafEdge>,double,double) CreatePipeList(LeafEdge[] all, int[] indices){
      List<LeafEdge> resultList = new List<LeafEdge>();
      double resultLength = 0;
      double resultSumMinimumLength = 0;
      for (int index = 0; index < indices.Length; ++index) {
        var le = all[indices[index]];
        if (le.PipingPiece is Pipe pipe){
          resultList.Add(le);
          resultLength += pipe.Length;
          resultSumMinimumLength += pipe.GetMinimumLength();
        }
      }
      return (resultList, resultLength,resultSumMinimumLength);
    }
    private static void SetSystemLength(List<LeafEdge>pipes, double target, double ratioDenom){
      foreach (var le in pipes){
        if (le.PipingPiece is Pipe pipe){
          double result = target * pipe.GetMinimumLength() / ratioDenom; //  無難に、MinimumLength のみに比例させる
          pipe.Length = result;
#if DEBUG_DUMP
          UnityEngine.Debug.Log($"{result}/{pipe.GetMinimumLength()}");
#endif
        }
      }
    }
#endif

    
    /// <summary>
    /// ルールとして登録するための Hook
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="property">システム長を示すProperty </param>
    public void UpdateSystemLength(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (!_activated)
        return;
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp){
        var group = ownerbp.NonEquipmentEdges.ElementAtOrDefault(_groupIndex) as IGroup;
        LeafEdge[] all = EndTopTypeMinimumFlowLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(group);
        //UnityEngine.Debug.Log($"sum of minimumlength{ GetMinimumLengthSum(all, _indices)}");

        double ord = GetLengthSum(all, _indices);
        double pipesum, denom;
        List<LeafEdge> edges;
        (edges, pipesum, denom) = CreatePipeList(all, _indices);

        if (denom != 0){
          double targetlength = property.Value - (ord - pipesum); //  パイプ以外の長さを引いてからパイプ長を設定する
          SetSystemLength(edges, targetlength, denom);        
        }
      }
    }
    /// <summary>
    /// Custom rule to get System Minimum Lengths
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="property"></param>
    /// <param name="targetPropName">target property name to inject value</param>
    public void CheckSystemMinimumLength(IPropertiedElement owner, IUserDefinedNamedProperty property, string targetPropertyName){
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp) {
        var group = ownerbp.NonEquipmentEdges.ElementAtOrDefault(_groupIndex) as IGroup;
        LeafEdge[] all = EndTopTypeMinimumFlowLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(group);
        double result = GetMinimumLengthSum(all, _indices);
        INamedProperty prop = ownerbp.GetProperty(targetPropertyName);
        if (prop != null){
          prop.Value = result;
          //UnityEngine.Debug.Log($"pipe length {result} stored to {targetPropertyName}");
        }
      }
    }
    /// <summary>
    /// calculate the sum of minimum lengths of LeafEdges in array and indices
    /// </summary>
    /// <param name="all">Array of LeafEdges</param>
    /// <param name="indices">indices for select LeafEdges</param>
    /// <returns></returns>
    public static double GetMinimumLengthSum(LeafEdge[] all, int [] indices){
      double sum = 0, length;
      for (int index = 0; index < indices.Length; ++index){
        var le = all[indices[index]] as LeafEdge;
        if (le != null){
          length = 0;
          if (le.PipingPiece is Pipe pipe) {
            length = (pipe.GetMinimumLength() > 0)?pipe.GetMinimumLength():pipe.Length;
          } else if (le.PipingPiece is WeldNeckFlange flange) {
            length = flange.Length;
          } else if (le.PipingPiece is GateValve valve) {
            length = valve.Length;
          } else if (le.PipingPiece is CheckValve cv) {
            length = cv.Length;
          } else if (le.PipingPiece is WeldOlet) {
            length = 0;
          } else if (le.PipingPiece is StubInReinforcingWeld) {
            length = 0;
          } else if (le.VertexCount < 2) {
            length = 0;
          } else if (le.VertexCount == 2) {
            Vector3d v0 = le.GetVertex(0).GlobalPoint;
            Vector3d v1 = le.GetVertex(1).GlobalPoint;
            length = (v1 - v0).magnitude;
          } else {
            UnityEngine.Debug.LogError($"Unimplemented {le.VertexCount} way leaf edge was found.");
          }
          sum += length;
        }
      }
      return sum;
    }

  }
}
