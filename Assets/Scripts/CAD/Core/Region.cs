using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Model ;
using UnityEngine;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// 領域クラスです。
  /// </summary>
  public abstract class Region : Entity, IRegion
  {
    public abstract double GetHeightOverBase() ;
    public abstract double GetDepthUnderBase() ;
    public abstract double GetBaseHeight() ;

    protected Region( Document document ) : base( document )
    {
    }

    public abstract RegionType RegionType { get ; }

    public abstract override Bounds? GetGlobalBounds() ;
    
    public abstract Vector3 Center { get ; }

    public abstract RegionHitInfo HitTest( in Vector3 rayPos, in Vector3 rayDir ) ;

    protected static double GetByUnit( double units )
    {
      switch ( ApplicationConfig.Current.PositionUnit ) {
        case LengthUnitType.YardPond: return units.Feet();
        default: return units.Meters();
      }
    }

    public bool CanIncludeZ( double hitPosZ, double requiredHeight, double requiredDepth )
    {
      return ( requiredHeight <= GetHeightOverBase() ) && ( requiredDepth <= GetDepthUnderBase() ) ;
    }

    public abstract bool CanIncludeRect( double hitPosX, double hitPosY, double requiredEastExtent, double requiredWestExtent, double requiredNorthExtent, double requiredSouthExtent ) ;
    public abstract bool CanIncludeCircle( double hitPosX, double hitPosY, double requiredExtentRadius ) ;
  }
}
