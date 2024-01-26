﻿using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  class TopTopTypePumpSystemLengthUpdater
  {
    int _groupIndex;
    int[] _indices;
    bool _activated;
    public TopTopTypePumpSystemLengthUpdater(int groupindex, int[] indices){
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
        var le = all[indices[index]];
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
            Debug.LogError("Unimplemented leaf edge");
          }
          sum += length;
        }
      }
      return sum;
    }

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
        LeafEdge[] all = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(group);
        double ord = GetLengthSum(all, _indices);
        double pipesum, denom;
        List<LeafEdge> edges;
        (edges, pipesum, denom) = CreatePipeList(all, _indices);

        if (denom != 0){
          double targetlength = property.Value - (ord - pipesum);
          SetSystemLength(edges, targetlength, denom);        
        }
      }
    }
    /*
    public void GetSystemMinimumLength(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      if (owner is Chiyoda.CAD.Topology.BlockPattern ownerbp) {
        var group = ownerbp.NonEquipmentEdges.ElementAtOrDefault(_groupIndex) as IGroup;
        LeafEdge[] all = EndTopTypeMinimumFlowLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(group);
        if (!_activated) {
          property.Value = GetLengthSum(all, _indices);
          return;
        }
        double dbl = FilterSystemPipeUtility.GetMinimumLengthSum(all, _indices);
        property.Value = dbl;
      }
    }
    */
  }
  /// <summary>
  /// プロパティにサブルーチン(delegate)をHookするためのRule
  /// </summary>
  public class GenericHookedRule : IUserDefinedRule
  {
    RuleHook _hook;
    IUserDefinedRule _completion;
    internal GenericHookedRule( RuleHook hook , IUserDefinedRule nextRule = null){
      _hook = hook;
      _completion = nextRule;
    }
    public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property ){
      _hook(owner, property);
      if (_completion!=null)
        _completion.Run(owner, property);
    }
  }
}
