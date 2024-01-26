using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.LegTypeVessel)]
  public class LegTypeVessel : Equipment
  {
    // LegTypeVesselのノズル種別
    public enum NozzleKind
    {
      UP = 0,
      HZ,
      DN,
    }

    public LegTypeVessel( Document document ) : base( document )
    {
      EquipmentName = "LegTypeVessel";

      diameterOfCylinder = CreateMementoAndSetupValueEvents(0.0);
      heightOfCylinder = CreateMementoAndSetupValueEvents(0.0);
      heightOfLeg = CreateMementoAndSetupValueEvents(0.0);
      legThickness = CreateMementoAndSetupValueEvents(0.0);
      isLowerCapFlat = CreateMementoAndSetupValueEvents(false);
    }

    private readonly Memento<double> diameterOfCylinder;
    private readonly Memento<double> heightOfCylinder;
    private readonly Memento<double> heightOfLeg;
    private readonly Memento<double> legThickness;
    private readonly Memento<bool> isLowerCapFlat;

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as LegTypeVessel;
      diameterOfCylinder.CopyFrom(entity.diameterOfCylinder.Value);
      heightOfCylinder.CopyFrom(entity.heightOfCylinder.Value);
      heightOfLeg.CopyFrom(entity.heightOfLeg.Value);
      legThickness.CopyFrom(entity.legThickness.Value);
      isLowerCapFlat.CopyFrom(entity.isLowerCapFlat.Value);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfCylinder", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfCylinder
    {
      get { return diameterOfCylinder.Value; }
      set { diameterOfCylinder.Value = value; }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfCylinder", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfCylinder
    {
      get { return heightOfCylinder.Value; }
      set { heightOfCylinder.Value = value; }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfLeg", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfLeg
    {
      get { return heightOfLeg.Value; }
      set { heightOfLeg.Value = value; }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LegThickness", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LegThickness
    {
      get { return legThickness.Value; }
      set { legThickness.Value = value; }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "IsLowerCapFlat", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable)]
    public bool IsLowerCapFlat
    {
      get { return isLowerCapFlat.Value; }
      set { isLowerCapFlat.Value = value; }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "UP", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public bool IsExistUPNozzle
    {
      get { return ExistsNozzle(NozzleKind.UP); }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.UP, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.UP);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZ", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistHZNozzle
    {
      get { return ExistsNozzle(NozzleKind.HZ); }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZ, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZ);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public bool IsExistDNNozzle
    {
      get { return ExistsNozzle(NozzleKind.DN); }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.DN, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.DN);
        }
      }
    }

    public double WidthOfLegs
    {
      get { return LegThickness; }
    }

    public double LengthOfUpperCap
    {
      get { return DiameterOfCylinder / 4.0; }
    }

    public double LengthOfLowerCap
    {
      get
      {
        if (IsLowerCapFlat)
        {
          return 0.0;
        }
        else
        {
          return DiameterOfCylinder / 4.0;
        }
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfCylinder = new Vector3(0.0f, 0.0f, (float)(HeightOfLeg + 0.5 * HeightOfCylinder));
      var sizeOfCylinder = new Vector3((float)DiameterOfCylinder, (float)DiameterOfCylinder, (float)HeightOfCylinder);
      var cylinderBounds = new Bounds(centerOfCylinder, sizeOfCylinder);

      var centerOfUpperCap = new Vector3(0.0f, 0.0f, (float)(HeightOfLeg + HeightOfCylinder));
      var sizeOfUpperCap = new Vector3((float)DiameterOfCylinder, (float)DiameterOfCylinder, (float)(2.0 * LengthOfUpperCap));
      var upperCapBounds = new Bounds(centerOfUpperCap, sizeOfUpperCap);
      cylinderBounds.Encapsulate(upperCapBounds);

      var centerOfLowerCap = new Vector3(0.0f, 0.0f, (float)HeightOfLeg);
      var sizeOfLowerCap = new Vector3((float)DiameterOfCylinder, (float)DiameterOfCylinder, (float)(2.0 * LengthOfLowerCap));
      var lowerCapBounds = new Bounds(centerOfLowerCap, sizeOfLowerCap);
      cylinderBounds.Encapsulate(lowerCapBounds);

      var centerOfLeg1 = new Vector3(0.0f, (float)(0.5 * DiameterOfCylinder), (float)(0.5 * HeightOfLeg));
      var sizeOfLeg1 = new Vector3((float)WidthOfLegs, (float)WidthOfLegs, (float)HeightOfLeg);
      var leg1Bounds = new Bounds(centerOfLeg1, sizeOfLeg1);
      cylinderBounds.Encapsulate(leg1Bounds);

      var centerOfLeg2 = new Vector3(0.0f, (float)(-0.5 * DiameterOfCylinder), (float)(0.5 * HeightOfLeg));
      var sizeOfLeg2 = new Vector3((float)WidthOfLegs, (float)WidthOfLegs, (float)HeightOfLeg);
      var leg2Bounds = new Bounds(centerOfLeg2, sizeOfLeg2);
      cylinderBounds.Encapsulate(leg2Bounds);

      var centerOfLeg3 = new Vector3((float)(0.5 * DiameterOfCylinder), 0.0f, (float)(0.5 * HeightOfLeg));
      var sizeOfLeg3 = new Vector3((float)WidthOfLegs, (float)WidthOfLegs, (float)HeightOfLeg);
      var leg3Bounds = new Bounds(centerOfLeg3, sizeOfLeg3);
      cylinderBounds.Encapsulate(leg3Bounds);

      var centerOfLeg4 = new Vector3((float)(-0.5 * DiameterOfCylinder), 0.0f, (float)(0.5 * HeightOfLeg));
      var sizeOfLeg4 = new Vector3((float)WidthOfLegs, (float)WidthOfLegs, (float)HeightOfLeg);
      var leg4Bounds = new Bounds(centerOfLeg4, sizeOfLeg4);
      cylinderBounds.Encapsulate(leg4Bounds);

      return cylinderBounds;
    }

    public Nozzle AddNozzle(NozzleKind kind, double length, Diameter diameter)
    {
      return AddNozzle(kind, length, diameter, 0.0, 0.0);
    }

    public Nozzle AddNozzle(NozzleKind kind, double length, Diameter diameter, double height, double angle)
    {
      Nozzle nozzle = GetNozzle((int)kind);
      if (null == nozzle)
      {
        switch (kind)
        {
          case NozzleKind.UP:
            (nozzle, _) = AddNozzleAndConnectPoint((int)kind, length, diameter);
            break;
          case NozzleKind.HZ:
          case NozzleKind.DN:
            (nozzle, _) = AddNozzleOnCylinder((int)kind, length, diameter, height, angle);
            break;
          default:
            throw new ArgumentException();
        }
      }

      return nozzle;
    }

    public void RemoveNozzle(NozzleKind kind)
    {
      RemoveNozzleAndConnectPoint((int)kind);
    }

    public bool ExistsNozzle(NozzleKind kind)
    {
      return ExistsNozzleNumber((int)kind);
    }

    private Vector3d NozzlePositionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.UP:
          return new Vector3d(0.0, 0.0, HeightOfLeg + HeightOfCylinder + LengthOfUpperCap);
        case NozzleKind.HZ:
        case NozzleKind.DN:
          var nozzle = GetNozzle((int)kind) as NozzleOnCylinder;
          return new Vector3d(0.5 * DiameterOfCylinder * -Math.Cos(nozzle.Angle.Deg2Rad()), 0.5 * DiameterOfCylinder * Math.Sin(nozzle.Angle.Deg2Rad()), nozzle.Height);
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
        case NozzleKind.DN:
          var nozzle = GetNozzle((int)kind) as NozzleOnCylinder;
          return new Vector3d(-Math.Cos(nozzle.Angle.Deg2Rad()), Math.Sin(nozzle.Angle.Deg2Rad()), 0.0);
        default:
          throw new InvalidOperationException();
      }
    }

    public override Vector3d GetNozzleOriginPosition(Nozzle nozzle)
    {
      return NozzlePositionOf((NozzleKind)nozzle.NozzleNumber);
    }

    public override Vector3d GetNozzleDirection(Nozzle nozzle)
    {
      return NozzleDirectionOf((NozzleKind)nozzle.NozzleNumber);
    }
  }
}
