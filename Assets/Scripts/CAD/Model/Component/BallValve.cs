using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.BallValve )]
  public class BallValve : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      Weld1Term,
      Weld2Term,
    }

    public ConnectPoint Weld1TermConnectPoint => GetConnectPoint( (int)ConnectPointType.Weld1Term ) ;
    public ConnectPoint Weld2TermConnectPoint => GetConnectPoint( (int)ConnectPointType.Weld2Term ) ;

    private readonly Memento<double> _length;

    public BallValve( Document document ) : base( document )
    {
      _length = new Memento<double>(this);

      ComponentName = "BallValve";
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.Weld1Term );
      AddNewConnectPoint( (int) ConnectPointType.Weld2Term );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as BallValve;
      _length.CopyFrom( entity._length.Value );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      var beforeDiameter = cp.Diameter.OutsideMeter;
      var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
      Length *= (afterDiameter / beforeDiameter);
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    public double Diameter
    {
      get
      {
        return Weld1TermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var weld1Term = Weld1TermConnectPoint ;
        var weld2Term = Weld2TermConnectPoint ;

        weld1Term.Diameter = DiameterFactory.FromOutsideMeter( value );
        weld2Term.Diameter = weld1Term.Diameter ;
      }
    }

    public double Length
    {
      get
      {
        return _length.Value;
      }

      set
      {
        var weld1Term = Weld1TermConnectPoint ;
        var weld2Term = Weld2TermConnectPoint ;

        weld1Term.SetPointVector( -0.5 * value * Axis );
        weld2Term.SetPointVector( 0.5 * value * Axis );

        _length.Value = value;
      }
    }

    public Vector3d Reference
    {
      get
      {
        return Vector3d.up;
      }
    }

    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
  }
}