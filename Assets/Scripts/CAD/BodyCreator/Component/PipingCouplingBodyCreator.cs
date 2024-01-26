using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingCouplingBodyCreator : BodyCreator<PipingCoupling, Body>
  {
    public PipingCouplingBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, PipingCoupling pipingCoupling )
    {
      var cylinder = body.gameObject;

      cylinder.transform.localScale = new Vector3( (float)pipingCoupling.Length / 2f, (float)pipingCoupling.Diameter, (float)pipingCoupling.Diameter ) * ModelScale;
      cylinder.transform.localPosition = (Vector3)pipingCoupling.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}
