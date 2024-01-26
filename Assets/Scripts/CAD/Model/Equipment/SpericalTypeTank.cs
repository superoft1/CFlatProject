using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.SphericalTypeTank)]
  public class SphericalTypeTank : Equipment
  {
    // Spherical Type Tankのノズル種別
    public enum NozzleKind
    {
      UP = 0,
      HZ,
      DN,
    }

    public SphericalTypeTank( Document document ) : base( document )
    {
      EquipmentName = "SpericalTypeTank";

      diameterOfCylinder = CreateMementoAndSetupValueEvents( 0.0 ) ;
      heightOfP1FromBase = CreateMementoAndSetupValueEvents( 0.0 ) ;
      legThickness = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    private readonly Memento<double> diameterOfCylinder;
    private readonly Memento<double> heightOfP1FromBase;
    private readonly Memento<double> legThickness;

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as SphericalTypeTank;
      diameterOfCylinder.CopyFrom( entity.diameterOfCylinder.Value );
      heightOfP1FromBase.CopyFrom( entity.heightOfP1FromBase.Value );
      legThickness.CopyFrom( entity.legThickness.Value );
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfCylinder", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfCylinder
    {
      get
      {
        return diameterOfCylinder.Value;
      }
      set
      {
        diameterOfCylinder.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfP1FromBase", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfP1FromBase
    {
      get
      {
        return heightOfP1FromBase.Value;
      }
      set
      {
        heightOfP1FromBase.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LegThickness", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LegThickness
    {
      get
      {
        return legThickness.Value;
      }
      set
      {
        legThickness.Value = value;
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfTank = new Vector3(0.0f, 0.0f, (float)HeightOfP1FromBase);
      var sizeOfTank = new Vector3((float)DiameterOfCylinder, (float)DiameterOfCylinder, (float)DiameterOfCylinder);
      var tankBounds = new Bounds(centerOfTank, sizeOfTank);

      var centerOfLeg1 = new Vector3(0.0f, (float)(0.5 * DiameterOfCylinder), (float)(0.5 * HeightOfP1FromBase));
      var sizeOfLeg1 = new Vector3((float)LegThickness, (float)LegThickness, (float)HeightOfP1FromBase);
      var leg1Bounds = new Bounds(centerOfLeg1, sizeOfLeg1);
      tankBounds.Encapsulate(leg1Bounds);

      var centerOfLeg2 = new Vector3(0.0f, (float)(-0.5 * DiameterOfCylinder), (float)(0.5 * HeightOfP1FromBase));
      var sizeOfLeg2 = new Vector3((float)LegThickness, (float)LegThickness, (float)HeightOfP1FromBase);
      var leg2Bounds = new Bounds(centerOfLeg2, sizeOfLeg2);
      tankBounds.Encapsulate(leg2Bounds);

      var centerOfLeg3 = new Vector3((float)(0.5 * DiameterOfCylinder), 0.0f, (float)(0.5 * HeightOfP1FromBase));
      var sizeOfLeg3 = new Vector3((float)LegThickness, (float)LegThickness, (float)HeightOfP1FromBase);
      var leg3Bounds = new Bounds(centerOfLeg3, sizeOfLeg3);
      tankBounds.Encapsulate(leg3Bounds);

      var centerOfLeg4 = new Vector3((float)(-0.5 * DiameterOfCylinder), 0.0f, (float)(0.5 * HeightOfP1FromBase));
      var sizeOfLeg4 = new Vector3((float)LegThickness, (float)LegThickness, (float)HeightOfP1FromBase);
      var leg4Bounds = new Bounds(centerOfLeg4, sizeOfLeg4);
      tankBounds.Encapsulate(leg4Bounds);

      return tankBounds;
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

    private Vector3d NozzlePositionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.UP:
          return new Vector3d(0.0, 0.0, HeightOfP1FromBase + 0.5 * DiameterOfCylinder);
        case NozzleKind.HZ:
          throw new NotImplementedException(); // TODO
        case NozzleKind.DN:
          throw new NotImplementedException(); // TODO
        default:
          throw new ArgumentException();
      }
    }

    private Vector3d NozzleDirectionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.UP:
          return Vector3d.forward;
        case NozzleKind.HZ:
          throw new NotImplementedException(); // TODO
        case NozzleKind.DN:
          throw new NotImplementedException(); // TODO
        default:
          throw new InvalidOperationException();
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
