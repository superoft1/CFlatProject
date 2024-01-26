using Chiyoda.CableRouting.Math ;

namespace Chiyoda.CableRouting
{
  public class BranchPath : ICablePath
  {
    public Rectangle Rect { get ; }

    public BranchPath(Vector3d pnt1, Vector3d pnt2)
    {
      // TODO 正しい大きさの領域を作成する
      var min = Vector3d.Min( pnt1, pnt2 ) ;
      var max = Vector3d.Max( pnt1, pnt2 ) ;
      this.Rect = new Rectangle(min, max) ;
    }
  }
}