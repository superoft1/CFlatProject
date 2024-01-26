using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingLateralTeeBodyCreator : BodyCreator<PipingLateralTee, Body>
  {
    public PipingLateralTeeBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupMaterials( Body body, PipingLateralTee tee )
    {
      // TODO: tee.IsHighlighted
      var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>();
      impl.MainPipe.GetComponent<MeshRenderer>().material = GetMaterial( body, tee );
      impl.ReferencePipe.GetComponent<MeshRenderer>().material = GetMaterial( body, tee );
    }

    protected override void SetupGeometry( Body body, PipingLateralTee tee )
    {
      var go = body.gameObject;
      go.transform.localPosition = (Vector3)tee.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;

      var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>();
      impl.MainPipe.transform.localPosition = (Vector3)tee.MainCenter;
      impl.MainPipe.transform.localScale = new Vector3( (float)tee.MainLength / 2, (float)tee.MainDiameter, (float)tee.MainDiameter ) * ModelScale;
      impl.ReferencePipe.transform.localScale = new Vector3( (float)tee.LateralDiameter, (float)tee.LateralDiameter, (float)tee.LateralLength ) * ModelScale;
      impl.ReferencePipe.transform.forward = (Vector3)tee.LateralAxis;
    }
  }
}