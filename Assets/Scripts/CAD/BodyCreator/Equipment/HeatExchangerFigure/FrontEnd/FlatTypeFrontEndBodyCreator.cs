using System;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class FlatTypeFrontEndBodyCreator : BodyCreator<FlatTypeFrontEnd, FlatTypeFrontEndBody>
  {
    public FlatTypeFrontEndBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupMaterials(FlatTypeFrontEndBody body, FlatTypeFrontEnd frontEnd)
    {
      SetupMaterial(body.PackFlangeBody, body, frontEnd);
      SetupMaterial(body.CylinderBody, body, frontEnd);
      SetupMaterial(body.FlangeBody, body, frontEnd);
    }

    private void SetupMaterial(GameObject go, FlatTypeFrontEndBody body, FlatTypeFrontEnd frontEnd)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, frontEnd);
    }

    protected override void SetupGeometry(FlatTypeFrontEndBody body, FlatTypeFrontEnd frontEnd)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateCylinder(go, frontEnd, body);
      CreateFlanges(go, frontEnd, body);
    }

    void CreateCylinder(GameObject top, FlatTypeFrontEnd frontEnd, FlatTypeFrontEndBody body)
    {
      var cylinderBody = body.CylinderBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      cylinderBody.transform.parent = top.transform;
      cylinderBody.transform.localPosition = new Vector3(0.0f, (float)(-0.5 * frontEnd.Length), (float)frontEnd.HeatExchanger.Shell.HeightOfFrontEndCenter);
      cylinderBody.transform.localRotation = Quaternion.identity;
      var diameterOfFrontEnd = frontEnd.HeatExchanger.Shell.DiameterOfFrontEnd;
      cylinderBody.transform.localScale = new Vector3((float)diameterOfFrontEnd, (float)(0.5 * frontEnd.Length), (float)diameterOfFrontEnd);
      body.CylinderBody = cylinderBody;
    }

    void CreateFlanges(GameObject top, FlatTypeFrontEnd frontEnd, FlatTypeFrontEndBody body)
    {
      var flangeThickness = frontEnd.HeatExchanger.Shell.ThicknessOfFlange;
      var flangeDiameter = frontEnd.HeatExchanger.Shell.DiameterOfFrontEndFlange;

      var flangeBody = body.FlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      flangeBody.transform.parent = top.transform;
      flangeBody.transform.localPosition = new Vector3(0.0f, (float)(-0.5 * flangeThickness), (float)frontEnd.HeatExchanger.Shell.HeightOfFrontEndCenter);
      flangeBody.transform.localRotation = Quaternion.identity;
      flangeBody.transform.localScale = new Vector3((float)flangeDiameter, (float)(0.5 * flangeThickness), (float)flangeDiameter);
      body.FlangeBody = flangeBody;

      var packFlangeBody = body.PackFlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      packFlangeBody.transform.parent = top.transform;
      packFlangeBody.transform.localPosition = new Vector3(0.0f, (float)(-frontEnd.Length + 0.5 * flangeThickness), (float)frontEnd.HeatExchanger.Shell.HeightOfFrontEndCenter);
      packFlangeBody.transform.localRotation = Quaternion.identity;
      packFlangeBody.transform.localScale = new Vector3((float)flangeDiameter, (float)(0.5 * flangeThickness), (float)flangeDiameter);
      body.PackFlangeBody = packFlangeBody;
    }
  }
}