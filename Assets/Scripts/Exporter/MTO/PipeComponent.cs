using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class PipeComponent : AxisymmetricComponent
  {
    private readonly Pipe _pipe;

    public PipeComponent( Pipe pipe )
    {
      _pipe = pipe;
    }

    public override Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _pipe.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _pipe.GetWorldPosition() * scale,
                                _pipe.Term1ConnectPoint.GlobalPoint * scale,
                                _pipe.Term2ConnectPoint.GlobalPoint * scale };
      }
    }
    public override double[] Diameters
    {
      get
      {
//        var unit = _pipe.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        return new double[] { LengthUnitType.YardPond == unit ? _pipe.Term1ConnectPoint.Diameter.NpsInch :
                                                                _pipe.Term1ConnectPoint.Diameter.NpsMm };
      }
    }

    public override double Length
    {
      get
      {
////        var unit = _pipe.Document.LengthUnits.DimensionUnit;
//        var unit = _pipe.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return _pipe.Length * scale;
      }
    }
    public override double Weight
    {
      get
      {
        return 0.0; // TODO: 
      }
    }
  }
}
