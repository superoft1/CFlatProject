using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using UnityEngine ;
using UnityEngine.EventSystems ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class FrameUnit1Plus1 : TransverseFrameBase, IFrameFloor
  {
    private readonly Memento<FrameFloor> _left ;
    private readonly Memento<FrameFloor> _right ;
    private readonly Memento<double> _connectionLength ;
    
    public FrameUnit1Plus1( History h ) : base( h )
    {
      var material = MaterialDataService.Steel( SteelShapeType.H ).Beam( 6.0 ) ;
      _left = CreateMemento( new FrameFloor( h, material ) ) ;
      _right = CreateMemento( new FrameFloor( h, material ) ) ;
      _connectionLength = CreateMemento( 4.0 ) ;
    }

    public IFrameFloor CreateCopy()
    {
      var copy = new FrameUnit1Plus1( History )
      {
        ConnectionLength = ConnectionLength,
      } ;
      copy._left.Value = (FrameFloor) _left.Value.CreateCopy() ;
      copy._right.Value = (FrameFloor) _right.Value.CreateCopy() ;
      return copy ;
    }

    public void CopyFrom( IFrameFloor src )
    {
      if( !(src is FrameUnit1Plus1 f ) ) {
        return ;
      }
      
      _left.Value.CopyFrom( f._left.Value );
      _right.Value.CopyFrom( f._right.Value ) ;
      _connectionLength.Value = f._connectionLength.Value ;
    }

/*    public double LeftWidth
    {
      get => _left.Value.Width ;
      set => _left.Value.Width = value ;
    }

    public double RightWidth
    {
      get => _right.Value.Width ;
      set => _right.Value.Width = value ;
    }*/

/*    public double Width
    {
      get => LeftWidth ;
      set
      {
        RightWidth = value ;
        LeftWidth = value ;
      }
    }*/

    public double ConnectionLength
    {
      get => _connectionLength.Value ;
      set => _connectionLength.Value = value ;
    }
    
    public double Height
    {
      get => _left.Value.Height ;
      set => SetValue( a => a.Height = value ) ;
    }

/*    public double PositionOffset
    {
      get => _left.Value.PositionOffset ;
      set => SetValue( a => a.PositionOffset = value ) ;
    }*/

    public IStructuralMaterial BeamMaterial
    {
      get => _left.Value.BeamMaterial ;
      set => SetValue( a => a.BeamMaterial = value ) ;
    }

    public IStructuralMaterial ColumnMaterial
    {
      get => _left.Value.ColumnMaterial ;
      set => SetValue( a => a.ColumnMaterial = value ) ;
    }
    
/*    public IEnumerable<Vector3d> ColumnPositions
      => _left.Value.ColumnPositions.Concat( _right.Value.ColumnPositions ).Select( AdjustRightPosition ) ;
    
    public IEnumerable<IStructureElement> Beams 
      => _left.Value.Beams
        .Concat( _right.Value.Beams.Select( AdjustRightPosition ) )
        .Concat( new[] { ConnectionBeam } ) ;
    
    private IStructureElement ConnectionBeam
    {
      get
      {
        var leftBeam = _left.Value.Beams.First() ;
        var dir = new Vector3( 0.5f * (float) ( ConnectionLength + LeftWidth ), 0.0f, 0.0f ) ;
        var elm = MaterialDataService.GetElement( _left.Value.BeamMaterial, ConnectionLength ) ;
        elm.LocalCod = leftBeam.LocalCod.Translate( dir ) ;
        return elm ;
      }
    }

    private Vector3d AdjustRightPosition( Vector3d p )
    {
      return p + new Vector3d( ( _left.Value.Width + _connectionLength.Value ), 0.0, 0.0 ) ;
    }
    
    private IStructureElement AdjustRightPosition( IStructureElement beam )
    {
      beam.LocalCod =
        beam.LocalCod.Translate( new Vector3( (float) ( _left.Value.Width + _connectionLength.Value ), 0f, 0f ) ) ;
      return beam ;
    }*/

    private void SetValue( Action<FrameFloor> action )
    {
      action( _left.Value ) ;
      action( _right.Value ) ;
      using ( ActivateWeightChangedEvent() ) {
        TryFireWeightChangedEvent() ;
      }
    }
  }
}