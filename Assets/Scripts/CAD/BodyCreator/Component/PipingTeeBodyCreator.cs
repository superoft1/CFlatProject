using System.Linq;
using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingTeeBodyCreator : BodyCreator<PipingTee, Body>
  {
    public PipingTeeBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupMaterials( Body body, PipingTee tee )
    {
      // 自動ルーティング要素であれば、色を設定する
      if ( Topology.Route.HasColor(entity, out Color newColor) ) {
        ChangeMaterialColor( body, newColor ) ;
      }
      else {
        // TODO: tee.IsHighlighted
        var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>() ;
        impl.MainPipe.GetComponent<MeshRenderer>().material = GetMaterial( body, tee ) ;
        impl.ReferencePipe.GetComponent<MeshRenderer>().material = GetMaterial( body, tee ) ;
      }
    }

    protected override void SetupGeometry( Body body, PipingTee tee )
    {
      var go = body.gameObject;

      go.transform.localPosition = (Vector3)tee.Origin;
      go.transform.localRotation = Quaternion.identity;
      //var cross = -Vector3d.Cross( tee.Axis, tee.Reference );
      //go.transform.localRotation = Quaternion.LookRotation( (Vector3)cross, -(Vector3)tee.Reference );

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;

      var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>();
      impl.MainPipe.transform.localScale = new Vector3( (float)tee.MainLength / 2, (float)tee.MainDiameter, (float)tee.MainDiameter ) * ModelScale;
      impl.ReferencePipe.transform.localScale = new Vector3( (float)tee.BranchDiameter, (float)tee.BranchDiameter, (float)tee.BranchLength ) * ModelScale;

      // 自動ルーティング要素であれば、インシュレーションを設定する
      if ( Topology.Route.GetInsulationThickness( tee, tee.MainDiameter, out double thickness ) ) {
        tee.InsulationThickness = thickness ;
      }

      SetupInsulation( body, tee );
    }

    private void SetupInsulation( Body body, PipingTee tee )
    {
      if ( tee.InsulationThickness < Tolerance.DistanceTolerance ||
           tee.MainLength < Tolerance.DistanceTolerance ||
           tee.BranchLength < Tolerance.DistanceTolerance ) {
        return;
      }

      var termPoint = new Vector3[] { (Vector3)tee.GetConnectPoint( 0 ).Point,
                                      (Vector3)tee.GetConnectPoint( 1 ).Point,
                                      (Vector3)tee.GetConnectPoint( 2 ).Point };
      var termVector = new Vector3[] { (Vector3)tee.GetConnectPoint( 0 ).Vector.normalized,
                                       (Vector3)tee.GetConnectPoint( 1 ).Vector.normalized,
                                       (Vector3)tee.GetConnectPoint( 2 ).Vector.normalized };
      var centerPoint = ( termPoint[0] + termPoint[1] ) / 2;

      var mainDist = (float)(tee.MainLength / 4.0); // 4分の1程度内側へ移動
      termPoint[0] -= termVector[0] * mainDist;
      termPoint[1] -= termVector[1] * mainDist;
      var branchDist = (float)(tee.BranchLength / 4.0); // 4分の1程度内側へ移動
      termPoint[2] -= termVector[2] * branchDist;
      centerPoint += termVector[2] * branchDist;

      var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>();
      var mainTrans = impl.MainPipe.transform.localPosition;
      var branchTrans = impl.ReferencePipe.transform.localPosition;

      var mainScale = impl.MainPipe.transform.localScale;
      mainTrans = Matrix4x4.Scale( mainScale ).MultiplyPoint3x4( mainTrans );
      var branchScale = impl.ReferencePipe.transform.localScale;
      branchTrans = Matrix4x4.Scale( branchScale ).MultiplyPoint3x4( branchTrans );

      var mainTerms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[0] - mainTrans, termVector[0] ), 
                                                                ( termPoint[1] - mainTrans, termVector[1] ) };
      var branchTerms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[2] - branchTrans, termVector[2] ), 
                                                                  ( centerPoint - branchTrans, -termVector[2] ) };

      var branchRot = Matrix4x4.Rotate( impl.ReferencePipe.transform.localRotation ).inverse;
      branchTerms = branchTerms.Select( end => ( branchRot.MultiplyPoint3x4( end.point ),
                                                 branchRot.MultiplyPoint3x4( end.vector ) ) ).ToArray();

      var thickness = (float)tee.InsulationThickness;
      AppendOffsetMesh( impl.MainPipe, thickness, mainTerms, mainScale );
      AppendOffsetMesh( impl.ReferencePipe, thickness, branchTerms, branchScale );
      AddFadeMaterial( impl.MainPipe );
      AddFadeMaterial( impl.ReferencePipe );
    }
  }
}