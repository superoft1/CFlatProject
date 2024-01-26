using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace MTO
{
  class PipingTeeComponent : IPipingComponent
  {
    private readonly PipingTee _tee;
    private readonly LocalCodSys3d _cod;

    public PipingTeeComponent( PipingTee tee, LocalCodSys3d cod )
    {
      _tee = tee;
      _cod = cod;
    }

    public Vector3d Axis
    {
      get
      {
        var axis = _tee.GetConnectPoint( (int)PipingTee.ConnectPointType.MainTerm1 ).Vector;
        return _cod.GlobalizeVector( axis ).normalized;
      }
    }
    public Vector3d Reference
    {
      get
      {
        var reference = -_tee.GetConnectPoint( (int)PipingTee.ConnectPointType.BranchTerm ).Vector;
        return _cod.GlobalizeVector( reference ).normalized;
      }
    }
    public Vector3d[] ConnectPoints
    {
      get
      {
//        var unit = _tee.Document.LengthUnits.PositionUnit;
        var unit = LengthUnitType.Meter;
        var scale = LengthUnitType.YardPond == unit ? 1000.0 * 25.4 : 1000.0;
        return new Vector3d[] { _tee.GetWorldPosition() * scale,
                                _tee.MainTerm1ConnectPoint.GlobalPoint * scale,
                                _tee.MainTerm2ConnectPoint.GlobalPoint * scale,
                                _tee.BranchTermConnectPoint.GlobalPoint * scale };
      }
    }
    public double[] Diameters
    {
      get
      {
//        var unit = _tee.Document.LengthUnits.DiameterUnit;
        var unit = LengthUnitType.YardPond;
        if ( LengthUnitType.YardPond == unit ) {
          return new double[] { _tee.MainTerm1ConnectPoint.Diameter.NpsInch,
                                _tee.BranchTermConnectPoint.Diameter.NpsInch };
        }
        else {
          return new double[] { _tee.MainTerm1ConnectPoint.Diameter.NpsMm,
                                _tee.BranchTermConnectPoint.Diameter.NpsMm };
        }
      }
    }

    public double Length => 0.0;
    public double Weight => 0.0;

    public bool IsAxisymmetric => false;
  }
}
