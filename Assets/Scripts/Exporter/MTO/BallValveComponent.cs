using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class BallValveComponent : AxisymmetricComponent
  {
    private readonly BallValve _valve;

    public BallValveComponent( BallValve valve )
    {
      _valve = valve;
    }

    public override Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _valve.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _valve.GetWorldPosition() * scale,
                                _valve.Weld1TermConnectPoint.GlobalPoint * scale,
                                _valve.Weld2TermConnectPoint.GlobalPoint * scale };
      }
    }
    public override double[] Diameters
    {
      get
      {
//        var unit = _valve.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _valve.Weld1TermConnectPoint.Diameter.NpsInch :
                                                                _valve.Weld1TermConnectPoint.Diameter.NpsMm };
      }
    }

    public override double Length => 0.0;
    public override double Weight => 0.0;
  }
}
