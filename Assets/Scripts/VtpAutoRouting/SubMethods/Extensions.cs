using System ;
using Routing;
using Routing.TempMath ;
using UnityEngine ;

namespace VtpAutoRouting.SubMethods
{
  internal static class Extensions
  {
    public static IPipeProperty GetMin( IPipeProperty diam1, IPipeProperty diam2 )
      => diam1.NPSmm < diam2.NPSmm ? diam1 : diam2 ;

    public static IPipeProperty GetMax( IPipeProperty diam1, IPipeProperty diam2 )
      => diam1.NPSmm > diam2.NPSmm ? diam1 : diam2 ;
    
    public static Vector3d To3d( this Vector3d_ v ) => new Vector3d( v.x, v.y, v.z);
    
    public static AxisDir GetAxis( this Vector3 direction )
    {
      if ( direction.x > 0.5f || direction.x < -0.5f ) {
        return AxisDir.X ;
      }
      if ( direction.y > 0.5f || direction.y < -0.5f ) {
        return AxisDir.Y ;
      }
      return AxisDir.Z ;
    }
    
    public static bool IsAxisAligned( this Vector3d v, float tolerance = (float)Chiyoda.CAD.Core.Tolerance.DistanceTolerance )
    {
      var isXZero = Math.Abs( v.x ) < tolerance ;
      var isYZero = Math.Abs( v.y ) < tolerance ;
      if ( ! isXZero && ! isYZero ) {
        return false ;
      }
      if ( isXZero && isYZero ) {
        return true ;
      }
      var isZZero = Math.Abs( v.z ) < tolerance ;
      return ( isXZero && isZZero ) || ( isYZero && isZZero ) ; // vector.zero も true にする
    }
  }
  
  
}