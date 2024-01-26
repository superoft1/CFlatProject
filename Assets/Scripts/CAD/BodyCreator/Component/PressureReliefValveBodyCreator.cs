using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PressureReliefValveBodyCreator : BodyCreator<PressureReliefValve, Body>
  {
    public PressureReliefValveBodyCreator( Entity _entity ) : base( _entity ) { }
    
    
    protected override void SetupMaterials( Body body, PressureReliefValve pressureReliefValve )
    {
      // 自動ルーティング要素であれば、色を設定する
      if ( Topology.Route.HasColor(entity, out Color newColor) ) {
        ChangeMaterialColor( body, newColor ) ;
      }
      else {
        var impl = body.MainObject.GetComponent<PressureReliefValveBodyImpl>() ;

        impl.Inlet.GetComponent<MeshRenderer>().material = GetMaterial(body, pressureReliefValve) ;
        impl.Outlet.GetComponent<MeshRenderer>().material = GetMaterial( body, pressureReliefValve ) ;
        impl.Bonnet.GetComponent<MeshRenderer>().material = GetMaterial( body, pressureReliefValve ) ;
        impl.Cap.GetComponent<MeshRenderer>().material = GetMaterial( body, pressureReliefValve ) ;
      }
    }
    

    protected override void SetupGeometry( Body body, PressureReliefValve pressureReliefValve )
    {
      var go = body.gameObject;
      
      go.transform.localPosition = (Vector3)pressureReliefValve.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
      
      var impl = body.MainObject.GetComponent<PressureReliefValveBodyImpl>();

      impl.Inlet.transform.localScale = new Vector3( (float)pressureReliefValve.InletLength, (float)pressureReliefValve.InletDiameter, (float)pressureReliefValve.InletDiameter ) * ModelScale;
      impl.Outlet.transform.localScale = new Vector3( (float)pressureReliefValve.OutletLength, (float)pressureReliefValve.OutletDiameter, (float)pressureReliefValve.OutletDiameter ) * ModelScale;
      impl.Bonnet.transform.localScale = new Vector3( (float)pressureReliefValve.BonnetLength / 2, (float)pressureReliefValve.BonnetDiameter, (float)pressureReliefValve.BonnetDiameter ) * ModelScale;
      impl.Bonnet.transform.localPosition = (Vector3)(pressureReliefValve.Axis * pressureReliefValve.BonnetLength / 2) ;
      impl.Cap.transform.localScale = new Vector3( (float)pressureReliefValve.CapLength / 2, (float)pressureReliefValve.CapDiameter, (float)pressureReliefValve.CapDiameter ) * ModelScale;
      impl.Cap.transform.localPosition = (Vector3)(pressureReliefValve.Axis * (pressureReliefValve.BonnetLength + pressureReliefValve.CapLength / 2)) ;
    }
  }

}