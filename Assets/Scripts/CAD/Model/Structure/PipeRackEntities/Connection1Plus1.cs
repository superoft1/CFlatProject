using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  [Entity( EntityType.Type.FrameConnections1Plus1 )]
  internal class Connection1Plus1 : AbstractConnection, IFrameConnection
  {
    private readonly Memento<ConnectionSingleFrame> _left ;
    private readonly Memento<ConnectionSingleFrame> _right ;
    private readonly Memento<bool> _useConnectionFloorBraces ;

    private ITransverseFrame _frame ;
    private int _floorNum ;
    
    private readonly Memento<double> _bottomH ;

    public Connection1Plus1( Document document ) : base( document )
    {
      _useConnectionFloorBraces = CreateMementoAndSetupValueEvents( false ) ;
      
      _left = CreateMementoAndSetupValueEvents<ConnectionSingleFrame>() ;
      _right = CreateMementoAndSetupValueEvents<ConnectionSingleFrame>() ;
    }

    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;

      _left.Value = Document.CreateEntity<ConnectionSingleFrame>() ;
      _right.Value = Document.CreateEntity<ConnectionSingleFrame>() ;
    }

    public double FloorHeight => _frame[_floorNum].Height ;

    [Chiyoda.UI.Property( UI.PropertyCategory.StructureId, "Name", ValueType = UI.ValueType.Label,
      Visibility = UI.PropertyVisibility.ReadOnly )]
    public string UnitName => Name ;

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Brace (Left)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 1 )]
    public bool BraceLeft
    {
      get => _left.Value.BraceLeft ;
      set => _left.Value.BraceLeft = value ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Brace (Right)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 1 )]
    public bool BraceRight
    {
      get => _right.Value.BraceRight ;
      set => _right.Value.BraceRight = value ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Floor Brace (Left)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 2 )]
    public bool HBraceLeft
    {
      get => _left.Value.HBrace ;
      set => _left.Value.HBrace = value ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Floor Brace (Right)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 2 )]
    public bool HBraceRight
    {
      get => _right.Value.HBrace ;
      set => _right.Value.HBrace = value ;
    }
    
    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "Floor Brace (Center)", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 3 )]
    public bool HBraceCenter
    {
      get => _useConnectionFloorBraces.Value ;
      set => _useConnectionFloorBraces.Value = value ;
    }


    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "HeightOffset (Left)",
      ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable, Order = 4 )]
    public double LeftSideBeamOffset
    {
      get => _left.Value.HeightOffset ;
      set => _left.Value.HeightOffset = value ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.OtherValues, "HeightOffset (Right)",
      ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable, Order = 5 )]
    public double RightSideBeamOffset
    {
      get => _right.Value.HeightOffset ;
      set => _right.Value.HeightOffset = value ;
    }

    public double Length
    {
      get => _left.Value.Length ;
      set => SetValue( a => a.Length = value ) ;
    }
    
    public double HeightOffset
    {
      get => _left.Value.HeightOffset ;
      set => SetValue( a => a.HeightOffset = value ) ;
    }

    public void SetReferenceFrame( ITransverseFrame frame, int floorNum )
    {
      _frame = frame ;
      _floorNum = floorNum ;
      if ( frame is IMemorableObject o ) { 
        SetupEvents( o ) ;
      }
      _right.Value.LocalCod = RightCodSys ;
    }

    public void SetBottomConnection( IFrameConnection c ) 
    {
      _left.Value.SetBottomConnection( c );
      _right.Value.SetBottomConnection( c );
    }

    public IFrameConnection CreateCopy()
    {
      var copy = Document.CreateEntity<Connection1Plus1>() ;
      copy._left.Value = (ConnectionSingleFrame)_left.Value.CreateCopy() ;
      copy._right.Value = (ConnectionSingleFrame) _right.Value.CreateCopy() ;
      copy._useConnectionFloorBraces.Value = _useConnectionFloorBraces.Value ;
      //upper.LocalCod = LocalCod.Translate( new Vector3d( 0.0, 0.0, _frame.Height ) ) ;
      return copy ;
    }

    public IStructuralMaterial BeamMaterial
    {
      get => _left.Value.BeamMaterial ;
      set => SetValue( a => a.BeamMaterial = value ) ;
    }

    public bool Brace
    {
      get => _left.Value.Brace ;
      set => SetValue( a => a.Brace = value ) ;
    }

    public bool HBrace => _left.Value.HBrace ;


    public override LocalCodSys3d LocalCod
    {
      get => _left.Value.LocalCod ;
      set
      {
        base.LocalCod = value ;
        _left.Value.LocalCod = value ;
        _right.Value.LocalCod = RightCodSys ;
      }
    }

    public void SetHeightOffset( double value )
    {
      _left.Value.SetHeightOffset( value );
      _right.Value.SetHeightOffset( value );
      TryFireValueChangedEvent();
    }
    
    public void SetHorizontalBrace( bool use )
    {
      _left.Value.SetHorizontalBrace( use );
      _right.Value.SetHorizontalBrace( use );
      HBraceCenter = use ;
    }

    protected override IEnumerable<IStructurePart> CreateStructureElements()
      => _left.Value.StructureElements
        .Concat( _right.Value.StructureElements.Select( AdjustRightPosition ) )
        .Concat( ConnectionFloorBraces ) ;

    public override Bounds? GetGlobalBounds()
      => new[] { _left.Value.GetGlobalBounds(), _right.Value.GetGlobalBounds() }.UnionBounds() ;

    public double Weight
    {
      get
      {
        var sidebeam = Length * MaterialDataService.SteelWeightPerLength( BeamMaterial ) ;
        return 2 * sidebeam ;
      }
    }

    private IEnumerable<IStructurePart> ConnectionFloorBraces
    {
      get
      {
        yield break;
/*        if ( ! _useConnectionFloorBraces.Value ) {
          yield break ;
        }
        
        var l = _connectionLength.Value ;
        var pos = new Vector3d( _left.Value.Width + 0.5 * l, 0.5 * Length, Height - 0.5*BeamThickness ) ;
        var baseDir = Quaternion.LookRotation( Vector3.right ) ;
        var rotAngle = (float) System.Math.Atan2( Length, l ) * Mathf.Rad2Deg ;
        var length = System.Math.Sqrt( l * l + Length * Length ) ;
        yield return new HSteel( BeamThickness, length ) 
        {
          LocalCod = new LocalCodSys3d( pos, Quaternion.AngleAxis( rotAngle, Vector3.forward ) * baseDir, false ),
        } ;
        yield return new HSteel( BeamThickness, length )
        {
          LocalCod = new LocalCodSys3d( pos, Quaternion.AngleAxis( -rotAngle, Vector3.forward ) * baseDir ),
        } ;*/
      }
    }    

    private IStructurePart AdjustRightPosition( IStructurePart beam )
    {
      beam.LocalCod =
        //beam.LocalCod.Translate( new Vector3( (float) ( _frame.Width + _frame.ConnectionLength ), 0f, 0f ) ) ;
        beam.LocalCod.Translate( new Vector3( (float) ( _frame.Width  ), 0f, 0f ) ) ;
      return beam ;
    }

    private void SetValue( System.Action<ConnectionSingleFrame> action )
    {
      action( _left.Value ) ;
      action( _right.Value ) ;
    }

    private LocalCodSys3d RightCodSys => LocalCod.Translate( RightOffset ) ;
    private Vector3d RightOffset //=> LocalCod.GlobalizeVector( new Vector3( (float) ( _frame.LeftWidth + _frame.ConnectionLength ), 0, 0 ) ) ;
      => LocalCod.GlobalizeVector( new Vector3( (float) ( _frame.Width  ), 0, 0 ) ) ;
  }
}