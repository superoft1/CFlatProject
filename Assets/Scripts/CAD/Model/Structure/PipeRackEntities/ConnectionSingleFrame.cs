using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  [Entity( EntityType.Type.FrameConnectionsSingle )]
  internal class ConnectionSingleFrame : AbstractConnection, IFrameConnection
  {
    private readonly Memento<double> _sideOffset ;
    private readonly Memento<bool> _hBrace ;
    private readonly Memento<bool> _leftBrace ;
    private readonly Memento<bool> _rightBrace ;
    private readonly Memento<IStructuralMaterial> _beamMaterial ;
    
    private ITransverseFrame _referenceFrame ;
    private int _floorNum ;
    private IFrameConnection _bottomConnection ;
    
    public ConnectionSingleFrame( Document document ) : base( document )
    {      
      _sideOffset = CreateMemento( 0.0 ) ;
      _beamMaterial = CreateMemento<IStructuralMaterial>() ;

      _hBrace = CreateMemento( false ) ;
      _leftBrace = CreateMemento( false ) ;
      _rightBrace = CreateMemento( false ) ;
    }

    public void SetReferenceFrame( ITransverseFrame frame, int floorNum ) 
    {
      _referenceFrame = frame ;
      _floorNum = floorNum ;
      if ( _beamMaterial.Value == null ) {
        _beamMaterial.Value = MaterialDataService.Steel( SteelShapeType.H ).Beam( frame.PositionOffset ) ;
      }
    }

    public void SetBottomConnection( IFrameConnection bottom )
    {
      _bottomConnection = bottom ;
    }
    
    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      //base.CopyFrom( another, storage ) ;

      if ( another is ConnectionSingleFrame src ) {
        Copy( src, this ) ;
      }
    }
    public IFrameConnection CreateCopy()
    {
      var copy = Document.CreateEntity<ConnectionSingleFrame>() ;
      Copy( this, copy ) ;
      return copy ;
    }

    private static void Copy( ConnectionSingleFrame src, ConnectionSingleFrame dst )
    {
      dst.HeightOffset = src.HeightOffset ;
      dst.BeamMaterial = src.BeamMaterial.CreateCopy() ;
      dst.BraceLeft = src.BraceLeft;
      dst.BraceRight = src.BraceRight ;
      dst.HBrace = src.HBrace ;
      dst.LocalCod = src.LocalCod ;
    }

    public double FloorHeight => _referenceFrame[_floorNum].Height ;
    
    [Chiyoda.UI.Property( UI.PropertyCategory.StructureId, "Name", ValueType = UI.ValueType.Label,
      Visibility = UI.PropertyVisibility.ReadOnly )]
    public string UnitName => Name ;

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Brace(left)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 1 )]
    public bool BraceLeft
    {
      get => _leftBrace.Value ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          _leftBrace.TryChangeValue( value, TryFireValueChangedEvent ) ;
        }
      }
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Brace(right)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 1 )]
    public bool BraceRight
    {
      get => _rightBrace.Value ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          _rightBrace.TryChangeValue( value, TryFireValueChangedEvent ) ;
        }
      }
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Floor Brace", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 2 )]
    public bool HBrace
    {
      get => _hBrace.Value ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          _hBrace.TryChangeValue( value, TryFireValueChangedEvent ) ;
        }
      }
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "HeightOffset", ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable, Order = 3 )]
    public double HeightOffset
    {
      get => _sideOffset.Value ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          SetHeightOffset( value );
        }
      }
    }

    public bool Brace
    {
      get => BraceLeft || BraceRight ;
      set 
      {
        if ( _leftBrace.Value == value && _rightBrace.Value == value ) {
          return ;
        }
        _leftBrace.Value = value ;
        _rightBrace.Value = value ;
        TryFireValueChangedEvent();
      }
    }
    
    public double Length
    {
      get => _referenceFrame.PositionOffset ;
      set => _referenceFrame.PositionOffset = value ;
    }
    
    public double Weight
    {
      get
      {
        var sidebeam = Length * MaterialDataService.SteelWeightPerLength( BeamMaterial ) ;
        var sideBraceCount = _leftBrace.Value ? 1 : 0 ;
        if ( _rightBrace.Value ) {
          sideBraceCount += 1 ;
        }
        var sideBraceW = ( sideBraceCount == 0 ) ? 0.0 : sideBraceCount * SideBraceWeight ;
        var floorBraceW = HBrace ? FloorBraceWeight : 0.0 ;
        return 2 * sidebeam + sideBraceW + floorBraceW ;
      }
    }

    private double SideBraceWeight
    {
      get
      {
        var h = SideBraceUpper - SideBraceLower ;
        var l = Math.Sqrt( 4.0 * h * h + Length * Length ) ;
        return l * BraceMaterialWeight ;
      }
    }

    private double FloorBraceWeight
    {
      get
      {
        var w = _referenceFrame.Width ;
        var l = Length ;
        return 2.0 * Math.Sqrt( w * w + l * l ) * BraceMaterialWeight ;
      }
    }
    
    private double BraceMaterialWeight => 0.3 * MaterialDataService.SteelWeightPerLength( BeamMaterial ) ;
    
    public void SetHeightOffset( double value ) => _sideOffset.TryChangeValue( value, TryFireValueChangedEvent );

    public void SetHorizontalBrace( bool value ) => _hBrace.TryChangeValue( value, TryFireValueChangedEvent ) ;
    
    public IStructuralMaterial BeamMaterial
    {
      get => _beamMaterial.Value ;
      set => _beamMaterial.TryChangeValue( value, TryFireValueChangedEvent );
    }
    
    public override Bounds? GetGlobalBounds()
    {
      var size = new Vector3( (float) _referenceFrame.Width, (float) Length, (float) _referenceFrame[ _floorNum].Height );

      var b = LocalCod.GlobalizeBounds( new Bounds( 0.5f * size, size ) ) ;
      var l = Parent as PlacementEntity ;
      return l?.GlobalCod.GlobalizeBounds( b ) ?? b ;
    }

    protected override IEnumerable<IStructurePart> CreateStructureElements()
      => SideBeams
      .Concat( CreateSideBraces( true ) )
      .Concat( CreateSideBraces( false ) )
      .Concat( CreateFloorBraces() ) ;
    
    private IEnumerable<IStructurePart> SideBeams
    {
      get
      {
        var h = FloorHeight - _sideOffset.Value ;
        yield return BeamUtility.GetSideBeam( BeamMaterial, Length, 0.0, h ) ;
        yield return BeamUtility.GetSideBeam( BeamMaterial, Length, _referenceFrame.Width, h ) ;
      }
    }

    private IEnumerable<IStructurePart> CreateFloorBraces()
    {
      if ( !HBrace ) {
        yield break ;
      }

      var l = Length ;
      var pos = new Vector3d(0.5 * _referenceFrame.Width, 0.5 * l, FloorHeight - 
                                                                   0.5* _referenceFrame[_floorNum].BeamMaterial.MainSize ) ;
      var baseDir = Quaternion.LookRotation( Vector3.right, Vector3.forward ) ;
      var rotAngle = (float) Math.Atan2( l, _referenceFrame.Width ) * Mathf.Rad2Deg ;
      var length = Math.Sqrt( _referenceFrame.Width * _referenceFrame.Width + l * l) ;
      var elm0 = MaterialDataService.GetElement( BeamMaterial, length ) ;
      elm0.LocalCod = new LocalCodSys3d( pos, Quaternion.AngleAxis( rotAngle, Vector3.forward ) * baseDir, false ) ;
      yield return elm0 ;

      var elm1 = MaterialDataService.GetElement( BeamMaterial, length ) ;
      elm1.LocalCod = new LocalCodSys3d( pos, Quaternion.AngleAxis( -rotAngle, Vector3.forward ) * baseDir, false ) ;
      yield return elm1 ;
    }

    private IEnumerable<IStructurePart> CreateSideBraces( bool left )
    {
      if ( ! ( left ? BraceLeft : BraceRight ) ) {
        yield break;
      }
      
      var x = ( left ? 0.0 : _referenceFrame.Width ) ;
      var upper = SideBraceUpper ;
      var bottom = SideBraceLower ;
      var z = 0.5 * ( upper + bottom ) ;
      var l = Length ;
      var pos0 = new Vector3d( x, 0.25 * l , z ) ;

      var h = upper - bottom ;
      var rotAngle = (float)Math.Atan2( h, 0.5*l ) * Mathf.Rad2Deg ;
      var length = Math.Sqrt( 0.25 * l * l + h * h ) ;
      var elm0 = MaterialDataService.GetElement( BeamMaterial, length ) ;
      var rot0 = Quaternion.AngleAxis( -rotAngle, Vector3.left ) * Quaternion.LookRotation( Vector3.up, Vector3.left ) ;
      elm0.LocalCod = new LocalCodSys3d( pos0, rot0, false ) ;
      yield return elm0 ;
      
      var pos1 = new Vector3d( x, 0.75 * l, z ) ;
      var elm1 = MaterialDataService.GetElement( BeamMaterial, length ) ;
      var rot1 = Quaternion.AngleAxis( rotAngle, Vector3.left ) * Quaternion.LookRotation( Vector3.up, Vector3.left ) ;
      elm1.LocalCod = new LocalCodSys3d( pos1, rot1, false ) ;
      yield return elm1 ;
    }

    private double SideBraceUpper => _referenceFrame[ _floorNum ].Height - _sideOffset.Value ;
    private double SideBraceLower => -_bottomConnection?.HeightOffset ?? 0.3 ;
  } ;

}