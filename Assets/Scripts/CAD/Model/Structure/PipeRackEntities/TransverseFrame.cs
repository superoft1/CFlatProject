using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class TransverseFrame<TFrameUnit> : TransverseFrameBase, ITransverseFrame 
    where TFrameUnit : class, IFrameFloor
  {
    private readonly Memento<double> _width ;
    private readonly MementoList<TFrameUnit> _frameUnits ;

    private readonly Document _doc ;
    
    public TransverseFrame( History h, TFrameUnit unit, double width ) : this( h, width, 0.0 )
    {
      AttachEvent( unit );
      _frameUnits.Add( unit );
    }

    private TransverseFrame( History h, double width, double offset ) : base( h )
    {
      _width = CreateMementoAndSetupValueEvents( width ) ;
      _frameUnits = CreateMementoListAndSetupValueEvents<TFrameUnit>() ;
    }

    public ITransverseFrame CreateNextFrame()
    {
      return new SubFrame( this, 6.0 );
    }

    public int FloorNumber => _frameUnits.Count ;
    
    public void ChangeFloorNumber( int value )
    {
      if ( value <= 0 ) {
        return ;
      }

      var addingCount = value - _frameUnits.Count ;
      if ( addingCount == 0 ) {
        return ;
      }

      if ( addingCount > 0 ) {
        var newFrames = Enumerable.Range( 0, addingCount )
            .Select( i => (TFrameUnit) _frameUnits.Last().CreateCopy() )
            .ToArray() ;
        newFrames.ForEach( AttachEvent );
        _frameUnits.AddRange( newFrames ) ;
        return ;
      }

      var count = Math.Min( _frameUnits.Count - 1, Math.Abs( addingCount ) ) ;
      _frameUnits.RemoveRange( _frameUnits.Count - count, count );
    }

    private void AttachEvent( TFrameUnit unit )
    {
      unit.WeightChanged += ( s, e ) => FireWeightChangeEvent() ;
    }
    
    public double Width
    {
      get => _width.Value ;
      set
      {
        _width.Value = value ;
        FireWeightChangeEvent();
      } 
    }

    private void FireWeightChangeEvent()
    {
      using ( ActivateWeightChangedEvent() ) {
        TryFireWeightChangedEvent() ;
      }
    }

    public double PositionOffset
    {
      get => 0.0 ; 
      set { } 
    }

    public IFrameFloor this[ int layer ] => _frameUnits[ layer ] ;
    
    public double HeightFromGround( int layer )
    {
      return Enumerable.Range( 0, layer+1 )
        .Select( i => _frameUnits[ i ].Height )
        .Aggregate( ( a, h ) => a + h ) ;
    }
    public IEnumerable<IFrameFloor> Items => _frameUnits ;

    public IEnumerable<IStructurePart> Elements => Beams.Concat( Columns ) ;

    public IEnumerable<Vector3> ColumnPositions
      => new[] { Vector3.zero, new Vector3( (float)Width, 0.0f, 0.0f ), } ;
    
    private IEnumerable<IStructurePart> Beams =>
      _frameUnits.ShiftZ( u => new [] { CreateBeam( u ) }, u => u.Height ) ;

    private IEnumerable<IStructurePart> Columns
    {
      get
      {
        foreach ( var e in CreateColumns( _frameUnits[ 0 ], 0.3 ) ) {
          yield return e ;
        }

        var offset = _frameUnits[ 0 ].Height ;
        foreach ( var i in Enumerable.Range( 1, _frameUnits.Count - 1 ) ) {
          foreach ( var c in CreateColumns( _frameUnits[ i ] ) ) {
            c.LocalCod = c.LocalCod.Translate( new Vector3d( 0.0, 0.0, offset ) ) ;
            yield return c ;
          }
          offset += _frameUnits[ i ].Height ;
        }
      }
    }
    
    private IEnumerable<IStructurePart> CreateColumns( IFrameFloor floor, double bottomOffset = 0.0 )
    {
      var length = floor.Height - bottomOffset ;
      var hPosition = 0.5 * ( floor.Height + bottomOffset ) ;
      return ColumnPositions.Select( p =>
      {
        var column = MaterialDataService.GetElement( floor.ColumnMaterial, length ) ;
        column.LocalCod = new LocalCodSys3d( p + new Vector3d( 0.0, 0.0, hPosition ) ) ;
        return column ;
      } ) ;
    }
    
    private IStructurePart CreateBeam( IFrameFloor floor )
    {
      return BeamUtility.GetBeam( floor.BeamMaterial, Width, 
        new Vector3d( Width * 0.5, 0.0, floor.Height - 0.5 * floor.BeamMaterial.MainSize ) ) ;
    }
  }
}