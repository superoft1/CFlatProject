using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class GateValveBodyCreator : BodyCreator<GateValve, Body>
  {
    public GateValveBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, GateValve valve )
    {
      var go = body.gameObject;

      var diameterScale = (float)valve.Diameter * ModelScale;
      var heightScale = (float)valve.Length / 2f * ModelScale;
      go.transform.localScale = new Vector3( heightScale, diameterScale, diameterScale );
      go.transform.localPosition = (Vector3)valve.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;

      SetupInsulation( body, valve );
    }

    private void SetupInsulation( Body body, GateValve valve )
    {
      if ( valve.InsulationThickness < Tolerance.DistanceTolerance ||
           valve.Length < Tolerance.DistanceTolerance ) {
        return;
      }

      var radius = (float)valve.Diameter / 2f;
      var length = (float)valve.Length / 2f;
      var thickness = (float)valve.InsulationThickness;

      var scale = body.gameObject.transform.localScale;
      var diameterScale = scale.y;
      var heightScale = scale.x;

      var impl = body.MainObject.GetComponent<GateValveBodyImpl>();
      AppendCylindricalMesh( impl.Weld1, ( radius + thickness ) / diameterScale, length / heightScale );
      AppendCylindricalMesh( impl.Weld2, ( radius + thickness ) / diameterScale, length / heightScale );
      AddFadeMaterial( impl.Weld1 );
      AddFadeMaterial( impl.Weld2 );
    }

    private void AppendCylindricalMesh( GameObject go, float radius, float length )
    {
      var meshFilter = go.GetComponent<MeshFilter>();
      var sharedMesh = meshFilter.sharedMesh;

      var vertices = sharedMesh.vertices;
      var triangles = sharedMesh.triangles;
      var vertexCount = sharedMesh.vertexCount;

      var cylinder = CreateCylindricalPolygon( radius, length );

      var mesh = new Mesh();
      mesh.vertices = vertices.Concat( cylinder.vertices ).ToArray();
      mesh.subMeshCount = 2;
      mesh.SetTriangles( triangles, 0 );
      mesh.SetTriangles( cylinder.triangles, 1, true, vertexCount );
      mesh.RecalculateNormals();

      meshFilter.mesh = mesh;

      var collider = go.GetComponent<MeshCollider>();
      if ( null != collider ) {
        collider.sharedMesh = meshFilter.sharedMesh;
      }
    }

    private ( Vector3[] vertices, int[] triangles ) CreateCylindricalPolygon( float radius, float length )
    {
      const int vertNum = 24; // Prefabの利用するfbxの頂点数に揃えた

      int numOfVertices = vertNum * 2;
      var vertices = new Vector3[numOfVertices];

      int numOfTrisIndices = vertNum * 6;
      var triangles = new int[numOfTrisIndices];

      var dTheta = 2.0f * Mathf.PI / vertNum;

      for ( int i = 0; i < vertNum; ++i ) {
        var cosTheta = Mathf.Cos( dTheta * i );
        var sinTheta = Mathf.Sin( dTheta * i );

        vertices[i] = new Vector3( length, radius * cosTheta, radius * sinTheta );
        vertices[i + vertNum] = new Vector3( 0.0f, radius * cosTheta, radius * sinTheta );

        var nextIndex = ( i == vertNum - 1 ) ? 0 : i + 1;
        triangles[6 * i] = i;
        triangles[6 * i + 1] = i + vertNum;
        triangles[6 * i + 2] = nextIndex;
        triangles[6 * i + 3] = nextIndex + vertNum;
        triangles[6 * i + 4] = nextIndex;
        triangles[6 * i + 5] = i + vertNum;
      }

      return ( vertices, triangles );
    }
  }
}