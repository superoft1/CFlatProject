using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  [Entity( EntityType.Type.PipeRack1Plus1 )]
  internal class PipeRack1Plus1 : AutoRoutingRackBase<FrameConnections1Plus1>
  {
    public PipeRack1Plus1( Document document ) : base( document )
    {
    }

    [UI.Property( UI.PropertyCategory.OtherValues, "Left Width",
      ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable,
      Order = AutoRoutingRackBase<FrameConnections1Plus1>.WidthPropOrder + 1 )]
    public double LeftWidth
    {
      get => Connections[ 0 ].LeftWidth ;
      set => Connections.ForEach( u => u.LeftWidth = value ) ;
    }
      
    [UI.Property( UI.PropertyCategory.OtherValues, "Right Width",
      ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, 
      Order = AutoRoutingRackBase<FrameConnections1Plus1>.WidthPropOrder + 2 )]
    public double RightWidth
    {
      get => Connections[ 0 ].RightWidth ;
      set => Connections.ForEach( u => u.RightWidth = value ) ;
    }
      
    [UI.Property( UI.PropertyCategory.OtherValues, "Connection Width",
      ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, 
      Order = AutoRoutingRackBase<FrameConnections1Plus1>.WidthPropOrder + 3 )]
    public double ConnectionWidth
    {
      get => Connections[ 0 ].ConnectionWidth ;
      set => Connections.ForEach( u => u.ConnectionWidth = value ) ;
    }
  }
}