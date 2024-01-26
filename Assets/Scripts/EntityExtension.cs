using System;
using System.Collections ;
using System.Collections.Generic;
using System.Linq;
using System.Net ;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda
{
  public static class EntityExtension
  {
    public static T Closest<T>( this IElement e ) where T : class, IElement
    {
      while ( null != e ) {
        switch ( e ) {
          case T t : return t ;
          case Document _ : return null ;
        }

        e = e.Parent;
      }
      return null;
    }

    public static IEnumerable<T> DescendantsAndSelf<T>( this IElement e )
    {
      if ( e is T t ) yield return t ;

      foreach ( var child in e.Children ) {
        foreach ( var elm in child.DescendantsAndSelf<T>() ) {
          yield return elm ;
        }
      }
    }

    public static IEnumerable<T> VisibleDescendantsAndSelf<T>( this IElement e )
    {
      if ( e is T t && e.IsVisible ) yield return t ;

      foreach ( var child in e.Children ) {
        foreach ( var elm in child.VisibleDescendantsAndSelf<T>() ) {
          yield return elm ;
        }
      }
    }

    public static T Ancestor<T>( this IElement e ) where T : class, IElement
    {
      return e?.Parent.Closest<T>();
    }

    public static bool IsAncestorOf( this Edge ancestor, Edge descendant )
    {
      IElement e = descendant;
      while ( null != e ) {
        if ( e == ancestor ) return true;

        if ( e is Document ) break;
        e = e.Parent;
      }
      return false;
    }

    public static IEnumerable<Edge> CollectAllEdges( this Edge edge )
    {
      yield return edge;
      if ( edge is CompositeEdge ce ) {
        foreach ( var child in ce.EdgeList ) {
          foreach ( var e in child.CollectAllEdges() ) yield return e;
        }
      }
    }

    public static IEnumerable<Edge> WithOlets( this IEnumerable<LeafEdge> edges )
    {
      var olets = new HashSet<LeafEdge>() ;
      foreach ( var edge in edges ) {
        switch ( edge.PipingPiece ) {
          case WeldOlet _:
            // Oletの場合、隣接するOletを含めて返す
            if ( olets.Add( edge ) ) {
              yield return edge ;
              foreach ( var oletEdge in GetNeighborOletsOfWeldOlet( edge ) ) {
                if ( olets.Add( oletEdge ) ) yield return oletEdge ;
              }
            }
            break;
          
          case Pipe _:
            // Olet以外の場合、隣接するOletを含めて返す
            yield return edge ;
            foreach ( var oletEdge in GetNeighborOletsOfPipe( edge ) ) {
              if ( olets.Add( oletEdge ) ) yield return oletEdge ;
            }
            break;
          
          default:
            yield return edge ;
            break ;
        }
      }
    }

    private static IEnumerable<LeafEdge> GetNeighborOletsOfWeldOlet( LeafEdge edge )
    {
      foreach ( var oletEdge in GetNeighborWeldOlets( edge.GetVertex( (int) WeldOlet.ConnectPointType.MainTerm1 ) ) ) {
        yield return oletEdge ;
      }

      foreach ( var oletEdge in GetNeighborWeldOlets( edge.GetVertex( (int) WeldOlet.ConnectPointType.MainTerm2 ) ) ) {
        yield return oletEdge ;
      }
    }

    private static IEnumerable<LeafEdge> GetNeighborOletsOfPipe( LeafEdge edge )
    {
      foreach ( var oletEdge in GetNeighborWeldOlets( edge.GetVertex( (int) Pipe.ConnectPointType.Term1 ) ) ) {
        yield return oletEdge ;
      }

      foreach ( var oletEdge in GetNeighborWeldOlets( edge.GetVertex( (int) Pipe.ConnectPointType.Term2 ) ) ) {
        yield return oletEdge ;
      }
    }

    private static IEnumerable<LeafEdge> GetNeighborWeldOlets( HalfVertex vertex )
    {
      while ( null != vertex ) {
        var v = vertex.Partner ;
        var le = v?.LeafEdge ;
        if ( ! ( le?.PipingPiece is WeldOlet ) ) yield break ;  // 終了

        switch ( (WeldOlet.ConnectPointType) v.ConnectPointIndex ) {
          case WeldOlet.ConnectPointType.MainTerm1 :
            yield return le ;
            // 末尾最適化: vertexに再代入してループ
            vertex = le.GetVertex( (int) WeldOlet.ConnectPointType.MainTerm2 ) ;
            continue ;

          case WeldOlet.ConnectPointType.MainTerm2 :
            yield return le ;
            // 末尾最適化: vertexに再代入してループ
            vertex = le.GetVertex( (int) WeldOlet.ConnectPointType.MainTerm1 ) ;
            continue ;

          default: yield break ;  // 終了
        }
      }
    }

    public static void MoveLocalPos( this IPlacement p, in Vector3d vec )
    {
      var cod = p.LocalCod;
      p.LocalCod = new LocalCodSys3d( cod.Origin + vec, cod );
    }

    public static IEnumerable<LeafEdge> GetAllLeafEdgesWithoutSubBlockPattern( this IGroup group )
    {
      foreach ( var edge in group.EdgeList ) {
        switch ( edge ) {
          case BlockPattern _: continue;
          case LeafEdge le: yield return le; break;
          case CompositeEdge ce:
            foreach ( var le in ce.GetAllLeafEdgesWithoutSubBlockPattern() ) yield return le;
            break;

          default: continue;
        }
      }
    }

    public static bool IsDestroyed( this UnityEngine.Object obj )
    {
      // nullではないのに比較演算子がnullを返した場合は削除済みオブジェクト
      return (false == (obj is null)) && (obj == null);
    }

    public static IEnumerable<LeafEdge> GetNextLeafEdges( this LeafEdge leafEdge )
    {
      return leafEdge.Vertices.Select( v => v.Partner?.LeafEdge ).Where( le => null != le ) ;
    }

    public static bool ChangeEdgeLengthFromEndVertices( this LeafEdge leafEdge )
    {
      var pipe = leafEdge.PipingPiece as Pipe ;
      if ( null == pipe ) return false ;

      var parentCod = leafEdge.ParentCod ;
      var v1 = leafEdge.GetVertex( (int) Pipe.ConnectPointType.Term1 ) ;
      var v2 = leafEdge.GetVertex( (int) Pipe.ConnectPointType.Term2 ) ;
      var p1 = parentCod.LocalizePoint( ( v1.Partner ?? v1 ).GlobalPoint ) ;
      var p2 = parentCod.LocalizePoint( ( v2.Partner ?? v2 ).GlobalPoint ) ;

      var dist = Vector3d.Distance( p1, p2 ) ;
      pipe.Length = dist ;
      if ( dist < Tolerance.DistanceTolerance ) {
        leafEdge.LocalCod = new LocalCodSys3d( ( p1 + p2 ) / 2, leafEdge.LocalCod ) ;
      }
      else {
        leafEdge.LocalCod = new LocalCodSys3d( ( p1 + p2 ) / 2, p2 - p1, Vector3d.zero, Vector3d.zero ) ;
      }

      return true ;
    }
    
    /// <summary>
    /// 指定したglobalPoint位置にgroupのローカル座標系を移動
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="globalPoint"></param>
    public static void TranslateLocalCoordinateTo( this Edge edge, Vector3d globalPoint )
    {
      var trans = edge.GlobalCod.Origin - globalPoint ;
      edge.Children.ForEach( e => ((Edge)e).Translate( trans ) ) ;
      edge.LocalCod = edge.LocalCod.Translate( -trans ) ;
    }
    
  }
}
