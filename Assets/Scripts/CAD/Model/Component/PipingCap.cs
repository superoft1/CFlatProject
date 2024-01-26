using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipingCap )]
  public class PipingCap : Component
  {
    public enum ConnectPointType
    {
      WeldTerm,
    }

    public ConnectPoint WeldTermConnectPoint => GetConnectPoint( (int)ConnectPointType.WeldTerm ) ;
    private readonly Memento<double> length;

    public PipingCap( Document document ) : base( document )
    {
      length = new Memento<double>( this ) ;

      ComponentName = "Cap" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.WeldTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as PipingCap;
      length.CopyFrom( entity.length.Value );
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
        return length.Value;
      }

      set
      {
        WeldTermConnectPoint.SetPointVector( -0.5 * value * Axis);

        length.Value = value;
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
        
        weldTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
      }
    }


    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
  }
}