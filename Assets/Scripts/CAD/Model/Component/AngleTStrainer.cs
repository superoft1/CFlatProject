using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.AngleTStrainer)]
  public class AngleTStrainer : Component
  {
    public enum ConnectPointType
    {
      Axis,
      Convex,
    }
    
    private readonly Memento<Vector3d> _origin;

    public ConnectPoint AxisTermConnectPoint => GetConnectPoint( (int)ConnectPointType.Axis ) ;
    public ConnectPoint ConvexTermConnectPoint => GetConnectPoint( (int)ConnectPointType.Convex ) ;

    public AngleTStrainer( Document document ) : base( document )
    {
      _origin = new Memento<Vector3d>(this);

      ComponentName = "AngleTStrainer";
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int)ConnectPointType.Axis ) ;
      AddNewConnectPoint( (int)ConnectPointType.Convex ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as AngleTStrainer;
      _origin.CopyFrom( entity._origin.Value );
    }
    
    public double AxisLength
    {
      get
      {
        return (_origin.Value - AxisTermConnectPoint.Point).magnitude * 2d;
      }
    }

    public double ReferenceLength
    {
      get
      {
        return (_origin.Value - ConvexTermConnectPoint.Point).magnitude;
      }
    }


    public Vector3d AxisVector
    {
      get
      {
        return (AxisTerm - _origin.Value).normalized;
      }
    }

    public Vector3d ReferenceVector
    {
      get
      {
        return (ConvexTerm - _origin.Value).normalized;
      }
    }

    public Vector3d AxisTerm
    {
      get
      {
        return AxisTermConnectPoint.Point;
      }

      set
      {
        AxisTermConnectPoint.SetPointVector( value );
      }
    }

    public Vector3d RemovalTerm
    {
      get
      {
        return (Origin - AxisTerm) + Origin;
      }
    }

    public Vector3d ConvexTerm
    {
      get
      {
        return ConvexTermConnectPoint.Point;
      }

      set
      {
        ConvexTermConnectPoint.SetPointVector( value );
      }
    }

    public double AxisDiameter
    {
      get
      {
        return AxisTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var axisTerm = AxisTermConnectPoint ;
        axisTerm.Diameter = DiameterFactory.FromOutsideMeter( value );
      }
    }

    public double ConvexDiameter
    {
      get
      {
        return ConvexTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var convexTerm = ConvexTermConnectPoint ;
        convexTerm.Diameter = DiameterFactory.FromOutsideMeter( value );
      }
    }
    
    public override Bounds GetBounds()
    {
      // 実行中に落ちないようにとりあえず実装
      return new Bounds((Vector3)Origin, new Vector3((float)AxisLength, (float)AxisDiameter, (float)ConvexDiameter));
    }
  }
}