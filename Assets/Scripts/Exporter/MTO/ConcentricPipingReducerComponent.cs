using System.Linq;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class ConcentricPipingReducerComponent : AxisymmetricComponent
  {
    public static IEnumerable<ConcentricPipingReducerComponent> Create( ConcentricPipingReducerCombination reducer, LocalCodSys3d cod )
    {
      var components = new List<ConcentricPipingReducerComponent>();
      foreach ( var connectPoint in reducer.CombinationConnectPoints ) {
        components.Add( new ConcentricPipingReducerComponent( ( cod.GlobalizePoint( connectPoint.largeTerm.point ), connectPoint.largeTerm.diameter ),
                                                              ( cod.GlobalizePoint( connectPoint.smallTerm.point ), connectPoint.smallTerm.diameter ) ) );
      }
      return components;
    }

    private readonly Vector3d[] _points = new Vector3d[2];
    private readonly Diameter[] _diameters = new Diameter[2];

    private ConcentricPipingReducerComponent( ( Vector3d point, Diameter diameter ) largeTerm,
                                              ( Vector3d point, Diameter diameter ) smallTerm )
    {
      _points[0] = largeTerm.point;
      _points[1] = smallTerm.point;

      _diameters[0] = largeTerm.diameter;
      _diameters[1] = smallTerm.diameter;
    }

    public override Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _reducer.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { ( _points[0] + _points[1] ) * 0.5 * scale,
                                _points[0] * scale,
                                _points[1] * scale };
      }
    }
    public override double[] Diameters
    {
      get
      {
//        var unit = _reducer.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        if ( LengthUnitType.YardPond == unit ) {
          return new double[] { _diameters[0].NpsInch,
                                _diameters[1].NpsInch };
        }
        else {
          return new double[] { _diameters[0].NpsMm,
                                _diameters[1].NpsMm };
        }
      }
    }

    public override double Length => 0.0;
    public override double Weight => 0.0;
  }
}
