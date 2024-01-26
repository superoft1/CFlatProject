using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;


namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.ThreeWayInstrumentRootvalve )]
  public class ThreeWayInstrumentRootvalve : Component
  {
    public enum ConnectPointType
    {
      AxisLeftTerm,
      AxisRightTerm,
      ReferenceTerm,
    }

    public ConnectPoint AxisLeftTermConnectPoint => GetConnectPoint( (int)ConnectPointType.AxisLeftTerm ) ;
    public ConnectPoint AxisRightTermConnectPoint => GetConnectPoint( (int)ConnectPointType.AxisRightTerm ) ;
    public ConnectPoint ReferenceTermConnectPoint => GetConnectPoint( (int)ConnectPointType.ReferenceTerm ) ;
    private readonly Memento<double> axisLength1;
    private readonly Memento<double> axisLength2;
    private readonly Memento<double> referenceLength;

    public override bool IsEndOfStream => true;


    public ThreeWayInstrumentRootvalve( Document document ) : base( document )
    {
      axisLength1 = new Memento<double>( this ) ;
      axisLength2 = new Memento<double>( this ) ;
      referenceLength = new Memento<double>( this ) ;

      ComponentName = "ThreeWayInstrumentRootValve" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.AxisLeftTerm );
      AddNewConnectPoint( (int) ConnectPointType.AxisRightTerm );
      AddNewConnectPoint( (int) ConnectPointType.ReferenceTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as ThreeWayInstrumentRootvalve;
      axisLength1.CopyFrom( entity.axisLength1.Value );
      axisLength2.CopyFrom( entity.axisLength2.Value );
      referenceLength.CopyFrom( entity.referenceLength.Value );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      var beforeDiameter = cp.Diameter.OutsideMeter;
      var afterDiameter = ConnectPoint.GetDiameterOfNpsMm(newDiameterNpsMm);
      AxisLength1 *= (afterDiameter / beforeDiameter);
      AxisLength2 *= (afterDiameter / beforeDiameter);
      ReferenceLength *= (afterDiameter / beforeDiameter);
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    public double AxisLength1
    {
      get
      {
        return axisLength1.Value;
      }
      set
      {
        axisLength1.Value = value;
        AxisLeftTermConnectPoint.SetPointVector( -axisLength1.Value * Axis);
      }
    }

    public double AxisLength2
    {
      get
      {
        return axisLength2.Value;
      }
      set
      {
        axisLength2.Value = value;
        AxisRightTermConnectPoint.SetPointVector( axisLength2.Value * Axis);
      }
    }

    public double ReferenceLength
    {
      get
      {
        return referenceLength.Value;
      }
      set
      {
        referenceLength.Value = value;
        ReferenceTermConnectPoint.SetPointVector( referenceLength.Value * Reference);
      }
    }



    public Vector3d Reference
    {
      get
      {
        return Vector3d.up;
      }
    }


    public double AxisDiameter
    {
      get
      {
        return AxisRightTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var axisRightTerm = AxisRightTermConnectPoint ;
        var axisLeftTerm = AxisLeftTermConnectPoint ;

        axisRightTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        axisLeftTerm.Diameter = axisRightTerm.Diameter ;
      }
    }

    public double ReferenceDiameter
    {
      get
      {
        return ReferenceTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var referenceTerm = ReferenceTermConnectPoint ;

        referenceTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
      }
    }

  }
}