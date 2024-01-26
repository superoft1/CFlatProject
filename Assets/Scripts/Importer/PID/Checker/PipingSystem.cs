using System;
using System.Linq;
using System.Collections.Generic;

namespace PID
{
  class PipingSystem
  {
    public string ID { get; set; } = "";

    public string LineID { get; set; } = "";

    public List<PipingSegment> Segments { get; } = new List<PipingSegment>();

    public List<PropertyBreak> Breaks { get; } = new List<PropertyBreak>();

    public void BuildComponentConnections()
    {
      // PipingNetworkSegment間のConnectionのみ構築 (IConnectable間のConnectionは未対応)
      foreach ( var segment in Segments ) {
        {
          // From方向
          var parentComponent = FindConnectableComponent( segment.Connection.From.id );
          if ( null != parentComponent &&
               ( null == parentComponent.Owner || parentComponent.Owner != segment ) ) {
            var childComponent = segment.Components.First();

//            parentComponent.Children.Add( ( childComponent, -1 ) ); // TODO: 接続インデックスの設定は未対応
//            childComponent.Parents.Add( ( parentComponent, segment.Connection.From.index ) );

            // PipingNetworkSegmentのNominalDiameterと同径接続は優先して探索されるように
            var childIndex = parentComponent.Children.Count;
            if ( parentComponent.Type == "PipingNetworkBranch" &&
//                 ( ( parentComponent as IPipingFitting )?.Diameters[segment.Connection.From.index] ?? "" ) ==  parentComponent.Owner.Diameter ) {
                 segment.Diameter == parentComponent.Owner.Diameter ) { // FromNodeが間違っている場合があったため...
              childIndex = 0;
            }
            parentComponent.Children.Insert( childIndex, ( childComponent, -1 ) ); // TODO: 接続インデックスの設定は未対応

            var parentIndex = childComponent.Parents.Count;
            if ( childComponent.Type == "PipingNetworkBranch" &&
                 ( (parentComponent as IPipingFitting )?.Diameters[segment.Connection.From.index] ?? "" ) == segment.Diameter ) {
              parentIndex = 0;
            }
            childComponent.Parents.Insert( parentIndex, ( parentComponent, segment.Connection.From.index ) );
          }
        }
        {
          // To方向
          var childComponent = FindConnectableComponent( segment.Connection.To.id );
          if ( null != childComponent && ( null == childComponent.Owner || childComponent.Owner != segment ) ) {
            var parentComponent = segment.Components.Last();

//            childComponent.Parents.Add( ( parentComponent, -1 ) ); // TODO: 接続インデックスの設定は未対応
//            parentComponent.Children.Add( ( childComponent, segment.Connection.To.index ) );

            // PipingNetworkSegmentのNominalDiameterと同径接続は優先して探索されるように
            var parentIndex = childComponent.Parents.Count;
            if ( childComponent.Type == "PipingNetworkBranch" &&
                 ( ( childComponent as IPipingFitting )?.Diameters[segment.Connection.To.index] ?? "" ) == childComponent.Owner.Diameter ) {
              parentIndex = 0;
            }
            childComponent.Parents.Insert( parentIndex, ( parentComponent, -1 ) ); // TODO: 接続インデックスの設定は未対応

            var childIndex = parentComponent.Children.Count;
            if ( parentComponent.Type == "PipingNetworkBranch" &&
                 ( ( childComponent as IPipingFitting)?.Diameters[segment.Connection.To.index] ?? "" ) == segment.Diameter ) {
//                 childComponent.Owner.Diameter ==  segment.Diameter ) { // ToNodeが間違っている場合はこの回避策が有効か...
              childIndex = 0;
            }
            parentComponent.Children.Insert( childIndex, ( childComponent, segment.Connection.To.index ) );
          }
        }
      }
    }

    private IConnectable FindConnectableComponent( string id )
    {
      var firstInSegment = Segments.Select( segment => segment.Components.First() )
                                   .SingleOrDefault( component => component.ID == id );
      if ( null != firstInSegment ) {
        return firstInSegment;
      }

      var lastInSegment = Segments.Select( segment => segment.Components.Last() )
                                  .SingleOrDefault( component => component.ID == id );
      if ( null != lastInSegment ) {
        return lastInSegment;
      }

      var breakInSystem = Breaks.SingleOrDefault( break_ => break_.ID == id );
      if ( null != breakInSystem ) {
        return breakInSystem;
      }

      return null;
    }
  }
}
