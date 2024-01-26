using System;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class CapTypeRearEndBodyCreator : BodyCreator<CapTypeRearEnd, CapTypeRearEndBody>
	{
    public CapTypeRearEndBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupMaterials(CapTypeRearEndBody body, CapTypeRearEnd rearEnd)
    {
      SetupMaterial(body.CapBody, body, rearEnd);
      SetupMaterial(body.CylinderBody, body, rearEnd);
      SetupMaterial(body.FlangeBody, body, rearEnd);
    }

    private void SetupMaterial(GameObject go, CapTypeRearEndBody body, CapTypeRearEnd rearEnd)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, rearEnd);
    }

    protected override void SetupGeometry(CapTypeRearEndBody body, CapTypeRearEnd rearEnd)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateCylinder(go, rearEnd, body);
      CreateCap(go, rearEnd, body);
      CreateFlange(go, rearEnd, body);
    }

    void CreateCylinder(GameObject top, CapTypeRearEnd rearEnd, CapTypeRearEndBody body)
    {
      var cylinderBody = body.CylinderBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      cylinderBody.transform.parent = top.transform;
      cylinderBody.transform.localPosition = new Vector3(0.0f, (float)(rearEnd.HeatExchanger.Shell.LengthOfShell + 0.5 * rearEnd.LengthOfTube), (float)rearEnd.HeatExchanger.Shell.HeightOfRearEndCenter);
      cylinderBody.transform.localRotation = Quaternion.identity;
      var diameterOfRearEnd = rearEnd.HeatExchanger.Shell.DiameterOfRearEnd;
      cylinderBody.transform.localScale = new Vector3((float)diameterOfRearEnd, (float)(0.5 * rearEnd.LengthOfTube), (float)diameterOfRearEnd);
      body.CylinderBody = cylinderBody;
    }

    void CreateFlange(GameObject top, CapTypeRearEnd rearEnd, CapTypeRearEndBody body)
    {
      var flangeBody = body.FlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      flangeBody.transform.parent = top.transform;
      var flangeThickness = rearEnd.HeatExchanger.Shell.ThicknessOfFlange;
      flangeBody.transform.localPosition = new Vector3(0.0f, (float)(rearEnd.HeatExchanger.Shell.LengthOfShell + 0.5 * flangeThickness), (float)rearEnd.HeatExchanger.Shell.HeightOfRearEndCenter);
      flangeBody.transform.localRotation = Quaternion.identity;
      var flangeDiameter = rearEnd.HeatExchanger.Shell.DiameterOfRearEndFlange;
      flangeBody.transform.localScale = new Vector3((float)flangeDiameter, (float)(0.5 * flangeThickness), (float)flangeDiameter);
      body.FlangeBody = flangeBody;
    }

    void CreateCap(GameObject top, CapTypeRearEnd rearEnd, CapTypeRearEndBody body)
    {
      var capBody = body.CapBody ?? GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody.transform.parent = top.transform;
      capBody.transform.localPosition = new Vector3(0.0f, (float)(rearEnd.HeatExchanger.Shell.LengthOfShell + rearEnd.LengthOfTube), (float)rearEnd.HeatExchanger.Shell.HeightOfRearEndCenter);
      capBody.transform.localRotation = Quaternion.identity;
      var diameterOfRearEnd = rearEnd.HeatExchanger.Shell.DiameterOfRearEnd;
      capBody.transform.localScale = new Vector3((float)diameterOfRearEnd, (float)(2.0 * rearEnd.CapLength), (float)diameterOfRearEnd);
      body.CapBody = capBody;
    }
  }
}