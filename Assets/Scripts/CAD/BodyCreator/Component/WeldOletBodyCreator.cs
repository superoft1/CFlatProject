using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class WeldOletBodyCreator : BodyCreator<WeldOlet, Body>
  {
    public WeldOletBodyCreator( Entity entity ) : base( entity )
    { }

    protected override void SetupGeometry( Body body, WeldOlet weldOlet )
    {
      var transform = body.MainObject.transform ;

      transform.localScale = new Vector3((float)weldOlet.LengthFromPipeCenter / 2f, (float)weldOlet.Diameter, (float)weldOlet.Diameter) * ModelScale;
      transform.localPosition = (Vector3) weldOlet.Origin + new Vector3( (float) weldOlet.LengthFromPipeCenter / 2f, 0, 0 );
      transform.localRotation = Quaternion.identity;
    }
  }
}