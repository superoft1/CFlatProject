using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipingPlug )]
  public class PipingPlug : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      WeldTerm,
      OutsideTerm,
    }

    public ConnectPoint WeldTermConnectPoint => GetConnectPoint( (int)ConnectPointType.WeldTerm ) ;
    public ConnectPoint OutsideTermConnectPoint => GetConnectPoint( (int)ConnectPointType.OutsideTerm ) ;
    private readonly Memento<double> _length;

    public PipingPlug( Document document ) : base( document )
    {
      _length = new Memento<double>( this ) ;

      ComponentName = "Plug" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.WeldTerm );
      AddNewConnectPoint( (int) ConnectPointType.OutsideTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as PipingPlug;
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

    public double Length
    {
      get
      {
        return _length.Value;
      }
      set
      {
        var weldTerm = WeldTermConnectPoint ;
        var outsideTerm = OutsideTermConnectPoint ;

        weldTerm.SetPointVector( -0.5 * value * Axis);
        outsideTerm.SetPointVector( 0.5 * value * Axis);

        _length.Value = value;
      }
    }


    public double Diameter
    {
      get
      {
        return WeldTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var weldTerm = WeldTermConnectPoint ;
        var outsideTerm = OutsideTermConnectPoint ;

        weldTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        outsideTerm.Diameter = weldTerm.Diameter ;
      }
    }

    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
  }
}