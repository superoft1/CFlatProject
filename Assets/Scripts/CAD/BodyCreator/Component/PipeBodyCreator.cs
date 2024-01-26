using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipeBodyCreator : BodyCreator<Pipe, Body>
  {
    public PipeBodyCreator( Entity _entity ) : base( _entity )
    {}

    protected override void SetupMaterials( Body body, Pipe entity )
    {
      // 自動ルーティング要素であれば、色を設定する
      if ( Topology.Route.HasColor(entity, out Color newColor) ) {
        ChangeMaterialColor( body, newColor ) ;
      }
      else {
        base.SetupMaterials( body, entity );
      }
    }

    protected override void SetupGeometry( Body body, Pipe entity )
    {
      var go = body.gameObject;

      go.transform.localScale = new Vector3((float)entity.Length / 2f, (float)entity.Diameter, (float)entity.Diameter) * ModelScale;
      go.transform.localPosition = (Vector3)entity.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
      
      // 自動ルーティング要素であれば、インシュレーションを設定する
      if ( Topology.Route.GetInsulationThickness( entity, entity.Diameter, out double thickness ) ) {
        entity.InsulationThickness = thickness ;
      }
      
      SetupInsulation( body, entity );
    }

    private void SetupInsulation( Body body, Pipe entity )
    {
      if ( entity.InsulationThickness < Tolerance.DistanceTolerance ||
           entity.Length < Tolerance.DistanceTolerance ) {
        return;
      }

      var termPoint = new Vector3[] { (Vector3)entity.GetConnectPoint( 0 ).Point,
                                      (Vector3)entity.GetConnectPoint( 1 ).Point };
      var termVector = new Vector3[] { (Vector3)entity.GetConnectPoint( 0 ).Vector.normalized,
                                       (Vector3)entity.GetConnectPoint( 1 ).Vector.normalized };

      var dist = (float)(entity.Length / 4.0); // 4分の1程度内側へ移動
      termPoint[0] -= termVector[0] * dist;
      termPoint[1] -= termVector[1] * dist;

      var terms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[0], termVector[0] ), 
                                                            ( termPoint[1], termVector[1] ) };

      AppendOffsetMesh( body.MainObject, (float)entity.InsulationThickness, terms, body.gameObject.transform.localScale );
      AddFadeMaterial( body.MainObject );
    }
  }
}
