using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class SockOletBodyCreator : BodyCreator<SockOlet, Body>
  {
    public SockOletBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, SockOlet sockOlet )
    {
      var olet = body.gameObject;

      olet.transform.localScale = new Vector3((float)sockOlet.Length / 2f, (float)sockOlet.Diameter, (float)sockOlet.Diameter) * ModelScale;
      olet.transform.localPosition = (Vector3)sockOlet.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}