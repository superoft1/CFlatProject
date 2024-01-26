namespace Chiyoda.CableRouting.Math
{
  public struct Rectangle
  {
    // zは無視
    public Vector3d min ;
    public Vector3d max ;

    public Rectangle(Vector3d min, Vector3d max)
    {
      this.min = min ;
      this.max = max ;
    }

    public Vector3d GetClosedTo(Vector3d refPoint)
    {
      // TODO 正しい結果に修正
      var dist1 = ( refPoint - min ).sqrMagnitude ;
      var dist2 = ( refPoint - max ).sqrMagnitude ;
      if ( dist1 < dist2 ) return min ;
      
      return max ;
    }
    
  }
}