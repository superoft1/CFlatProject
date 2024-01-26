using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model.Routing
{
  [Entity(EntityType.Type.DirectionalPoint)]
  public class DirectionalPoint : Point
  {
    public static Vector3d DefaultDirection = Vector3d.right ;
    
    public enum ShowArrowMode
    {
      None,
      Always,
      InHighlight
    };

    private readonly Memento<ShowArrowMode> _showArrow;
    private readonly Memento<Vector3d> _direction;

    public DirectionalPoint( Document document ) : base( document )
    {
      _direction = CreateMementoAndSetupValueEvents( DefaultDirection ) ;
      _showArrow = CreateMementoAndSetupValueEvents( ShowArrowMode.InHighlight ) ;
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as DirectionalPoint;
      _direction.CopyFrom( entity._direction.Value );
      _showArrow.CopyFrom( entity._showArrow.Value ) ;
    }

    [UI.Property( UI.PropertyCategory.Position, "Direction", ValueType = UI.ValueType.Position,
      Visibility = UI.PropertyVisibility.ReadOnly )]
    public Vector3d Direction
    {
      get { return _direction.Value ; }
      set { _direction.Value = value ; }
    }

    public ShowArrowMode ShowArrow
    {
      get { return _showArrow.Value; }
      set { _showArrow.Value = value; }
    }

  }
}