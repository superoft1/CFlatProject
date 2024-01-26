using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal partial class RackBase<T> : EmbodyStructureWithUIProperties, IStructure, IFreeDraggablePlacement, Chiyoda.CableRouting.ICablePathAvailable
    where T : Entity, IFrameConnections
  {
    protected const int IntervalPropOrder = 10;
    protected const int WidthPropOrder = 20 ;
    protected const int FloorCountPropOrder = 30 ;
    protected const int BeamIntervalPropOrder = 40 ;
    protected const int TailPropOrder = 50 ;    
    
    private RackFrames _frames ;
    private readonly MementoList<T> _connections ;
    private readonly Memento<Foundation> _foundation ;

    private IList<PipeRackLayerControl> _layerControls ;
    private readonly Memento<PhysicalProperty> _physicalProp ;

    protected RackBase( Document document ) : base( document )
    {
      _connections = CreateMementoListAndSetupChildrenEvents<T>() ;
      _layerControls = new List<PipeRackLayerControl>() ;
      _foundation = CreateMementoAndSetupChildrenEvents<Foundation>() ;
      _physicalProp = CreateMementoAndSetupValueEvents( new PhysicalProperty( 0.0, 0.0 ) ) ;
    }

    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;

      var frames = new RackFrames( Document.History, TransverseFrameFactory.Create( Document.History, typeof( T ), 6.0 ) )
      {
        FrameExpanded = nextFrames =>
        {
          var c = Enumerable.Range( 0, nextFrames.Count )
            .Select( i => ( i + _connections.Count, (T) _connections.Last().ExpandConnection( nextFrames[ i ] ) ) )
            .Select( item => Setup( item.Item2, item.Item1 ) )
            .ToArray() ;
          _connections.AddRange( c ) ;
          UpdatePhysicalProperties();
          UpdateConnectionLocations() ;
        },
        FrameCurtailed = num =>
        {
          var removingNum = Math.Min( _connections.Count - 1, num ) ;
          _connections.RemoveRange( _connections.Count - removingNum, removingNum ) ;
          UpdatePhysicalProperties();
        },
        FloorNumberChanged = () =>  
        {
          _connections.SelectMany( c => c.SyncConnectionsToRefFrame() )
            .ForEach( c => c.WeightChanged += OnUpdatePhysicalProperties ) ;
          UpdatePhysicalProperties();
        }, 
      } ;
      frames.WeightChanged += OnUpdatePhysicalProperties ;
      _frames = frames ;

      var connections = Document.CreateEntity<T>() ;
      connections.Initialize( _frames.TailFrame ) ;
      Setup( connections, 0 ) ;
      _connections.Add( connections ) ;
      _layerControls.Add( new PipeRackLayerControl( this, 0 ) ) ;

      _foundation.Value = Document.CreateEntity<Foundation>() ;
      _foundation.Value.Name = "Foundation" ;
      _foundation.Value.Link( frames ) ;
      
      UpdatePhysicalProperties();
    }

    protected internal override void RegisterNonMementoMembersFromDefaultObjects()
    {
      base.RegisterNonMementoMembersFromDefaultObjects() ;

      SetupEvents( _foundation.Value );
    }

    protected IList<T> Connections => _connections ;

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      if ( ! ( another is RackBase<T> rack ) ) {
        return ;
      }

      Name = rack.Name ;
      
      IntervalCount = rack.IntervalCount ;
      FloorCount = rack.FloorCount ;

      _frames.CopyFloorProperties( rack._frames );
      foreach ( var wNum in Enumerable.Range( 0, IntervalCount ) ) {
        _connections[ wNum ].BeamInterval = rack._connections[ wNum ].BeamInterval ;
        foreach ( var floor in Enumerable.Range( 0, FloorCount ) ) {
          _connections[ wNum ][ floor ].CopyFrom( rack._connections[ wNum ][ floor ], storage ) ;
        }
      }

      _foundation.Value.Capacity = rack._foundation.Value.Capacity ;
      _physicalProp.Value = new PhysicalProperty( rack.SteelWeight, rack.RcVolume ) ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Interval",
      Visibility = UI.PropertyVisibility.Editable, Order = IntervalPropOrder )]
    public int IntervalCount
    {
      get => _connections.Count ;
      set
      {
        if ( value <= 0 || value == _connections.Count ) {
          return ;
        }

        var addingNum = value - _connections.Count ;
        if ( addingNum < 0 ) {
          _frames.CurtailFrames( Math.Abs( addingNum ) );
          return ;
        }
        _frames.Expand( addingNum ) ;
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Width",
      Visibility = UI.PropertyVisibility.Editable, Order = WidthPropOrder )]
    public double Width
    {
      get => _frames.Width ;
      set => _frames.Width = value ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Floor Count", 
      Visibility = UI.PropertyVisibility.Editable, Order = FloorCountPropOrder )]
    public int FloorCount
    {
      get => _frames.FloorNumber ;
      set => _frames.FloorNumber = value ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "BeamInterval",
      Visibility = UI.PropertyVisibility.Editable, Order = BeamIntervalPropOrder )]
    public double BeamInterval
    {
      get => _frames.PositionOffset ;
      set
      {
        _frames.PositionOffset = value ;
        UpdateConnectionLocations();
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Steel Mass", Visibility = UI.PropertyVisibility.ReadOnly,
      Order = BeamIntervalPropOrder + 1 )]
    public double SteelWeight => _physicalProp.Value.SteelMass ;
    
    [UI.Property( UI.PropertyCategory.Dimension, "RC Volume", Visibility = UI.PropertyVisibility.ReadOnly,
      Order = BeamIntervalPropOrder + 2 )]
    public double RcVolume => _physicalProp.Value.RcVolume ;

    [UI.Property( UI.PropertyCategory.OtherValues, "Floors", ValueType = UI.ValueType.Composite, IndexLabelPrefix = "Floor",
      Visibility = UI.PropertyVisibility.Editable, Order = TailPropOrder )]
    public IList<PipeRackLayerControl> FloorLayers
    {
      get
      {
        if ( _layerControls.Count != FloorCount ) {
          _layerControls = Enumerable.Range( 0, FloorCount ).Select( i => new PipeRackLayerControl( this, i ) )
            .ToList() ;
        }
        return _layerControls ;
      }
    }

    public void SetWidthAndStandardMaterials( double width, double beamInterval )
    {
      Width = width ;
      BeamInterval = beamInterval ;
      
      _frames.BeamMaterial = MaterialDataService.Steel( SteelShapeType.H ).Beam( Width ) ;
      SetSideBeamMaterial( MaterialDataService.Steel( SteelShapeType.H ).Beam( beamInterval ) ) ;
      var mat = ( _frames.BeamMaterial.SubSize > _connections[ 0 ][ 0 ].BeamMaterial.SubSize )
        ? _frames.BeamMaterial
        : _connections[ 0 ][ 0 ].BeamMaterial ;
      _frames.ColumnMaterial = MaterialDataService.Steel( SteelShapeType.H ).Column( mat ) ;
    }

    public double BeamHeight => _frames.BeamMaterial.MainSize ;

    void SetSideBeamMaterial( IStructuralMaterial value )
    {      
      foreach ( var i in Enumerable.Range( 0, FloorCount - 1 ) ) {
        foreach ( var connections in _connections ) {
          connections[ i ].BeamMaterial = value.CreateCopy() ;
        }
      }

      SetSideBeamMaterial( value, FloorCount - 1 ) ;
    }

    public override IEnumerable<IElement> Children 
      => null != _foundation.Value 
        ? ( new [] { _foundation.Value } ).Concat<IElement>( _connections )
        : _connections ;
    
    protected override IEnumerable<IStructurePart> GetElements() => _frames.CreateElements() ;

    public void SetStandardBraces()
    {      
      var doubleSideBrace = ( IntervalCount >= 4 && _connections.Sum( c => c.BeamInterval ) > 30.0 ) ;
      var sideBeamPos = _connections.Count / 2 ;
      bool UseSideBraces( int interval ) =>
        ( interval == sideBeamPos ) || ( doubleSideBrace && ( interval == sideBeamPos - 1 ) ) ;

      var floorBraceFlags = Enumerable.Repeat( false, IntervalCount ).ToArray() ;
      floorBraceFlags[ sideBeamPos ] = true ;
      if ( doubleSideBrace ) {
        floorBraceFlags[ sideBeamPos - 1 ] = true ;
      }
      for ( var i = sideBeamPos + 2 ; i < floorBraceFlags.Length ; i += 2 ) {
        floorBraceFlags[ i ] = true ;
      }
      for ( var i = ( doubleSideBrace ? sideBeamPos - 3 : sideBeamPos - 2 ) ; i >= 0 ; i -= 2 ) {
        floorBraceFlags[ i ] = true ;
      }
      
      foreach ( var i in Enumerable.Range( 0, floorBraceFlags.Length ) ) {
        foreach ( var floor in Enumerable.Range( 0, FloorCount ) ) {
          _connections[ i ][ floor ].Brace = UseSideBraces( i ) ;
          _connections[ i ][ floor ].SetHorizontalBrace( floorBraceFlags[i] ) ;
        }
      }
    }
    
    public override Bounds? GetGlobalBounds()
    {
      var y = Enumerable.Range( 0, _frames.FrameCount ).Sum( i => _frames[ i ].PositionOffset ) ;
      var h = _frames.HeightFromGround( _frames.FloorNumber - 1 ) ;
      var size = new Vector3d( _frames.Width, y, h );
      return LocalCod.GlobalizeBounds( new Bounds( 0.5f * (Vector3)size, (Vector3)size ) ) ;
    }
    
    public void SetFloorHeight( int layer, double value )
    {
      _frames.SetFloorHeight( layer, value ) ;
      _connections.ForEach( u => u.SetHeight( layer, value ) );
    }

    public void SetSideBeamOffset( int layer, double value )
    {
      var target = _connections
        .Where( c => Math.Abs( c[ layer ].HeightOffset - value ) > Tolerance.FloatEpsilon )
        .ToArray() ;
      if ( ! target.Any() ) {
        return ;
      }
      
      target.Take( target.Length - 1 ).ForEach( u => u[ layer ].SetHeightOffset( value ) ) ;
      using ( target.Last()[ layer ].ActivateWeightChangedEvent() ) {
        _connections.Last()[ layer ].SetHeightOffset( value ) ;
      }
    }
    protected double HeightFromGround( int layer ) => _frames.HeightFromGround( layer ) ;

    private void OnUpdatePhysicalProperties( object sender, EventArgs e ) 
      => UpdatePhysicalProperties() ;
    
    private void UpdatePhysicalProperties()
    {
      var (frameSteel, frameRc) = _frames.PhysicalProperties ;
      var steel = frameSteel + _connections.Sum( u => u.CalcSteelWeight() ) ;
      var rc = frameRc + _connections.Sum( u => u.RcVolume() ) ;
      var prop = new PhysicalProperty( steel, rc );
      _foundation.Value.Load = prop.Load ; 
      _physicalProp.Value = prop ;
    }

    private T Setup( T connections, int frameNumber )
    {
      if ( connections == null ) {
        return null;
      }
      connections.Name = $"W-{frameNumber}" ;
      connections.BeamIntervalChanged += ( s, e ) => UpdateConnectionLocations() ;
      Enumerable.Range( 0, FloorCount )
        .ForEach( i =>connections[ i ].WeightChanged += OnUpdatePhysicalProperties ) ;
      return connections ;
    }

    private void UpdateConnectionLocations()
    {
      var current = LocalCodSys3d.Identity; 
      foreach ( var c in _connections ) {
        c.LocalCod = current ;
        current = current.CalcNextConnectionCodSys( c.BeamInterval ) ;
      }
    }
    
    private void SetSideBeamMaterial( IStructuralMaterial mat, int layer )
    {
      _connections.Take( _connections.Count - 1 ).ForEach( u => u[ layer ].BeamMaterial = mat.CreateCopy() ) ;
      using ( _connections.Last()[ layer ].ActivateWeightChangedEvent() ) {
        _connections.Last()[ layer ].BeamMaterial = mat.CreateCopy() ;
      }
    }
    
    #region 
    public IList<CableRouting.ICablePath> GetCablePath()
    {
      // ひとまず最上階のみ
      var rtnList = new List<CableRouting.ICablePath>();
      var bounds = this.GetGlobalBounds();
      if (bounds == null) return rtnList;

      var minX = bounds.Value.min.x;
      var minY = bounds.Value.min.y;
      var maxX = bounds.Value.max.x;
      var maxY = bounds.Value.max.y;

      var layerCnt = FloorCount;
      var z = HeightFromGround(layerCnt - 1);

      var doc = this.Document ;
      var cablePathEntity = Electricals.ElectricalsFactory.CreateCablePath( doc ) ;
      cablePathEntity.Init(new Vector3d( minX, minY, z ), new Vector3d( maxX, maxY, z ), this) ;
      rtnList.Add( cablePathEntity as CableRouting.ICablePath);
      
      return rtnList;
    }
    #endregion
  }
}