using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public abstract class BodyCreator
  {
    protected Entity entity;

    protected const float ModelScale = 20f;

    public BodyCreator( Entity _entity )
    {
      entity = _entity;
    }

    public abstract Body Create();

    public virtual Body Update( Body body ) { return Create(); } // TODO: abstractに

    public abstract void UpdateMaterials( Body body );
  }

  public abstract class BodyCreator<TEntity, TBody> : BodyCreator
    where TEntity : Entity
    where TBody : Body
  {
    public BodyCreator( Entity entity ) : base( entity )
    {
    }

    protected virtual bool RecreateOnUpdate { get { return true; } }

    public override Body Create()
    {
      return Create( null );
    }
    
    private Body Create( Body recreated )
    {
      var body = CreateGameObject();
      if ( null == body ) return null;

      SetupName( body.gameObject );

      var tentity = entity as TEntity;

      SetupGeometry( body, tentity );

      if ( null != recreated ) {
        body.IsHighlighted = recreated.IsHighlighted;
        body.IsDrawBound = recreated.IsDrawBound;
      }

      SetupMaterials( body, tentity );
      SetupVisibility( body, tentity );

      return body;
    }

    public override Body Update( Body body )
    {
      if ( RecreateOnUpdate ) {
        var children = body.transform.OfType<Transform>().Where( child => null != child.GetComponent<Body>() ||
                                                                          null != child.GetComponent<PickableGizmo>() ).ToArray();
        foreach ( var child in children ) {
          child.SetParent( null, false );
        }

        var createdBody = Create( body );

        foreach ( var child in children ) {
          child.SetParent( createdBody.transform, false );
        }
        return createdBody;
      }

      var tbody = body as TBody;
      var tentity = entity as TEntity;

      SetupGeometry( tbody, tentity );
      SetupMaterials(tbody, tentity);
      SetupVisibility( tbody, tentity );

      return body;
    }

    public override void UpdateMaterials( Body body )
    {
      var tbody = body as TBody;
      var tentity = entity as TEntity;

      SetupMaterials( tbody, tentity );
    }

    protected virtual TBody CreateGameObject()
    {
      var rootObject = new GameObject();

      var body = rootObject.GetComponent<TBody>();
      if ( null == body ) {
        body = rootObject.AddComponent<TBody>();
      }      

      var gameObject = BodyPrefabAccessor.Instance().Create(entity);
      //if (null == gameObject) return null;
      if (null != gameObject)
      {
        gameObject.transform.parent = rootObject.transform;
        body.MainObject = gameObject;
      }

      return body;
    }
    protected virtual void SetupName( GameObject go )
    {
      go.name = typeof( TEntity ).Name;
    }
    protected virtual void SetupMaterials( TBody body, TEntity entity )
    {
      if (body == null || body.MainObject == null) return;
      var material = GetMaterial( body, entity ) ;
      var renderers = body.MainObject.GetComponentsInChildren<MeshRenderer>();
      foreach (var render in renderers) {
        render.material = material;
      }
    }

    protected virtual Material GetMaterial( TBody body, TEntity entity, bool isFoundation = false )
    {
      return GetMaterial( entity, body.IsHighlighted, isFoundation );
    }

    private Material GetMaterial( TEntity entity, bool highlight = false, bool isFoundation = false )
    {
      return isFoundation ?
               BodyMaterialAccessor.Instance().GetFoundationMaterial( entity, highlight ) :
               BodyMaterialAccessor.Instance().GetMaterial( entity, highlight );
    }

    protected virtual void SetupVisibility( TBody body, TEntity entity )
    {
      body.gameObject.SetActive( entity.IsVisible );
    }

    protected abstract void SetupGeometry( TBody body, TEntity entity );

    protected void AppendOffsetMesh( GameObject go, float offset, ( Vector3 point, Vector3 vector )[] terms, Vector3 scale, Vector3? correctDir = null )
    {
      var meshFilter = go.GetComponent<MeshFilter>();
      var sharedMesh = meshFilter.sharedMesh;

      var vertices = sharedMesh.vertices;
      var triangles = sharedMesh.triangles;
      var vertexCount = sharedMesh.vertexCount;

      var inverseScale = Matrix4x4.Scale( scale ).inverse;

      var scaledTerms = terms.Select( term => ( inverseScale.MultiplyPoint3x4( term.point ), term.vector ) ).ToArray();
      var offsetNormals = OffsetNormals( sharedMesh, scaledTerms, correctDir ).Select( normal => inverseScale.MultiplyPoint3x4( normal ) ).ToList();
      var fattenVertices = vertices.Select( ( vertex, i ) => vertex + offsetNormals[i] * offset ).ToList();

      var mesh = new Mesh();
      mesh.vertices = vertices.Concat( fattenVertices ).ToArray();
      mesh.subMeshCount = 2;
      mesh.SetTriangles( triangles, 0 );
      mesh.SetTriangles( triangles, 1, true, vertexCount );
      mesh.RecalculateNormals();

      meshFilter.mesh = mesh;

      var collider = go.GetComponent<MeshCollider>();
      if ( null != collider ) {
        collider.sharedMesh = meshFilter.sharedMesh;
      }
    }

    private List<Vector3> OffsetNormals( Mesh mesh, ( Vector3 point, Vector3 vector )[] terms, Vector3? correctDir )
    {
      var vertices = mesh.vertices;
      var normals = mesh.normals;

      var offsetNormals = new List<Vector3>( normals );

      var groups = vertices.Select( ( vertex, i ) => new KeyValuePair<Vector3, int>( vertex, i ) )
                           .GroupBy( pair => pair.Key );

      foreach ( var group in groups ) {
        var fattenNormal = Vector3.zero;
 
        foreach ( var pair in group ) {
          var nearestIndex = terms.Select( ( term, i ) => new { distance = ( term.point - pair.Key ).sqrMagnitude, index = i } )
                                  .Aggregate( ( result, next ) => result.distance < next.distance ? result : next ).index;
          var nearestTerm = terms[nearestIndex];

          var normal = normals[pair.Value];
          if ( Vector3.Angle( normal, nearestTerm.vector ) >= 30 ) {
            if ( Vector3.Dot( normal, ( nearestTerm.point - pair.Key ) ) >= 0 ) {
              normal *= -1;
            }
//          if ( Vector3.Dot( normal, ( nearestTerm.point - pair.Key ) ) < 0 &&
//               Vector3.Angle( normal, nearestTerm.vector ) >= 30 ) {
            fattenNormal += normal;
          }
        }

        fattenNormal.Normalize();

        if ( correctDir.HasValue ) {
          var angle = Vector3.Angle( correctDir.Value, fattenNormal ) - 90;
          if ( Math.Abs( angle ) >= Tolerance.AngleTolerance ) {
            var axis = Vector3.Cross( correctDir.Value, fattenNormal );
            fattenNormal += Quaternion.AngleAxis( angle, axis ) * correctDir.Value * Mathf.Tan( angle * Mathf.Deg2Rad );
          }
        }

        foreach ( var pair in group ) {
          offsetNormals[pair.Value] = fattenNormal;
        }
      }

      return offsetNormals;
    }

    protected void AddFadeMaterial( GameObject go )
    {
      var material = new Material( GetMaterial( entity as TEntity ) );
      material.SetFloat("_Mode", 2 ); // 0:Opaque 1:Cutout 2:Fade 3:Transparent
      material.SetOverrideTag( "RenderType", "Transparent" );
      material.SetInt( "_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha );
      material.SetInt( "_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
      material.SetInt( "_ZWrite", 0 );
      material.DisableKeyword( "_ALPHATEST_ON" );
      material.EnableKeyword( "_ALPHABLEND_ON" );
      material.DisableKeyword( "_ALPHAPREMULTIPLY_ON" );
      material.renderQueue = 3000;

      var color = material.color;
      color.a = 128f / 255f;
      material.color = color;

      var renderer = go.GetComponent<MeshRenderer>();
      var materials = renderer.sharedMaterials.ToList();
      materials.Add( material );
      renderer.materials = materials.ToArray();
    }

    protected static void ChangeMaterialColor( TBody body, Color newColor )
    {
      var renderes = body.MainObject.GetComponentsInChildren<MeshRenderer>() ;
      foreach ( var render in renderes ) {
        render.material.color = newColor ;
      }
    }
  }

}