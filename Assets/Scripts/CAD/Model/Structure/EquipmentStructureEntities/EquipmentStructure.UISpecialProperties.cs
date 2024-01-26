using System.Linq ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal partial class EquipmentStructure
  {
    private int _floorNumToEdit = 1 ;
    private int _spanNumX = 1 ;
    private int _spanNumY = 1 ;
    
    [UI.Property( UI.PropertyCategory.OtherValues, "Target Floor", Visibility = UI.PropertyVisibility.Editable, Order = 30 )]
    public int FloorNum
    {
      get
      {
        if ( FloorCount < _floorNumToEdit ) {
          _floorNumToEdit = FloorCount ;
        }
        return _floorNumToEdit ;
      }
      set
      {
        if ( ( value < 1 ) || ( FloorCount < value ) ) {
          return ;
        }
        _floorNumToEdit = value ;
      }
    }
    
    [UI.Property( UI.PropertyCategory.OtherValues, "Floor Height", Visibility = UI.PropertyVisibility.Editable, Order = 31 )]
    public double FloorHeight
    {
      get => _rooms.TryGetValue( new Vector3Int( 0, 0, _floorNumToEdit - 1 ), out var r ) ? r.Height : 6.0 ; 
      set => SetFloorHeight( _floorNumToEdit-1, value ) ;
    }

    [UI.Property( UI.PropertyCategory.OtherValues, "Target X Number", Visibility = UI.PropertyVisibility.Editable, Order = 41 )]
    public int SpanXNumber
    {
      get
      {
        if ( _size.Value.x < _spanNumX ) {
          _spanNumX = _size.Value.x ;
        }
        return _spanNumX ;
      }
      set
      {
        if ( ( value < 1 ) || ( _size.Value.x < value ) ) {
          return ;
        }
        _spanNumX = value ;
      }
    }
    
    [UI.Property( UI.PropertyCategory.OtherValues, "X Span Width", Visibility = UI.PropertyVisibility.Editable, Order = 42 )]
    public double SpanWidthX
    {
      get => _rooms.TryGetValue( new Vector3Int( _spanNumX-1, 0, 0 ), out var r ) ? r.Size.x : 6.0 ;
      set => SetSpanWidthX( _spanNumX-1, value ) ;
    }
    
    [UI.Property( UI.PropertyCategory.OtherValues, "Target Y Number", Visibility = UI.PropertyVisibility.Editable, Order = 51 )]
    public int SpanYNumber
    {
      get
      {
        if ( _size.Value.x < _spanNumY ) {
          _spanNumY = _size.Value.y ;
        }
        return _spanNumY ;
      }
      set
      {
        if ( ( value < 1 ) || ( _size.Value.y < value ) ) {
          return ;
        }
        _spanNumY = value ;
      }
    }
    
    [UI.Property( UI.PropertyCategory.OtherValues, "Y Span Width ", Visibility = UI.PropertyVisibility.Editable, Order = 52 )]
    public double SpanWidthY
    {
      get => _rooms.TryGetValue( new Vector3Int( 0, _spanNumY-1, 0 ), out var r ) ? r.Size.y : 6.0 ;
      set => SetSpanWidthY( _spanNumY-1, value ) ;
    }
    
    private void SetSpanWidthX( int spanNumX, double value )
    {
      var yzIndices = 
        from y in Enumerable.Range( 0, _size.Value.y )
        from z in Enumerable.Range( 0, _size.Value.z )
        select new Vector2Int( y, z ) ;
      foreach ( var yz in yzIndices ) {
        if( _rooms.TryGetValue( new Vector3Int( spanNumX, yz.x, yz.y ), out var r ) ) {
          r.SetX( value );
        } 
      }
      UpdateRoomLocations();
      UpdatePhysicalProperty();
    }

    private void SetSpanWidthY( int spanNumY, double value )
    {
      var xzIndices = 
        from y in Enumerable.Range( 0, _size.Value.x )
        from z in Enumerable.Range( 0, _size.Value.z )
        select new Vector2Int( y, z ) ;
      foreach ( var xz in xzIndices ) {
        if( _rooms.TryGetValue( new Vector3Int( xz.x, spanNumY, xz.y ), out var r ) ) {
          r.SetY( value );
        } 
      }
      UpdateRoomLocations();
      UpdatePhysicalProperty();
    }
  }
}