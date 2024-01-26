using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class BlindFlangeBodyCreator : BodyCreator<BlindFlange, Body>
  {
    public BlindFlangeBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, BlindFlange flange )
    {
      var blind = body.gameObject;

      blind.transform.localScale = new Vector3((float)flange.Length / 2f, (float)flange.Diameter * 1.5f, (float)flange.Diameter * 1.5f) * ModelScale;
      blind.transform.localPosition = (Vector3)flange.Origin;
      blind.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;

      SetupInsulation( body, flange );
    }

    private void SetupInsulation( Body body, BlindFlange flange )
    {
      if ( flange.InsulationThickness < Tolerance.DistanceTolerance ||
           flange.Length < Tolerance.DistanceTolerance ) {
        return;
      }

      var termPoint = (Vector3)flange.GetConnectPoint( 0 ).Point;
      var termVector = (Vector3)flange.GetConnectPoint( 0 ).Vector.normalized;
      termPoint -= termVector * (float)(flange.Length / 4.0); // 4分の1程度内側へ移動

      var terms = new ( Vector3 point, Vector3 vector )[] { ( termPoint, termVector ) };

      AppendOffsetMesh( body.MainObject, (float)flange.InsulationThickness, terms, body.gameObject.transform.localScale );
      AddFadeMaterial( body.MainObject );
    }
  }
}