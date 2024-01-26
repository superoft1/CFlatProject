namespace Chiyoda.CableRouting.Math
{
  // 自動ルーティングをUnityから切り離す方針の検討
  // UnityDngine依存したくないので暫定てきにコピーしてきた
  public struct Vector3d
  {
    public const double kEpsilon = 1e-5 ;

    public double x ;
    public double y ;
    public double z ;
    
    public Vector3d normalized
    {
      get { return Vector3d.Normalize( this ) ; }
    }

    public double magnitude
    {
      get { return System.Math.Sqrt( this.x * this.x + this.y * this.y + this.z * this.z ) ; }
    }

    public double sqrMagnitude
    {
      get { return this.x * this.x + this.y * this.y + this.z * this.z ; }
    }

    public static Vector3d zero
    {
      get { return new Vector3d( 0d, 0d, 0d ) ; }
    }

    public static Vector3d one
    {
      get { return new Vector3d( 1d, 1d, 1d ) ; }
    }
    
    
    public Vector3d( double x, double y, double z )
    {
      this.x = x ;
      this.y = y ;
      this.z = z ;
    }
    
    public Vector3d( double x, double y )
    {
      this.x = x ;
      this.y = y ;
      this.z = 0d ;
    }
    public static Vector3d operator +( in Vector3d a, in Vector3d b )
    {
      return new Vector3d( a.x + b.x, a.y + b.y, a.z + b.z ) ;
    }

    public static Vector3d operator -( in Vector3d a, in Vector3d b )
    {
      return new Vector3d( a.x - b.x, a.y - b.y, a.z - b.z ) ;
    }

    public static Vector3d operator -( in Vector3d a )
    {
      return new Vector3d( -a.x, -a.y, -a.z ) ;
    }

    public static Vector3d operator *( in Vector3d a, double d )
    {
      return new Vector3d( a.x * d, a.y * d, a.z * d ) ;
    }

    public static Vector3d operator *( double d, in Vector3d a )
    {
      return new Vector3d( a.x * d, a.y * d, a.z * d ) ;
    }

    public static Vector3d operator /( in Vector3d a, double d )
    {
      return new Vector3d( a.x / d, a.y / d, a.z / d ) ;
    }

    public static bool operator ==( in Vector3d lhs, in Vector3d rhs )
    {
      return ( lhs.x == rhs.x ) && ( lhs.y == rhs.y ) && ( lhs.z == rhs.z ) ;
    }

    public static bool operator !=( in Vector3d lhs, in Vector3d rhs )
    {
      return ! ( lhs == rhs ) ;
    }
    public static Vector3d Cross( in Vector3d lhs, in Vector3d rhs )
    {
      return new Vector3d( lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x ) ;
    }
    
    
    public override bool Equals( object other )
    {
      if ( other is Vector3d vector3d ) {
        if ( this.x.Equals( vector3d.x ) && this.y.Equals( vector3d.y ) )
          return this.z.Equals( vector3d.z ) ;
        else
          return false ;
      }
      else {
        return false ;
      }
    }
    
    public static Vector3d Normalize( in Vector3d value )
    {
      double num = Vector3d.Magnitude( value ) ;
      if ( num >= kEpsilon )
        return value / num ;
      else
        return Vector3d.zero ;
    }

    public void Normalize()
    {
      double num = Vector3d.Magnitude( this ) ;
      if ( num >= kEpsilon )
        this = this / num ;
      else
        this = Vector3d.zero ;
    }

    public override string ToString()
    {
      return string.Format( "({0}, {1}, {2})", this.x, this.y, this.z ) ;
    }

    public static double Dot( in Vector3d lhs, in Vector3d rhs )
    {
      return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z ;
    }

    public static Vector3d Project( in Vector3d vector, in Vector3d onNormal )
    {
      double num = Vector3d.Dot( onNormal, onNormal ) ;
      if ( num >= kEpsilon )
        return Vector3d.zero ;
      else
        return onNormal * Vector3d.Dot( vector, onNormal ) / num ;
    }

    public static double Distance( in Vector3d a, in Vector3d b )
    {
      var x = a.x - b.x ;
      var y = a.y - b.y ;
      var z = a.z - b.z ;
      return System.Math.Sqrt( x * x + y * y + z * z ) ;
    }
    public static double Magnitude( in Vector3d a )
    {
      return System.Math.Sqrt( a.x * a.x + a.y * a.y + a.z * a.z ) ;
    }
    public static Vector3d Min( in Vector3d lhs, in Vector3d rhs )
    {
      return new Vector3d( System.Math.Min( lhs.x, rhs.x ), System.Math.Min( lhs.y, rhs.y ), System.Math.Min( lhs.z, rhs.z ) ) ;
    }

    public static Vector3d Max( in Vector3d lhs, in Vector3d rhs )
    {
      return new Vector3d( System.Math.Max( lhs.x, rhs.x ), System.Math.Max( lhs.y, rhs.y ), System.Math.Max( lhs.z, rhs.z ) ) ;
    }

  }
}