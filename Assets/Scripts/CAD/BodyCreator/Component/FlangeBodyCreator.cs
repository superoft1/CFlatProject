using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class FlangeBodyCreator : BodyCreator<Flange, Body>
  {
    public FlangeBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, Flange flange )
    {
      var blind = body.gameObject;

      blind.transform.localScale = new Vector3( (float)flange.Length / 2f, (float)flange.Diameter, (float)flange.Diameter) * ModelScale;
      blind.transform.localPosition = (Vector3)flange.Origin;
      blind.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;
    }
  }
}