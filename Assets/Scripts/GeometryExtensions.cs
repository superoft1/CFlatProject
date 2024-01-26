using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda
{
  public static class GeometryExtensions
  {
    public static Bounds? UnionBounds( this IEnumerable<Bounds> bounds )
    {
      using ( var enu = bounds.GetEnumerator() ) {
        if ( ! enu.MoveNext() ) {
          // ひとつもない
          return null ;
        }

        var first = enu.Current ;
        Vector3 min = first.min ;
        Vector3 max = first.max ;
        while ( enu.MoveNext() ) {
          var item = enu.Current ;
          min = Vector3.Min( min, item.min ) ;
          max = Vector3.Max( max, item.max ) ;
        }

        var result = new Bounds() ;
        result.SetMinMax( min, max ) ;
        return result ;
      }
    }

    public static Bounds? UnionBounds( this IEnumerable<Bounds?> bounds )
    {
      return bounds.Where( b => b.HasValue ).Select( b => b.Value ).UnionBounds();
    }

    public static bool IsParallelTo( this in Vector3d vec1, in Vector3d vec2 )
    {
      // 微小量チェックなので、Crossのみ取ってsinとチェック
      var sqrSinThMag2 = Vector3d.Cross( vec1, vec2 ).sqrMagnitude ;
      var radTolerance = Tolerance.AngleTolerance.Deg2Rad() ;
      return ( sqrSinThMag2 <= radTolerance * radTolerance * ( vec1.sqrMagnitude * vec2.sqrMagnitude ) ) ;
    }

    public static bool IsSameDirectionTo( this in Vector3d vec1, in Vector3d vec2 )
    {
      if ( ! vec1.IsParallelTo( vec2 ) ) return false ;
      return ( 0 <= Vector3d.Dot( vec1, vec2 ) ) ;
    }

    public static bool IsOppositeDirectionTo( this in Vector3d vec1, in Vector3d vec2 )
    {
      if ( ! vec1.IsParallelTo( vec2 ) ) return false ;
      return ( Vector3d.Dot( vec1, vec2 ) <= 0 ) ;
    }

    public static bool IsPerpendicularTo( this in Vector3d vec1, in Vector3d vec2 )
    {
      // 微小量チェックなので、Dotのみ取ってcosとチェック
      var dot = Vector3d.Dot( vec1, vec2 ) ;
      var sqrCosThMag2 = dot * dot ;
      var radTolerance = Tolerance.AngleTolerance.Deg2Rad() ;
      return ( sqrCosThMag2 <= radTolerance * radTolerance * ( vec1.sqrMagnitude * vec2.sqrMagnitude ) ) ;
    }
    
    public static LocalCodSys3d MirrorXYBy( this LocalCodSys3d localCodSys, in Vector3d mirrorOrigin, in Vector3d normalDirection )
    {
      return new LocalCodSys3d(
        localCodSys.Origin.MirrorPointBy( mirrorOrigin,normalDirection ),
        localCodSys.DirectionX.MirrorVectorBy( normalDirection ),
        localCodSys.DirectionY.MirrorVectorBy( normalDirection ),
        localCodSys.DirectionZ.MirrorVectorBy( normalDirection ) ) ;
    }

    public static Vector3d MirrorPointBy( this in Vector3d point, in Vector3d mirrorOrigin, in Vector3d normalDirection )
    {
      return MirrorVectorBy( point - mirrorOrigin, normalDirection ) + mirrorOrigin ;
    }

    public static Vector3d MirrorVectorBy( this in Vector3d vector, in Vector3d normalDirection )
    {
      var dot = Vector3d.Dot( vector, normalDirection ) ;
      return vector - normalDirection * ( 2.0 * dot / normalDirection.sqrMagnitude ) ;
    }


    public static void SetLocalCodSys( this Transform transform, LocalCodSys3d localCod )
    {
      transform.localPosition = (Vector3) localCod.Origin ;
      transform.localRotation = localCod.Rotation ;
      transform.localScale = (Vector3) localCod.Scale ;
    }
  }
}