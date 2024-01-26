using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.KettleTypeHeatExchanger)]
  public class KettleTypeHeatExchanger : Equipment
  {
    // KettleType HEのノズル種別
    public enum NozzleKind
    {
      TDN = 0,
      TUP,
      THZE,
      THZW,
      UP,
      DN,
      HZN,
      HZS,
      HZE,
      HZW
    }
    private readonly Memento<double> diameterOfTube;
    private readonly Memento<double> lengthOfTube;
    private readonly Memento<double> widthOfSaddle;
    private readonly Memento<double> heightOfSaddles;

    public KettleTypeHeatExchanger( Document document ) : base( document )
    {
      EquipmentName = "KettleType HE";

      diameterOfTube = CreateMementoAndSetupValueEvents( 0.0 ) ;
      lengthOfTube = CreateMementoAndSetupValueEvents( 0.0 ) ;
      widthOfSaddle = CreateMementoAndSetupValueEvents( 0.0 ) ;
      heightOfSaddles = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as KettleTypeHeatExchanger;
      DiameterOfTube = entity.DiameterOfTube;
      LengthOfTube = entity.LengthOfTube;
      WidthOfSaddle = entity.WidthOfSaddle;
      HeightOfSaddles = entity.HeightOfSaddles;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfTube { get => diameterOfTube.Value; set => diameterOfTube.Value = value; }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfTube { get => lengthOfTube.Value; set => lengthOfTube.Value = value; }

    [UI.Property(UI.PropertyCategory.BaseData, "TDN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
    public bool IsExistTDNNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.TDN);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.TDN, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.TDN);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "TUP", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
    public bool IsExistTUPNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.TUP);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.TUP, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.TUP);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "THZE", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 3)]
    public bool IsExistTHZENozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.THZE);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.THZE, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.THZE);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "THZW", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 4)]
    public bool IsExistTHZWNozzle
    {
      get
      {
        return ExistsNozzle(NozzleKind.THZW);
      }
      set
      {
        if (value)
        {
          AddNozzle(NozzleKind.THZW, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter*1000d));
        }
        else
        {
          RemoveNozzle(NozzleKind.THZW);
        }
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "UP", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 5)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZN", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 7)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZS", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 8)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZE", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 9)]
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

    [UI.Property(UI.PropertyCategory.BaseData, "HZW", ValueType = UI.ValueType.CheckBox, Visibility = UI.PropertyVisibility.Editable, Order = 10)]
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

    public double DistanceOfSaddle => LengthOfTube / 8.0;

    public double WidthOfSaddle { get => widthOfSaddle.Value; set => widthOfSaddle.Value = value; }


    public double LengthOfSaddles => DiameterOfTube / 2.0;

    public double HeightOfSaddles { get => heightOfSaddles.Value; set => heightOfSaddles.Value = value; }


    public double LengthOfCaps => DiameterOfTube / 4.0;

    public double DiameterOfTip => DiameterOfTube * 3.0 / 10.0;

    public double LengthOfTip => DiameterOfTube / 3.0;

    public double LengthOfTaper
    {
      get
      {
        var halfRoot3 = Math.Sqrt(3.0) / 2.0;
        return halfRoot3 * (DiameterOfTube - DiameterOfTip);
      }
    }

    public override Bounds GetBounds()
    {
      var centerOfDrumCap = new Vector3(0.0f, 0.0f, (float)(HeightOfSaddles + 0.5 * DiameterOfTube));
      var sizeOfDrumCap = new Vector3((float)DiameterOfTube, (float)(2.0 * LengthOfCaps), (float)DiameterOfTube);
      var drumCapBounds = new Bounds(centerOfDrumCap, sizeOfDrumCap);

      var centerOfDrum = new Vector3(0.0f, (float)(0.5 * LengthOfTube), (float)(HeightOfSaddles + 0.5 * DiameterOfTube));
      var sizeOfDrum = new Vector3((float)DiameterOfTube, (float)LengthOfTube, (float)DiameterOfTube);
      var drumBounds = new Bounds(centerOfDrum, sizeOfDrum);
      drumCapBounds.Encapsulate(drumBounds);

      var centerOfTip = new Vector3(0.0f, (float)(LengthOfTube + LengthOfTaper + 0.5 * LengthOfTip), (float)(HeightOfSaddles + 0.5 * DiameterOfTube));
      var sizeOfTip = new Vector3((float)DiameterOfTip, (float)LengthOfTip, (float)DiameterOfTip);
      var tipBounds = new Bounds(centerOfTip, sizeOfTip);
      drumCapBounds.Encapsulate(tipBounds);

      var centerOfTipCap = new Vector3(0.0f, (float)(LengthOfTube + LengthOfTaper + LengthOfTip), (float)(HeightOfSaddles + 0.5 * DiameterOfTube));
      var sizeOfTipCap = new Vector3((float)DiameterOfTip, (float)(2.0 * LengthOfCaps), (float)DiameterOfTube);
      var tipCapBounds = new Bounds(centerOfTipCap, sizeOfTipCap);
      drumCapBounds.Encapsulate(tipCapBounds);

      var centerOfSaddle1 = new Vector3(0.0f, (float)DistanceOfSaddle, (float)HeightOfSaddles);
      var sizeOfSaddle1 = new Vector3((float)LengthOfSaddles, (float)WidthOfSaddle, (float)(2.0 * HeightOfSaddles));
      var saddle1Bounds = new Bounds(centerOfSaddle1, sizeOfSaddle1);
      drumCapBounds.Encapsulate(saddle1Bounds);

      var centerOfSaddle2 = new Vector3(0.0f, (float)(LengthOfTube - DistanceOfSaddle), (float)HeightOfSaddles);
      var sizeOfSaddle2 = new Vector3((float)LengthOfSaddles, (float)WidthOfSaddle, (float)(2.0 * HeightOfSaddles));
      var saddle2Bounds = new Bounds(centerOfSaddle2, sizeOfSaddle2);
      drumCapBounds.Encapsulate(saddle2Bounds);

      return drumCapBounds;
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
          case NozzleKind.TDN :
          case NozzleKind.TUP :
          case NozzleKind.THZE :
          case NozzleKind.THZW :
          case NozzleKind.HZN :
          case NozzleKind.HZS :
            ( nozzle, _ ) = AddNozzleAndConnectPoint( (int) kind, length, diameter ) ;
            break ;
          case NozzleKind.UP :
          case NozzleKind.DN :
          case NozzleKind.HZE :
          case NozzleKind.HZW :
            ( nozzle, _ ) = AddNozzleWithDistanceFromBaseAndConnectPoint( (int) kind, length, diameter, distanceFromBase ) ;
            break ;
          default :
            throw new ArgumentException() ;
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
        case NozzleKind.TDN:
          return new Vector3d(0.0, LengthOfTube + LengthOfTaper + 0.5 * LengthOfTip, HeightOfSaddles + 0.5 * DiameterOfTube - 0.5 * DiameterOfTip);
        case NozzleKind.TUP:
          return new Vector3d(0.0, LengthOfTube + LengthOfTaper + 0.5 * LengthOfTip, HeightOfSaddles + 0.5 * DiameterOfTube + 0.5 * DiameterOfTip);
        case NozzleKind.THZE:
          return new Vector3d(-0.5 * DiameterOfTip, LengthOfTube + LengthOfTaper + 0.5 * LengthOfTip, HeightOfSaddles + 0.5 * DiameterOfTube);
        case NozzleKind.THZW:
          return new Vector3d(0.5 * DiameterOfTip, LengthOfTube + LengthOfTaper + 0.5 * LengthOfTip, HeightOfSaddles + 0.5 * DiameterOfTube);
        case NozzleKind.UP:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(0.0, nozzle.DistanceFromBase, HeightOfSaddles + DiameterOfTube);
          }
        case NozzleKind.DN:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(0.0, nozzle.DistanceFromBase, HeightOfSaddles);
          }
        case NozzleKind.HZN:
          return new Vector3d(0.0, LengthOfTube + LengthOfTaper + LengthOfTip + LengthOfCaps, HeightOfSaddles + 0.5 * DiameterOfTube);
        case NozzleKind.HZS:
          return new Vector3d(0.0, -LengthOfCaps, HeightOfSaddles + 0.5 * DiameterOfTube);
        case NozzleKind.HZE:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(-0.5 * DiameterOfTube, nozzle.DistanceFromBase, HeightOfSaddles + 0.5 * DiameterOfTube);
          }
        case NozzleKind.HZW:
          {
            var nozzle = GetNozzle( (int) kind ) as NozzleWithDistanceFromBase ;
            return new Vector3d(0.5 * DiameterOfTube, nozzle.DistanceFromBase, HeightOfSaddles + 0.5 * DiameterOfTube);
          }
        default:
          throw new InvalidOperationException();
      }
    }

    private Vector3d NozzleDirectionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.TDN:
          return Vector3d.back;
        case NozzleKind.TUP:
          return Vector3d.forward;
        case NozzleKind.THZE:
          return Vector3d.left;
        case NozzleKind.THZW:
          return Vector3d.right;
        case NozzleKind.UP:
          return Vector3d.forward;
        case NozzleKind.DN:
          return Vector3d.back;
        case NozzleKind.HZN:
          return Vector3d.up;
        case NozzleKind.HZS:
          return Vector3d.down;
        case NozzleKind.HZE:
          return Vector3d.left;
        case NozzleKind.HZW:
          return Vector3d.right;
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
