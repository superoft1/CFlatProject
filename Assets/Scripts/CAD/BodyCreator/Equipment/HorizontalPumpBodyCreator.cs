using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class HorizontalPumpBodyCreator : BodyCreator<HorizontalPump, HorizontalPumpBody>
  {
    public HorizontalPumpBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( HorizontalPumpBody body, HorizontalPump pump )
    {
      SetupMaterial( body.BasePlateBody, body, pump, true );
      SetupMaterial( body.ImpellerBody, body, pump, false );
      SetupMaterial( body.MotorBody, body, pump, false );
      SetupMaterial( body.DriverBody, body, pump, false );
    }
    
    private void SetupMaterial( GameObject go, HorizontalPumpBody body, HorizontalPump pump, bool isBasePlate )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, pump, isBasePlate );
    }
    
    protected override void SetupGeometry(HorizontalPumpBody body, HorizontalPump pump)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;// (Vector3)pump.Location;
      CreatePumpBase(go, pump, body);

      go.transform.localRotation = Quaternion.identity;// pump.Rotation;
    }

    GameObject CreateCubeBody(GameObject top)
    {
      var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
      body.transform.parent = top.transform;
      return body;
    }

    void CreatePumpBase(GameObject top, HorizontalPump pump, HorizontalPumpBody _body)
    {
      var foundationBody = CreateCubeBody(top);
      foundationBody.transform.localPosition = (Vector3)pump.BasePlateLocalCenter;
      foundationBody.transform.localScale = (Vector3)pump.BasePlateLocalSize;
      _body.BasePlateBody = foundationBody;

      var impellerBody = CreateCubeBody(top);
      impellerBody.transform.localPosition = (Vector3)pump.ImpellerLocalCenter;
      impellerBody.transform.localScale = (Vector3)pump.ImpellerLocalSize;
      _body.ImpellerBody = impellerBody;

      var motorBody = CreateCubeBody(top);
      motorBody.transform.localPosition = (Vector3)pump.MotorLocalCenter;
      motorBody.transform.localScale = (Vector3)pump.MotorLocalSize;
      _body.MotorBody = motorBody;

      var driverBody = CreateCubeBody(top);
      driverBody.transform.localPosition = (Vector3)pump.DriverLocalCenter;
      driverBody.transform.localScale = (Vector3)pump.DriverLocalSize;
      _body.DriverBody = driverBody;
    }
  }
}
