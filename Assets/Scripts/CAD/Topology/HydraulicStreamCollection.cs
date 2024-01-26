using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  public class HydraulicStreamCollection : ICopyable, IBoundary, ICollection<HydraulicStream>
  {
    private readonly MementoList<HydraulicStream> _streams;

    public event EventHandler<ItemChangedEventArgs<HydraulicStream>> AfterNewlyItemChanged
    {
      add => _streams.AfterNewlyItemChanged += value;
      remove => _streams.AfterNewlyItemChanged -= value;
    }
    public event EventHandler<ItemChangedEventArgs<HydraulicStream>> AfterHistoricallyItemChanged
    {
      add => _streams.AfterHistoricallyItemChanged += value;
      remove => _streams.AfterHistoricallyItemChanged -= value;
    }

    public int Count => _streams.Count;

    public bool IsReadOnly => _streams.IsReadOnly;

    public HydraulicStreamCollection( Document document )
    {
      _streams = new MementoList<HydraulicStream>( document );
    }

    public void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var collection = another as HydraulicStreamCollection;

      _streams.SetCopyObjectOrCloneFrom( collection._streams, storage );
    }

    public void RefreshStreamEdges()
    {
      foreach ( var stream in _streams ) {
        stream.RefreshLeafEdgesCache();
      }
    }

    public Bounds? GetGlobalBounds()
    {
      return this.Select( stream => stream.GetGlobalBounds() ).UnionBounds();
    }

    public IEnumerator<HydraulicStream> GetEnumerator()
    {
      return _streams.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add( HydraulicStream item )
    {
      _streams.Add( item );
    }

    public void Clear()
    {
      _streams.Clear();
    }

    public bool Contains( HydraulicStream item )
    {
      return _streams.Contains( item );
    }

    public void CopyTo( HydraulicStream[] array, int arrayIndex )
    {
      _streams.CopyTo( array, arrayIndex );
    }

    public bool Remove( HydraulicStream item )
    {
      return _streams.Remove( item );
    }
  }
}
