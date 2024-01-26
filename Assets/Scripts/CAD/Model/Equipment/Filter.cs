  using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.Filter)]
  public class Filter : Equipment
  {
    // ノズル設置面
    public enum Placement
    {
      FilterSide = 0,
      FilterBottom,
      FilterTop,
    }

    // Filterのノズル種別
    // TODO: 現時点では適当な名前
    public enum NozzleKind {
      SUC,
      SUC2,
      SUC3,
      SUC4,
      DIS,
      DIS2,
      DIS3,
      DIS4,
    }

    public Filter( Document document ) : base( document )
    {
      EquipmentName = "Filter";

      diameterOfFilter = CreateMementoAndSetupValueEvents(0.0);
      heightOfFilter = CreateMementoAndSetupValueEvents(0.0);
      diameterOfFlange = CreateMementoAndSetupValueEvents(0.0);
      heightOfFlange = CreateMementoAndSetupValueEvents(0.0);
      heightOfLeg = CreateMementoAndSetupValueEvents(0.0);
      legThickness = CreateMementoAndSetupValueEvents(0.0);
      lengthOfCap = CreateMementoAndSetupValueEvents(0.0);

      placementMap = new MementoDictionary<NozzleKind, Placement>(this);
    }

    private readonly Memento<double> diameterOfFilter;
    private readonly Memento<double> heightOfFilter;
    private readonly Memento<double> diameterOfFlange;
    private readonly Memento<double> heightOfFlange;
    private readonly Memento<double> heightOfLeg;
    private readonly Memento<double> legThickness;
    private readonly Memento<double> lengthOfCap;

    private readonly MementoDictionary<NozzleKind, Placement> placementMap;

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as Filter;
      diameterOfFilter.CopyFrom(entity.diameterOfFilter.Value);
      heightOfFilter.CopyFrom(entity.heightOfFilter.Value);
      diameterOfFlange.CopyFrom(entity.diameterOfFlange.Value);
      heightOfFlange.CopyFrom(entity.heightOfFlange.Value);
      heightOfLeg.CopyFrom(entity.heightOfLeg.Value);
      legThickness.CopyFrom(entity.legThickness.Value);
      lengthOfCap.CopyFrom(entity.lengthOfCap.Value);

      placementMap.CopyFrom(entity.placementMap);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfFilter", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public double DiameterOfFilter
    {
      get => diameterOfFilter.Value;
      set => diameterOfFilter.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfFilter", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public double HeightOfCylinder
    {
      get => heightOfFilter.Value;
      set => heightOfFilter.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfFlange", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public double DiameterOfFlange
    {
      get => diameterOfFlange.Value;
      set => diameterOfFlange.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfFlange", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public double HeightOfFlange
    {
      get => heightOfFlange.Value;
      set => heightOfFlange.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfLeg", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
    public double HeightOfLeg
    {
      get => heightOfLeg.Value;
      set => heightOfLeg.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LegThickness", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
    public double LegThickness
    {
      get => legThickness.Value;
      set => legThickness.Value = value;
    }

    public double WidthOfLegs => LegThickness;

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfCap", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 7)]
    public double LengthOfCap
    {
      get => lengthOfCap.Value;
      set => lengthOfCap.Value = value;
    }

    public override Bounds GetBounds()
    {
      var xySize = (float)Math.Max(DiameterOfFilter + 2.0 * LegThickness, DiameterOfFlange);
      var totalHeight = HeightOfLeg + HeightOfCylinder + HeightOfFlange;
      var sizeOfBounds = new Vector3(xySize, xySize, (float)totalHeight);
      var centerOfBounds = new Vector3(0.0f, 0.0f, (float)(0.5 * totalHeight));
      return new Bounds(centerOfBounds, sizeOfBounds);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DIS", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 8)]
    public bool IsExistDISNozzle
    {
      get => ExistsNozzle(NozzleKind.DIS);
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.DIS, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.DIS);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "SUC", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 9)]
    public bool IsExistSUCNozzle
    {
      get => ExistsNozzle(NozzleKind.SUC);
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.SUC, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.SUC);
        }
      }
    }

    public override Vector3d GetNozzleDirection(Nozzle nozzle)
    {
      var kind = (NozzleKind)nozzle.NozzleNumber;
      switch (placementMap[kind])
      {
        case Placement.FilterSide:
          var noz = nozzle as NozzleOnCylinder;
          return new Vector3d(-Math.Cos(noz.Angle.Deg2Rad()), Math.Sin(noz.Angle.Deg2Rad()), 0.0);
        case Placement.FilterBottom:
          return Vector3d.back;
        case Placement.FilterTop:
          return Vector3d.forward;
        default:
          throw new ArgumentException();
      }
    }

    public override Vector3d GetNozzleOriginPosition(Nozzle nozzle)
    {
      var kind = (NozzleKind)nozzle.NozzleNumber;
      switch (placementMap[kind])
      {
        case Placement.FilterSide:
          {
            var noz = nozzle as NozzleOnCylinder;
            return new Vector3d(0.5 * DiameterOfFilter * -Math.Cos(noz.Angle.Deg2Rad()), 0.5 * DiameterOfFilter * Math.Sin(noz.Angle.Deg2Rad()), noz.Height);
          }
        case Placement.FilterBottom:
          return new Vector3d(0.0, 0.0, HeightOfLeg - LengthOfCap);
        case Placement.FilterTop:
          {
            var noz = nozzle as NozzleOnPlane;
            return new Vector3d(noz.X, noz.Y, HeightOfLeg + HeightOfCylinder + HeightOfFlange);
          }
        default:
          throw new ArgumentException();
      }
    }

    public Nozzle AddNozzle(NozzleKind kind, double length, Diameter diameter, Placement placement = Placement.FilterBottom, double param1 = 0.0, double param2 = 0.0)
    {
      Nozzle nozzle = GetNozzle((int)kind);
      if (null == nozzle)
      {
        placementMap[kind] = placement;
        switch (placement)
        {
          case Placement.FilterSide:
            (nozzle, _) = AddNozzleOnCylinder((int)kind, length, diameter, param1/1000.0, param2);
            break;
          case Placement.FilterBottom:
            (nozzle, _) = AddNozzleAndConnectPoint((int)kind, length, diameter);
            break;
          case Placement.FilterTop:
            (nozzle, _) = AddNozzleOnPlane((int)kind, length, diameter, param1 / 1000.0, param2 / 1000.0, Vector3d.left, Vector3d.up);
            break;
          default:
            throw new ArgumentException();
        }
      }

      return nozzle;
    }

    public void RemoveNozzle(NozzleKind kind)
    {
      placementMap.Remove(kind);
      RemoveNozzleAndConnectPoint((int)kind);
    }

    public bool ExistsNozzle(NozzleKind kind)
    {
      return ExistsNozzleNumber((int)kind);
    }
  }
}
