using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.SkirtTypeVessel)]
  public class SkirtTypeVessel : Equipment
  {
    // Skirt Type Vesselのノズル種別
    public enum NozzleKind
    {
      UP = 0,
      DOWN,
      HZ1,
      HZ2,
      HZ3,
      HZ4,
      HZ5,
    }

    public SkirtTypeVessel( Document document ) : base( document )
    {
      EquipmentName = "SkirtTypeVessel";

      heightOfTower = CreateMementoAndSetupValueEvents(0.0);
      diameterOfTower = CreateMementoAndSetupValueEvents(0.0);
      heightOfSkirt = CreateMementoAndSetupValueEvents(0.0);
      isLowerCapFlat = CreateMementoAndSetupValueEvents(false);
    }

    private readonly Memento<double> heightOfTower;
    private readonly Memento<double> diameterOfTower;
    private readonly Memento<double> heightOfSkirt;
    private readonly Memento<bool> isLowerCapFlat;

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as SkirtTypeVessel;
      heightOfTower.CopyFrom(entity.heightOfTower.Value);
      diameterOfTower.CopyFrom(entity.diameterOfTower.Value);
      heightOfSkirt.CopyFrom(entity.heightOfSkirt.Value);
      isLowerCapFlat.CopyFrom(entity.isLowerCapFlat.Value);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfTower", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfTower
    {
      get
      {
        return heightOfTower.Value;
      }
      set
      {
        heightOfTower.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfTower", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfTower
    {
      get
      {
        return diameterOfTower.Value;
      }
      set
      {
        diameterOfTower.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfSkirt", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfSkirt
    {
      get
      {
        return heightOfSkirt.Value;
      }
      set
      {
        heightOfSkirt.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "IsLowerCapFlat", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable)]
    public bool IsLowerCapFlat
    {
      get
      {
        return isLowerCapFlat.Value;
      }
      set
      {
        isLowerCapFlat.Value = value;
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

    [UI.Property(UI.PropertyCategory.BaseData, "DOWN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistDOWNNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.DOWN);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.DOWN, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.DOWN);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZ1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZ2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZ3", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
    public bool IsExistHZ3Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZ3);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZ3, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZ3);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZ4", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
    public bool IsExistHZ4Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZ4);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZ4, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZ4);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZ5", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 7)]
    public bool IsExistHZ5Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZ5);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZ5, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZ5);
        }
      }
    }

    public double LengthOfUpperCap
    {
      get
      {
        return DiameterOfTower / 4.0;
      }
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
          return DiameterOfTower / 4.0;
        }
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfSkirt = new Vector3(0.0f, 0.0f, (float)(0.5 * HeightOfSkirt));
      var sizeOfSkirt = new Vector3((float)DiameterOfTower, (float)DiameterOfTower, (float)HeightOfSkirt);
      var skirtBounds = new Bounds(centerOfSkirt, sizeOfSkirt);

      var centerOfTower = new Vector3(0.0f, 0.0f, (float)(HeightOfSkirt + 0.5 * HeightOfTower));
      var sizeOfTower = new Vector3((float)DiameterOfTower, (float)DiameterOfTower, (float)HeightOfTower);
      var towerBounds = new Bounds(centerOfTower, sizeOfTower);
      skirtBounds.Encapsulate(towerBounds);

      var centerOfUpperCap = new Vector3(0.0f, 0.0f, (float)(HeightOfSkirt + HeightOfTower));
      var sizeOfUpperCap = new Vector3((float)DiameterOfTower, (float)DiameterOfTower, (float)(2.0 * LengthOfUpperCap));
      var upperCapBounds = new Bounds(centerOfUpperCap, sizeOfUpperCap);
      skirtBounds.Encapsulate(upperCapBounds);

      var centerOfLowerCap = new Vector3(0.0f, 0.0f, (float)HeightOfSkirt);
      var sizeOfLowerCap = new Vector3((float)DiameterOfTower, (float)DiameterOfTower, (float)(2.0 * LengthOfLowerCap));
      var lowerCapBounds = new Bounds(centerOfLowerCap, sizeOfLowerCap);
      skirtBounds.Encapsulate(lowerCapBounds);

      return skirtBounds;
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
          case NozzleKind.DOWN:
            (nozzle, _) = AddNozzleAndConnectPoint((int)kind, length, diameter);
            break;
          case NozzleKind.HZ1:
          case NozzleKind.HZ2:
          case NozzleKind.HZ3:
          case NozzleKind.HZ4:
          case NozzleKind.HZ5:
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
          return new Vector3d(0.0, 0.0, HeightOfSkirt + HeightOfTower + LengthOfUpperCap);
        case NozzleKind.DOWN:
          return new Vector3d(0.0, 0.0, HeightOfSkirt - LengthOfLowerCap);
        case NozzleKind.HZ1:
        case NozzleKind.HZ2:
        case NozzleKind.HZ3:
        case NozzleKind.HZ4:
        case NozzleKind.HZ5:
          var nozzle = GetNozzle((int)kind) as NozzleOnCylinder;
          return new Vector3d(0.5 * DiameterOfTower * -Math.Cos(nozzle.Angle.Deg2Rad()), 0.5 * DiameterOfTower * Math.Sin(nozzle.Angle.Deg2Rad()), nozzle.Height);
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
        case NozzleKind.DOWN:
          return Vector3d.back;
        case NozzleKind.HZ1:
        case NozzleKind.HZ2:
        case NozzleKind.HZ3:
        case NozzleKind.HZ4:
        case NozzleKind.HZ5:
          var nozzle = GetNozzle((int)kind) as NozzleOnCylinder;
          return new Vector3d(-Math.Cos(nozzle.Angle.Deg2Rad()), Math.Sin(nozzle.Angle.Deg2Rad()), 0.0);
        default:
          throw new ArgumentException();
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
