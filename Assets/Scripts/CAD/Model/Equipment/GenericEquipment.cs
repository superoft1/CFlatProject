using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.GenericEquipment)]
  public class GenericEquipment : Equipment
  {
    // GenericEquipmentのノズル種別
    public enum NozzleKind
    {
      UP = 0,
      HZ1,
      HZ2,
      HZ3,
      HZ4,
      DOWN
    }

    private readonly Memento<double> lengthOfEquipment;
    private readonly Memento<double> widthOfEquipment;
    private readonly Memento<double> heightOfEquipment;

    public GenericEquipment( Document document ) : base( document )
    {
      EquipmentName = "Generic Equipment";

      lengthOfEquipment = CreateMementoAndSetupValueEvents( 0.0 ) ;
      widthOfEquipment = CreateMementoAndSetupValueEvents( 0.0 ) ;
      heightOfEquipment = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as GenericEquipment;
      lengthOfEquipment.CopyFrom( entity.lengthOfEquipment.Value );
      widthOfEquipment.CopyFrom( entity.widthOfEquipment.Value );
      heightOfEquipment.CopyFrom( entity.heightOfEquipment.Value );
    }
    
    [UI.Property( UI.PropertyCategory.EquipmentType, "ID", ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string EquipmentType { get ; set ; }

    private Vector3d NozzlePositionOf(NozzleKind kind)
    {
      // とりあえず各面の中心
      switch (kind)
      {
        case NozzleKind.UP:
          return new Vector3d(0.0, 0.0, HeightOfEquipment);
        case NozzleKind.HZ1:
          return new Vector3d(0.5 * LengthOfEquipment, 0.0, 0.5 * HeightOfEquipment);
        case NozzleKind.HZ2:
          return new Vector3d(0.0, 0.5 * WidthOfEquipment, 0.5 * HeightOfEquipment);
        case NozzleKind.HZ3:
          return new Vector3d(-0.5 * LengthOfEquipment, 0.0, 0.5 * HeightOfEquipment);
        case NozzleKind.HZ4:
          return new Vector3d(0.0, -0.5 * WidthOfEquipment, 0.5 * HeightOfEquipment);
        case NozzleKind.DOWN:
          return new Vector3d(0.0, 0.0, 0.0);
        default:
          throw new ArgumentException();
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZ3", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZ4", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "DOWN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
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

    private Vector3d NozzleDirectionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.UP:
          return Vector3d.forward;
        case NozzleKind.HZ1:
          return Vector3d.right;
        case NozzleKind.HZ2:
          return Vector3d.up;
        case NozzleKind.HZ3:
          return Vector3d.left;
        case NozzleKind.HZ4:
          return Vector3d.down;
        case NozzleKind.DOWN:
          return Vector3d.back;
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

    public override Bounds GetBounds()
    {
      var centerOfBounds = new Vector3(0.0f, 0.0f, (float)(0.5 * HeightOfEquipment));
      var sizeOfBounds = new Vector3((float)LengthOfEquipment, (float)WidthOfEquipment, (float)HeightOfEquipment);
      return new Bounds(centerOfBounds, sizeOfBounds);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfEquipment", ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfEquipment
    {
      get => lengthOfEquipment.Value;
      set => lengthOfEquipment.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "WidthOfEquipment", ValueType = UI.ValueType.GeneralNumeric,
      Visibility = UI.PropertyVisibility.Editable)]
    public double WidthOfEquipment
    {
      get => widthOfEquipment.Value;
      set => widthOfEquipment.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfEquipment", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfEquipment { get => heightOfEquipment.Value; set => heightOfEquipment.Value = value; }


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
  }
}
