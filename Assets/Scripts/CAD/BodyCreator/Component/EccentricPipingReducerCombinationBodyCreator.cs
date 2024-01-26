using System.Linq;
using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class EccentricPipingReducerCombinationBodyCreator : BodyCreator<EccentricPipingReducerCombination, Body>
  {
    public EccentricPipingReducerCombinationBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupMaterials( Body body, EccentricPipingReducerCombination reducer )
    {
      var material = GetMaterial( body, reducer );
      var renderers = body.gameObject.GetComponentsInChildren<MeshRenderer>();
      foreach ( var render in renderers ) {
        render.material = material;
      }
    }

    protected override void SetupGeometry( Body body, EccentricPipingReducerCombination reducer )
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = (Vector3)reducer.Origin;
      go.transform.localRotation = Quaternion.identity;

      foreach ( var connectPoint in reducer.CombinationConnectPoints ) {
        var largeTerm = connectPoint.largeTerm;
        var smallTerm = connectPoint.smallTerm;

        var largeRadius = largeTerm.diameter.OutsideMeter * 0.5;
        var smallRadius = smallTerm.diameter.OutsideMeter * 0.5;
        var length = Vector3d.Dot( largeTerm.point - smallTerm.point, reducer.Axis );

        if ( length < Tolerance.DistanceTolerance ) {
          continue;
        }

        var origin = ( largeTerm.point + smallTerm.point ) * 0.5f;
        
        var taper = CreateTaperBody( (float)largeRadius, (float)smallRadius, (float)length );
        taper.transform.parent = go.transform;
        taper.transform.localPosition = (Vector3)origin;

        SetupInsulation( taper, reducer, largeTerm.point - origin, smallTerm.point - origin, length );
      }
    }

    private void SetupInsulation( GameObject go, EccentricPipingReducerCombination reducer, Vector3d largeTerm, Vector3d smallTerm, double length )
    {
      if ( reducer.InsulationThickness < Tolerance.DistanceTolerance ) {
        return;
      }

      var termPoint = new Vector3[] { (Vector3)largeTerm, (Vector3)smallTerm };
      var termVector = new Vector3[] { (Vector3)reducer.GetConnectPoint( 0 ).Vector.normalized,
                                       (Vector3)reducer.GetConnectPoint( 1 ).Vector.normalized };

      var dist = (float)(length / 4.0); // 4分の1程度内側へ移動
      termPoint[0] -= termVector[0] * dist;
      termPoint[1] -= termVector[1] * dist;

      var terms = new ( Vector3 point, Vector3 vector )[] { ( termPoint[0], termVector[0] ), 
                                                            ( termPoint[1], termVector[1] ) };

      AppendOffsetMesh( go, (float)reducer.InsulationThickness, terms, Vector3.one, (Vector3)reducer.Axis );
      AddFadeMaterial( go );
    }

    private GameObject CreateTaperBody( float largeRadius, float smallRadius, float length )
    {
      var taper = CreateTaperPolygon( largeRadius, smallRadius, length );

      var mesh = new Mesh();
      mesh.vertices = taper.vertices;
      mesh.triangles = taper.triangles;
      mesh.RecalculateNormals();

      var go = new GameObject( "EPR" );

      var filter = go.AddComponent<MeshFilter>();
      filter.mesh = mesh;

      go.AddComponent<MeshRenderer>();

      var collider = go.AddComponent<MeshCollider>();
      collider.sharedMesh = filter.sharedMesh;

      return go;
    }

    private ( Vector3[] vertices, int[] triangles ) CreateTaperPolygon( float largeRadius, float smallRadius, float length )
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

        vertices[i] = new Vector3( length * 0.5f, largeRadius * cosTheta + ( largeRadius - smallRadius ) * 0.5f, largeRadius * sinTheta );
        vertices[i + vertNum] = new Vector3( length * -0.5f, smallRadius * cosTheta + ( largeRadius - smallRadius ) * -0.5f, smallRadius * sinTheta );

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