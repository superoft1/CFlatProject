using System.Collections.Generic ;
using System.Linq ;

namespace CAD.Math
{
  /// <summary>
  /// 円上の範囲判定クラス
  /// </summary>
  public class CircleRange
  {
    private class Range
    {
      public double MinAngle { get ; }
      public double MaxAngle { get ; }

      public Range( double minAngle, double maxAngle )
      {
        MinAngle = minAngle ;
        MaxAngle = maxAngle;
      }

      public bool IsInside( double angle )
      {
        return MinAngle <= angle && angle <= MaxAngle ;
      }
    }
    
    private readonly List<Range> _ranges = new List<Range>();
    
    public CircleRange( double minAngle, double maxAngle )
    {
      var min = Normalize( minAngle ) ;
      var max = Normalize( maxAngle ) ;
      if ( min < max ) {
        _ranges.Add( new Range( min, max ) ) ;
      }
      else {
        _ranges.Add( new Range( 0, max ) ) ;
        _ranges.Add( new Range( min, 360 ) ) ;
      }
    }

    /// <summary>
    /// 範囲内か（境界含む）
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public bool IsInside( double angle )
    {
      return _ranges.Any( r => r.IsInside( Normalize(angle) ) ) ;
    }

    /// <summary>
    /// 0 <= angle < 360の範囲で返す
    /// </summary>
    /// <param name="plusDir"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public double Clamp( bool plusDir, double angle )
    {
      angle = Normalize( angle ) ;
      if ( IsInside( angle ) ) {
        return angle ;
      }

      if ( plusDir ) {
        if ( _ranges[0].MaxAngle < angle ) {
          return _ranges[0].MaxAngle ;
        }
      }
      else {
        if ( _ranges.Last().MinAngle > angle ) {
          return _ranges.Last().MinAngle ;
        }
      }

      return angle ;
    }
    
    public static double Normalize( double angle )
    {
      if ( angle < 0 ) {
        while ( angle < 0 ) {
          angle += 360 ;
        }

        return angle ;
      }

      while ( angle >= 360 ) {
        angle -= 360 ;
      }

      return angle ;
    }
  }
}
