using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class WeldNeckFlangeComponent : IPipingComponent
  {
    private readonly WeldNeckFlange _flange;
    private readonly LocalCodSys3d _cod;

    public WeldNeckFlangeComponent( WeldNeckFlange flange, LocalCodSys3d cod )
    {
      _flange = flange;
      _cod = cod;
    }

    public Vector3d Axis
    {
      get
      {
        var axis = _flange.OutsideTermConnectPoint.Vector;
        return _cod.GlobalizeVector( axis ).normalized;
      }
    }
    public Vector3d Reference => Axis;
    public Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _flange.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _flange.GetWorldPosition() * scale,
                                _flange.OutsideTermConnectPoint.GlobalPoint * scale,
                                _flange.WeldTermConnectPoint.GlobalPoint * scale };
      }
    }
    public double[] Diameters
    {
      get
      {
//        var unit = _flange.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _flange.WeldTermConnectPoint.Diameter.NpsInch :
                                                                _flange.WeldTermConnectPoint.Diameter.NpsMm };
      }
    }

    public double Length => 0.0;
    public double Weight => 0.0;

    public bool IsAxisymmetric => false;
  }
}