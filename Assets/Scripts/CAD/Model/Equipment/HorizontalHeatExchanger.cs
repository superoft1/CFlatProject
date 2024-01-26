using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.HorizontalHeatExchanger)]
  public class HorizontalHeatExchanger : Equipment
  {
    // ノズル設置面
    public enum Placement
    {
      FrontEnd = 0,
      Shell,
      RearEnd,
    }

    // HOR. HEのノズル種別
    public enum NozzleKind
    {
      S1UP = 0,
      S2UP,
      TUP,
      S1HZ,
      S2HZ,
      S3HZ,
      THN,
      TDN,
      S1DN,
      S2DN,
    }

    private readonly Memento<Shell> shell;
    private readonly Memento<FrontEnd> frontEnd;
    private readonly Memento<RearEnd> rearEnd;
    private readonly MementoDictionary<NozzleKind, Placement> placementMap;

    [UI.Property(UI.PropertyCategory.BaseData, "Type", ValueType = UI.ValueType.Select, Order = 0)]
    public NozzleKind SelectNozzleKind
    {
      get;
      set;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "AddNozzle", ValueType = UI.ValueType.Button, Order = 1)]
    public System.Object Prop
    {
      get => null;
      set
      {
        AddNozzle(SelectNozzleKind, DefaultNozzleLength, DiameterFactory.FromNpsMm(DefaultNozzleDiameter * 1000d));
      }
    }

    public HorizontalHeatExchanger( Document document ) : base( document )
    {
      EquipmentName = "HOR. HE";

      shell = CreateMementoAndSetupChildrenEvents<Shell>();
      frontEnd = CreateMementoAndSetupChildrenEvents<FrontEnd>();
      rearEnd = CreateMementoAndSetupChildrenEvents<RearEnd>();

      placementMap = new MementoDictionary<NozzleKind, Placement>(this);
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as HorizontalHeatExchanger;
      shell.CopyFrom(entity.shell.Value);
      frontEnd.CopyFrom(entity.frontEnd.Value);
      rearEnd.CopyFrom(entity.rearEnd.Value);
      WidthOfSaddle = entity.WidthOfSaddle;
      HeightOfSaddle = entity.HeightOfSaddle;

      placementMap.CopyFrom(entity.placementMap);
    }

    private Placement GetDefaultPlacement(NozzleKind kind)
    {
      switch(kind)
      {
        case NozzleKind.S1DN:
        case NozzleKind.S1HZ:
        case NozzleKind.S1UP:
        case NozzleKind.S2DN:
        case NozzleKind.S2HZ:
        case NozzleKind.S2UP:
        case NozzleKind.S3HZ:
          return Placement.Shell;
        case NozzleKind.TDN:
        case NozzleKind.THN:
        case NozzleKind.TUP:
          return Placement.RearEnd;
        default:
          throw new ArgumentException();
      }
    }

    private Vector3d NozzlePositionOf(NozzleKind kind)
    {
      var nozzle = GetNozzle((int)kind) as NozzleWithDistanceFromBase;
      double yBaseOffset;
      double endDiameter;
      switch (this.placementMap[kind])
      {
        case Placement.FrontEnd:
          yBaseOffset = -0.5 * FrontEnd.LengthOfFrontEnd;
          endDiameter = Shell.DiameterOfFrontEnd;
          break;
        case Placement.RearEnd:
          yBaseOffset = Shell.LengthOfShell + 0.5 * RearEnd.LengthOfRearEnd;
          endDiameter = Shell.DiameterOfRearEnd;
          break;
        default:
          yBaseOffset = 0.0;
          endDiameter = 0.0;
          break;
      }

      switch (kind)
      {
        case NozzleKind.S1UP:
          return new Vector3d(0.0, 0.2 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle + Shell.DiameterOfFrontEnd);
        case NozzleKind.S2UP:
          return new Vector3d(0.0, 0.8 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle + Shell.DiameterOfRearEnd);
        case NozzleKind.TUP:
          return new Vector3d(0.0, yBaseOffset + nozzle.DistanceFromBase, HeightOfSaddle + endDiameter);
        case NozzleKind.S1HZ:
          return new Vector3d(0.5 * Shell.DiameterOfFrontEnd, 0.2 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle + 0.5 * Shell.DiameterOfFrontEnd);
        case NozzleKind.S2HZ:
          return new Vector3d(0.5 * Shell.DiameterOfRearEnd, 0.8 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle + 0.5 * Shell.DiameterOfRearEnd);
        case NozzleKind.S3HZ:
          return new Vector3d(0.5 * Shell.DiameterOfFrontEnd, 0.5 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle + 0.5 * Shell.DiameterOfFrontEnd);
        case NozzleKind.THN:
          return new Vector3d(0.5 * endDiameter, yBaseOffset + nozzle.DistanceFromBase, HeightOfSaddle + 0.5 * endDiameter);
        case NozzleKind.TDN:
          return new Vector3d(0.0, yBaseOffset + nozzle.DistanceFromBase, HeightOfSaddle);
        case NozzleKind.S1DN:
          return new Vector3d(0.0, 0.2 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle);
        case NozzleKind.S2DN:
          return new Vector3d(0.0, 0.8 * Shell.LengthOfShell + nozzle.DistanceFromBase, HeightOfSaddle);
        default:
          throw new ArgumentException();
      }
    }

    private Vector3d NozzleDirectionOf(NozzleKind kind)
    {
      switch (kind)
      {
        case NozzleKind.S1UP:
          return Vector3d.forward;
        case NozzleKind.S2UP:
          return Vector3d.forward;
        case NozzleKind.TUP:
          return Vector3d.forward;
        case NozzleKind.S1HZ:
          return Vector3d.right;
        case NozzleKind.S2HZ:
          return Vector3d.right;
        case NozzleKind.S3HZ:
          return Vector3d.right;
        case NozzleKind.THN:
          return Vector3d.right;
        case NozzleKind.TDN:
          return Vector3d.back;
        case NozzleKind.S1DN:
          return Vector3d.back;
        case NozzleKind.S2DN:
          return Vector3d.back;
        default:
          throw new ArgumentException();
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

    public override IEnumerable<IElement> Children
    {
      get
      {
        if ( null != Shell ) {
          yield return Shell;
        }
        if ( null != FrontEnd ) {
          yield return FrontEnd;
        }
        if ( null != RearEnd ) {
          yield return RearEnd;
        }

        foreach (var child in base.Children)
        {
          yield return child;
        }
      }
    }

    public Shell Shell
    {
      get => shell.Value;
      set
      {
        value.HeatExchanger = this;
        shell.Value = value;
      }
    }

    public FrontEnd FrontEnd
    {
      get => frontEnd.Value;
      set
      {
        value.HeatExchanger = this;
        frontEnd.Value = value;
      }
    }

    public RearEnd RearEnd
    {
      get => rearEnd.Value;
      set {
        value.HeatExchanger = this;
        rearEnd.Value = value;
      }
    }

    public double WidthOfSaddle
    {
      get => shell.Value.WidthOfSaddle;
      set => shell.Value.WidthOfSaddle = value;
    }

    public double HeightOfSaddle
    {
      get => shell.Value.HeightOfSaddle;
      set => shell.Value.HeightOfSaddle = value;
    }

    public double LengthOfSaddle => shell.Value.LengthOfSaddle;

    public double FlangeThickness => 0.150;

    public override Bounds GetBounds()
    {
      var bounds = new [] { Shell?.GetBounds(),
                            FrontEnd?.GetBounds(),
                            RearEnd?.GetBounds() };
      return bounds.UnionBounds() ?? new Bounds();
    }

    public Nozzle AddNozzle(NozzleKind kind, double length, Diameter diameter, double offset = 0.0)
    {
      Nozzle nozzle = GetNozzle((int)kind);
      if (null == nozzle)
      {
        placementMap[kind] = GetDefaultPlacement(kind);
        (nozzle, _) = AddNozzleWithDistanceFromBaseAndConnectPoint((int)kind, length, diameter, offset);
      }

      return nozzle;
    }

    public void RemoveNozzle( NozzleKind kind )
    {
      placementMap.Remove(kind);
      RemoveNozzleAndConnectPoint( (int) kind ) ;
    }

    public bool ExistsNozzle( NozzleKind kind )
    {
      return ExistsNozzleNumber( (int) kind ) ;
    }
  }
}
