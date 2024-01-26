using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class ConeRoofTypeTankBodyCreator : BodyCreator<ConeRoofTypeTank, ConeRoofTypeTankBody>
  {
    public ConeRoofTypeTankBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( ConeRoofTypeTankBody body, ConeRoofTypeTank tank )
    {
      body.CylinderBody.GetComponent<MeshRenderer>().material = GetMaterial( body, tank );
      body.CapBody.GetComponent<MeshRenderer>().material = GetMaterial( body, tank );
    }

    protected override void SetupGeometry(ConeRoofTypeTankBody body, ConeRoofTypeTank tank)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateCylinder(go, tank, body);
      CreateCap(go, tank, body);
    }

    void CreateCylinder(GameObject top, ConeRoofTypeTank tank, ConeRoofTypeTankBody body)
    {
      var cylinderBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      cylinderBody.transform.parent = top.transform;
      cylinderBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * tank.HeightOfCylinder));
      cylinderBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      cylinderBody.transform.localScale = new Vector3((float)tank.DiameterOfCylinder, (float)(0.5 * tank.HeightOfCylinder), (float)tank.DiameterOfCylinder);
      body.CylinderBody = cylinderBody;
    }

    void CreateCap(GameObject top, ConeRoofTypeTank tank, ConeRoofTypeTankBody body)
    {
      var capBody = CreateCapBody((float)(tank.DiameterOfCylinder / 2.0), (float)tank.LengthOfUpperCap);
      capBody.transform.parent = top.transform;
      capBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)tank.HeightOfCylinder);
      capBody.transform.localRotation = Quaternion.identity;
      capBody.transform.localScale = Vector3.one;
      body.CapBody = capBody;
    }

    GameObject CreateCapBody(float radiusOfCone, float heightOfCone)
    {
      const int vertNum = 20;

      var cone = new GameObject("Cone");
      var mesh = new Mesh();

      var vertices = new Vector3[2 * vertNum];
      var normals = new Vector3[2 * vertNum];
      var uvs = new Vector2[2 * vertNum];
      var tris = new int[3 * vertNum];

      var slopeAngle = Mathf.Atan(radiusOfCone / heightOfCone);
      var cosSlope = Mathf.Cos(slopeAngle);
      var sinSlope = Mathf.Sin(slopeAngle);

      var dTheta = 2.0f * Mathf.PI / vertNum;

      for (int i = 0; i < vertNum; ++i)
      {
        var cosTheta = Mathf.Cos(dTheta * i);
        var sinTheta = Mathf.Sin(dTheta * i);

        vertices[i] = new Vector3(radiusOfCone * cosTheta, radiusOfCone * sinTheta, 0.0f);
        vertices[i + vertNum] = new Vector3(0.0f, 0.0f, heightOfCone);
        normals[i] = new Vector3(cosSlope * cosTheta, cosSlope * sinTheta, sinSlope);
        normals[i + vertNum] = normals[i];
        uvs[i] = new Vector2(i / vertNum, 0.0f);
        uvs[i + vertNum] = new Vector2(i / vertNum, 1.0f);

        var nextIndex = (i == vertNum - 1) ? 0 : i + 1;
        tris[3 * i] = i;
        tris[3 * i + 1] = nextIndex;
        tris[3 * i + 2] = i + vertNum; ;
      }

      mesh.vertices = vertices;
      mesh.normals = normals;
      mesh.uv = uvs;
      mesh.triangles = tris;

      var filter = cone.AddComponent<MeshFilter>();
      filter.mesh = mesh;

      cone.AddComponent<MeshRenderer>();
      var collider = cone.AddComponent<MeshCollider>();
      collider.sharedMesh = filter.sharedMesh;

      return cone;
    }
  }
}
