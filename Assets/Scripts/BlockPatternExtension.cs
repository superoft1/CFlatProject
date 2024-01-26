using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;

namespace Chiyoda
{
  public static class BlockPatternExtension
  {
    public static bool SetFlowType( this CompositeBlockPattern compositeBlock )
    {
      var joints = new List<CompositeEdge>() ;
      foreach ( var child in compositeBlock.Children ) {
        switch ( child ) {
          case BlockPattern block :
            block.SetFlowType() ;
            break ;
          case CompositeEdge joint :
            joints.Add( joint ) ;
            break ;
          default :
            throw new InvalidOperationException() ;
        }
      }

      // joint部分の対応
      foreach ( var joint in joints ) {
        SetFlowTypeOfJointEdge( joint ) ;
      }
      return false ;
    }

    private static HalfVertex FindEntryVertex( HalfVertex trackedVertex, CompositeEdge edgeGroup )
    {
      var entryVertices = new List<HalfVertex>() ;

      var trackedVertices = new Queue<HalfVertex>() ;
      trackedVertices.Enqueue( trackedVertex ) ;

      while ( trackedVertices.Any() ) {
        trackedVertex = trackedVertices.Dequeue() ;

        while ( trackedVertex != null ) {
          var partnerVertex = trackedVertex.Partner ;
          if ( partnerVertex == null ) {
            if ( trackedVertex.Flow == HalfVertex.FlowType.FromAnotherToThis ) {
              entryVertices.Add( trackedVertex ) ;
            }
            break ;
          }

          var nextLeafEdge = partnerVertex.LeafEdge ;
          if ( !edgeGroup.IsAncestorOf( nextLeafEdge ) ) {
            entryVertices.Add( trackedVertex ) ;
            break ;
          }

          var nextVertices = nextLeafEdge.Vertices.Where( v => v != partnerVertex ) ;
          trackedVertex = nextVertices.FirstOrDefault() ;
          foreach ( var nextVertex in nextVertices.Skip( 1 ) ) {
            trackedVertices.Enqueue( nextVertex ) ;
          }
        }
      }

      if ( entryVertices.Count > 1 ) {
        throw new InvalidOperationException( "SetFlowType (LeafEdge, int, HalfVertex.FlowType, Edge) is not supported for multiple inlet pattern." ) ;
      }
      return entryVertices.SingleOrDefault() ?? throw new InvalidOperationException() ;
    }

    private static bool SetFlowType( this BlockPattern block )
    {
      block.GetEveryVertex().Where( v => v.Partner != null )
                            .ForEach( v => v.Flow = HalfVertex.FlowType.Undefined ) ; // 一旦リセット
      foreach ( var equipment in block.Equipments ) {
        if ( ! ( equipment is HorizontalPump ) ) {
          throw new NotSupportedException() ;
        }
        foreach ( var nozzle in equipment.Nozzles ) {
          var cp = equipment.GetConnectPoint( nozzle.NozzleNumber ) ;
          var vtx = equipment.LeafEdge.GetVertex( cp.ConnectPointNumber ) ;
          vtx.Flow = nozzle.NozzleType == Nozzle.Type.Discharge ? HalfVertex.FlowType.FromThisToAnother : HalfVertex.FlowType.FromAnotherToThis ;
          if ( vtx.Partner != null ) {
            var entryVtx = vtx.Partner ;
            if ( nozzle.NozzleType == Nozzle.Type.Suction ) {
              entryVtx = FindEntryVertex( vtx, block ) ; // 上流からFlowTypeを設定するためInletを探索
            }
            SetFlowType( entryVtx.LeafEdge, entryVtx.ConnectPointIndex, HalfVertex.FlowType.FromAnotherToThis, block ) ;
          }
        }
      }

      return true ;
    }

    private static bool SetFlowTypeOfJointEdge( CompositeEdge joint )
    {
      if ( joint.Ancestor<BlockPattern>() != null ) {
        throw new InvalidOperationException() ;
      }
      if ( joint.Ancestor<CompositeBlockPattern>() == null ) {
        throw new InvalidOperationException() ;
      }

      joint.GetEveryVertex().Where( v => v.Partner != null )
                            .ForEach( v => v.Flow = HalfVertex.FlowType.Undefined ) ; // 一旦リセット

      foreach ( var vtx in joint.Vertices
        .Where( v => v.Partner != null )
        .Where( v => ! joint.IsAncestorOf( v.Partner.LeafEdge ) ) ) {
        if ( vtx.Partner.Flow == HalfVertex.FlowType.Undefined ) {
          throw new InvalidOperationException($"{vtx.Partner.LeafEdge.PipingPiece.Name} {vtx.Partner.LeafEdge.PipingPiece.ObjectName} flow type is undefined.") ;
        }

        SetFlowType( vtx.LeafEdge, vtx.ConnectPointIndex, vtx.Partner.Flow.Opposite(), joint ) ;
      }

      return false ;
    }

    private static void SetFlowType( LeafEdge le, int connectPointNumber, HalfVertex.FlowType flowType, Edge parentEdge )
    {
      if ( flowType == HalfVertex.FlowType.Undefined ) {
        throw new ArgumentException($"flowType of {flowType} is invalid.");
      }
      if ( le.Vertices.Any( v => v.Flow != HalfVertex.FlowType.Undefined &&
                                 v.ConnectPointIndex != connectPointNumber ) ) {
        // Joint部分でTeeの先に進みすぎることを防止
        var v = le.GetVertex( connectPointNumber ) ;
        v.Flow = flowType ;
      }
      else {
        foreach ( var v in le.Vertices ) {
          if ( v.ConnectPointIndex == connectPointNumber ) {
            v.Flow = flowType ;
            continue ;
          }
          v.Flow = flowType.Opposite() ;

          var v2 = v.Partner ;
          var nextLeafEdge = v2?.LeafEdge ;
          if ( nextLeafEdge == null ) {
            continue ;
          }
          if ( ! parentEdge.IsAncestorOf( nextLeafEdge ) ) {
            continue ;
          }
          SetFlowType( nextLeafEdge, v2.ConnectPointIndex, v.Flow.Opposite(), parentEdge ) ;
        }
      }
    }

    /// <summary>
    /// 存在する全てのvertexを取得（端点だけではなく内部も含む）
    /// </summary>
    /// <param name="compositeEdge"></param>
    public static IEnumerable<HalfVertex> GetEveryVertex( this CompositeEdge compositeEdge )
    {
      var check = new HashSet<HalfVertex>() ;
      foreach ( var leafEdge in compositeEdge.GetAllLeafEdges() ) {
        foreach ( var vertex in leafEdge.Vertices ) {
          if ( check.Add( vertex ) ) {
            yield return vertex ;
          }
        }
      }
    }
  }
}
