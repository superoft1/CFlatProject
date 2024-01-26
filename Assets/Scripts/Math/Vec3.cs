using Tests.MathTest.Editor ;

namespace Chiyoda.Math
{
  public struct Vec3
  {
    public double X ;
    public double Y ;
    public double Z ;

    public Vec3( double x, double y, double z )
    {
      this.X = x  ;
      this.Y = y  ;
      this.Z = z  ;
    }

    public static Vec3 zero => new Vec3( 0.0, 0.0, 0.0 ) ;

    public override string ToString()
    {
      return $"Vec{ToElementString()}" ;
    }
    
    public string ToElementString()
    {
      return $"[{X},{Y},{Z}]" ;
    }
    
    public static bool operator ==( in Vec3 lhs, in Vec3 rhs )
    {
      return ( lhs.X == rhs.X ) && ( lhs.Y == rhs.Y ) && ( lhs.Z == rhs.Z ) ;
    }

    public static bool operator !=( in Vec3 lhs, in Vec3 rhs )
    {
      return ! ( lhs == rhs ) ;
    }
    
    public override int GetHashCode()
    {
      return this.X.GetHashCode() ^ this.Y.GetHashCode() << 2 ^ this.Z.GetHashCode() >> 2 ;
    }

    public override bool Equals( object other )
    {
      if ( other is Vec3 vec ) {
        if ( this.X.Equals( vec.X ) && this.Y.Equals( vec.Y ) &&  this.Z.Equals( vec.Z ))
          return true ;
        else
          return false ;
      }
      else {
        return false ;
      }
    }
    public static Vec3 operator +( in Vec3 a, in Vec3 b )
    {
      return new Vec3( a.X + b.X, a.Y + b.Y, a.Z + b.Z ) ;
    }

    public static Vec3 operator -( in Vec3 a, in Vec3 b )
    {
      return new Vec3( a.X - b.X, a.Y - b.Y, a.Z - b.Z ) ;
    }

    public static Vec3 operator -( in Vec3 a )
    {
      return new Vec3( -a.X, -a.Y, -a.Z ) ;
    }

    public static Vec3 operator *( in Vec3 a, double d )
    {
      return new Vec3( a.X * d, a.Y * d, a.Z * d ) ;
    }

    public static Vec3 operator *( double d, in Vec3 a )
    {
      return new Vec3( a.X * d, a.Y * d, a.Z * d ) ;
    }

    public static Vec3 operator /( in Vec3 a, double d )
    {
      return new Vec3( a.X / d, a.Y / d, a.Z / d ) ;
    }
    
    public double Dot( in Vec3 rhs )
    {
      return this.X * rhs.X + this.Y * rhs.Y + this.Z * rhs.Z ;
    }
    
    public Vec3 Cross( in Vec3 rhs )
    {
      return new Vec3( this.Y * rhs.Z - this.Z * rhs.Y, this.Z * rhs.X - this.X * rhs.Z, this.X * rhs.Y - this.Y * rhs.X ) ;
    }
    
    public double Length => System.Math.Sqrt( this.X * this.X + this.Y * this.Y + this.Z * this.Z ) ;

    public double DistanceTo( in Vec3 rhs ) => ( rhs - this ).Length ;
    
    public double Angle( in Vec3 to )
    {
      double dot = this.Dot( to ) ;
      double cross = this.Cross( to ).Length ;
      return System.Math.Atan2( cross, dot );
    }

    public bool IsParallelTo( in Vec3 to )
    {
      if ( this.Length < Tol.Double || to.Length < Tol.Double ) return false ;
      if ( this.Cross(to).Length > Tol.Double ) return false ;
      return true ;
    }

    public bool IsSameDirectionTo( in Vec3 to )
    {
      if ( ! this.IsParallelTo( to ) ) return false ;
      if ( this.Dot( to ) < Tol.Double ) return false ;
      return true ;
    }

    public Vec3 Normalized => this / this.Length ;
    
    public (Vec3 Point, double Parmeter) ProjectTo( in Vec3 onNormal )
    {
      var dot = onNormal.Dot( onNormal ) ;
      if ( dot < Tol.Double ) {
        return ( Vec3.zero, 0.0 ) ;
      }
      var parameter = this.Dot( onNormal ) / dot ;
      var projectedPoint = onNormal * parameter ;
      return ( projectedPoint, parameter ) ;
    }
  }
}