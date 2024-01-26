using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingElbow90BodyCreator : BodyCreator<PipingElbow90, Body>
  {
    public PipingElbow90BodyCreator( Entity _entity ) : base( _entity )
    { }
    
    protected override void SetupMaterials( Body body, PipingElbow90 entity )
    {
      // 自動ルーティング要素であれば、色を設定する
      if ( Topology.Route.HasColor(entity, out Color newColor) ) {
        ChangeMaterialColor( body, newColor ) ;
      }
      else {
        base.SetupMaterials( body, entity );
      }
    }
    protected override void SetupGeometry( Body body, PipingElbow90 elbow )
    {
      var go = body.gameObject;

      go.transform.localScale = Vector3.one * (float)(elbow.Diameter * ModelScale);
      go.transform.localPosition = (Vector3)elbow.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;

      //var cross = -Vector3d.Cross( elbow.Axis, elbow.Reference );
      //go.transform.localRotation = Quaternion.LookRotation( (Vector3)cross, (Vector3)elbow.Axis );
      
      // 自動ルーティング要素であれば、インシュレーションを設定する
      if ( Topology.Route.GetInsulationThickness( entity, elbow.Diameter, out double thickness ) ) {
        elbow.InsulationThickness = thickness ;
      }
      SetupInsulation( body, elbow );
    }

    private void SetupInsulation( Body body, PipingElbow90 elbow )
    {
      if ( elbow.InsulationThickness < Tolerance.DistanceTolerance ||
           elbow.BendLength < Tolerance.DistanceTolerance ) {
        return;
      }

      var termPoint = new Vector3[] { (Vector3)elbow.GetConnectPoint( 0 ).Point,
                                      (Vector3)elbow.GetConnectPoint( 1 ).Point };
      var termVector = new Vector3[] { (Vector3)elbow.GetConnectPoint( 0 ).Vector.normalized,
                                       (Vector3)elbow.GetConnectPoint( 1 ).Vector.normalized };

      var dist = (float)(elbow.BendLength / 4.0); // 4分の1程度内側へ移動
      termPoint[0] -= termVector[0] * dist;
      termPoint[1] -= termVector[1] * dist;

      var terms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[0], termVector[0] ), 
                                                            ( termPoint[1], termVector[1] ) };

      AppendOffsetMesh( body.MainObject, (float)elbow.InsulationThickness, terms, body.gameObject.transform.localScale );
      AddFadeMaterial( body.MainObject );
    }
  }

}