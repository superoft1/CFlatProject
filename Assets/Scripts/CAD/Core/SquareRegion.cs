using System ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public abstract class SquareRegion : Region
  {
    protected SquareRegion( Document document ) : base( document )
    {
    }

    public override Bounds? GetGlobalBounds()
    {
      return new Bounds( Center, Size ) ;
    }

    
    public override Vector3 Center
    {
      get
      {
        return new Vector3(
          (float)(GetSizeX() * 0.5 - GetOriginX()),
          (float)(GetSizeY() * 0.5 - GetOriginY()),
          (float)((GetHeightOverBase() - GetDepthUnderBase()) * 0.5 + GetBaseHeight())
        );
      }
    }

    public Vector3 Size
    {
      get { return new Vector3( (float) GetSizeX(), (float) GetSizeY(), (float) ( GetHeightOverBase() + GetDepthUnderBase() ) ) ; }
    }

    protected abstract double GetSizeX() ;
    protected abstract double GetSizeY() ;
    protected abstract double GetOriginX() ;
    protected abstract double GetOriginY() ;

    public override RegionHitInfo HitTest( in Vector3 rayPos, in Vector3 rayDir )
    {
      if ( Math.Abs( rayDir.z ) < Vector3.kEpsilon ) return null ;

      var len = ( (float)GetBaseHeight() - rayPos.z ) / rayDir.z ;
      // lenが負ならレイが進む方向に面はない。
      if ( len < 0 ) return null ;

      // posからdir方向にlenの長さだけ進んだ点
      var hitPos = rayPos + rayDir * len ;

      // hitPosがエリア内にあるかチェック
      var c = Center ;
      var s = Size ;
      if ( hitPos.x < c.x - s.x * 0.5f ) return null ;
      if ( c.x + s.x * 0.5f < hitPos.x ) return null ;
      if ( hitPos.y < c.y - s.y * 0.5f ) return null ;
      if ( c.y + s.y * 0.5f < hitPos.y ) return null ;

      return new RegionHitInfo( this, hitPos, len ) ;
    }

    public override bool CanIncludeRect( double hitPosX, double hitPosY, double requiredEastExtent, double requiredWestExtent, double requiredNorthExtent, double requiredSouthExtent )
    {
      Vector3d center = Center, size = Size ;

      if ( hitPosX - requiredWestExtent < center.x - size.x * 0.5 ) return false ;
      if ( center.x + size.x * 0.5 < hitPosX + requiredEastExtent ) return false ;
      if ( hitPosY - requiredSouthExtent < center.y - size.y * 0.5 ) return false ;
      if ( center.y + size.y * 0.5 < hitPosY + requiredNorthExtent ) return false ;

      return true ;
    }

    public override bool CanIncludeCircle( double hitPosX, double hitPosY, double requiredExtentRadius )
    {
      return CanIncludeRect( hitPosX, hitPosY, requiredExtentRadius, requiredExtentRadius, requiredExtentRadius, requiredExtentRadius ) ;
    }
  }
}