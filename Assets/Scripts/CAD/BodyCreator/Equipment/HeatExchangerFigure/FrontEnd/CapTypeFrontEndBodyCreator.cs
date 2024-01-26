using System;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class CapTypeFrontEndBodyCreator : BodyCreator<CapTypeFrontEnd, CapTypeFrontEndBody>
	{
    public CapTypeFrontEndBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupMaterials(CapTypeFrontEndBody body, CapTypeFrontEnd frontEnd)
    {
      SetupMaterial(body.CapBody, body, frontEnd);
      SetupMaterial(body.CylinderBody, body, frontEnd);
      SetupMaterial(body.FlangeBody, body, frontEnd);
    }

    private void SetupMaterial(GameObject go, CapTypeFrontEndBody body, CapTypeFrontEnd frontEnd)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, frontEnd);
    }

    protected override void SetupGeometry(CapTypeFrontEndBody body, CapTypeFrontEnd frontEnd)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateCylinder(go, frontEnd, body);
      CreateCap(go, frontEnd, body);
      CreateFlange(go, frontEnd, body);
    }

    void CreateCylinder(GameObject top, CapTypeFrontEnd frontEnd, CapTypeFrontEndBody body)
    {
      var cylinderBody = body.CylinderBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      cylinderBody.transform.parent = top.transform;
      cylinderBody.transform.localPosition = new Vector3(0.0f, (float)(-0.5 * frontEnd.LengthOfTube), (float)frontEnd.HeatExchanger.Shell.HeightOfFrontEndCenter);
      cylinderBody.transform.localRotation = Quaternion.identity;
      var diameterOfFrontEnd = frontEnd.HeatExchanger.Shell.DiameterOfFrontEnd;
      cylinderBody.transform.localScale = new Vector3((float)diameterOfFrontEnd, (float)(0.5 * frontEnd.LengthOfTube), (float)diameterOfFrontEnd);
      body.CylinderBody = cylinderBody;
    }

    void CreateFlange(GameObject top, CapTypeFrontEnd frontEnd, CapTypeFrontEndBody body)
    {
      var flangeBody = body.FlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      flangeBody.transform.parent = top.transform;
      var flangeThickness = frontEnd.HeatExchanger.Shell.ThicknessOfFlange;
      flangeBody.transform.localPosition = new Vector3(0.0f, (float)(-0.5 * flangeThickness), (float)frontEnd.HeatExchanger.Shell.HeightOfFrontEndCenter);
      flangeBody.transform.localRotation = Quaternion.identity;
      var flangeDiameter = frontEnd.HeatExchanger.Shell.DiameterOfFrontEndFlange;
      flangeBody.transform.localScale = new Vector3((float)flangeDiameter, (float)(0.5 * flangeThickness), (float)flangeDiameter);
      body.FlangeBody = flangeBody;
    }

    void CreateCap(GameObject top, CapTypeFrontEnd frontEnd, CapTypeFrontEndBody body)
    {
      var capBody = body.CapBody ?? GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody.transform.parent = top.transform;
      capBody.transform.localPosition = new Vector3(0.0f, (float)(-frontEnd.LengthOfTube), (float)frontEnd.HeatExchanger.Shell.HeightOfFrontEndCenter);
      capBody.transform.localRotation = Quaternion.identity;
      var diameterOfFrontEnd = frontEnd.HeatExchanger.Shell.DiameterOfFrontEnd;
      capBody.transform.localScale = new Vector3((float)diameterOfFrontEnd, (float)(2.0 * frontEnd.CapLength), (float)diameterOfFrontEnd);
      body.CapBody = capBody;
    }
  }
}