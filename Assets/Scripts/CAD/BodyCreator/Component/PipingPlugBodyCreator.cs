using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingPlugBodyCreator : BodyCreator<PipingPlug, Body>
  {
    public PipingPlugBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, PipingPlug pipingPlug )
    {
      var cap = body.gameObject;

      cap.transform.localScale = new Vector3( (float)pipingPlug.Length / 2f, (float)pipingPlug.Diameter, (float)pipingPlug.Diameter ) * ModelScale;
      cap.transform.localPosition = (Vector3)pipingPlug.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}