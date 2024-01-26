using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class ControlValveComponent : IPipingComponent
  {
    private readonly ControlValve _valve;
    private readonly LocalCodSys3d _cod;

    public ControlValveComponent( ControlValve valve, LocalCodSys3d cod )
    {
      _valve = valve;
      _cod = cod;
    }

    public Vector3d Axis
    {
      get
      {
        var axis = _valve.SecondAxis;
        return _cod.GlobalizeVector( axis ).normalized;
      }
    }
    public Vector3d Reference
    {
      get
      {
        var reference = _valve.Term1ConnectPoint.Vector;
        return _cod.GlobalizeVector( reference ).normalized;
      }
    }
    public Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _valve.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _valve.GetWorldPosition() * scale,
                                _valve.Term1ConnectPoint.GlobalPoint * scale,
                                _valve.Term2ConnectPoint.GlobalPoint * scale };
      }
    }
    public double[] Diameters
    {
      get
      {
//        var unit = _valve.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _valve.Term1ConnectPoint.Diameter.NpsInch :
                                                                _valve.Term1ConnectPoint.Diameter.NpsMm };
      }
    }

    public double Length => 0.0;
    public double Weight => 0.0;

    public bool IsAxisymmetric => false;
  }
}
