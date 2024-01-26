using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager ;
using NpgsqlTypes ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{

  [Entity( EntityType.Type.AFCNozzleArray )]
  public class AFCNozzleArray : NozzleArray, INozzlePlacement
  {
    private readonly Memento<AirFinCooler.PlacementPlane> _placement ;
    private readonly Memento<double> _interval ;
    private readonly Memento<double> _margin ;
    
    public event EventHandler NozzlePositionChanged ;
    


    public AFCNozzleArray( Document document ) : base( document )
    {
      _placement = CreateMementoAndSetupValueEvents( AirFinCooler.PlacementPlane.UpForward ) ;
      _margin = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _interval = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }
    
    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as AFCNozzleArray;
      _placement.CopyFrom( entity._placement.Value );
      _margin.CopyFrom( entity._margin.Value );
      _interval.CopyFrom( entity._interval.Value );
    }
    
    internal virtual void OnNozzlePositionChanged( EventArgs e )
    {
      NozzlePositionChanged?.Invoke( this, e );
      CalculateConnectPoint();
    }

    public Vector3d GetNozzleOriginPosition( Nozzle nozzle )
    {
      var afc = this.Parent as AirFinCooler ;
      var codSys = afc.PlacementLocalCodeSys( _placement.Value ) ;
      return codSys.Origin + Margin * codSys.DirMargin + GetArrayVec( nozzle, codSys ) ;
    }

    Vector3d GetArrayVec(Nozzle nozzle, AirFinCooler.AFCNozzleCod codSys)
    {
      var index = Array.IndexOf( Nozzles.ToArray(), nozzle ) ;
      var centerIndex = ( NozzleCount % 2 == 1 ) ? (double) ( NozzleCount / 2 + 1 ) : (double) NozzleCount / 2.0 + 0.5 ;
      var vec = ((double)((index+1) - centerIndex) * Interval) * codSys.DirNozzleArray ;
      return vec ;
    }

    public Vector3d GetNozzleDirection( Nozzle nozzle )
    {
      var afc = this.Parent as AirFinCooler ;
      var codSys = afc.PlacementLocalCodeSys( _placement.Value ) ;
      return codSys.DirNozzle ;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Placement", ValueType = UI.ValueType.Select, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public AirFinCooler.PlacementPlane Placement
    {
      get { return _placement.Value ; }
      set
      {
        _placement.Value = value ;
        OnNozzlePositionChanged(EventArgs.Empty);
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Margin", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public double Margin
    {
      get { return _margin.Value ; }
      set
      {
        _margin.Value = value ;
        OnNozzlePositionChanged(EventArgs.Empty);
      }
    }
    
    [UI.Property(UI.PropertyCategory.BaseData, "Interval", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public double Interval
    {
      get { return _interval.Value ; }
      set
      {
        _interval.Value = value ;
        OnNozzlePositionChanged(EventArgs.Empty);
      }
    }
    
    [UI.Property(UI.PropertyCategory.BaseData, "NozzleCount", ValueType = UI.ValueType.GeneralInteger, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public override int NozzleCount
    {
      get
      {
        return base.NozzleCount ;
      }
      set
      {
        base.NozzleCount = value ;
        OnNozzlePositionChanged(EventArgs.Empty);
      }
    }

    void CalculateConnectPoint()
    {
      var parent = Parent as Equipment ;
      var nozzles = Nozzles.ToArray() ;
      for ( int i = 0 ; i < NozzleCount ; ++i ) {
        var nozzle = nozzles[ i ] ;
        var cp = parent.GetConnectPoint( InitialConnectPointIndex + i ) ;
        cp.SetPointVector( GetNozzleOriginPosition( nozzle ) + GetNozzleDirection( nozzle ) * nozzle.Length, GetNozzleDirection(nozzle) );
        cp.Diameter = nozzle.Diameter ;
      }
    }
  }

}
