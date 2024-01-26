using System ;
using Chiyoda.CAD.Core;
using Chiyoda.DB ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipingElbow45 )]
  public class PipingElbow45 : Component
  {
    private static readonly double SQR1_2 = Math.Sqrt( 0.5 ) ;

    public enum ConnectPointType
    {
      Term1,
      Term2,
    }

    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;

    public PipingElbow45( Document document ) : base( document )
    {
      ComponentName = "Elbow45" ;
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

      var entity = another as PipingElbow45;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      switch ((ConnectPointType)connectPointNumber)
      {
        case ConnectPointType.Term1:
        case ConnectPointType.Term2:
          {
            var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
            BendLength = GetDefaultBendLength(afterDiameter);
            GetConnectPoint((int)ConnectPointType.Term1).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            GetConnectPoint((int)ConnectPointType.Term2).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // ElbOletには伝播しない
            break;
          }

        default:  // ElbOlet
          {
            GetConnectPoint(connectPointNumber).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // 他のConnectPointには伝播しない
            break;
          }
      }
    }

    public Vector3d Term1 => Term1ConnectPoint.Point ;
    public Vector3d Term2 => Term2ConnectPoint.Point ;

    public double BendLength
    {
      get => -Term2.x ;
      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.SetPointVector( new Vector3d( +value * SQR1_2, -value * SQR1_2, 0 ) );
        term2.SetPointVector( new Vector3d( -value, 0, 0 ) );
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
      var bounds = new Bounds((Vector3)Origin, Vector3.one * (float)Diameter);
      bounds.Encapsulate(new Bounds((Vector3)Term1, Vector3.one * (float)Diameter));
      bounds.Encapsulate(new Bounds((Vector3)Term2, Vector3.one * (float)Diameter));
      return bounds;
    }
  
    public double GetDefaultBendLength( double diameter )
    {
      var diameterObj = DiameterFactory.FromOutsideMeter( diameter ) ;
      var table = Chiyoda.DB.DB.Get<DimensionOfElbowsTable>() ;
      return ((double)table.GetOne( diameterObj.NpsMm, "45deg" ).CenterToEnd).Millimeters() ;
    }
  }
}