using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.VenturiTube)]
  public class VenturiTube : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      Term1,
      Term2,
    }

    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;

    public VenturiTube( Document document ) : base( document )
    {
      ComponentName = "VenturiTube" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.Term1 );
      AddNewConnectPoint( (int) ConnectPointType.Term2 );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as VenturiTube;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    public Vector3d Direction
    {
      get
      {
        return (Term2ConnectPoint.Point - Term1ConnectPoint.Point).normalized;
      }
    }

    public double Length
    {
      get
      {
        return (Term2ConnectPoint.Point - Term1ConnectPoint.Point).magnitude;
      }
    }



    public double Diameter
    {
      get
      {
        return Term1ConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        term2.Diameter = term1.Diameter ;
      }
    }

    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
	}
}
