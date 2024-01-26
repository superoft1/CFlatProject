using Chiyoda.CableRouting.Math;

namespace Chiyoda.CableRouting
{
  public interface ICable
  {
    Vector3d From { get ; }
    Vector3d To { get ; }
    void Add( Vector3d point ) ;
    void ClearPoints() ;
  }
}