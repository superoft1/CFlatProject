using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  public abstract class HorizontalPump : Equipment
  {
    // ノズル設置面
    public enum PlacementPlane
    {
      ImpellerTop = 0,
      ImpellerEnd,
      ImpellerLeft,
      ImpellerRight,
    }

    // HOR. Pumpのノズル種別
    public enum NozzleKind
    {
      DIS = 0,
      SUC,
    }

    private readonly Memento<double> basePlateWidth; // D2に相当
    private readonly Memento<double> basePlaneLength; // D1に相当
    private readonly Memento<double> basePlateHeight; // D3に相当
    private readonly Memento<double> impellerWidth; // D8に相当
    private readonly Memento<double> impellerLength; // D5に相当
    private readonly Memento<double> impellerHeight; // D7に相当
    private readonly Memento<double> impellerOffset; // D6に相当
    private readonly Memento<double> motorWidth; // D12に相当
    private readonly Memento<double> motorLength; // D11に相当
    private readonly Memento<double> motorHeight; // D7に相当
    private readonly Memento<double> driverWidth; // D9に相当
    private readonly Memento<double> driverLength; // D10に相当
    private readonly Memento<double> driverAxisHeight; // D4に相当
    private readonly Memento<double> equipMargin; // D13に相当

    private readonly MementoDictionary<NozzleKind,PlacementPlane> placementMap;
    private readonly MementoList<NozzleKind> nozzleKindList;

    public HorizontalPump( Document document ) : base( document )
    {
      EquipmentName = "HOR. Pump";

      basePlateWidth = CreateMementoAndSetupValueEvents(0.0);
      basePlaneLength = CreateMementoAndSetupValueEvents(0.0);
      basePlateHeight = CreateMementoAndSetupValueEvents(0.0);
      impellerWidth = CreateMementoAndSetupValueEvents(0.0);
      impellerLength = CreateMementoAndSetupValueEvents(0.0);
      impellerHeight = CreateMementoAndSetupValueEvents(0.0);
      impellerOffset = CreateMementoAndSetupValueEvents(0.0);
      motorWidth = CreateMementoAndSetupValueEvents(0.0);
      motorLength = CreateMementoAndSetupValueEvents(0.0);
      motorHeight = CreateMementoAndSetupValueEvents(0.0);
      driverWidth = CreateMementoAndSetupValueEvents(0.0);
      driverLength = CreateMementoAndSetupValueEvents(0.0);
      driverAxisHeight = CreateMementoAndSetupValueEvents(0.0);
      equipMargin = CreateMementoAndSetupValueEvents(0.0);

      placementMap = new MementoDictionary<NozzleKind, PlacementPlane>(this);
      nozzleKindList = new MementoList<NozzleKind>( this ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as HorizontalPump;
      basePlateWidth.CopyFrom( entity.basePlateWidth.Value );
      basePlaneLength.CopyFrom( entity.basePlaneLength.Value );
      basePlateHeight.CopyFrom( entity.basePlateHeight.Value );
      impellerWidth.CopyFrom( entity.impellerWidth.Value );
      impellerLength.CopyFrom( entity.impellerLength.Value );
      impellerHeight.CopyFrom( entity.impellerHeight.Value );
      impellerOffset.CopyFrom( entity.impellerOffset.Value );
      motorWidth.CopyFrom( entity.motorWidth.Value );
      motorLength.CopyFrom( entity.motorLength.Value );
      motorHeight.CopyFrom( entity.motorHeight.Value );
      driverWidth.CopyFrom( entity.driverWidth.Value );
      driverLength.CopyFrom( entity.driverLength.Value );
      driverAxisHeight.CopyFrom( entity.driverAxisHeight.Value );
      equipMargin.CopyFrom( entity.equipMargin.Value );

      placementMap.CopyFrom(entity.placementMap);
      nozzleKindList.CopyFrom(entity.nozzleKindList);
    }

    public override Vector3d GetNozzleDirection(Nozzle nozzle)
    {
      var kind = (NozzleKind)nozzle.NozzleNumber;
      return GetNozzleDirectionOn(placementMap[kind]);
    }

    public override Vector3d GetNozzleOriginPosition(Nozzle nozzle)
    {
      var nozzleOnPlane = nozzle as NozzleOnPlane;
      var kind = (NozzleKind)nozzle.NozzleNumber;
      var refPoint = GetReferencePointOn(placementMap[kind]);
      return refPoint + nozzleOnPlane.Offset;
    }

    private Vector3d GetReferencePointOn(PlacementPlane plane)
    {
      switch (plane)
      {
        case PlacementPlane.ImpellerTop:
          return new Vector3d(0.0, 0.5 * ImpellerLength - ImpellerOffset, BasePlateHeight + ImpellerHeight);
        case PlacementPlane.ImpellerEnd:
          return new Vector3d(0.0, -ImpellerOffset, BasePlateHeight + 0.5 * ImpellerHeight);
        case PlacementPlane.ImpellerLeft:
          return new Vector3d(-0.5 * ImpellerWidth, 0.5 * ImpellerLength - ImpellerOffset, BasePlateHeight + 0.5 * ImpellerHeight);
        case PlacementPlane.ImpellerRight:
          return new Vector3d(0.5 * ImpellerWidth, 0.5 * ImpellerLength - ImpellerOffset, BasePlateHeight + 0.5 * ImpellerHeight);
        default:
          throw new ArgumentException();
      }
    }

    private Vector3d GetNozzleDirectionOn(PlacementPlane plane)
    {
      switch (plane)
      {
        case PlacementPlane.ImpellerTop:
          return Vector3d.forward;
        case PlacementPlane.ImpellerEnd:
          return Vector3d.down;
        case PlacementPlane.ImpellerLeft:
          return Vector3d.left;
        case PlacementPlane.ImpellerRight:
          return Vector3d.right;
        default:
          throw new ArgumentException();
      }
    }

    private (Vector3d, Vector3d) GetAxesOn(PlacementPlane plane)
    {
      switch (plane)
      {
        case PlacementPlane.ImpellerTop:
          return (Vector3d.left, Vector3d.up);
        case PlacementPlane.ImpellerEnd:
          return (Vector3d.left, Vector3d.forward);
        case PlacementPlane.ImpellerLeft:
          return (Vector3d.up, Vector3d.forward);
        case PlacementPlane.ImpellerRight:
          return (Vector3d.up, Vector3d.forward);
        default:
          throw new ArgumentException();
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "BasePlateWidth", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public double BasePlateWidth { get => basePlateWidth.Value; set => basePlateWidth.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "BasePlateLength", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public double BasePlateLength { get => basePlaneLength.Value; set => basePlaneLength.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "BasePlateHeight", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public double BasePlateHeight { get => basePlateHeight.Value; set => basePlateHeight.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "ImpellerWidth", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public double ImpellerWidth { get => impellerWidth.Value; set => impellerWidth.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "ImpellerLength", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
    public double ImpellerLength { get => impellerLength.Value; set => impellerLength.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "ImpellerHeight", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
    public double ImpellerHeight { get => impellerHeight.Value; set => impellerHeight.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "ImpellerOffset", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 7)]
    public double ImpellerOffset { get => impellerOffset.Value; set => impellerOffset.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "MotorWidth", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 8)]
    public double MotorWidth { get => motorWidth.Value; set => motorWidth.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "MotorLength", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 9)]
    public double MotorLength { get => motorLength.Value; set => motorLength.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "MotorHeight", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 10)]
    public double MotorHeight { get => motorHeight.Value; set => motorHeight.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "DriverWidth", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 11)]
    public double DriverWidth { get => driverWidth.Value; set => driverWidth.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "DriverLength", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 12)]
    public double DriverLength { get => driverLength.Value; set => driverLength.Value = value; }
    [UI.Property(UI.PropertyCategory.BaseData, "DriverAxisHeight", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 13)]
    public double DriverAxisHeight { get => driverAxisHeight.Value; set => driverAxisHeight.Value = value; }
    public double EquipMargin => 0.5 * (BasePlateLength - ImpellerLength - DriverLength - MotorLength);

    public Vector3d BasePlateLocalCenter => new Vector3d(0.0, 0.5 * BasePlateLength - ImpellerOffset - EquipMargin, 0.5 * BasePlateHeight);
    public Vector3d BasePlateLocalSize => new Vector3d(BasePlateWidth, BasePlateLength, BasePlateHeight);

    public Vector3d ImpellerLocalCenter => new Vector3d(0.0, 0.5 * ImpellerLength - ImpellerOffset, BasePlateHeight + 0.5 * ImpellerHeight);
    public Vector3d ImpellerLocalSize => new Vector3d(ImpellerWidth, ImpellerLength, ImpellerHeight);

    public Vector3d MotorLocalCenter => new Vector3d(0.0, -ImpellerOffset + ImpellerLength + DriverLength + 0.5 * MotorLength, BasePlateHeight + 0.5 * MotorHeight);
    public Vector3d MotorLocalSize => new Vector3d(MotorWidth, MotorLength, MotorHeight);

    public Vector3d DriverLocalCenter => new Vector3d(0.0, -ImpellerOffset + ImpellerLength + 0.5 * DriverLength, DriverAxisHeight);
    public Vector3d DriverLocalSize => new Vector3d(DriverWidth, DriverLength, DriverWidth);

    [UI.Property(UI.PropertyCategory.BaseData, "DIS", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 14)]
    public bool IsExistDISNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.DIS);
      }
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

    [UI.Property(UI.PropertyCategory.BaseData, "SUC", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 15)]
    public bool IsExistSUCNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.SUC);
      }
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

    public override Bounds GetBounds()
    {
      var foundationBounds = new Bounds((Vector3)BasePlateLocalCenter, (Vector3)BasePlateLocalSize);
      var impellerBounds = new Bounds((Vector3)ImpellerLocalCenter, (Vector3)ImpellerLocalSize);
      var motorBounds = new Bounds((Vector3)MotorLocalCenter, (Vector3)MotorLocalSize);
      var driverBounds = new Bounds((Vector3)DriverLocalCenter, (Vector3)DriverLocalSize);
      foundationBounds.Encapsulate(impellerBounds);
      foundationBounds.Encapsulate(motorBounds);
      foundationBounds.Encapsulate(driverBounds);
      return foundationBounds;
    }

    public Nozzle AddNozzle( NozzleKind kind, double length, Diameter diameter, PlacementPlane plane = PlacementPlane.ImpellerTop, double xFromReferencePoint = 0.0, double yFromReferencePoint = 0.0 )
    {
      Nozzle nozzle = GetNozzle( (int) kind ) ;
      if ( null == nozzle ) {
        placementMap[kind] = plane;
        (var xAxis, var yAxis) = GetAxesOn(plane);
        ( nozzle, _ ) = AddNozzleOnPlane( (int) kind, length, diameter, xFromReferencePoint, yFromReferencePoint, xAxis, yAxis ) ;
      }

      return nozzle ;
    }

    public void RemoveNozzle( NozzleKind kind )
    {
      placementMap.Remove(kind);
      RemoveNozzleAndConnectPoint( (int) kind ) ;
    }

    public bool ExistsNozzle(NozzleKind kind)
    {
      return ExistsNozzleNumber((int)kind);
    }
  }
}
