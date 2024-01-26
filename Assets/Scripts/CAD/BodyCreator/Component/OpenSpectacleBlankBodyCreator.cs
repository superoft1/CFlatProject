using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class OpenSpectacleBlankBodyCreator : BodyCreator<OpenSpectacleBlank, Body>
  {
    public OpenSpectacleBlankBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, OpenSpectacleBlank blank )
    {
      var cylinder = body.gameObject;

      var diameterScale = (float)blank.Diameter * ModelScale;
      var heightScale = (float)blank.Length / 2f * ModelScale;

      cylinder.transform.localScale = new Vector3(heightScale, diameterScale, diameterScale);
      cylinder.transform.localPosition = (Vector3)blank.Origin;
      cylinder.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }

}