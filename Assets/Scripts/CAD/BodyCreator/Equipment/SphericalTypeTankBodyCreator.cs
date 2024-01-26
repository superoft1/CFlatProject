using System.Collections.Generic;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class SphericalTypeTankBodyCreator : BodyCreator<SphericalTypeTank, SphericalTypeTankBody>
  {
    public SphericalTypeTankBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( SphericalTypeTankBody body, SphericalTypeTank tank )
    {
      SetupMaterial( body.TankBody, body, tank );
      SetupMaterial( body.NorthLegBody, body, tank );
      SetupMaterial( body.SouthLegBody, body, tank );
      SetupMaterial( body.EastLegBody, body, tank );
      SetupMaterial( body.WestLegBody, body, tank );
    }
    
    private void SetupMaterial( GameObject go, SphericalTypeTankBody body, SphericalTypeTank tank )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, tank );
    }
    
    protected override void SetupGeometry(SphericalTypeTankBody body, SphericalTypeTank tank)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateTank(go, tank, body);
      CreateLegs(go, tank, body);
    }

    void CreateTank(GameObject top, SphericalTypeTank tank, SphericalTypeTankBody body)
    {
      var sphereBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphereBody.transform.parent = top.transform;
      sphereBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(tank.HeightOfP1FromBase));
      sphereBody.transform.localRotation = Quaternion.identity;
      sphereBody.transform.localScale = new Vector3((float)tank.DiameterOfCylinder, (float)tank.DiameterOfCylinder, (float)tank.DiameterOfCylinder);
      body.TankBody = sphereBody;
    }

    void CreateLegs(GameObject top, SphericalTypeTank tank, SphericalTypeTankBody body)
    {
      var northLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      northLegBody.transform.parent = top.transform;
      northLegBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * tank.DiameterOfCylinder), (float)(0.5 * tank.HeightOfP1FromBase));
      northLegBody.transform.localRotation = Quaternion.identity;
      northLegBody.transform.localScale = new Vector3((float)tank.LegThickness, (float)tank.LegThickness, (float)tank.HeightOfP1FromBase);
      body.NorthLegBody = northLegBody;

      var southLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      southLegBody.transform.parent = top.transform;
      southLegBody.transform.localPosition = new Vector3(0.0f, -(float)(0.5 * tank.DiameterOfCylinder), (float)(0.5 * tank.HeightOfP1FromBase));
      southLegBody.transform.localRotation = Quaternion.identity;
      southLegBody.transform.localScale = new Vector3((float)tank.LegThickness, (float)tank.LegThickness, (float)tank.HeightOfP1FromBase);
      body.SouthLegBody = southLegBody;

      var eastLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      eastLegBody.transform.parent = top.transform;
      eastLegBody.transform.localPosition = new Vector3((float)(0.5 * tank.DiameterOfCylinder), 0.0f, (float)(0.5 * tank.HeightOfP1FromBase));
      eastLegBody.transform.localRotation = Quaternion.identity;
      eastLegBody.transform.localScale = new Vector3((float)tank.LegThickness, (float)tank.LegThickness, (float)tank.HeightOfP1FromBase);
      body.EastLegBody = eastLegBody;

      var westLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      westLegBody.transform.parent = top.transform;
      westLegBody.transform.localPosition = new Vector3(-(float)(0.5 * tank.DiameterOfCylinder), 0.0f, (float)(0.5 * tank.HeightOfP1FromBase));
      westLegBody.transform.localRotation = Quaternion.identity;
      westLegBody.transform.localScale = new Vector3((float)tank.LegThickness, (float)tank.LegThickness, (float)tank.HeightOfP1FromBase);
      body.WestLegBody = westLegBody;
    }

  }
}
