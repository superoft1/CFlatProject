using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class WeldNeckFlangeBodyCreator : BodyCreator<WeldNeckFlange, Body>
  {
    public WeldNeckFlangeBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, WeldNeckFlange flange )
    {
      var blind = body.gameObject;

      blind.transform.localScale = new Vector3((float)flange.Length / 4f, (float)flange.WeldDiameter, (float)flange.WeldDiameter) * ModelScale;
      blind.transform.localPosition = (Vector3)flange.Origin;
      blind.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;

      SetupInsulation( body, flange );
    }

    private void SetupInsulation( Body body, WeldNeckFlange flange )
    {
      if ( flange.InsulationThickness < Tolerance.DistanceTolerance ||
           flange.Length < Tolerance.DistanceTolerance ) {
        return;
      }

      var termPoint = new Vector3[] { (Vector3)flange.GetConnectPoint( 0 ).Point,
                                      (Vector3)flange.GetConnectPoint( 1 ).Point };
      var termVector = new Vector3[] { (Vector3)flange.GetConnectPoint( 0 ).Vector.normalized,
                                       (Vector3)flange.GetConnectPoint( 1 ).Vector.normalized };
      var centerPoint = new Vector3[] { ( termPoint[0] + termPoint[1] ) / 2,
                                        ( termPoint[0] + termPoint[1] ) / 2 };

      var dist = (float)(flange.Length / 2.0 / 4.0); // 4分の1程度内側へ移動
      termPoint[0] -= termVector[0] * dist;
      termPoint[1] -= termVector[1] * dist;
      centerPoint[0] += termVector[0] * dist;
      centerPoint[1] += termVector[1] * dist;

      var impl = body.MainObject.GetComponent<WeldNeckFlangeBodyImpl>();
      var outsideTrans = impl.Outside.transform.localPosition;
      var weldTrans = impl.Weld.transform.localPosition;

      var outsideScale = Vector3.Scale( body.gameObject.transform.localScale, impl.Outside.transform.localScale );
      outsideTrans = Matrix4x4.Scale( outsideScale ).MultiplyPoint3x4( outsideTrans );
      var weldScale = Vector3.Scale( body.gameObject.transform.localScale, impl.Weld.transform.localScale );
      weldTrans = Matrix4x4.Scale( weldScale ).MultiplyPoint3x4( weldTrans );

      var outsideTerms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[0] - outsideTrans, termVector[0] ),
                                                                   ( centerPoint[0] - outsideTrans, -termVector[0] ) };
      var weldTerms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[1] - weldTrans, termVector[1] ),
                                                                ( centerPoint[1] - weldTrans, -termVector[1] ) };

      var thickness = (float)flange.InsulationThickness;
      AppendOffsetMesh( impl.Outside, thickness, outsideTerms, outsideScale );
      AppendOffsetMesh( impl.Weld, thickness, weldTerms, weldScale );
      AddFadeMaterial( impl.Outside );
      AddFadeMaterial( impl.Weld );
    }
  }
}