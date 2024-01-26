using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager ;
using UnityEngine;

namespace Chiyoda.CAD.Model.Routing
{
  [Entity( EntityType.Type.Point )]
  public class Point : Entity
  {
    private readonly Memento<Diameter> _diameter ;
    private readonly Memento<Vector3d> _origin ;

    public Point( Document document ) : base( document )
    {
      _diameter = CreateMementoAndSetupValueEvents( DiameterFactory.FromNpsMm( 1.0 ) ) ;
      _origin = CreateMementoAndSetupValueEvents( Vector3d.zero ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      var entity = another as Point ;
      _diameter.CopyFrom( entity._diameter.Value ) ;
      _origin.CopyFrom( entity._origin.Value ) ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "NPS", ValueType = UI.ValueType.Length,
      Visibility = UI.PropertyVisibility.Editable )]
    public double NPS // 単位はMeter
    {
      get => 0.001*_diameter.Value.NpsMm ;
      set => _diameter.Value = DiameterFactory.FromNpsMm( 1000.0*value ) ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "OutsideDiameter", ValueType = UI.ValueType.Length,
      Visibility = UI.PropertyVisibility.ReadOnly)]
    public double OutsideDiameter // 単位はMeter
    {
      get => _diameter.Value.OutsideMeter ;
    }

    
    [UI.Property( UI.PropertyCategory.Position, "Origin", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.Editable )]
    public Vector3d Origin
    {
      get => _origin.Value ;
      set => _origin.Value = value ;
    }


    public override Bounds? GetGlobalBounds()
    {
      BodyMap.Instance.TryGetBody( this, out var body ) ;

      return body?.GetGlobalBounds() ;
    }

    protected Diameter DiameterBody => _diameter.Value ;
  }
}
