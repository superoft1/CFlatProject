using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingCapBodyCreator : BodyCreator<PipingCap, Body>
  {
    public PipingCapBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, PipingCap pipingCap )
    {
      var cap = body.gameObject;

      cap.transform.localScale = new Vector3((float)pipingCap.Length / 2f, (float)pipingCap.Diameter, (float)pipingCap.Diameter ) * ModelScale;
      cap.transform.localPosition = (Vector3)pipingCap.Origin;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;
    }
  }
}