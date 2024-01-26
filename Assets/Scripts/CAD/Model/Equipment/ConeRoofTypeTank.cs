using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.ConeRoofTypeTank)]
  public class ConeRoofTypeTank : Equipment
  {
    // ConeRoofTypeTankのノズル種別
    public enum NozzleKind
    {
      UP = 0,
      HZ1,
      HZ2,
    }

    private readonly Memento<double> heightOfCylinder;
    private readonly Memento<double> diameterOfCylinder;

    public ConeRoofTypeTank( Document document ) : base( document )
    {
      EquipmentName = "ConeRoofTypeTank";

      heightOfCylinder = CreateMementoAndSetupValueEvents(0.0); ;
      diameterOfCylinder = CreateMementoAndSetupValueEvents(0.0); ;
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as ConeRoofTypeTank;
      heightOfCylinder.CopyFrom(entity.heightOfCylinder.Value);
      diameterOfCylinder.CopyFrom(entity.diameterOfCylinder.Value);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfCylinder", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfCylinder
    {
      get
      {
        return heightOfCylinder.Value;
      }
      set
      {
        heightOfCylinder.Value = value;
      }
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

    [UI.Property(UI.PropertyCategory.BaseData, "UP", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public bool IsExistUPNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.UP);
      }
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZ1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistHZ1Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZ1);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZ1, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZ1);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZ2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public bool IsExistHZ2Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZ2);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZ2, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZ2);
        }
      }
    }

    public double LengthOfUpperCap
    {
      get
      {
        return DiameterOfCylinder / 4.0;
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfCap = new Vector3(0.0f, 0.0f, (float)(HeightOfCylinder + 0.5 * LengthOfUpperCap));
      var sizeOfCap = new Vector3((float)DiameterOfCylinder, (float)DiameterOfCylinder, (float)LengthOfUpperCap);
      var capBounds = new Bounds(centerOfCap, sizeOfCap);

      var centerOfCylinder = new Vector3(0.0f, 0.0f, (float)(0.5 * HeightOfCylinder));
      var sizeOfCylinder = new Vector3((float)DiameterOfCylinder, (float)DiameterOfCylinder, (float)HeightOfCylinder);
      var cylinderBounds = new Bounds(centerOfCylinder, sizeOfCylinder);
      capBounds.Encapsulate(cylinderBounds);

      return capBounds;
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
          case NozzleKind.HZ1:
          case NozzleKind.HZ2:
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
          return new Vector3d(0.0, 0.0, HeightOfCylinder + LengthOfUpperCap);
        case NozzleKind.HZ1:
        case NozzleKind.HZ2:
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
        case NozzleKind.HZ1:
        case NozzleKind.HZ2:
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
