using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model
{
  class ConnectPointCollection : IList<ConnectPoint>
  {
    private readonly MementoList<ConnectPoint> _connectPoints;

    public event EventHandler<ItemChangedEventArgs<ConnectPoint>> AfterNewlyItemChanged
    {
      add => _connectPoints.AfterNewlyItemChanged += value;
      remove => _connectPoints.AfterNewlyItemChanged -= value;
    }
    public event EventHandler<ItemChangedEventArgs<ConnectPoint>> AfterHistoricallyItemChanged
    {
      add => _connectPoints.AfterHistoricallyItemChanged += value;
      remove => _connectPoints.AfterHistoricallyItemChanged -= value;
    }

    public ConnectPointCollection( PipingPiece pipingPiece )
    {
      _connectPoints = new MementoList<ConnectPoint>( pipingPiece );
    }

    public ConnectPoint this[int index]
    {
      get => _connectPoints[index];
      set
      {
        for ( int i = 0, n = _connectPoints.Count ; i < n ; ++i ) {
          if ( (i != index) && HasSamePoint( _connectPoints[i], value ) ) throw new InvalidOperationException();
        }
        _connectPoints[index] = value;
      }
    }

    public void AddRange( IEnumerable<ConnectPoint> points )
    {
      ICollection<ConnectPoint> list = (points is ICollection<ConnectPoint> collection) ? collection : points.ToArray();
      foreach ( var item in list ) {
        foreach ( var cp in _connectPoints ) {
          if ( HasSamePoint( cp, item ) ) throw new InvalidOperationException();
        }
      }
      _connectPoints.AddRange( list );
    }

    public int Count => _connectPoints.Count;

    public bool IsReadOnly => _connectPoints.IsReadOnly;

    public void Add( ConnectPoint item )
    {
      foreach ( var cp in _connectPoints ) {
        if (HasSamePoint(cp, item))
        {
          throw new InvalidOperationException();
        }
      }

      _connectPoints.Add( item );
    }

    private static bool HasSamePoint( ConnectPoint cp1, ConnectPoint cp2 )
    {
      return UnityEngine.Vector3d.Distance( cp1.Point, cp2.Point ) < Tolerance.MergeToleranceForImporter;
    }

    public void Clear()
    {
      _connectPoints.Clear();
    }

    public bool Contains( ConnectPoint item )
    {
      return _connectPoints.Contains( item );
    }

    public void CopyTo( ConnectPoint[] array, int arrayIndex )
    {
      _connectPoints.CopyTo( array, arrayIndex );
    }

    public IEnumerator<ConnectPoint> GetEnumerator()
    {
      return _connectPoints.GetEnumerator();
    }

    public int IndexOf( ConnectPoint item )
    {
      return _connectPoints.IndexOf( item );
    }

    public void Insert( int index, ConnectPoint item )
    {
      _connectPoints.Insert( index, item );
    }

    public bool Remove( ConnectPoint item )
    {
      return _connectPoints.Remove( item );
    }

    public void RemoveAt( int index )
    {
      _connectPoints.RemoveAt( index );
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _connectPoints.GetEnumerator();
    }
  }
}
