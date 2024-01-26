using System ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class FrameFloor : TransverseFrameBase, IFrameFloor
  {
    private readonly Memento<double> _height ;
    private readonly Memento<IStructuralMaterial> _beam ;
    private readonly Memento<IStructuralMaterial> _column ;
    
    public FrameFloor( History h, IStructuralMaterial beamMaterial )
      : this( h, 6.0, beamMaterial, DefaultColumnMat( beamMaterial ) )
    {}

    private FrameFloor( History h, double height, IStructuralMaterial beam, IStructuralMaterial column ) : base( h )
    {
      _height = CreateMemento( height ) ;
      _beam = CreateMemento( beam ) ;
      _column = CreateMemento( column ) ;
    }
    
    public IFrameFloor CreateCopy()
    {
      return new FrameFloor( History, Height, BeamMaterial.CreateCopy(), ColumnMaterial.CreateCopy() ) ;
    }

    public void CopyFrom( IFrameFloor src )
    {
      _height.Value = src.Height ;
      _beam.Value = src.BeamMaterial.CreateCopy() ;
      _column.Value = src.ColumnMaterial.CreateCopy() ;
    }

    public double Height
    {
      get => _height.Value ;
      set => _height.TryChangeValue( value, TryFireWeightChangedEvent );
    }

    public IStructuralMaterial ColumnMaterial
    {
      get => _column.Value ;
      set => _column.TryChangeValue( value, TryFireWeightChangedEvent );
    }

    public IStructuralMaterial BeamMaterial
    {
      get => _beam.Value ;
      set => _beam.TryChangeValue(  value, TryFireWeightChangedEvent );
    }

    private static IStructuralMaterial DefaultColumnMat( IStructuralMaterial beam )
      => beam.IsSteel ? MaterialDataService.Steel( beam.ShapeType ).Column( beam ) : MaterialDataService.Rc.Column( beam ) ;
    
  }
}