using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class PipingElbow90Component : IPipingComponent
  {
    public enum Elbow90Type
    {
      Short,
      Long
    }

    private readonly PipingElbow90 _elbow;
    private readonly LocalCodSys3d _cod;

    public PipingElbow90Component( PipingElbow90 elbow, LocalCodSys3d cod )
    {
      _elbow = elbow;
      _cod = cod;
    }

    public Vector3d Axis
    {
      get
      {
        var axis = _elbow.GetConnectPoint( (int)PipingElbow90.ConnectPointType.Term1 ).Vector;
        return _cod.GlobalizeVector( axis ).normalized;
      }
    }
    public Vector3d Reference
    {
      get
      {
        var reference = -_elbow.GetConnectPoint( (int)PipingElbow90.ConnectPointType.Term2 ).Vector;
        return _cod.GlobalizeVector( reference ).normalized;
      }
    }
    public Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _elbow.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _elbow.GetWorldPosition() * scale,
                                _elbow.Term1ConnectPoint.GlobalPoint * scale,
                                _elbow.Term2ConnectPoint.GlobalPoint * scale };
      }
    }
    public double[] Diameters
    {
      get
      {
//        var unit = _elbow.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _elbow.Term1ConnectPoint.Diameter.NpsInch :
                                                                _elbow.Term1ConnectPoint.Diameter.NpsMm };
      }
    }

    public double Length => 0.0;
    public double Weight => 0.0;

    public bool IsAxisymmetric => false;

    public Elbow90Type ElbowType =>
      _elbow.ElbowType == PipingElbow90.Elbow90Type.LongElbow ? Elbow90Type.Long : Elbow90Type.Short;
  }
}
