using System;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class KettleTypeShellBodyCreator : BodyCreator<KettleTypeShell, KettleTypeShellBody>
  {
    public KettleTypeShellBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupMaterials(KettleTypeShellBody body, KettleTypeShell shell)
    {
      SetupMaterial(body.DrumBody, body, shell);
      SetupMaterial(body.TaperBody, body, shell);
      SetupMaterial(body.TipBody, body, shell);
      SetupMaterial(body.Saddle1Body, body, shell);
      SetupMaterial(body.Saddle2Body, body, shell);
      SetupMaterial(body.RearEndFlangeBody, body, shell);
      SetupMaterial(body.FrontEndFlangeBody, body, shell);
    }

    private void SetupMaterial(GameObject go, KettleTypeShellBody body, KettleTypeShell shell)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, shell);
    }

    protected override void SetupGeometry(KettleTypeShellBody body, KettleTypeShell shell)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateDrum(go, shell, body);
      CreateTip(go, shell, body);
      CreateTaper(go, shell, body);
      CreateSaddles(go, shell, body);
      CreateFlanges(go, shell, body);
    }

    void CreateDrum(GameObject top, KettleTypeShell shell, KettleTypeShellBody body)
    {
      var drumBody = body.DrumBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      drumBody.transform.parent = top.transform;
      drumBody.transform.localPosition = new Vector3(0.0f, (float)(shell.LengthOfTip + shell.LengthOfCone + 0.5 * shell.LengthOfTube), (float)(shell.HeightOfSaddle + 0.5 * shell.DiameterOfDrum));
      drumBody.transform.localRotation = Quaternion.identity;
      drumBody.transform.localScale = new Vector3((float)shell.DiameterOfDrum, (float)(0.5 * shell.LengthOfTube), (float)shell.DiameterOfDrum);
      body.DrumBody = drumBody;
    }

    void CreateTip(GameObject top, KettleTypeShell shell, KettleTypeShellBody body)
    {
      var tipBody = body.TipBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      tipBody.transform.parent = top.transform;
      tipBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * shell.LengthOfTip), (float)(shell.HeightOfSaddle + 0.5 * shell.DiameterOfDrum));
      tipBody.transform.localRotation = Quaternion.identity;
      tipBody.transform.localScale = new Vector3((float)shell.DiameterOfTip, (float)(0.5 * shell.LengthOfTip), (float)shell.DiameterOfTip);
      body.TipBody = tipBody;
    }

    void CreateTaper(GameObject top, KettleTypeShell shell, KettleTypeShellBody body)
    {
      var taperBody = body.TaperBody ?? GameObjectUtil.CreateTaperBody((float)(shell.DiameterOfTip / 2.0), (float)(shell.DiameterOfDrum / 2.0), (float)shell.LengthOfCone);
      taperBody.transform.parent = top.transform;
      taperBody.transform.localPosition = new Vector3(0.0f, (float)shell.LengthOfTip, (float)(shell.HeightOfSaddle + 0.5 * shell.DiameterOfDrum));
      taperBody.transform.localRotation = Quaternion.identity;
      taperBody.transform.localScale = Vector3.one;
      body.TaperBody = taperBody;
    }

    void CreateSaddles(GameObject top, KettleTypeShell shell, KettleTypeShellBody body)
    {
      var modelingSaddleHeight = shell.HeightOfSaddle + 0.5 * shell.DiameterOfDrum;
      var saddle1Body = body.Saddle1Body ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddle1Body.transform.parent = top.transform;
      saddle1Body.transform.localPosition = new Vector3(0.0f, (float)(shell.LengthOfTip + shell.LengthOfCone + shell.DistanceOf1stSaddle), (float)(0.5 * modelingSaddleHeight));
      saddle1Body.transform.localRotation = Quaternion.identity;
      saddle1Body.transform.localScale = new Vector3((float)shell.LengthOfSaddle, (float)shell.WidthOfSaddle, (float)modelingSaddleHeight);
      body.Saddle1Body = saddle1Body;

      var saddle2Body = body.Saddle2Body ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddle2Body.transform.parent = top.transform;
      saddle2Body.transform.localPosition = new Vector3(0.0f, (float)(shell.LengthOfTip + shell.LengthOfCone + shell.DistanceOf2ndSaddle), (float)(0.5 * modelingSaddleHeight));
      saddle2Body.transform.localRotation = Quaternion.identity;
      saddle2Body.transform.localScale = new Vector3((float)shell.LengthOfSaddle, (float)shell.WidthOfSaddle, (float)modelingSaddleHeight);
      body.Saddle2Body = saddle2Body;
    }

    void CreateFlanges(GameObject top, KettleTypeShell shell, KettleTypeShellBody body)
    {
      var frontEndFlange = body.FrontEndFlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      frontEndFlange.transform.parent = top.transform;
      frontEndFlange.transform.localPosition = new Vector3(0.0f, (float)(0.5 * shell.ThicknessOfFlange), (float)(shell.HeightOfSaddle + 0.5 * shell.DiameterOfDrum));
      frontEndFlange.transform.localRotation = Quaternion.identity;
      frontEndFlange.transform.localScale = new Vector3((float)shell.DiameterOfFrontEndFlange, (float)(0.5 * shell.ThicknessOfFlange), (float)shell.DiameterOfFrontEndFlange);
      body.FrontEndFlangeBody = frontEndFlange;

      var rearEndFlange = body.RearEndFlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      rearEndFlange.transform.parent = top.transform;
      rearEndFlange.transform.localPosition = new Vector3(0.0f, (float)(shell.LengthOfTip + shell.LengthOfCone + shell.LengthOfTube - 0.5 * shell.ThicknessOfFlange), (float)(shell.HeightOfSaddle + 0.5 * shell.DiameterOfDrum));
      rearEndFlange.transform.localRotation = Quaternion.identity;
      rearEndFlange.transform.localScale = new Vector3((float)shell.DiameterOfRearEndFlange, (float)(0.5 * shell.ThicknessOfFlange), (float)shell.DiameterOfRearEndFlange);
      body.RearEndFlangeBody = rearEndFlange;
    }
  }
}