using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class CheckValveComponent : AxisymmetricComponent
  {
    private readonly CheckValve _valve;

    public CheckValveComponent( CheckValve valve )
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
                                _valve.Term1ConnectPoint.GlobalPoint * scale,
                                _valve.Term2ConnectPoint.GlobalPoint * scale };
      }
    }
    public override double[] Diameters
    {
      get
      {
//        var unit = _valve.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _valve.Term1ConnectPoint.Diameter.NpsInch :
                                                                _valve.Term1ConnectPoint.Diameter.NpsMm };
      }
    }

    public override double Length => 0.0;
    public override double Weight => 0.0;
  }
}
