using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.PlateTypeHeatExchanger)]
  public class PlateTypeHeatExchanger : Equipment
  {
    // PlateType HEのノズル種別
    public enum NozzleKind
    {
      N1 = 0,
      N2,
      N3,
      N4,
    }

    private readonly Memento<double> width;
    private readonly Memento<double> length;
    private readonly Memento<double> height;

    public PlateTypeHeatExchanger( Document document ) : base( document )
    {
      EquipmentName = "PlateType HE";

      width = CreateMementoAndSetupValueEvents( 0.0 ) ;
      length = CreateMementoAndSetupValueEvents( 0.0 ) ;
      height = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as PlateTypeHeatExchanger;
      width.CopyFrom( entity.width.Value );
      length.CopyFrom( entity.length.Value );
      height.CopyFrom( entity.height.Value );
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Width", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Width
    {
      get
      {
        return width.Value;
      }

      set
      {
        width.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Length", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Length
    {
      get
      {
        return length.Value;
      }

      set
      {
        length.Value = value;
      }
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

    [UI.Property(UI.PropertyCategory.BaseData, "N1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public bool IsExistN1Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N1);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N1, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N1);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistN2Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N2);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N2, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N2);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N3", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public bool IsExistN3Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N3);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N3, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N3);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N4", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public bool IsExistN4Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N4);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N4, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N4);
        }
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfBody = new Vector3(0.0f, 0.0f, (float)(0.5 * Height));
      var sizeOfBody = new Vector3((float)Length, (float)Width, (float)Height);
      return new Bounds(centerOfBody, sizeOfBody);
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
        case NozzleKind.N1:
          return new Vector3d(-0.25 * Length, -0.5 * Width, 0.8 * Height );
        case NozzleKind.N2:
          return new Vector3d(-0.25 * Length, -0.5 * Width, 0.2 * Height);
        case NozzleKind.N3:
          return new Vector3d(0.25 * Length, -0.5 * Width, 0.2 * Height);
        case NozzleKind.N4:
          return new Vector3d(0.25 * Length, -0.5 * Width, 0.8 * Height);
        default:
          throw new ArgumentException();
      }
    }

    private Vector3d NozzleDirectionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.N1:
          return Vector3d.down;
        case NozzleKind.N2:
          return Vector3d.down;
        case NozzleKind.N3:
          return Vector3d.down;
        case NozzleKind.N4:
          return Vector3d.down;
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
