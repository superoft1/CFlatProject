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

namespace Importer.BlockPattern.Equipment.Filter
{
  delegate void PropertyInjectorHook(IPropertiedElement owner, IUserDefinedNamedProperty property, string targetPropertyName);
  class FilterSystemPipeUtility
  {
    int _groupIndex;
    IGroup _group;
    int[] _indices;
    LeafEdge[] _leafEdges;
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="bp"></param>
    /// <param name="groupIndex"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    
    public FilterSystemPipeUtility(Chiyoda.CAD.Topology.BlockPattern bp, int groupIndex, int startIndex, int endIndex){
      var group = bp.NonEquipmentEdges.ElementAtOrDefault(groupIndex) as IGroup;
      int num;
      _groupIndex = groupIndex;
      _group = group;
      Dictionary<LeafEdge,int> all = FilterGroupLeafEdgeExtractor.ExtractLeafEdgeDictionaryFromGroupHierarachy(group);
      (_leafEdges,num)=FilterGroupLeafEdgeExtractor.FindTheStraightPath(group, startIndex, endIndex);
      if (_leafEdges != null) {
        Array.Resize<LeafEdge>(ref _leafEdges, num);
        _indices = new int[num];
        for (int i = 0; i < num; ++i){
          if (all.ContainsKey(_leafEdges[i])) {
            _indices[i] = all[_leafEdges[i]];
          } else {
            UnityEngine.Debug.LogError("Assigning indices LeafEdge not found.");
          }
        }
      }
    }
    /// <summary>
    /// reassign indices
    /// </summary>
    public void ReassignIndices(){
      Dictionary<LeafEdge,int> all = FilterGroupLeafEdgeExtractor.ExtractLeafEdgeDictionaryFromGroupHierarachy(_group);
      for (int i = 0; i < _indices.Length; ++i){
        var le = _leafEdges[i];
        if (all.ContainsKey(_leafEdges[i])) {
          _indices[i] = all[_leafEdges[i]];
        } else {
          UnityEngine.Debug.LogError("Reassigning indices LeafEdge not found.");
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
        LeafEdge[] all = FilterGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(group);
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
  /// <summary>
  /// Property injector
  /// </summary>
  public class InjectorHookedRule : IUserDefinedRule
  {
    PropertyInjectorHook _hook;
    string _targetPropertyName;
    internal InjectorHookedRule( PropertyInjectorHook hook , string targetName ){
      _hook = hook;
      _targetPropertyName = String.Copy(targetName);
    }
    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ){
      _hook(owner, property,_targetPropertyName);
    }
  }
}
