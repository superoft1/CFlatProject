// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class ControlValveBodyCreator : BodyCreator<ControlValve, Body>
  {
    public ControlValveBodyCreator( Entity _entity ) : base( _entity )
    {}

    protected override void SetupMaterials( Body body, ControlValve controlValve )
    {
      // 自動ルーティング要素であれば、色を設定する
      if ( Topology.Route.HasColor(entity, out Color newColor) ) {
        ChangeMaterialColor( body, newColor ) ;
      }
      else {
        var impl = body.MainObject.GetComponent<ControlValveBodyImpl>() ;
        var renderers = impl.MainValve.GetComponentsInChildren<MeshRenderer>();
        var material = GetMaterial( body, controlValve ) ;
        foreach (var render in renderers) {
          render.material = material;
        }
        impl.ReferenceOperation.GetComponent<MeshRenderer>().material = GetMaterial( body, controlValve ) ;
      }
    }
    
    protected override void SetupGeometry( Body body, ControlValve controlValve )
    {
      var go = body.gameObject;

      go.transform.localPosition = (Vector3)controlValve.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;

      var impl = body.MainObject.GetComponent<ControlValveBodyImpl>();
      impl.MainValve.transform.localScale = new Vector3( (float)controlValve.Length / 2, (float)controlValve.Diameter, (float)controlValve.Diameter ) * ModelScale;
      impl.ReferenceOperation.transform.localScale = new Vector3( (float)controlValve.DiaphramLength, (float)controlValve.DiaphramDiameter, (float)controlValve.DiaphramDiameter ) * ModelScale;
    }
  }
}
