using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class BlankSpectacleBlankBodyCreator : BodyCreator<BlankSpectacleBlank, Body>
  {
    public BlankSpectacleBlankBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, BlankSpectacleBlank blank )
    {
      var cylinder = body.gameObject;
      cylinder.transform.localScale = new Vector3( (float)blank.Length / 2f, (float)blank.Diameter, (float)blank.Diameter ) * ModelScale;
      cylinder.transform.localPosition = (Vector3)blank.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}