using System.Linq ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class AutoRoutingRackBase<T> : RackBase<T>, IPipeRack
    where T : Entity, IFrameConnections
  {
    private readonly MementoList<Region> _regions ;
    private readonly Memento<bool> _halfDownSide ;

    protected AutoRoutingRackBase( Document document ) : base( document )
    {
      _halfDownSide = CreateMementoAndSetupValueEvents( false ) ;
      _regions = CreateMementoListAndSetupChildrenEvents<Region>() ;
    }
    
    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;
      _regions.CopyFrom( _regions.Select( region => region.GetCopyObjectOrClone( storage ) ) ) ;

      if ( ! ( another is AutoRoutingRackBase<T> rack ) ) {
        return ;
      }
      _halfDownSide.CopyFrom( rack.IsHalfDownSideBeam ) ;
    }

    [UI.Property( UI.PropertyCategory.OtherValues, "Half-Down Side Beam", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable, Order = RackBase<T>.TailPropOrder - 1)]
    public bool IsHalfDownSideBeam
    {
      get => _halfDownSide.Value ;
      set
      {
        Connections.ForEach( r => r.UseHalfDownSideBeam( value ) ) ;
        _halfDownSide.Value = value ;
      }
    }

    public (float, float) GetFloorHeight( int layerNum )
    {
      var h = HeightFromGround( layerNum ) ;
      var offset = Connections[ 0 ][ layerNum ].HeightOffset ;
      return ( (float) h, (float) ( h - offset ) ) ;
    }
  }
}