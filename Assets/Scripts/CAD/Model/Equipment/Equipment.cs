using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  public abstract class Equipment : PipingPiece, INozzlePlacement
  {
    protected static double DefaultNozzleLength = 0.5 ;
    protected static double DefaultNozzleDiameter = 0.6 ;

    private readonly MementoDictionary<int, Nozzle> _nozzles ;
    private readonly Memento<string> _equipmentName ;
    private readonly Memento<string> _equipNo ;
    
    public event EventHandler NozzlePositionChanged ;

    public override bool IsEndOfStream => true ;

    protected Equipment( Document document ) : base( document )
    {
      _nozzles = new MementoDictionary<int, Nozzle>( this ) ;

      _equipmentName = new Memento<string>( this ) ;
      _equipNo = new Memento<string>( this ) ;
    }

    protected internal override void RegisterNonMementoMembersFromDefaultObjects()
    {
      base.RegisterNonMementoMembersFromDefaultObjects() ;

      _nozzles.AfterNewlyItemChanged += ( sender, e ) =>
      {
        SetupNozzleEvents( e, true ) ;
        OnNewlyChildrenChanged( e.Convert<IElement>( pair => pair.Value ) ) ;
        OnAfterNewlyValueChanged() ;
      } ;
      _nozzles.AfterHistoricallyItemChanged += ( sender, e ) =>
      {
        SetupNozzleEvents( e, false ) ;
        OnHistoricallyChildrenChanged( e.Convert<IElement>( pair => pair.Value ) ) ;
        OnAfterHistoricallyValueChanged() ;
      } ;
      foreach ( var nozzle in _nozzles.Values ) {
        nozzle.AfterNewlyValueChanged += Nozzle_AfterNewlyValueChanged ;
      }
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      var entity = another as Equipment ;
      _equipmentName.CopyFrom( entity._equipmentName.Value ) ;
      _equipNo.CopyFrom( entity._equipNo.Value ) ;
      _nozzles.CopyFrom( entity._nozzles.Select( pair => new KeyValuePair<int, Nozzle>( pair.Key, pair.Value.Clone( storage ) ) ) ) ;
      //不要なはず
      //foreach ( var nozzle in _nozzles.Values ) {
      //  nozzle.AfterNewlyValueChanged += Nozzle_AfterNewlyValueChanged ;
      //}
    }

    private void SetupNozzleEvents( ItemChangedEventArgs<KeyValuePair<int, Nozzle>> e, bool callImmediately )
    {
      foreach ( var pair in e.AddedItems ) {
        pair.Value.AfterNewlyValueChanged += Nozzle_AfterNewlyValueChanged ;
        if ( callImmediately ) {
          OnNozzleChanged( pair.Value ) ;
        }
      }

      foreach ( var pair in e.RemovedItems ) {
        pair.Value.AfterNewlyValueChanged -= Nozzle_AfterNewlyValueChanged ;
      }
    }

    private void Nozzle_AfterNewlyValueChanged( object sender, EventArgs e )
    {
      OnNozzleChanged( (Nozzle) sender ) ;
    }

    [UI.Property( UI.PropertyCategory.EquipmentName, "ID", ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string EquipmentName
    {
      get => _equipmentName.Value ;
      set => _equipmentName.Value = value ;
    }

    public abstract Vector3d GetNozzleOriginPosition( Nozzle nozzle ) ;
    public abstract Vector3d GetNozzleDirection( Nozzle nozzle ) ;

    [UI.Property( UI.PropertyCategory.EquipNo, "EquipNo", ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string EquipNo
    {
      get => _equipNo.Value ;
      set => _equipNo.Value = value ;
    }

    public override IEnumerable<IElement> Children
    {
      get
      {
        foreach ( var nozzle in _nozzles.Values ) yield return nozzle ;

        foreach ( var child in base.Children ) {
          yield return child ;
        }
      }
    }
    
    protected (Nozzle, ConnectPoint) AddNozzleAndConnectPoint( int nozzleNumber, double length, Diameter diameter )
    {
      return AddNozzleAndConnectPoint<Nozzle>( nozzleNumber, length, diameter, null ) ;
    }

    protected (Nozzle, ConnectPoint) AddNozzleOnCylinder( int nozzleNumber, double length, Diameter diameter, double height, double angle )
    {
      return AddNozzleAndConnectPoint<NozzleOnCylinder>( nozzleNumber, length, diameter, nozzle =>
      {
        nozzle.Height = height ;
        nozzle.Angle = angle ;
      } ) ;
    }

    protected (Nozzle, ConnectPoint) AddNozzleWithDistanceFromBaseAndConnectPoint( int nozzleNumber, double length, Diameter diameter, double distanceFromBase )
    {
      return AddNozzleAndConnectPoint<NozzleWithDistanceFromBase>( nozzleNumber, length, diameter, nozzle => nozzle.DistanceFromBase = distanceFromBase ) ;
    }

    protected (Nozzle, ConnectPoint) AddNozzleOnPlane( int nozzleNumber, double length, Diameter diameter, double x, double y, Vector3d xAxis, Vector3d yAxis )
    {
      return AddNozzleAndConnectPoint<NozzleOnPlane>(nozzleNumber, length, diameter, nozzle =>
      {
        nozzle.X = x;
        nozzle.Y = y;
        nozzle.XAxis = xAxis;
        nozzle.YAxis = yAxis;
      } );
    }

    private (TNozzle, ConnectPoint) AddNozzleAndConnectPoint<TNozzle>( int nozzleNumber, double length, Diameter diameter, Action<TNozzle> action ) where TNozzle : Nozzle
    {
      if ( _nozzles.ContainsKey( nozzleNumber ) || null != GetConnectPoint( nozzleNumber ) ) {
        throw new InvalidOperationException() ;
      }

      var cp = AddNewConnectPoint( nozzleNumber ) ;
      cp.Diameter = diameter;

      var nozzle = Document.CreateEntity<TNozzle>() ;
      nozzle.NozzleNumber = nozzleNumber ;
      nozzle.Length = length ;
      nozzle.Diameter = diameter ;
      action?.Invoke( nozzle ) ;
      _nozzles.Add( nozzleNumber, nozzle ) ;

      return ( nozzle, cp ) ;
    }

    protected bool RemoveNozzleAndConnectPoint( int nozzleNumber )
    {
      if ( ! _nozzles.Remove( nozzleNumber ) ) return false ;
      return RemoveConnectPoint( nozzleNumber ) ;
    }

    public virtual IEnumerable<Nozzle> Nozzles => _nozzles.Values ;
    public int NozzleCount => _nozzles.Count ;

    public virtual Nozzle GetNozzle( int nozzleNumber )
    {
      if ( ! _nozzles.TryGetValue( nozzleNumber, out var nozzle ) ) return null ;

      return nozzle ;
    }

    public virtual bool ExistsNozzleNumber( int nozzleNumber )
    {
      return _nozzles.ContainsKey( nozzleNumber ) ;
    }

    public Nozzle FindNozzle( string nozzleName )
    {
      int? nzlIndex = GetConnectPointIndex( nozzleName ) ;
      if ( nzlIndex.HasValue ) {
        return _nozzles[ nzlIndex.Value ] ;
      }

      throw new InvalidOperationException( "FindNozzle failed." ) ;
    }

    internal ConnectPoint FindConnectPoint( string nozzleName )
    {
      int? nzlIndex = GetConnectPointIndex( nozzleName ) ;
      if ( nzlIndex.HasValue ) {
        return this.GetConnectPoint( nzlIndex.Value ) ;
      }

      throw new InvalidOperationException( "FindConnectPoint failed." ) ;
    }

    public int? GetConnectPointIndex( string nozzleName )
    {
      foreach ( var pair in _nozzles ) {
        if ( pair.Value.Name == nozzleName ) return pair.Key ;
      }

      return null ;
    }

    protected void OnNozzleChanged( Nozzle nozzle )
    {
      var nozzleNumber = nozzle.NozzleNumber ;

      var connectPoint = GetConnectPoint( nozzleNumber ) ;
      connectPoint.SetPointVector( GetNozzleOriginPosition( nozzle ) + GetNozzleDirection( nozzle ) * nozzle.Length, GetNozzleDirection( nozzle )) ;
      connectPoint.Diameter = nozzle.Diameter;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      cp.Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
      var nozzle = GetNozzle(connectPointNumber);
      nozzle.Diameter = cp.Diameter;
      cp.SetPointVector( GetNozzleOriginPosition(nozzle) + GetNozzleDirection(nozzle) * nozzle.Length, GetNozzleDirection( nozzle ));
      // 他のConnectPointには伝播しない
    }
  }
}
