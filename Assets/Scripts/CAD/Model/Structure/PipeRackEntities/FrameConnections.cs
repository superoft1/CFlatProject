using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class FrameConnections<TConnection> : PlacementEntity, IFrameConnections
    where TConnection : Entity, IFrameConnection
  {
    public event EventHandler BeamIntervalChanged ;

    private ITransverseFrame _referenceFrame ;
    private readonly MementoList<TConnection> _connections ;

    protected FrameConnections( Document document ) : base( document )
    {
      _connections = CreateMementoListAndSetupChildrenEvents<TConnection>() ;
    }

    [UI.Property( UI.PropertyCategory.StructureId, "Name", ValueType = UI.ValueType.Label,
      Visibility = UI.PropertyVisibility.ReadOnly )]
    public string UnitName => Name ;

    [UI.Property( UI.PropertyCategory.Dimension, "Length", ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable )]
    public double BeamInterval
    {
      get => _referenceFrame.PositionOffset ;
      set
      {
        if ( ( Math.Abs( BeamInterval - value ) < Tolerance.DoubleEpsilon ) ) {
          return ;
        }
        using ( _referenceFrame.ActivateWeightChangedEvent() ) {
          _referenceFrame.PositionOffset = value ;
        }
        BeamIntervalChanged?.Invoke( this, EventArgs.Empty ) ;
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Braces", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 4 )]
    public bool Braces
    {
      get => _connections.Any( a => a.Brace ) ;
      set
      {
        var target = _connections.Where( a => a.Brace != value).ToArray();
        if ( ! target.Any() ) {
          return ;
        }
        target.Take( target.Length -1  ).ForEach( a => a.Brace = value ) ;
        using ( _connections.Last().ActivateWeightChangedEvent() ) {
          _connections.Last().Brace = value ;
        }
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Floor Braces", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = 5 )]
    public bool FloorBraces
    {
      get => _connections.Take( _connections.Count-1 ).Any( a => a.HBrace ) ;
      set
      {
        var target = _connections.Where( a => a.HBrace != value ).ToArray();
        if ( ! target.Any() ) {
          return ;
        }
        target.Take( target.Length - 1 ).ForEach( a => a.SetHorizontalBrace( value ) ) ;
        using ( target.Last().ActivateWeightChangedEvent() ) {
          target.Last().SetHorizontalBrace( value ) ;
        }
      }
    }

    public int FloorNumber => _connections.Count ;
    
    public IEnumerable<IFrameConnection> SyncConnectionsToRefFrame()
    {
      var additionalFloorNum = _referenceFrame.FloorNumber - _connections.Count ;

      if ( additionalFloorNum == 0 ) {
        return Enumerable.Empty<IFrameConnection>();
      }

      if ( additionalFloorNum < 0 ) {
        var c = Math.Abs( additionalFloorNum ) ;
        _connections.RemoveRange( _connections.Count - c, c);
        return Enumerable.Empty<IFrameConnection>();
      }
      
      var connections = Enumerable.Range( _connections.Count, additionalFloorNum )
        .Select( i => CreateConnection( _referenceFrame, i, () => (TConnection) _connections.Last().CreateCopy() ) )
        .ToArray() ;
      AddConnections( connections );
      UpdateUpperFloorCodSys( 0 );
      return connections ;
    }

    public IFrameConnections ExpandConnection( ITransverseFrame frame )
    {
      if ( ! ( Document.CreateEntity( GetType() ) is FrameConnections<TConnection> next ) ) {
        return null ;
      }

      next._referenceFrame = frame ;  
      var connections = Enumerable.Range( 0, frame.FloorNumber )
        .Select( i => CreateConnection( frame, i, () => _connections[ i ].CreateCopy() ) )
        .ToArray() ;
      next.AddConnections( connections );
      UpdateUpperFloorCodSys( 0 );
      next.LocalCod = LocalCod.CalcNextConnectionCodSys( BeamInterval ) ;
      return next ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      if( (another is FrameConnections<TConnection> t ) && t._connections != null ) {
        Enumerable.Range( 0, FloorNumber ).ForEach( i => _connections[ i ].CopyFrom( t._connections[ i ], storage ) ) ;
      }
    }


    public virtual void Initialize( ITransverseFrame frame )
    {
      _referenceFrame = frame ;
      var connections = Enumerable.Range( 0, frame.FloorNumber )
        .Select( i => CreateConnection( frame, i, () => Document.CreateEntity<TConnection>() ) )
        .ToArray() ;
      AddConnections( connections );
      UpdateUpperFloorCodSys( 0 );
    }

    public void UseHalfDownSideBeam( bool use )
    {
      _connections.ForEach( u => u.SetHeightOffset( use ? 0.5*u.FloorHeight : 0.0 ) ) ;
    }

    public IFrameConnection this[ int floor ] => _connections[ floor ] ;
    
    public void SetHeight( int floor, double h )
    {
      UpdateUpperFloorCodSys( floor );
    }

    public override IEnumerable<IElement> Children => _connections ;

    public override Bounds? GetGlobalBounds() => _connections.Select( a => a.GetGlobalBounds() ).UnionBounds() ;

    private void UpdateUpperFloorCodSys( int rootFloor )
    {
      for ( var i = rootFloor + 1 ; i < _connections.Count ; ++i ) {
        _connections[ i ].LocalCod =
          _connections[ i - 1 ].LocalCod.Translate( new Vector3d( 0, 0, _referenceFrame[i-1].Height ) ) ;
      }
    }

    private void AddConnections( TConnection[] connections )
    {
      Enumerable.Range( 0, connections.Length - 1 )
        .ForEach( i => connections[ i + 1 ].SetBottomConnection( connections[ i ] ) ) ;
      if ( _connections.Any() ) {
        connections[0].SetBottomConnection( _connections.Last() );
      }
      _connections.AddRange( connections );
    }
    
    private static TConnection CreateConnection( ITransverseFrame f, int floorNum, Func<IFrameConnection> create )
    {
      var connection = (TConnection) create() ;
      connection.SetReferenceFrame( f, floorNum ) ;
      SetName( connection, floorNum ) ;
      return connection ;
    }

    private static void SetName( IFrameConnection connection, int floorNum ) 
    {
      if ( connection is Entity entity ) {
        entity.Name = $"Floor-{floorNum+1}" ;
      }
    }
  }
}