using System.Linq;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  public class EccentricPipingReducerComponent : IPipingComponent
  {
    public static IEnumerable<EccentricPipingReducerComponent> Create( EccentricPipingReducerCombination reducer, LocalCodSys3d cod )
    {
      var axis = cod.GlobalizeVector( reducer.Reference ).normalized;

      var components = new List<EccentricPipingReducerComponent>();
      foreach ( var connectPoint in reducer.CombinationConnectPoints ) {
        components.Add( new EccentricPipingReducerComponent( axis,
                                                             ( cod.GlobalizePoint( connectPoint.largeTerm.point ), connectPoint.largeTerm.diameter ),
                                                             ( cod.GlobalizePoint( connectPoint.smallTerm.point ), connectPoint.smallTerm.diameter ) ) );
      }
      return components;
    }

    private readonly Vector3d _axis;

    private readonly Vector3d[] _points = new Vector3d[2];
    private readonly Diameter[] _diameters = new Diameter[2];

    private EccentricPipingReducerComponent( Vector3d axis,
                                             ( Vector3d point, Diameter diameter ) largeTerm,
                                             ( Vector3d point, Diameter diameter ) smallTerm )
    {
      _axis = axis;

      _points[0] = largeTerm.point;
      _points[1] = smallTerm.point;

      _diameters[0] = largeTerm.diameter;
      _diameters[1] = smallTerm.diameter;
    }

    public Vector3d Axis => _axis;
    public Vector3d Reference => ( _points[0] - _points[1] ).normalized;
    public Vector3d[] ConnectPoints
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
    public double[] Diameters
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

    public double Length => 0.0;
    public double Weight => 0.0;

    public bool IsAxisymmetric => false;
  }
}
