using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  class DocumentBody : MonoBehaviour
  {
    private static Color ThinColor = new Color( 0.7f, 0.7f, 0.7f );
    private static Color ThickColor = new Color( 1.0f, 1.0f, 1.0f );

    public Document Document { get; set; }

    public bool IsDrawBound { get; set; } = false;

    private Camera _mainCamera;
    private Material _material;

    private void Awake()
    {
      _material = BodyMaterialAccessor.Instance().GetLineMaterial();
      _mainCamera = null;
    }

    private bool IsForMainCamera()
    {
      if ( null != _mainCamera ) {
        return (_mainCamera == Camera.current);
      }
      else {
        if ( Camera.current.name == "MainCamera" ) {
          _mainCamera = Camera.current;
          return true;
        }
        return false;
      }
    }

    private void OnRenderObject()
    {
      if ( !IsForMainCamera() ) return;

      if ( !_material.SetPass( 0 ) ) return;

      var region = Document.Area ;

      GL.PushMatrix();
      try {
        GL.MultMatrix( transform.localToWorldMatrix );

        DrawRegion( region, Document.LengthUnits.DimensionUnit );
        if ( IsDrawBound ) {
          DrawBound( region , Document.LengthUnits.DimensionUnit );
        }
      }
      finally {
        GL.PopMatrix();
      }
    }

    private void DrawRegion( DocumentRegion region, LengthUnitType dimensionUnit )
    {
      float ox = (float)region.EWOrigin, oy = (float)region.NSOrigin, z = (float)region.BaseHeight, w = (float)region.EWWidth, h = (float)region.NSWidth;
      float d = (float)region.GridInterval.YardOrMeter( dimensionUnit );

      GL.Begin( GL.LINES );
      SetColor( ThinColor );
      float minX = -ox, maxX = w - ox, minY = -oy, maxY = h - oy;
      for ( var x = minX ; x < maxX ; x += d ) {
        GL.Vertex( new Vector3( x, minY, z ) );
        GL.Vertex( new Vector3( x, maxY, z ) );
      }

      for ( var y = minY ; y < maxY ; y += d ) {
        GL.Vertex( new Vector3( minX, y, z ) );
        GL.Vertex( new Vector3( maxX, y, z ) );
      }
      GL.End();

      GL.Begin( GL.LINE_STRIP );
      SetColor( ThickColor );

      GL.Vertex( new Vector3( minX, minY, z ) );
      GL.Vertex( new Vector3( maxX, minY, z ) );
      GL.Vertex( new Vector3( maxX, maxY, z ) );
      GL.Vertex( new Vector3( minX, maxY, z ) );
      GL.Vertex( new Vector3( minX, minY, z ) );

      GL.End();
    }
    private void DrawBound( DocumentRegion region, LengthUnitType dimensionUnit )
    {
      float ox = (float)region.EWOrigin, oy = (float)region.NSOrigin, z = (float)region.BaseHeight, w = (float)region.EWWidth, h = (float)region.NSWidth;
      float minX = -ox, maxX = w - ox, minY = -oy, maxY = h - oy;
      float minZ = z - (float)region.Depth, maxZ = z + (float)region.Height;

      if ( minZ != z ) {
        GL.Begin( GL.LINE_STRIP );
        SetColor( ThickColor );

        GL.Vertex( new Vector3( minX, minY, minZ ) );
        GL.Vertex( new Vector3( minX, maxY, minZ ) );
        GL.Vertex( new Vector3( maxX, maxY, minZ ) );
        GL.Vertex( new Vector3( maxX, minY, minZ ) );
        GL.Vertex( new Vector3( minX, minY, minZ ) );

        GL.End();
      }

      if ( maxZ != z ) {
        GL.Begin( GL.LINE_STRIP );
        SetColor( ThickColor );

        GL.Vertex( new Vector3( minX, minY, maxZ ) );
        GL.Vertex( new Vector3( minX, maxY, maxZ ) );
        GL.Vertex( new Vector3( maxX, maxY, maxZ ) );
        GL.Vertex( new Vector3( maxX, minY, maxZ ) );
        GL.Vertex( new Vector3( minX, minY, maxZ ) );

        GL.End();
      }

      if ( minZ != maxZ ) {
        GL.Begin( GL.LINES );
        SetColor( ThickColor );

        GL.Vertex( new Vector3( minX, minY, minZ ) );
        GL.Vertex( new Vector3( minX, minY, maxZ ) );

        GL.Vertex( new Vector3( minX, maxY, minZ ) );
        GL.Vertex( new Vector3( minX, maxY, maxZ ) );

        GL.Vertex( new Vector3( maxX, maxY, minZ ) );
        GL.Vertex( new Vector3( maxX, maxY, maxZ ) );

        GL.Vertex( new Vector3( maxX, minY, minZ ) );
        GL.Vertex( new Vector3( maxX, minY, maxZ ) );

        GL.End();
      }
    }

    public void SetColor( in Color color )
    {
      GL.Color( color );
    }
  }
}
