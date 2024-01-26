using System ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public class DocumentRegion : SquareRegion
  {
    private readonly Memento<double> _originX ;
    private readonly Memento<double> _originY ;
    private readonly Memento<double> _originZ ;
    private readonly Memento<double> _sizeX ;
    private readonly Memento<double> _sizeY ;
    private readonly Memento<double> _height ;
    private readonly Memento<double> _depth ;

    public override RegionType RegionType => RegionType.Document ;

    /// <summary>
    /// 敷地の東西サイズです。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "EW Width", Order = 10 )]
    public double EWWidth
    {
      get { return _sizeX.Value ; }
      set { _sizeX.Value = Math.Abs( value ) ; }
    }

    /// <summary>
    /// 敷地の南北サイズです。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "NS Width", Order = 20 )]
    public double NSWidth
    {
      get { return _sizeY.Value ; }
      set { _sizeY.Value = Math.Abs( value ) ; }
    }

    /// <summary>
    /// 敷地の東西原点位置です(敷地西端基準)。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "EW Origin", Order = 30 )]
    public double EWOrigin
    {
      get { return _originX.Value ; }
      set { _originX.Value = value ; }
    }

    /// <summary>
    /// 敷地の南北原点位置です(敷地北端基準)。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "NS Origin", Order = 40 )]
    public double NSOrigin
    {
      get { return _originY.Value ; }
      set { _originY.Value = value ; }
    }

    /// <summary>
    /// 敷地の基準高さです。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "Height", Order = 50 )]
    public double BaseHeight
    {
      get { return _originZ.Value ; }
      set { _originZ.Value = value ; }
    }

    /// <summary>
    /// 敷地の高さです。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "Height", Order = 50 )]
    public double Height
    {
      get { return _height.Value ; }
      set { _height.Value = Math.Max( 0, value ) ; }
    }

    /// <summary>
    /// 敷地の深さです。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "Depth", Order = 60 )]
    public double Depth
    {
      get { return _depth.Value ; }
      set { _depth.Value = Math.Max( 0, value ) ; }
    }

    public double GridInterval { get; set; } = 10.0d;

    public override double GetBaseHeight() => BaseHeight ;
    public override double GetHeightOverBase() => Height ;
    public override double GetDepthUnderBase() => Depth ;
    protected override double GetSizeX() => EWWidth ;
    protected override double GetSizeY() => NSWidth ;
    protected override double GetOriginX() => EWOrigin ;
    protected override double GetOriginY() => NSOrigin ;

    public DocumentRegion( Document document ) : base( document )
    {
      _originX = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _originY = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _originZ = CreateMementoAndSetupValueEvents( 0.0 ) ;

      _sizeX = CreateMementoAndSetupValueEvents( GetByUnit( 280 ) ) ;
      _sizeY = CreateMementoAndSetupValueEvents( GetByUnit( 180 ) ) ;

      _height = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _depth = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;
      
      var region = another as DocumentRegion;
      _originX.CopyFrom( region._originX.Value );
      _originY.CopyFrom( region._originY.Value );
      _originZ.CopyFrom( region._originZ.Value );
      _sizeX.CopyFrom( region._sizeX.Value );
      _sizeY.CopyFrom( region._sizeY.Value );
      _height.CopyFrom( region._height.Value );
      _depth.CopyFrom( region._depth.Value );
    }
  }
}