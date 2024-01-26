using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using Chiyoda.CAD.Util ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class StructureRoom : EmbodyStructure, IWeightChangedEventEntity
  {
    private readonly Memento<bool> _isActive ;
    private readonly Memento<bool> _platForm ;
    private readonly Memento<Vector3d> _size ;
    
    private readonly Memento<IStructuralMaterial> _columnMat ;
    private readonly Memento<IStructuralMaterial> _beam ;
    
    private bool _isWeightChangeEventActive ;

    public StructureRoom( Document doc ) : base( doc )
    {
      _isActive = CreateMemento( true ) ;
      _platForm = CreateMemento( true ) ;
      _size = CreateMemento( new Vector3d( 6.0, 6.0, 6.0 ) ) ;

      var beam = MaterialDataService.Steel( SteelShapeType.H ).Beam( 6.0 ) ;
      _beam = CreateMemento( beam ) ;
      var column = MaterialDataService.Steel( SteelShapeType.H ).Column( beam ) ;
      _columnMat = CreateMemento( column ) ;
    }
    
    [UI.Property( UI.PropertyCategory.Dimension, "IsActive",
      Visibility = UI.PropertyVisibility.Editable, Order = 10 )]
    public bool IsActive
    {
      get => _isActive.Value ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          ChangeProperty( _isActive, value ) ;
        }
      }
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Platform",
      Visibility = UI.PropertyVisibility.Editable, Order = 11 )]
    public bool PlatForm
    {
      get => _platForm.Value ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          ChangeProperty( _platForm, value ) ;
        }
      }
    }

/*    [UI.Property( UI.PropertyCategory.Dimension, "Height",
      Visibility = UI.PropertyVisibility.Editable, Order = 10 )]*/
    public double Height
    {
      get => _size.Value.z ;
      set
      {
        using ( ActivateWeightChangedEvent() ) {
          ChangeProperty( _size, new Vector3d( _size.Value.x, _size.Value.y, value ) ) ;
        }
      }
    }

    public Vector3d Size => _size.Value ;
    
    public void SetZ( double z ) => _size.Value = new Vector3d( _size.Value.x, _size.Value.y, z ) ;
    public void SetX( double x ) => _size.Value = new Vector3d( x, _size.Value.y, _size.Value.z ) ;
    public void SetY( double y ) => _size.Value = new Vector3d( _size.Value.x, y, _size.Value.z ) ;

    public void SetSize( Vector3d size ) => _size.Value = size ;
    
    public IStructuralMaterial ColumnMaterial
    {
      get => _columnMat.Value ;
      set => ChangeProperty( _columnMat, value ) ;
    }

    public IStructuralMaterial BeamMaterial
    {
      get => _beam.Value ;
      set => ChangeProperty( _beam, value ) ;
    }

    public override Bounds? GetGlobalBounds()
    {
      var local = new Bounds( (Vector3)(0.5 * _size.Value), (Vector3)_size.Value );
      var b = LocalCod.GlobalizeBounds( local ) ;
      var l = Parent as PlacementEntity ;
      return l?.GlobalCod.GlobalizeBounds( b ) ?? b ;
    }

    public override IEnumerable<IStructurePart> StructureElements
    {
      get
      {
        if ( ! IsActive ) {
          yield break;
        }
        
        var x = _size.Value.x ;
        var y = _size.Value.y ;
        if ( PlatForm ) {
          yield return new HSteel( _size.Value.x, _size.Value.y, 0.01 )
          {
            LocalCod = new LocalCodSys3d( new Vector3d( 0.5 * x, 0.5 * y, Height-0.005 ) ),
          } ;
        }

        var h = Height - 0.5 * _beam.Value.MainSize ;

        yield return BeamUtility.GetSideBeam( BeamMaterial, _size.Value.y, new Vector3d( 0.0, 0.5 * y, h ) ) ;
        yield return BeamUtility.GetSideBeam( BeamMaterial, _size.Value.y, new Vector3d( x, 0.5 * y, h ) ) ;
        yield return BeamUtility.GetBeam( BeamMaterial, _size.Value.x, new Vector3d( 0.5 * x, 0.0, h ) ) ;
        yield return BeamUtility.GetBeam( BeamMaterial, _size.Value.x, new Vector3d( 0.5 * x, y, h ) ) ;
      }
    }
    
    public event EventHandler WeightChanged ;
    public IDisposable ActivateWeightChangedEvent()
    {
      _isWeightChangeEventActive = true ;
      return new DisposableAction( () => _isWeightChangeEventActive = false ) ;
    }

    private void TryInvokeWeightChanged()
    {
      if ( _isWeightChangeEventActive ) {
        WeightChanged?.Invoke( this, EventArgs.Empty ) ;
      }
    }
    
    private void ChangeProperty<T>( Memento<T> prop, T value )
    {
      if ( prop.Value.Equals( value ) ) {
        return ;
      }

      prop.Value = value ;
      TryInvokeWeightChanged();
    }    
  }
}