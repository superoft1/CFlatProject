using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.HorizontalVessel)]
  public class HorizontalVessel : Equipment
  {
    // HOR. Vesselのノズル種別
    public enum NozzleKind
    {
      UPEN = 0,
      UPES,
      UP,
      HZN,
      HZS,
      HZE,
      HZW,
      DNEN,
      DNES,
      DN,
    }

    private readonly Memento<double> diameterOfDrum;
    private readonly Memento<double> lengthOfDrum;

    public HorizontalVessel( Document document ) : base( document )
    {
      EquipmentName = "HOR. Vessel";

      diameterOfDrum = CreateMementoAndSetupValueEvents( 0.0 ) ;
      lengthOfDrum = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as HorizontalVessel;
      diameterOfDrum.CopyFrom( entity.diameterOfDrum.Value );
      lengthOfDrum.CopyFrom( entity.lengthOfDrum.Value );
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfDrum", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfDrum
    {
      get
      {
        return diameterOfDrum.Value;
      }
      set
      {
        diameterOfDrum.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfDrum", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfDrum
    {
      get
      {
        return lengthOfDrum.Value;
      }
      set
      {
        lengthOfDrum.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "UPEN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public bool IsExistUPENNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.UPEN);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.UPEN, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.UPEN);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "UPES", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistUPESNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.UPES);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.UPES, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.UPES);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "UP", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public bool IsExistHZNNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZN);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZN, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZN);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZS", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
    public bool IsExistHZSNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZS);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZS, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZS);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZE", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
    public bool IsExistHZENozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZE);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZE, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZE);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HZW", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 7)]
    public bool IsExistHZWNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.HZW);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.HZW, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.HZW);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DNEN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 8)]
    public bool IsExistDNENNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.DNEN);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.DNEN, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.DNEN);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DNES", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 9)]
    public bool IsExistDNESNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.DNES);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.DNES, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.DNES);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 10)]
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

    public double DistanceOfSaddle
    {
      get
      {
        return LengthOfDrum / 6.0;
      }
    }

    public double WidthOfSaddle
    {
      get
      {
        return 0.250;
      }
    }

    public double LengthOfSaddle
    {
      get
      {
        return DiameterOfDrum / 2.0;
      }
    }

    public double HeightOfSaddle
    {
      get
      {
        return 0.500;
      }
    }

    public double LengthOfCaps
    {
      get
      {
        return DiameterOfDrum / 4.0;
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfCap1 = new Vector3(0.0f, 0.0f, (float)(HeightOfSaddle + 0.5 * DiameterOfDrum));
      var sizeOfCap1 = new Vector3((float)DiameterOfDrum, (float)(2.0 * LengthOfCaps), (float)DiameterOfDrum);
      var cap1Bounds = new Bounds(centerOfCap1, sizeOfCap1);

      var centerOfDrum = new Vector3(0.0f, (float)(0.5 * LengthOfDrum), (float)(HeightOfSaddle + 0.5 * DiameterOfDrum));
      var sizeOfDrum = new Vector3((float)DiameterOfDrum, (float)LengthOfDrum, (float)DiameterOfDrum);
      var drumBounds = new Bounds(centerOfDrum, sizeOfDrum);
      cap1Bounds.Encapsulate(drumBounds);

      var centerOfCap2 = new Vector3(0.0f, (float)LengthOfDrum, (float)(HeightOfSaddle + 0.5 * DiameterOfDrum));
      var sizeOfCap2 = new Vector3((float)DiameterOfDrum, (float)(2.0 * LengthOfCaps), (float)DiameterOfDrum);
      var cap2Bounds = new Bounds(centerOfCap2, sizeOfCap2);
      cap1Bounds.Encapsulate(cap2Bounds);

      var centerOfSaddle1 = new Vector3(0.0f, (float)DistanceOfSaddle, (float)HeightOfSaddle);
      var sizeOfSaddle1 = new Vector3((float)LengthOfSaddle, (float)WidthOfSaddle, (float)(2.0 * HeightOfSaddle));
      var saddle1Bounds = new Bounds(centerOfSaddle1, sizeOfSaddle1);
      cap1Bounds.Encapsulate(saddle1Bounds);

      var centerOfSaddle2 = new Vector3(0.0f, (float)(LengthOfDrum - DistanceOfSaddle), (float)HeightOfSaddle);
      var sizeOfSaddle2 = new Vector3((float)LengthOfSaddle, (float)WidthOfSaddle, (float)(2.0 * HeightOfSaddle));
      var saddle2Bounds = new Bounds(centerOfSaddle2, sizeOfSaddle2);
      cap1Bounds.Encapsulate(saddle2Bounds);

      return cap1Bounds;
    }

    public Nozzle AddNozzle(NozzleKind kind, double length, Diameter diameter)
    {
      return AddNozzle(kind, length, diameter, 0.0);
    }

    public Nozzle AddNozzle( NozzleKind kind, double length, Diameter diameter, double distanceFromBase )
    {
      Nozzle nozzle = GetNozzle( (int) kind ) ;
      if ( null == nozzle ) {
        switch ( kind ) {
          case NozzleKind.HZN :
          case NozzleKind.HZS :
            ( nozzle, _ ) = AddNozzleAndConnectPoint( (int) kind, length, diameter) ;
            break ;
          case NozzleKind.UPEN :
          case NozzleKind.UPES :
          case NozzleKind.UP :
          case NozzleKind.HZE :
          case NozzleKind.HZW :
          case NozzleKind.DNEN :
          case NozzleKind.DNES :
          case NozzleKind.DN :
            ( nozzle, _ ) = AddNozzleWithDistanceFromBaseAndConnectPoint( (int) kind, length, diameter, distanceFromBase ) ;
            break ;
          default :
            throw new InvalidOperationException() ;
        }
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
        case NozzleKind.HZN:
          return new Vector3d(0.0, LengthOfDrum + LengthOfCaps, HeightOfSaddle + 0.5 * DiameterOfDrum);
        case NozzleKind.HZS:
          return new Vector3d(0.0, -LengthOfCaps, HeightOfSaddle + 0.5 * DiameterOfDrum);
        case NozzleKind.UPEN:
        case NozzleKind.UPES:
        case NozzleKind.UP:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(0.0, nozzle.DistanceFromBase, HeightOfSaddle + DiameterOfDrum);
          }
        case NozzleKind.HZE:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(-0.5 * DiameterOfDrum, nozzle.DistanceFromBase, HeightOfSaddle + 0.5 * DiameterOfDrum);
          }
        case NozzleKind.HZW:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(0.5 * DiameterOfDrum, nozzle.DistanceFromBase, HeightOfSaddle + 0.5 * DiameterOfDrum);
          }
        case NozzleKind.DNEN:
        case NozzleKind.DNES:
        case NozzleKind.DN:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(0.0, nozzle.DistanceFromBase, HeightOfSaddle);
          }
        default:
          throw new InvalidOperationException();
      }
    }

    private Vector3d NozzleDirectionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.UPEN:
          return Vector3d.forward;
        case NozzleKind.UPES:
          return Vector3d.forward;
        case NozzleKind.UP:
          return Vector3d.forward;
        case NozzleKind.HZN:
          return Vector3d.up;
        case NozzleKind.HZS:
          return Vector3d.down;
        case NozzleKind.HZE:
          return Vector3d.left;
        case NozzleKind.HZW:
          return Vector3d.right;
        case NozzleKind.DNEN:
          return Vector3d.back;
        case NozzleKind.DNES:
          return Vector3d.back;
        case NozzleKind.DN:
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
  }
}
