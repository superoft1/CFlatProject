using System ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public abstract class CircularRegion : Region
  {
    protected CircularRegion( Document document ) : base( document )
    {
    }
    
    public abstract override Vector3 Center { get ; }

    public abstract float InnerRadius { get ; }
    public abstract float OuterRadius { get ; }
    public abstract float BaseZ { get ; }
    
    public override RegionHitInfo HitTest( in Vector3 rayPos, in Vector3 rayDir )
    {
      if ( Math.Abs( rayDir.z ) < Vector3.kEpsilon ) return null ;

      var len = ( BaseZ - rayPos.z ) / rayDir.z ;
      // lenが負ならレイが進む方向に面はない。
      if ( len < 0 ) return null ;
        
      // posからdir方向にlenの長さだけ進んだ点
      var hitPos = rayPos + rayDir * len;

      // hitPosがエリア内にあるかチェック
      var dir = hitPos - Center ;
      var r2 = dir.x * dir.x + dir.y * dir.y ;

      var rad1 = InnerRadius ;
      if ( rad1 * rad1 < r2 ) return null ;

      var rad2 = OuterRadius ;
      if ( r2 < rad2 * rad2 ) return null ;

      return new RegionHitInfo( this, hitPos, len ) ;
    }
    
    public override bool CanIncludeRect( double hitPosX, double hitPosY, double requiredEastExtent, double requiredWestExtent, double requiredNorthExtent, double requiredSouthExtent )
    {
      var center = Center ;
      double xMin = hitPosX - requiredWestExtent - center.x, xMax = hitPosX + requiredEastExtent - center.x ;
      double yMin = hitPosY - requiredSouthExtent - center.y, yMax = hitPosX + requiredNorthExtent - center.y ;
      
      // 外側チェック
      double rOuter2 = OuterRadius * OuterRadius, xMin2 = xMin * xMin, yMin2 = yMin * yMin ;
      if ( rOuter2 < xMin2 + yMin2 ) return false ;
      double xMax2 = xMax * xMax ;
      if ( rOuter2 < xMax2 + yMin2 ) return false ;
      double yMax2 = yMax * yMax ;
      if ( rOuter2 < xMin2 + yMax2 ) return false ;
      if ( rOuter2 < xMax2 + yMax2 ) return false ;

      double rInner = InnerRadius ;
      if ( Tolerance.DistanceTolerance < rInner ) {
        // 内側チェック
        double rInner2 = rInner * rInner ;
        // 頂点チェック
        if ( xMin2 + yMin2 < rInner2 ) return false ;
        if ( xMax2 + yMin2 < rInner2 ) return false ;
        if ( xMin2 + yMax2 < rInner2 ) return false ;
        if ( xMax2 + yMax2 < rInner2 ) return false ;

        // 辺チェック
        if ( yMin < 0 && 0 < yMax ) {
          if ( -rInner < xMin && xMin < rInner ) return false ;
          if ( -rInner < xMax && xMax < rInner ) return false ;
        }
        if ( xMin < 0 && 0 < xMax ) {
          if ( -rInner < yMin && yMin < rInner ) return false ;
          if ( -rInner < yMax && yMax < rInner ) return false ;
        }
      }

      return true ;
    }

    public override bool CanIncludeCircle( double hitPosX, double hitPosY, double requiredExtentRadius )
    {
      // 外側チェック
      var center = Center ;
      double x = hitPosX - center.x, y = hitPosY - center.y, r2 = ( x * x + y * y ), r = Math.Sqrt( r2 ), oRadius = OuterRadius ;
      if ( oRadius < requiredExtentRadius + r ) return false ;
      
      double rInner = InnerRadius ;
      if ( Tolerance.DistanceTolerance < rInner ) {
        // 内側チェック
        if ( r - requiredExtentRadius < rInner ) return false ;
      }

      return true ;
    }
  }
}