using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.VerticalHeatExchanger)]
  public class VerticalHeatExchanger : Equipment
  {
    // VER. HEのノズル種別
    public enum NozzleKind
    {
      UP = 0,
      HZT1,
      HZS1,
      HZS2,
      HZT2,
      DN,
    }

    private readonly Memento<double> height;
    private readonly Memento<double> diameter;

    public VerticalHeatExchanger( Document document ) : base( document )
    {
      EquipmentName = "VER. HE";

      height = CreateMementoAndSetupValueEvents(0.0);
      diameter = CreateMementoAndSetupValueEvents(0.0);
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as VerticalHeatExchanger;
      height.CopyFrom(entity.height.Value);
      diameter.CopyFrom(entity.diameter.Value);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Height", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Height
    {
      get
      {
        return height.Value;
      }

      set
      {
        height.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Diameter", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Diameter
    {
      get
      {
        return diameter.Value;
      }

      set
      {
        diameter.Value = value;
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZT1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistHZT1Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZT1);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZT1, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZT1);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZS1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public bool IsExistHZS1Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZS1);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZS1, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZS1);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZS2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public bool IsExistHZS2Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZS2);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZS2, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZS2);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZT2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
    public bool IsExistHZT2Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZT2);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZT2, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZT2);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
    public bool IsExistDNNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.DN);
      }
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

    public override Bounds GetBounds()
    {
      var centerOfBody = new Vector3(0.0f, 0.0f, (float)(0.5 * Height));
      var sizeOfBody = new Vector3((float)Diameter, (float)Diameter, (float)Height);
      return new Bounds(centerOfBody, sizeOfBody);
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
          case NozzleKind.DN:
            (nozzle, _) = AddNozzleAndConnectPoint((int)kind, length, diameter);
            break;
          case NozzleKind.HZT1:
          case NozzleKind.HZS1:
          case NozzleKind.HZS2:
          case NozzleKind.HZT2:
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
          return new Vector3d(0.0, 0.0, Height);
        case NozzleKind.DN:
          return new Vector3d(0.0, 0.0, 0.0);
        case NozzleKind.HZT1:
        case NozzleKind.HZS1:
        case NozzleKind.HZS2:
        case NozzleKind.HZT2:
          var nozzle = GetNozzle((int)kind) as NozzleOnCylinder;
          return new Vector3d(0.5 * Diameter * -Math.Cos(nozzle.Angle.Deg2Rad()), 0.5 * Diameter * Math.Sin(nozzle.Angle.Deg2Rad()), nozzle.Height);
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
        case NozzleKind.DN:
          return Vector3d.down;
        case NozzleKind.HZT1:
        case NozzleKind.HZS1:
        case NozzleKind.HZS2:
        case NozzleKind.HZT2:
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
