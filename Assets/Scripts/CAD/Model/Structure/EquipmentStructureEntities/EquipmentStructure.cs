using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine ;
using UnityEngine.Experimental ;

namespace Chiyoda.CAD.Model.Structure.Entities 
{
  [Entity( EntityType.Type.PlainRack )]
  internal partial class EquipmentStructure : EmbodyStructureWithUIProperties, IEquipmentStructure, IFreeDraggablePlacement
  {
    private readonly Memento<Vector3Int> _size ;
    private readonly MementoDictionary<Vector3Int, StructureRoom> _rooms ;
    private readonly Memento<PhysicalProperty> _physicalProp ;

    public EquipmentStructure( Document doc ) : base( doc )
    {
      _size = CreateMemento( new Vector3Int( 0, 0, 0 ) ) ;
      _rooms = CreateMementoDictionaryAndSetupChildrenEvents<Vector3Int, StructureRoom>() ;
      _physicalProp = CreateMementoAndSetupValueEvents( new PhysicalProperty( 0.0, 0.0 ) ) ;
    }

    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;
      
      var key = new Vector3Int( 0, 0, 0 ) ;
      RegisterRoom( key );
      _size.Value = new Vector3Int( 1, 1, 1 );
    }

    
    protected override IEnumerable<IStructurePart> GetElements()
    {
      return CreateColumns() ;
    }
    
    [UI.Property( UI.PropertyCategory.Dimension, "Steel Mass", Visibility = UI.PropertyVisibility.ReadOnly,
      Order = 100 )]
    public double SteelWeight => _physicalProp.Value.SteelMass ;
    
    [UI.Property( UI.PropertyCategory.Dimension, "RC Volume", Visibility = UI.PropertyVisibility.ReadOnly,
      Order = 101 )]
    public double RcVolume => _physicalProp.Value.RcVolume ;


    public override IEnumerable<IElement> Children => _rooms.Values ; 

    [UI.Property( UI.PropertyCategory.Dimension, "Span (X)", Visibility = UI.PropertyVisibility.Editable, Order = 10 )]
    public int SpanDirectionCount
    {
      get => _size.Value.x ;
      set
      {
        if ( (value < 1) || (SpanDirectionCount == value) ) {
          return ;
        }

        if ( SpanDirectionCount < value ) {
          ExpandAlongX( value ) ;
          _size.Value = new Vector3Int( value, _size.Value.y, _size.Value.z );
          UpdateRoomLocations();
        }
        else {
          RemoveRooms( k => k.x >= value );
          _size.Value = new Vector3Int( value, _size.Value.y, _size.Value.z );
        }
        UpdatePhysicalProperty();
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Length (X)", Visibility = UI.PropertyVisibility.Editable,
      Order = 11 )]
    public double PropertyXLength
    {
      get => LengthX( 0 ) ;
      set
      {
        foreach ( var r in _rooms.Values ) {
          r.SetX( value );
        }
        UpdateRoomLocations();
        UpdatePhysicalProperty();
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Span (Y)", Visibility = UI.PropertyVisibility.Editable, Order = 12 )]
    public int IntervalCount
    {
      get => _size.Value.y ;
      set
      {
        if ( (value < 1) || (IntervalCount == value) ) {
          return ;
        }
        if ( IntervalCount < value ) {
          ExpandAlongY( value ) ;
          _size.Value = new Vector3Int( _size.Value.x, value, _size.Value.z );
          UpdateRoomLocations();
        }
        else {
          RemoveRooms( k => k.y >= value );
          _size.Value = new Vector3Int( _size.Value.x, value, _size.Value.z );
        }
        UpdatePhysicalProperty();
      }
    }
    
    [UI.Property( UI.PropertyCategory.Dimension, "Length (Y)", Visibility = UI.PropertyVisibility.Editable, Order = 13 )]
    public double PropertyYLength
    {
      get => LengthY( 0 ) ;
      set
      {
        foreach ( var r in _rooms.Values ) {
          r.SetY( value );
        }
        UpdateRoomLocations();
        UpdatePhysicalProperty();
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Number of Floor", Visibility = UI.PropertyVisibility.Editable, Order = 14 )]
    public int FloorCount
    {
      get => _size.Value.z ;
      set
      {
        if ( (value < 1) || (FloorCount == value) ) {
          return ;
        }

        if ( FloorCount < value ) {
          ExpandFloor( value );
        }
        else {
          RemoveRooms( k => k.z >= value );
        }

        _size.Value = new Vector3Int( _size.Value.x, _size.Value.y, value ) ;
        UpdatePhysicalProperty();
      }
    }
    
    
    public double BeamInterval => _rooms.First().Value.Size.y ;

    public double Width => _rooms.First().Value.Size.x ;

    public void SetWidthAndStandardMaterials( double width, double beamInterval )
    {
      var beamMat = MaterialDataService.Steel( SteelShapeType.H ).Beam( width ) ;
      var column = MaterialDataService.Steel( SteelShapeType.H ).Column( beamMat ) ;
      foreach ( var r in _rooms.Values ) {
        r.SetX( width );
        r.SetY( beamInterval );
        r.BeamMaterial = beamMat.CreateCopy() ;
        r.ColumnMaterial = column.CreateCopy() ;
      }
      UpdateRoomLocations();
      UpdatePhysicalProperty();
    }

    public void SetFloorHeight( int layer, double value )
    {
      foreach ( var xy in XyIndices() ) {
        if ( ! _rooms.TryGetValue( new Vector3Int( xy.x, xy.y, layer ), out var r ) ) {
          continue;
        }
        if ( Math.Abs( r.Height - value ) > Tolerance.DoubleEpsilon ) {
          r.SetZ( value ) ;
        }
      }
      UpdateRoomLocations();
      UpdatePhysicalProperty();
    }

    public void SetSideBeamOffset( int layer, double value )
    {
      
    }
    
    
    private StructureRoom RegisterRoom( Vector3Int key )
    {
      var room = Document.CreateEntity<StructureRoom>() ;
      if ( room == null ) {
        return null ;
      }
      room.Name = key.ToName() ;
      room.WeightChanged += RoomWeightChanged ;
      _rooms.Add( key, room ) ;
      return room ;
    }

    private void RoomWeightChanged( object sender, EventArgs e )
    {
      UpdateRoomLocations();
      UpdatePhysicalProperty();
    }

    private void UpdatePhysicalProperty()
    {
      var refRoom = _rooms[ Vector3Int.zero ] ;
      var wColumn = FloorReferenceRooms.Where( item => item.r != null )
        .Sum( item => item.r.Height * MaterialDataService.SteelWeightPerLength( item.r.ColumnMaterial )
                                    * ( _size.Value.x + 1 ) * ( _size.Value.y + 1 ) ) ;
      var wXBeams = ReferenceRoomsAlongX.Where( item => item.r != null )
        .Sum( item => item.r.Size.x * MaterialDataService.SteelWeightPerLength( item.r.BeamMaterial )
                                    * ( _size.Value.y + 1 ) * _size.Value.z ) ;
      var wYBeams = ReferenceRoomsAlongY.Where( item => item.r != null )
        .Sum( item => item.r.Size.y * MaterialDataService.SteelWeightPerLength( item.r.BeamMaterial )
                                    * ( _size.Value.x + 1 ) * _size.Value.z ) ;
      _physicalProp.Value = new PhysicalProperty( wColumn + wXBeams + wYBeams, 0.0 ) ;
    }


    private IEnumerable<(int, StructureRoom r)> ReferenceRoomsAlongY
      => Enumerable.Range( 0, _size.Value.y ).Select( y =>
        ( y, _rooms.TryGetValue( new Vector3Int( 0, y, 0 ), out var r ) ? r : null ) ) ;
    
    private IEnumerable<(int, StructureRoom r)> ReferenceRoomsAlongX
      => Enumerable.Range( 0, _size.Value.x ).Select( x =>
        ( x, _rooms.TryGetValue( new Vector3Int( x, 0, 0 ), out var r ) ? r : null ) ) ;
    
    private IEnumerable<(int floor, StructureRoom r)> FloorReferenceRooms 
      => Enumerable.Range( 0, FloorCount ).Select( f =>
        (f, _rooms.TryGetValue( new Vector3Int( 0, 0, f ), out var r ) ? r : null ) ) ;
 
    private class HeightMaterialList
    { 
      private readonly List<(double h , IStructuralMaterial m)> _list 
        = new List<(double, IStructuralMaterial)>();

      public void Add( double h, IStructuralMaterial mat )
      {
        _list.Add( (h, mat) );
      }
      
      public IEnumerable<IStructurePart> ToColumns( double columnLengthUnit )
      {
        var discretizedValues = _list
          .Select( item => ( (int) ( (item.h + 0.5 * columnLengthUnit) / columnLengthUnit ), item.m ) )
          .OrderBy( item => item.Item1 )
          .GroupBy( item => item.Item1 ) ;
        var prevH = 0.0 ;
        foreach ( var group in discretizedValues ) {
          var h = group.Key * columnLengthUnit ;
          var mat = group.First().m ;
          var column = MaterialDataService.GetElement( mat, ( h - prevH ) ) ;
          column.LocalCod = new LocalCodSys3d( new Vector3d( 0.0, 0.0, 0.5*( prevH + h ) ) );
          yield return column ;
          prevH = h ;
        }
      }
    }

    private IEnumerable<IStructurePart> CreateColumns()
    {
      var columnMap = new HeightMaterialList[ _size.Value.x + 1, _size.Value.y + 1 ] ;
      foreach ( var x in Enumerable.Range( 0, _size.Value.x+1 ) ) {
        foreach ( var y in Enumerable.Range( 0, _size.Value.y+1 ) ) {
          columnMap[x, y] = new HeightMaterialList();
        }
      }

      foreach ( var xy in XyIndices() ) {
        var h = 0.0 ;
        var x = xy.x ;
        var y = xy.y ;
        foreach ( var z in Enumerable.Range( 0, FloorCount ) ) {
          var key = new Vector3Int( x, y, z ) ;
          if ( ! _rooms.TryGetValue( key, out var r ) || ! r.IsActive ) {
            continue ;
          }

          h += r.Height ;
          columnMap[ x, y ].Add( h, r.ColumnMaterial ) ;
          columnMap[ x + 1, y ].Add( h, r.ColumnMaterial ) ;
          columnMap[ x, y + 1 ].Add( h, r.ColumnMaterial ) ;
          columnMap[ x + 1, y + 1 ].Add( h, r.ColumnMaterial ) ;
        }
      }

      var xOffset = 0.0 ;
      foreach ( var x in Enumerable.Range( 0, _size.Value.x+1 ) ) {
        var yOffset = 0.0 ;
        foreach ( var y in Enumerable.Range( 0, _size.Value.y+1 ) ) {
          var translateVec = new Vector3d( xOffset, yOffset, 0.0 );
          foreach ( var c in columnMap[x, y].ToColumns( 0.01 ) ) {
            c.LocalCod = c.LocalCod.Translate( translateVec ) ;
            yield return c ;
          }
          yOffset += LengthY( y ) ;
        }
        xOffset += LengthX( x ) ;
      }
    }

    private double LengthX( int x )
      => _rooms.TryGetValue( new Vector3Int( x, 0, 0 ), out var r ) ? r.Size.x : 6.0 ;

    private double LengthY( int y )
      => _rooms.TryGetValue( new Vector3Int( 0, y, 0 ), out var r ) ? r.Size.y : 6.0 ;
    
    private void RemoveRooms( Predicate<Vector3Int> isTarget )
    {
      var removingKeys = _rooms.Keys.Where( k => isTarget( k ) ).ToArray() ;
      removingKeys.ForEach( k => _rooms.Remove( k ) );
    }
    
    private void ExpandAlongX( int value )
    {
      foreach ( var y in Enumerable.Range( 0, _size.Value.y ) ) {
        var ySize = LengthY( y ) ;
        foreach ( var z in Enumerable.Range( 0, _size.Value.z ) ) {
          var r = _rooms[ new Vector3Int( _size.Value.x - 1, y, z ) ] ;

          for ( int x = _size.Value.x ; x < value ; ++x ) {
            var room = RegisterRoom( new Vector3Int( x, y, z ) ) ;
            if ( room == null ) {
              continue ;
            }

            room.SetY( ySize ) ;
            room.SetZ( r.Height ) ;
          }
        }
      }
    }
    
    private void ExpandAlongY( int value )
    {
      foreach ( var x in Enumerable.Range( 0, _size.Value.x ) ) {
        var xSize = LengthX( x ) ;
        foreach ( var z in Enumerable.Range( 0, _size.Value.z ) ) {
          var r = _rooms[ new Vector3Int( x, _size.Value.y - 1, z ) ] ;

          for ( var y = _size.Value.y ; y < value ; ++y ) {
            var room = RegisterRoom( new Vector3Int( x, y, z ) ) ;
            if ( room == null ) {
              continue ;
            }

            room.SetX( xSize ) ;
            room.SetZ( r.Height ) ;
          }
        }
      }
    }

    private IEnumerable<Vector2Int> XyIndices()
    {
      return
        from x in Enumerable.Range( 0, _size.Value.x )
        from y in Enumerable.Range( 0, _size.Value.y )
        select new Vector2Int(x, y) ;
    }
    
    private void ExpandFloor( int value )
    {
      var refRooms = new StructureRoom[ _size.Value.x, _size.Value.y ] ;
      
      foreach ( var xy in XyIndices() ) {
        if ( _rooms.TryGetValue( new Vector3Int( xy.x, xy.y, FloorCount-1 ), out var r ) ) {
          refRooms[ xy.x, xy.y ] = r ;
        }
      }
      
      for( var z = _size.Value.z ; z < value ; ++z ) {
        foreach ( var xy in XyIndices() ) {
          var refRoom = refRooms[ xy.x, xy.y ] ;
          var room = RegisterRoom( new Vector3Int( xy.x, xy.y, z ) ) ;
          if ( refRoom != null ) {
            room.SetSize( refRoom.Size ) ;
            room.LocalCod = refRoom.LocalCod.Translate( new Vector3d( 0.0, 0.0, refRoom.Height ) ) ;
          }
          refRooms[xy.x, xy.y] = room ;
        }
      }
    }
    
    private void UpdateRoomLocations()
    {
      var heightMap = new double[ _size.Value.x, _size.Value.y ] ;
      foreach ( var z in Enumerable.Range( 0, FloorCount ) ) {
        var offsetY = 0.0 ;
        foreach ( var y in Enumerable.Range( 0, _size.Value.y ) ) {
          var offsetX = 0.0 ;
          foreach ( var x in Enumerable.Range( 0, _size.Value.x ) ) {
            if ( _rooms.TryGetValue( new Vector3Int( x, y, z ), out var r ) ) {
              r.LocalCod = new LocalCodSys3d( new Vector3d( offsetX, offsetY, heightMap[ x, y ] ) ) ;
              heightMap[ x, y ] += r.Height ;
            }
            offsetX += LengthX( x ) ;
          }
          offsetY += LengthY( y ) ;
        }
      }
    }
  }
}