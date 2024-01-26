using System;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class FlatTypeRearEndBodyCreator : BodyCreator<FlatTypeRearEnd, FlatTypeRearEndBody>
  {
    public FlatTypeRearEndBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupMaterials(FlatTypeRearEndBody body, FlatTypeRearEnd rearEnd)
    {
      SetupMaterial(body.PackFlangeBody, body, rearEnd);
      SetupMaterial(body.CylinderBody, body, rearEnd);
      SetupMaterial(body.FlangeBody, body, rearEnd);
    }

    private void SetupMaterial(GameObject go, FlatTypeRearEndBody body, FlatTypeRearEnd rearEnd)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, rearEnd);
    }

    protected override void SetupGeometry(FlatTypeRearEndBody body, FlatTypeRearEnd rearEnd)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateCylinder(go, rearEnd, body);
      CreateFlanges(go, rearEnd, body);
    }

    void CreateCylinder(GameObject top, FlatTypeRearEnd rearEnd, FlatTypeRearEndBody body)
    {
      var cylinderBody = body.CylinderBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      cylinderBody.transform.parent = top.transform;
      cylinderBody.transform.localPosition = new Vector3(0.0f, (float)(rearEnd.HeatExchanger.Shell.LengthOfShell + 0.5 * rearEnd.Length), (float)rearEnd.HeatExchanger.Shell.HeightOfRearEndCenter);
      cylinderBody.transform.localRotation = Quaternion.identity;
      var diameterOfRearEnd = rearEnd.HeatExchanger.Shell.DiameterOfRearEnd;
      cylinderBody.transform.localScale = new Vector3((float)diameterOfRearEnd, (float)(0.5 * rearEnd.Length), (float)diameterOfRearEnd);
      body.CylinderBody = cylinderBody;
    }

    void CreateFlanges(GameObject top, FlatTypeRearEnd rearEnd, FlatTypeRearEndBody body)
    {
      var flangeThickness = rearEnd.HeatExchanger.Shell.ThicknessOfFlange;
      var flangeDiameter = rearEnd.HeatExchanger.Shell.DiameterOfRearEndFlange;

      var flangeBody = body.FlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      flangeBody.transform.parent = top.transform;
      flangeBody.transform.localPosition = new Vector3(0.0f, (float)(rearEnd.HeatExchanger.Shell.LengthOfShell + 0.5 * flangeThickness), (float)rearEnd.HeatExchanger.Shell.HeightOfRearEndCenter);
      flangeBody.transform.localRotation = Quaternion.identity;
      flangeBody.transform.localScale = new Vector3((float)flangeDiameter, (float)(0.5 * flangeThickness), (float)flangeDiameter);
      body.FlangeBody = flangeBody;

      var packFlangeBody = body.PackFlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      packFlangeBody.transform.parent = top.transform;
      packFlangeBody.transform.localPosition = new Vector3(0.0f, (float)(rearEnd.HeatExchanger.Shell.LengthOfShell + rearEnd.Length - 0.5 * flangeThickness), (float)rearEnd.HeatExchanger.Shell.HeightOfRearEndCenter);
      packFlangeBody.transform.localRotation = Quaternion.identity;
      packFlangeBody.transform.localScale = new Vector3((float)flangeDiameter, (float)(0.5 * flangeThickness), (float)flangeDiameter);
      body.PackFlangeBody = packFlangeBody;
    }
  }
}