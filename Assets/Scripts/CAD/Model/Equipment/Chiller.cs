using System;
using System.Collections ;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.Chiller)]
  public class Chiller : Equipment
  {
    // Columnのノズル種別
    public enum NozzleKind
    {
      N1 = 0,
      N2,
      N3_1,
      N3_2,
      N4_1,
      N4_2,
      N4_3
    }

    public Chiller( Document document ) : base( document )
    {
      EquipmentName = "Chiller";

      lengthOfTube = CreateMementoAndSetupValueEvents( 0.0 ) ;
      diameterOfTube =  CreateMementoAndSetupValueEvents( 0.0 ) ;
      heightOfTip = CreateMementoAndSetupValueEvents( 0.0 ) ;

    }

    private readonly Memento<double> lengthOfTube;
    private readonly Memento<double> diameterOfTube;
    private readonly Memento<double> heightOfTip;

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
      return new Bounds(new Vector3(0.0f, -(float)(0.5 * LengthOfTube), (float)(0.5 * (DiameterOfTube + HeightOfSaddle))),
                        new Vector3((float)DiameterOfTube,
                                    (float)(LengthOfTube + 2.0 * LengthOfCaps + 2.0 * LengthOfTip),
                                    (float)(HeightOfSaddle + DiameterOfTube)));
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
          return new Vector3d(0.0, - (LengthOfTube + LengthOfCaps + LengthOfTip), HeightOfTip);
        case NozzleKind.N2:
          return new Vector3d(0.0, LengthOfCaps + LengthOfTip, HeightOfTip);
        case NozzleKind.N3_1:
          return new Vector3d(0.0, -(2.0 / 3.0) * LengthOfTube, HeightOfSaddle + DiameterOfTube);
        case NozzleKind.N3_2:
          return new Vector3d(0.0, -(1.0 / 3.0) * LengthOfTube, HeightOfSaddle + DiameterOfTube);
        case NozzleKind.N4_1:
          return new Vector3d(-Math.Sqrt(2) * DiameterOfTube / 3.0, -(2.0 / 3.0) * LengthOfTube, HeightOfSaddle + (2.0 / 3.0) * DiameterOfTube);
        case NozzleKind.N4_2:
          return new Vector3d(-Math.Sqrt(2) * DiameterOfTube / 3.0, -(1.0 / 3.0) * LengthOfTube, HeightOfSaddle + (2.0 / 3.0) * DiameterOfTube);
        case NozzleKind.N4_3:
          return new Vector3d(Math.Sqrt(2) * DiameterOfTube / 3.0, -(2.0 / 3.0) * LengthOfTube, HeightOfSaddle + (2.0 / 3.0) * DiameterOfTube);
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
          return Vector3d.up;
        case NozzleKind.N3_1:
          return Vector3d.forward;
        case NozzleKind.N3_2:
          return Vector3d.forward;
        case NozzleKind.N4_1:
          return Vector3d.left;
        case NozzleKind.N4_2:
          return Vector3d.left;
        case NozzleKind.N4_3:
          return Vector3d.right;
        default:
          throw new ArgumentException();
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfTube
    {
      get
      {
        return lengthOfTube.Value;
      }
      set
      {
        lengthOfTube.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfTube
    {
      get
      {
        return diameterOfTube.Value;
      }
      set
      {
        diameterOfTube.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfTip", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfTip
    {
      get
      {
        return heightOfTip.Value;
      }
      set
      {
        heightOfTip.Value = value;
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

    [UI.Property(UI.PropertyCategory.BaseData, "N3_1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public bool IsExistN3_1Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N3_1);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N3_1, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N3_1);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N3_2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public bool IsExistN3_2Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N3_2);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N3_2, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N3_2);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N4_1", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
    public bool IsExistN4_1Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N4_1);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N4_1, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N4_1);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N4_2", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 6)]
    public bool IsExistN4_2Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N4_2);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N4_2, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N4_2);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "N4_3", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 7)]
    public bool IsExistN4_3Nozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.N4_3);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.N4_3, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.N4_3);
        }
      }
    }

    public double DistanceOfSaddle => LengthOfTube / 8.0;
    public double WidthOfSaddle => 0.25;// TODO 固定値だが不明 KettleTypeHEを参考に決定
    public double LengthOfSaddle => DiameterOfTube / 2.0;
    public double HeightOfSaddle => 1.0; // TODO 固定値だが不明 KettleTypeHEを参考に決定
    public double LengthOfCaps => DiameterOfTube / 4.0;
    public double DiameterOfTip => DiameterOfTube / 2.0; // D1/2.0とあるがD2/2.0の間違いでは？
    public double LengthOfTip => DiameterOfTip / 3.0;
    public double LengthOfTipCylinder => 0.15; // 固定値 LengthOfTipのうちの一部
    public double LengthOfTaper => (LengthOfTip - LengthOfTipCylinder);
    public double DiameterOfTerm => (DiameterOfTip - (2.0 / Math.Sqrt(3.0)) * LengthOfTip);// 端部分の直径
 }
}