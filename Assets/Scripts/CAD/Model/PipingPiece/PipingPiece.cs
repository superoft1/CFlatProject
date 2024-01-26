using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;


namespace Chiyoda.CAD.Model
{
  [System.Serializable]
  public abstract class PipingPiece : Entity
  {
    private readonly MementoDictionary<int, ConnectPoint> _connectPoints;

    protected PipingPiece( Document document ) : base( document )
    {
      _connectPoints = new MementoDictionary<int, ConnectPoint>( this );
      _connectPoints.AfterNewlyItemChanged += ConnectPoints_AfterNewlyItemChanged;
      _connectPoints.AfterHistoricallyItemChanged += ConnectPoints_AfterHistoricallyItemChanged;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as PipingPiece;
      _connectPoints.CopyFrom( entity._connectPoints.Select( pair =>
      {
        var copy = new ConnectPoint( this, pair.Key ) ;
        copy.CopyFrom( pair.Value, storage ) ;
        copy.AfterNewlyValueChanged += ConnectPoint_AfterNewlyValueChanged;
        copy.AfterHistoricallyValueChanged += ConnectPoint_AfterHistoricallyValueChanged;
        return new KeyValuePair<int, ConnectPoint>( pair.Key, copy ) ;
      } ) ) ;
    }

    public virtual void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var diameterRange = DiameterFactory.GetDiameterRangeFromLine(LeafEdge.Line);
      var diffIndex = DiameterFactory.GetDiffIndexBetween( GetConnectPoint(connectPointNumber).Diameter.NpsMm, newDiameterNpsMm);
      foreach (var cp in ConnectPoints)
      {        
        cp.Diameter = diameterRange.FromIndex(diameterRange.GetIndex(cp.Diameter) + diffIndex);
      }
    }

    public virtual bool IsEndOfStream => false;

    public LeafEdge LeafEdge => Parent as LeafEdge ;

    private void ConnectPoints_AfterNewlyItemChanged( object sender, ItemChangedEventArgs<KeyValuePair<int, ConnectPoint>> e )
    {
      SetupItemEvents( e ) ;
      OnAfterNewlyValueChanged() ;
    }

    private void ConnectPoints_AfterHistoricallyItemChanged( object sender, ItemChangedEventArgs<KeyValuePair<int, ConnectPoint>> e )
    {
      SetupItemEvents( e ) ;
      OnAfterHistoricallyValueChanged() ;
    }

    private void SetupItemEvents(ItemChangedEventArgs<KeyValuePair<int, ConnectPoint>> e )
    {
      foreach ( var item in e.RemovedItems ) {
        item.Value.AfterNewlyValueChanged -= ConnectPoint_AfterNewlyValueChanged;
        item.Value.AfterHistoricallyValueChanged -= ConnectPoint_AfterHistoricallyValueChanged;
      }
      foreach ( var item in e.AddedItems ) {
        item.Value.AfterNewlyValueChanged += ConnectPoint_AfterNewlyValueChanged;
        item.Value.AfterHistoricallyValueChanged += ConnectPoint_AfterHistoricallyValueChanged;
      }
    }

    private void ConnectPoint_AfterNewlyValueChanged( object sender, EventArgs e )
    {
      OnAfterNewlyValueChanged() ;
    }

    private void ConnectPoint_AfterHistoricallyValueChanged( object sender, EventArgs e )
    {
      OnAfterHistoricallyValueChanged() ;
    }

    public IEnumerable<ConnectPoint> ConnectPoints => _connectPoints.Values ;
    public int ConnectPointCount => _connectPoints.Count ;

    public ConnectPoint AddNewConnectPoint( int connectPointNumber )
    {
      var cp = new ConnectPoint( this, connectPointNumber ) ;
      _connectPoints.Add( connectPointNumber, cp ) ;
      return cp ;
    }

    public bool RemoveConnectPoint( int connectPointNumber )
    {
      return _connectPoints.Remove( connectPointNumber ) ;
    }

    public ConnectPoint GetConnectPoint( int connectPointNumber )
    {
      return _connectPoints.TryGetValue( connectPointNumber, out var cp ) ? cp : null ;
    }

    public override Bounds? GetGlobalBounds()
    {
      if ( null == LeafEdge ) {
        return GetBounds() ;
      }
      else {
        return LeafEdge.GlobalCod.GlobalizeBounds( GetBounds() ) ;
      }
    }

    public virtual Bounds GetBounds()
    {
      throw new NotImplementedException();
    }

    public virtual ConnectPoint GetAntiPoleConnectPoint( int connectPointIndex )
    {
      if ( ! _connectPoints.ContainsKey( connectPointIndex ) ) throw new ArgumentOutOfRangeException( nameof( connectPointIndex ) ) ;
      
      var v = GetConnectPoint( connectPointIndex ).Vector ;
      foreach ( var c in ConnectPoints ) {
        if ( c.ConnectPointNumber == connectPointIndex ) continue ;

        var v2 = GetConnectPoint( c.ConnectPointNumber ).Vector ;
        if ( v.IsOppositeDirectionTo( v2 ) ) return c ;
      }

      return null ;
    }
  }
}
