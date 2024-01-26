using System ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public class RegionHitInfo
  {
    public RegionHitInfo( IRegion region, in Vector3 hitPos, float hitLength )
    {
      Region = region ;
      HitPos = hitPos ;
      HitLength = hitLength ;
    }
    
    public IRegion Region { get ; }
    public Vector3 HitPos { get ; }
    public float HitLength { get ; }
  }

  [Flags]
  public enum RegionType
  {
    None     = 0x0000,
    Document = 0x0001,
    Pit      = 0x0002,
    // TODO: 追加
  }

  public interface IRegion
  {
    RegionType RegionType { get ; }

    RegionHitInfo HitTest( in Vector3 rayPos, in Vector3 rayDir ) ;
    bool CanIncludeZ( double hitPosZ, double requiredHeight, double requiredDepth ) ;
    bool CanIncludeRect( double hitPosX, double hitPosY, double requiredEastExtent, double requiredWestExtent, double requiredNorthExtent, double requiredSouthExtent ) ;
    bool CanIncludeCircle( double hitPosX, double hitPosY, double requiredExtentRadius ) ;
  }
}