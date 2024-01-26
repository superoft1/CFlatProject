using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class CompressorBodyCreator : BodyCreator<Compressor, CompressorBody>
  {
    public CompressorBodyCreator(Entity _entity) : base(_entity)
    {

    }

    protected override void SetupMaterials(CompressorBody body, Compressor comp)
    {
      SetupMaterial(body.FoundationBody, body, comp, true);
      SetupMaterial(body.Equip1Body, body, comp, false);
      SetupMaterial(body.ConnectorBody1, body, comp, false);
      SetupMaterial(body.Equip2Body, body, comp, false);
      SetupMaterial(body.ConnectorBody2, body, comp, false);
      SetupMaterial(body.Equip3Body, body, comp, false);
    }

    private void SetupMaterial(GameObject go, CompressorBody body, Compressor comp, bool isFoundation)
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial(body, comp, isFoundation);
    }

    protected override void SetupGeometry(CompressorBody body, Compressor comp)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      CreateCompressorBase(go, comp, body);

      go.transform.localRotation = Quaternion.identity;
    }

    GameObject CreateCubeBody(GameObject top)
    {
      var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
      body.transform.parent = top.transform;
      return body;
    }

    void CreateCompressorBase(GameObject top, Compressor comp, CompressorBody _body)
    {
      var foundationBody = CreateCubeBody(top);
      foundationBody.transform.localPosition = (Vector3)comp.FoundationCenter;
      foundationBody.transform.localScale = (Vector3)comp.FoundationSize;
      _body.FoundationBody = foundationBody;

      var equip1Body = CreateCubeBody(top);
      equip1Body.transform.localPosition = (Vector3)comp.Equip1Center;
      equip1Body.transform.localScale = (Vector3)comp.Equip1Size;
      _body.Equip1Body = equip1Body;

      var connector1Body = CreateCubeBody(top);
      connector1Body.transform.localPosition = (Vector3)comp.Driver1Center;
      connector1Body.transform.localScale = (Vector3)comp.Driver1Size;
      _body.ConnectorBody1 = connector1Body;

      var equip2Body = CreateCubeBody(top);
      equip2Body.transform.localPosition = (Vector3)comp.Equip2Center;
      equip2Body.transform.localScale = (Vector3)comp.Equip2Size;
      _body.Equip2Body = equip2Body;

      var connector2Body = CreateCubeBody(top);
      connector2Body.transform.localPosition = (Vector3)comp.Driver2Center;
      connector2Body.transform.localScale = (Vector3)comp.Driver2Size;
      _body.ConnectorBody2 = connector2Body;

      var equip3Body = CreateCubeBody(top);
      equip3Body.transform.localPosition = (Vector3)comp.Equip3Center;
      equip3Body.transform.localScale = (Vector3)comp.Equip3Size;
      _body.Equip3Body = equip3Body;
    }
  }

}