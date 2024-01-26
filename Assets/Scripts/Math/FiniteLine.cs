namespace Chiyoda.Math
{
  public struct FiniteLine
  {
    public Vec3 From ;
    public Vec3 To ;

    public FiniteLine(Vec3 from, Vec3 to)
    {
      this.From = from ;
      this.To = to ;
    }
    
    public override string ToString()
    {
      return $"FiniteLine[From{From.ToElementString()},To{To.ToElementString()}]" ;
    }
    
    public static bool operator ==( in FiniteLine lhs, in FiniteLine rhs )
    {
      return ( lhs.From == rhs.From ) && ( lhs.To == rhs.To );
    }

    public static bool operator !=( in FiniteLine lhs, in FiniteLine rhs )
    {
      return ! ( lhs == rhs ) ;
    }
    public override int GetHashCode()
    {
      return this.From.GetHashCode() ^ this.To.GetHashCode() ;
    }

    public override bool Equals( object other )
    {
      if ( other is FiniteLine line ) {
        if ( this.From.Equals( line.From ) && this.To.Equals( line.To ))
          return true ;
        else
          return false ;
      }
      else {
        return false ;
      }
    }

    public Vec3 GetPointAt(double parameter)
    {
      if (parameter < 0.0) return this.From ;
      if (parameter > 1.0) return this.To ;
      return this.From * ( 1.0 - parameter ) + this.To * parameter ;
    }

    public double Length => this.From.DistanceTo( this.To ) ;

    public (double Distance, Vec3 PointOnLine, double Parameter) DistanceTo( Vec3 point )
    {
      return ( 0.0, Vec3.zero, 0.0 ) ;
    }
  }
}