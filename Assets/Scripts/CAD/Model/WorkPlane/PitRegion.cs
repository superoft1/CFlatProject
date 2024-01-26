using System ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model
{
  public class PitRegion : SquareRegion
  {
    private readonly Memento<double> _baseDepth ;
    private readonly Memento<double> _heightOverBase ;
    private readonly Memento<double> _depthUnderBase ;
    private readonly Memento<Vector2> _center ;
    private readonly Memento<Vector2> _size ;
    
    public PitRegion( Document document ) : base( document )
    {
      _baseDepth = CreateMementoAndSetupValueEvents( 0d ) ;
      _heightOverBase = CreateMementoAndSetupValueEvents( 0d ) ;
      _depthUnderBase = CreateMementoAndSetupValueEvents( 0d ) ;
      _center = CreateMementoAndSetupValueEvents( Vector2.zero ) ;
      _size = CreateMementoAndSetupValueEvents( Vector2.zero ) ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "PitDepth", Order = 50 )]
    public double PitDepth
    {
      get { return _baseDepth.Value ; }
      set { _baseDepth.Value =  Math.Max( 0, value ) ; }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Height", Order = 50 )]
    public double HeightFromPitBase
    {
      get { return _heightOverBase.Value ; }
      set { _heightOverBase.Value = Math.Max( 0, value ) ; }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "ExtraDepth", Order = 60 )]
    public double ExtraDepth
    {
      get { return _depthUnderBase.Value ; }
      set { _depthUnderBase.Value = Math.Max( 0, value ) ; }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Center", Order = 60 )]
    public Vector2 CenterXY
    {
      get { return _center.Value ; }
      set { _center.Value = value ; }
    }
    [UI.Property( UI.PropertyCategory.Dimension, "Size", Order = 60 )]
    public Vector2 SizeXY
    {
      get { return _size.Value ; }
      set { _size.Value = value ; }
    }

    public override double GetHeightOverBase() => HeightFromPitBase ;
    public override double GetDepthUnderBase() => ExtraDepth ;
    public override double GetBaseHeight() => Document.Region.GetBaseHeight() - PitDepth ;
    protected override double GetSizeX() => SizeXY.x ;
    protected override double GetSizeY() => SizeXY.y ;
    protected override double GetOriginX() => CenterXY.x ;
    protected override double GetOriginY() => CenterXY.y ;

    public override RegionType RegionType => RegionType.Pit ;
  }
}