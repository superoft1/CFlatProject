using System;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class StraightTypeShellBodyCreator : BodyCreator<StraightTypeShell, StraightTypeShellBody>
  {
    public StraightTypeShellBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupMaterials(StraightTypeShellBody body, StraightTypeShell shell)
    {
      SetupMaterial(body.ShellBody, body, shell);
      SetupMaterial(body.Saddle1Body, body, shell);
      SetupMaterial(body.Saddle2Body, body, shell);
      SetupMaterial(body.RearEndFlangeBody, body, shell);
      SetupMaterial(body.FrontEndFlangeBody, body, shell);
    }

    private void SetupMaterial(GameObject go, StraightTypeShellBody body, StraightTypeShell shell)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, shell);
    }

    protected override void SetupGeometry(StraightTypeShellBody body, StraightTypeShell shell)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateDrum(go, shell, body);
      CreateSaddles(go, shell, body);
      CreateFlanges(go, shell, body);
    }

    void CreateDrum(GameObject top, StraightTypeShell shell, StraightTypeShellBody body)
    {
      var shellBody = body.ShellBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      shellBody.transform.parent = top.transform;
      shellBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * shell.Length), (float)(shell.HeatExchanger.HeightOfSaddle + 0.5 * shell.Diameter));
      shellBody.transform.localRotation = Quaternion.identity;
      shellBody.transform.localScale = new Vector3((float)shell.Diameter, (float)(0.5 * shell.Length), (float)shell.Diameter);
      body.ShellBody = shellBody;
    }

    void CreateSaddles(GameObject top, StraightTypeShell shell, StraightTypeShellBody body)
    {
      var modelingSaddleHeight = shell.HeightOfSaddle + 0.5 * shell.Diameter;
      var saddleBody1 = body.Saddle1Body ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody1.transform.parent = top.transform;
      saddleBody1.transform.localPosition = new Vector3(0.0f, (float)shell.DistanceOf1stSaddle, (float)(0.5 * modelingSaddleHeight));
      saddleBody1.transform.localRotation = Quaternion.identity;
      saddleBody1.transform.localScale = new Vector3((float)shell.LengthOfSaddle, (float)shell.WidthOfSaddle, (float)modelingSaddleHeight);
      body.Saddle1Body = saddleBody1;

      var saddleBody2 = body.Saddle2Body ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody2.transform.parent = top.transform;
      saddleBody2.transform.localPosition = new Vector3(0.0f, (float)shell.DistanceOf2ndSaddle, (float)(0.5 * modelingSaddleHeight));
      saddleBody2.transform.localRotation = Quaternion.identity;
      saddleBody2.transform.localScale = new Vector3((float)shell.LengthOfSaddle, (float)shell.WidthOfSaddle, (float)modelingSaddleHeight);
      body.Saddle2Body = saddleBody2;
    }

    void CreateFlanges(GameObject top, StraightTypeShell shell, StraightTypeShellBody body)
    {
      var frontEndFlange = body.FrontEndFlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      frontEndFlange.transform.parent = top.transform;
      frontEndFlange.transform.localPosition = new Vector3(0.0f, (float)(shell.Length - 0.5 * shell.ThicknessOfFlange), (float)(shell.HeightOfSaddle + 0.5 * shell.Diameter));
      frontEndFlange.transform.localRotation = Quaternion.identity;
      frontEndFlange.transform.localScale = new Vector3((float)shell.DiameterOfFlange, (float)(0.5 * shell.ThicknessOfFlange), (float)shell.DiameterOfFlange);
      body.FrontEndFlangeBody = frontEndFlange;

      var rearEndFlange = body.RearEndFlangeBody ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      rearEndFlange.transform.parent = top.transform;
      rearEndFlange.transform.localPosition = new Vector3(0.0f, (float)(0.5 * shell.ThicknessOfFlange), (float)(shell.HeightOfSaddle + 0.5 * shell.Diameter));
      rearEndFlange.transform.localRotation = Quaternion.identity;
      rearEndFlange.transform.localScale = new Vector3((float)shell.DiameterOfFlange, (float)(0.5 * shell.ThicknessOfFlange), (float)shell.DiameterOfFlange);
      body.RearEndFlangeBody = rearEndFlange;
    }
  }
}