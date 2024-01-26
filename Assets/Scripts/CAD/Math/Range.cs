namespace CAD.Math
{
  public class RangeD
  {
    private double? _min ;

    public RangeD( double v )
    {
      _min = v ;
      Max = v ;
    }

    public RangeD()
    {
      _min = null ;
      Max = double.MinValue ;
    }

    public bool HasValue() => _min != null ;

    public double Min => _min ?? double.MinValue ;

    public double Max { get ; private set ; }

    public void Add( double v )
    {
      if ( _min == null || v < _min.Value ) {
        _min = v ;
      }
      if ( Max < v ) {
        Max = v ;
      }
    }

    public double Interval => HasValue() ? Max - _min.Value : 0.0 ;
    public float IntervalF => (float) Interval ;
  }
}