using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class SubFrame : TransverseFrameBase, ITransverseFrame
  {
    private readonly Memento<double> _positionOffset ;
    
    private readonly ITransverseFrame _baseFrame ;

    public SubFrame( ITransverseFrame baseFrame, double positionOffset ) : base( baseFrame.History )
    {
      _baseFrame = baseFrame ;
      _positionOffset = CreateMemento( positionOffset ) ;
    }

    public ITransverseFrame CreateNextFrame() => new SubFrame( _baseFrame, PositionOffset ) ;
    
    public int FloorNumber => _baseFrame.FloorNumber ;

    public void ChangeFloorNumber( int value ) { }

    public double Width
    {
      get => _baseFrame.Width ;
      set {}
    }

    public double PositionOffset
    {
      get => _positionOffset.Value ;
      set => _positionOffset.TryChangeValue( value, TryFireWeightChangedEvent ) ;
    }

    public double HeightFromGround( int layer ) => _baseFrame.HeightFromGround( layer ) ;
    public IFrameFloor this[ int floor ] => _baseFrame[ floor ] ;

    public IEnumerable<IFrameFloor> Items => _baseFrame.Items ;
    public IEnumerable<IStructurePart> Elements => _baseFrame.Elements ;
    public IEnumerable<Vector3> ColumnPositions => _baseFrame.ColumnPositions ;
  }
}