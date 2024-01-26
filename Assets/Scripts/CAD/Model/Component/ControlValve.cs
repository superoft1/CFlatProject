using System;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;


namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.ControlValve )]
  public class ControlValve : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      Term1,
      Term2,
    }

    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;
    private readonly Memento<double> mainValveLength;
    private readonly Memento<double> diaphramLength;
    private readonly Memento<double> diaphramDiameter;

    public ControlValve( Document document ) : base( document )
    {
      mainValveLength = CreateMementoAndSetupValueEvents( 0.0 ) ;
      diaphramLength = CreateMementoAndSetupValueEvents( 0.0 ) ;
      diaphramDiameter = CreateMementoAndSetupValueEvents( 0.0 ) ;

      ComponentName = "DiaphramControlValve" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.Term1 );
      AddNewConnectPoint( (int) ConnectPointType.Term2 );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as ControlValve;
      mainValveLength.CopyFrom( entity.mainValveLength.Value );
      diaphramLength.CopyFrom( entity.diaphramLength.Value);
      diaphramDiameter.CopyFrom( entity.diaphramDiameter.Value);
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      var beforeDiameter = cp.Diameter.OutsideMeter;
      var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
      var rate = afterDiameter / beforeDiameter;
      Length *= rate;
      DiaphramLength *= rate;
      DiaphramDiameter *= rate;
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    public double Diameter
    {
      get { return Term1ConnectPoint.Diameter.OutsideMeter ; }

      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.Diameter = DiameterFactory.FromOutsideMeter( value ); ;
        term2.Diameter = term1.Diameter ;
      }
    }

    public double Length
    {
      get { return mainValveLength.Value ; }
      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.SetPointVector( 0.5 * value * Axis) ;
        term2.SetPointVector( -0.5 * value * Axis) ;

        mainValveLength.Value = value ;
      }
    }

    public double DiaphramLength
    {
      get { return diaphramLength.Value; }
      set { diaphramLength.Value = value ; }
    }
    
    public double DiaphramDiameter
    {
      get { return diaphramDiameter.Value; }
      set { diaphramDiameter.Value = value ; }
    }
    
    public override Bounds GetBounds()
    {
      var bounds = new Bounds((Vector3)Origin, Vector3.zero);
      bounds.Encapsulate( (Vector3)Term1ConnectPoint.Point ) ;
      bounds.Encapsulate( (Vector3)Term2ConnectPoint.Point ) ;
      bounds.Encapsulate( (Vector3) ( SecondAxis * DiaphramLength ) ) ;
      
      var flowRadius = Diameter / 2 ;
      bounds.Encapsulate( (Vector3) (SecondAxis * flowRadius) );
      bounds.Encapsulate( (Vector3) (-SecondAxis * flowRadius) );
      bounds.Encapsulate( (Vector3) (ThirdAxis * flowRadius) );
      bounds.Encapsulate( (Vector3) (-ThirdAxis * flowRadius) );
            
      var diaphramRadius = DiaphramDiameter / 2 ;
      bounds.Encapsulate( (Vector3) (Axis * diaphramRadius) );
      bounds.Encapsulate( (Vector3) (-Axis * diaphramRadius) );
      bounds.Encapsulate( (Vector3) (ThirdAxis * diaphramRadius) );
      bounds.Encapsulate( (Vector3) (-ThirdAxis * diaphramRadius) );
      return bounds ;
    }
  }
}
