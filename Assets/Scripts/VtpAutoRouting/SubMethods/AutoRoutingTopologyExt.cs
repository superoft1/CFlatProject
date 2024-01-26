using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace VtpAutoRouting.SubMethods
{

  public static class AutoRoutingTopologyExt
  {
    public static bool Connect(
      IList<LeafEdge> edges, IEnumerable<HalfVertex> vertices, float tolerance = 1.0f )
    {
      var terms = edges.SelectMany( e => e.Vertices )
        .Where( v => v.Partner == null ).ToArray() ;
      if ( terms.Length == 0 ) {
        return false ;
      }

      bool allOK = true ;
      foreach ( var v in vertices ) {
        var nearest = terms
          .OrderBy( t => ( v.GlobalPoint - t.GlobalPoint ).sqrMagnitude )
          .First() ;
        if ( nearest.Partner != null ) {
          allOK = false ;
          continue ;
        }

        if ( tolerance > 0.0f && ( nearest.GlobalPoint - v.GlobalPoint ).magnitude > tolerance ) {
          allOK = false ;
          continue ;
        }

        if ( v.Partner != null ) {
          allOK = false ;
          continue ;
        }

        nearest.Partner = v ;
      }

      return allOK ;
    }

    public static bool Connect( this Edge node, Edge next )
    {
      var p0 = node.GlobalCod.Origin ;
      var p1 = next.GlobalCod.Origin ;
      var dir = ( p1 - p0 ).normalized ;
      var vtx0 = node.Vertices
        .OrderByDescending( v => Vector3d.Dot( ( v.GlobalPoint - p0 ).normalized, dir ) )
        .First() ;
      if ( vtx0 == null || vtx0.Partner != null ) {
        return false ;
      }

      var vtx1 = next.Vertices
        .OrderByDescending( v => Vector3d.Dot( ( p1 - v.GlobalPoint ).normalized, dir ) )
        .First() ;
      if ( vtx1 == null || vtx1.Partner != null ) {
        return false ;
      }

      vtx0.Partner = vtx1 ;
      return true ;
    }

    public static bool Connect( this HalfVertex vtx, LeafEdge e )
    {
      var p = vtx.GlobalPoint ;
      var nearest = e.Vertices.Where( v => v.Partner == null )
        .OrderBy( v => ( v.GlobalPoint - p ).magnitude )
        .FirstOrDefault() ;
      if ( nearest == null ) {
        return false ;
      }

      vtx.Partner = nearest ;
      return true ;
    }
  }
}