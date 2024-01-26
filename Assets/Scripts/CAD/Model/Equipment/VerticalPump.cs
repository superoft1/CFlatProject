using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.VerticalPump )]
  public class VerticalPump : Equipment
  {
    // VER Pumpのノズル種別
    public enum NozzleKind
    {
      DIS = 0,
      SUC,
    }

    private readonly Memento<double> height ;
    private readonly Memento<double> diameter ;

    public VerticalPump( Document document ) : base( document )
    {
      EquipmentName = "VER. Pump" ;

      height = CreateMementoAndSetupValueEvents( 0.0 ) ;
      diameter = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      var entity = another as VerticalPump ;
      height.CopyFrom( entity.height.Value ) ;
      diameter.CopyFrom( entity.diameter.Value ) ;
    }

    [UI.Property( UI.PropertyCategory.BaseData, "Height", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable )]
    public double Height
    {
      get { return height.Value ; }

      set { height.Value = value ; }
    }

    [UI.Property( UI.PropertyCategory.BaseData, "Diameter", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable )]
    public double Diameter
    {
      get { return diameter.Value ; }

      set { diameter.Value = value ; }
    }

    [UI.Property( UI.PropertyCategory.BaseData, "DIS", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 1 )]
    public bool IsExistDISNozzle
    {
      get { return ExistsNozzle( NozzleKind.DIS ) ; }
      set
      {
        if ( value ) {
          AddNozzle( NozzleKind.DIS, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d)) ;
        }
        else {
          RemoveNozzle( NozzleKind.DIS ) ;
        }
      }
    }

    [UI.Property( UI.PropertyCategory.BaseData, "SUC", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2 )]
    public bool IsExistSUCNozzle
    {
      get { return ExistsNozzle( NozzleKind.SUC ) ; }
      set
      {
        if ( value ) {
          AddNozzle( NozzleKind.SUC, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d)) ;
        }
        else {
          RemoveNozzle( NozzleKind.SUC ) ;
        }
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfBody = new Vector3( 0.0f, 0.0f, (float) ( 0.5 * Height ) ) ;
      var sizeOfBody = new Vector3( (float) Diameter, (float) Diameter, (float) Height ) ;
      return new Bounds( centerOfBody, sizeOfBody ) ;
    }

    public Nozzle AddNozzle( NozzleKind kind, double length, Diameter diameter )
    {
      Nozzle nozzle = GetNozzle( (int) kind ) ;
      if ( null == nozzle ) {
        ( nozzle, _ ) = AddNozzleAndConnectPoint( (int) kind, length, diameter ) ;
      }

      return nozzle ;
    }

    public void RemoveNozzle( NozzleKind kind )
    {
      RemoveNozzleAndConnectPoint( (int) kind ) ;
    }

    public bool ExistsNozzle( NozzleKind kind )
    {
      return ExistsNozzleNumber( (int) kind ) ;
    }

    private Vector3d NozzlePositionOf( NozzleKind kind )
    {
      switch ( kind ) {
        case NozzleKind.DIS :
          return new Vector3d( -0.5 * Diameter, 0.0, 0.9 * Height ) ;
        case NozzleKind.SUC :
          return new Vector3d( 0.5 * Diameter, 0.0, 0.9 * Height ) ;
        default :
          throw new ArgumentException() ;
      }
    }

    private Vector3d NozzleDirectionOf( NozzleKind kind )
    {
      switch ( kind ) {
        case NozzleKind.DIS :
          return Vector3d.left ;
        case NozzleKind.SUC :
          return Vector3d.right ;
        default :
          throw new ArgumentException() ;
      }
    }

    public override Vector3d GetNozzleOriginPosition( Nozzle nozzle )
    {
      return NozzlePositionOf( (NozzleKind) nozzle.NozzleNumber ) ;
    }

    public override Vector3d GetNozzleDirection( Nozzle nozzle )
    {
      return NozzleDirectionOf( (NozzleKind) nozzle.NozzleNumber ) ;
    }
  }
}