using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  [Entity( EntityType.Type.HydraulicStream )]
  public partial class HydraulicStream : Entity
  {
    private readonly Memento<string> _streamName;
    private readonly MementoList<HydraulicStream> _fromSideNeighborStreams;
    private readonly MementoList<HydraulicStream> _toSideNeighborStreams;
    private readonly Memento<HalfVertex> _promoterVertex;
    private readonly Memento<HydraulicStreamEdgeList> _edgeList ;

    public HydraulicStream( Document document ) : base( document )
    {
      _streamName = new Memento<string>( this, "" );

      _promoterVertex = new Memento<HalfVertex>( this, null );
      _promoterVertex.AfterNewlyValueChanged += ( sender, e ) => ClearLeafEdgesCache();
      
      _fromSideNeighborStreams = new MementoList<HydraulicStream>( this );
      _fromSideNeighborStreams.AfterNewlyItemChanged += ( sender, e ) => ClearLeafEdgesCache();
      _toSideNeighborStreams = new MementoList<HydraulicStream>( this );
      _toSideNeighborStreams.AfterNewlyItemChanged += ( sender, e ) => ClearLeafEdgesCache();

      _edgeList = new Memento<HydraulicStreamEdgeList>( this ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var stream = another as HydraulicStream;
      _streamName.CopyFrom( stream._streamName.Value );

      _fromSideNeighborStreams.SetCopyObjectOrCloneFrom( stream._fromSideNeighborStreams, storage );
      _toSideNeighborStreams.SetCopyObjectOrCloneFrom( stream._toSideNeighborStreams, storage );
      _promoterVertex.CopyFrom( stream._promoterVertex.Value.GetCopyObjectOrClone( storage ) ) ;

      _edgeList.Value = null;
    }

    [UI.Property( UI.PropertyCategory.StreamName, "ID", Visibility = UI.PropertyVisibility.ReadOnly )]
    public string StreamName { get => _streamName.Value; set => _streamName.Value = value; }

    public HalfVertex PromoterVertex
    {
      get => _promoterVertex.Value;
      set => _promoterVertex.Value = value;
    }

    /// <summary>
    /// 流入側に接続する全ストリーム (始点から流出する平行ストリームも含む)
    /// </summary>
    public ICollection<HydraulicStream> FromSideNeighborStreams => _fromSideNeighborStreams;
    /// <summary>
    /// 流出側に接続する全ストリーム (終点へ流出する平行ストリームも含む)
    /// </summary>
    public ICollection<HydraulicStream> ToSideNeighborStreams => _toSideNeighborStreams;

    public IEnumerable<HydraulicStream> NeighborStreams => _toSideNeighborStreams.Concat( _fromSideNeighborStreams );

    public IEnumerable<LeafEdge> LeafEdges
    {
      get
      {
        CollectEdges();
        return _edgeList.Value?.AllLeafEdges;
      }
    }

    private void CollectEdges()
    {
      if ( null == _edgeList.Value ) {
        HydraulicStreamEdgeList.Create( this );
      }
    }
    
    public override Bounds? GetGlobalBounds()
    {
      return LeafEdges.Select( le => le.GetGlobalBounds() ).UnionBounds();
    }

    public void RefreshLeafEdgesCache()
    {
      ClearLeafEdgesCache();
      CollectEdges();
    }

    private void ClearLeafEdgesCache()
    {
      if ( null == _edgeList.Value ) return;

      _edgeList.Value.AllLeafEdges.SelectMany( le => le.Vertices ).Where( v => this == v.HydraulicStream ).ForEach( v => v.ResetHydraulicStream() );

      _edgeList.Value = null;
    }
  }
}
