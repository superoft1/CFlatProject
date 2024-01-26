using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using System;

namespace Chiyoda.CAD.Body
{

  public class AngleTStrainerBodyCreator : BodyCreator<AngleTStrainer, Body>
  {
    public AngleTStrainerBodyCreator(Entity _entity) : base(_entity)
    {}

    protected override void SetupMaterials( Body body, AngleTStrainer tee )
    {
      // TODO: tee.IsHighlighted
      var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>();
      impl.MainPipe.GetComponent<MeshRenderer>().material = GetMaterial( body, tee );
      impl.ReferencePipe.GetComponent<MeshRenderer>().material = GetMaterial( body, tee );
    }

    protected override void SetupGeometry( Body body, AngleTStrainer tee )
    {
      var go = body.gameObject;
      go.transform.position = (Vector3)tee.Origin;
      var cross = Vector3d.Cross( tee.AxisVector, tee.ReferenceVector );
      go.transform.rotation = Quaternion.LookRotation( (Vector3)cross, (Vector3)tee.ReferenceVector );

      var impl = body.MainObject.GetComponent<PipingTeeBodyImpl>();
      impl.MainPipe.transform.localScale = new Vector3( (float)tee.AxisLength / 2, (float)tee.AxisDiameter, (float)tee.AxisDiameter ) * ModelScale;
      impl.ReferencePipe.transform.localScale = new Vector3( (float)tee.ConvexDiameter, (float)tee.ConvexDiameter, (float)tee.ReferenceLength ) * ModelScale;
    }
  }

}