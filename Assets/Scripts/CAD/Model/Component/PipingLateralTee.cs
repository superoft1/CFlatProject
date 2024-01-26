using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipingLateralTee )]
  public class PipingLateralTee : Component
  {
    public enum ConnectPointType
    {
      MainTerm1,
      MainTerm2,
      LateralTerm,
    }

    public ConnectPoint MainTerm1ConnectPoint => GetConnectPoint( (int)ConnectPointType.MainTerm1 ) ;
    public ConnectPoint MainTerm2ConnectPoint => GetConnectPoint( (int)ConnectPointType.MainTerm2 ) ;
    public ConnectPoint LateralTermConnectPoint => GetConnectPoint( (int)ConnectPointType.LateralTerm ) ;
    private readonly Memento<Vector3d> lateralAxis;
    private readonly Memento<double> length1;
    private readonly Memento<double> length2;
    private readonly Memento<double> lateralLength;

    public override bool IsEndOfStream => true;


    public PipingLateralTee( Document document ) : base( document )
    {
      lateralAxis = new Memento<Vector3d>( this ) ;
      length1 = new Memento<double>( this ) ;
      length2 = new Memento<double>( this ) ;
      lateralLength = new Memento<double>( this ) ;

      ComponentName = "LateralTee" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.MainTerm1 );
      AddNewConnectPoint( (int) ConnectPointType.MainTerm2 );
      AddNewConnectPoint( (int) ConnectPointType.LateralTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as PipingLateralTee;
      lateralAxis.CopyFrom( entity.lateralAxis.Value );
      length1.CopyFrom( entity.length1.Value );
      length2.CopyFrom( entity.length2.Value );
      lateralLength.CopyFrom( entity.lateralLength.Value );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      switch ((ConnectPointType)connectPointNumber)
      {
        case ConnectPointType.MainTerm1:
        case ConnectPointType.MainTerm2:
          {
            var afterDiameter = ConnectPoint.GetDiameterOfNpsMm(newDiameterNpsMm);
            var lateralDiameter = LateralDiameter;
            Length1 = GetDefaultLength1(afterDiameter, lateralDiameter);
            Length2 = GetDefaultLength2(afterDiameter, lateralDiameter);
            LateralLength = GetDefaultLateralLength(afterDiameter, lateralDiameter);
            GetConnectPoint((int)ConnectPointType.MainTerm1).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            GetConnectPoint((int)ConnectPointType.MainTerm2).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // Branch側には伝播しない
            break;
          }

        case ConnectPointType.LateralTerm:
          {
            var mainDiameter = MainDiameter;
            var afterDiameter = ConnectPoint.GetDiameterOfNpsMm(newDiameterNpsMm);
            Length1 = GetDefaultLength1(mainDiameter, afterDiameter);
            Length2 = GetDefaultLength2(mainDiameter, afterDiameter);
            LateralLength = GetDefaultLateralLength(mainDiameter, afterDiameter);
            GetConnectPoint(connectPointNumber).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // 他のConnectPointには伝播しない
            break;
          }

        default:  // ElbOlet
          {
            GetConnectPoint(connectPointNumber).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // 他のConnectPointには伝播しない
            break;
          }
      }
    }

    public double MainLength
    {
      get
      {
        return Length1 + Length2;
      }
    }

    public double Length1
    {
      get
      {
        return length1.Value;
      }
      set
      {
        length1.Value = value;
        MainTerm1ConnectPoint.SetPointVector( -length1.Value * Axis);
      }
    }

    public double Length2
    {
      get
      {
        return length2.Value;
      }
      set
      {
        length2.Value = value;
        MainTerm2ConnectPoint.SetPointVector( length2.Value * Axis);
      }
    }

    public double LateralLength
    {
      get
      {
        return lateralLength.Value;
      }
      set
      {
        lateralLength.Value = value;
        LateralTermConnectPoint.SetPointVector( lateralLength.Value * LateralAxis);
      }
    }

    public Vector3d MainCenter
    {
      get
      {
        return Vector3d.Lerp(MainTerm1ConnectPoint.Point, MainTerm2ConnectPoint.Point, 0.5);
      }
    }


    public Vector3d Reference
    {
      get
      {
        return Vector3d.up;
      }
    }

    public double MainDiameter
    {
      get
      {
        return MainTerm1ConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var mainTerm1 = MainTerm1ConnectPoint ;
        var mainTerm2 = MainTerm2ConnectPoint ;

        mainTerm1.Diameter = DiameterFactory.FromOutsideMeter( value );
        mainTerm2.Diameter = mainTerm1.Diameter;
      }
    }

    public double LateralDiameter
    {
      get
      {
        return LateralTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var lateralTerm = LateralTermConnectPoint ;

        lateralTerm.Diameter = DiameterFactory.FromOutsideMeter( value );
      }
    }

    public Vector3d LateralAxis
    {
      get
      {
        return lateralAxis.Value;
      }

      set
      {
        lateralAxis.Value = value;
        LateralTermConnectPoint.SetPointVector( LateralLength * lateralAxis.Value);
      }
    }

    public override Bounds GetBounds()
    {
      var mainTerm1 = MainTerm1ConnectPoint ;
      var mainTerm2 = MainTerm2ConnectPoint ;
      var lateralTerm = LateralTermConnectPoint ;

      var bounds = new Bounds((Vector3)Origin, Vector3.one * (float)MainDiameter);
      bounds.Encapsulate(new Bounds((Vector3)mainTerm1.Point, Vector3.one * (float)MainDiameter));
      bounds.Encapsulate(new Bounds((Vector3)mainTerm2.Point, Vector3.one * (float)MainDiameter));
      bounds.Encapsulate(new Bounds((Vector3)lateralTerm.Point, Vector3.one * (float)LateralDiameter));
      return bounds;
    }

    public static double GetDefaultLength1( double mainDiameter, double branchDiameter )
    {
      // TODO: 仮
      return branchDiameter * 1 ;
    }

    public static double GetDefaultLength2( double mainDiameter, double branchDiameter )
    {
      // TODO: 仮
      return branchDiameter * 1 ;
    }
    
    public static double GetDefaultLateralLength( double mainDiameter, double branchDiameter )
    {
      // TODO: 仮
      return mainDiameter ;
    }
  }
}
