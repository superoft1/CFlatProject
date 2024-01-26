using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class OpenSpectacleBlankComponent : AxisymmetricComponent
  {
    private readonly OpenSpectacleBlank _blank;

    public OpenSpectacleBlankComponent( OpenSpectacleBlank blank )
    {
      _blank = blank;
    }

    public override Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _blank.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _blank.GetWorldPosition() * scale,
                                _blank.Term1ConnectPoint.GlobalPoint * scale,
                                _blank.Term2ConnectPoint.GlobalPoint * scale };
      }
    }
    public override double[] Diameters
    {
      get
      {
//        var unit = _blank.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _blank.Term1ConnectPoint.Diameter.NpsInch :
                                                                _blank.Term1ConnectPoint.Diameter.NpsMm };
      }
    }

    public override double Length => 0.0;
    public override double Weight => 0.0;
  }
}
