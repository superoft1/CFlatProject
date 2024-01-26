using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model
{
  public class ElectricalDevices :Electricals.Electricals, IPlacement
  {
    private readonly Memento<LocalCodSys3d> _localCod ;
    private readonly Memento<Vector3d> _size ;
    
    protected ElectricalDevices( Document document ) : base( document )
    {
      _localCod = CreateMementoAndSetupValueEvents( LocalCodSys3d.Identity ) ;
      _size = CreateMementoAndSetupValueEvents( Vector3d.one ) ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.Position, "Rotation", ValueType = UI.ValueType.Rotation,
      Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public double Rotation
    {
      get => LocalCod.Rotation.eulerAngles.z ;
      set => LocalCod = new LocalCodSys3d( LocalCod.Origin, Quaternion.AngleAxis( (float) value, Vector3.forward ), LocalCod.IsMirrorType ) ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.Position, "World Position", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public Vector3d Position
    {
      get => LocalCod.Origin ;
      set => LocalCod = new LocalCodSys3d( value, LocalCod ) ;
    }

    [Chiyoda.UI.Property( UI.PropertyCategory.Dimension, "Size", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public Vector3d Size
    {
      get => _size.Value ;
      set => _size.Value = value;
    }

    public virtual LocalCodSys3d LocalCod
    {
      get => _localCod.Value ;
      set => _localCod.Value = value ;
    }
    public LocalCodSys3d ParentCod
    {
      get
      {
        for ( var parent = Parent ; null != parent && ! ( parent is Document ) ; parent = parent.Parent ) {
          if ( parent is IPlacement p ) {
            return p.GlobalCod ;
          }
        }
        return LocalCodSys3d.Identity ;
      }
    }

    public LocalCodSys3d GlobalCod => ParentCod.GlobalizeCodSys( LocalCod ) ;
  }
}