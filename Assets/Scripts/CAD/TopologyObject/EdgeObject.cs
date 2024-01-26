using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Topology;
using UnityEngine;

public class EdgeObject : MonoBehaviour
{

  [SerializeField]
  EdgeObject parent;

  [SerializeField]
  Edge edge;

  [SerializeField]
  Body pipingPiece;

  [SerializeField]
  List<EdgeObject> children = new List<EdgeObject>();

  public EdgeObject Parent
  {
    get
    {
      return parent;
    }

    set
    {
      parent = value;
    }
  }

  public Edge Edge
  {
    get
    {
      return edge;
    }

    set
    {
      edge = value;
    }
  }

  public Body PipingPiece
  {
    get
    {
      return pipingPiece;
    }

    set
    {
      pipingPiece = value;
    }
  }

  public List<EdgeObject> Children
  {
    get { return children; }
    set { children = value; }
  }

  public bool IsDrawBound { get; set; } = false;

  private Color _boundColor = new Color( 248f / 255f, 152f / 255f, 0f / 255f );

  private Camera _mainCamera;

  private void Awake()
  {
    _mainCamera = null;
  }

  private bool IsForMainCamera()
  {
    if ( null == _mainCamera )
    {
      if ( Camera.current.name == "MainCamera" )
      {
        _mainCamera = Camera.current;
        return true;
      }
      return false;
    }
    else
    {
      return ( _mainCamera == Camera.current );
    }
  }

  private void OnRenderObject()
  {
    if ( !IsForMainCamera() ) return;

    if ( !IsDrawBound ) return;

    Bounds? bounds = Edge.GetGlobalBounds();
    if ( bounds.HasValue )
    {
      Vector3 org = bounds.Value.min;
      Vector3 size = bounds.Value.size;

      Vector3[] lower = { org,
                          org += new Vector3( size.x, 0.0f, 0.0f ),
                          org += new Vector3( 0.0f, size.y, 0.0f ),
                          org += new Vector3( -size.x, 0.0f, 0.0f )};
      Vector3[] upper = lower.Select( pos => pos + new Vector3( 0.0f, 0.0f, size.z ) ).ToArray();

      GL.Begin( GL.LINE_STRIP );
      GL.Color( _boundColor );
      for ( int i = 0; i < 5; ++i ) {
        GL.Vertex( lower[i % 4] );
      }
      GL.End();

      GL.Begin( GL.LINE_STRIP );
      GL.Color( _boundColor );
      for ( int i = 0; i < 5; ++i ) {
        GL.Vertex( upper[i % 4] );
      }
      GL.End();

      GL.Begin( GL.LINES );
      GL.Color( _boundColor );
      for ( int i = 0; i < 4; ++i )
      {
        GL.Vertex( lower[i] );
        GL.Vertex( upper[i] );
      }
      GL.End();
    }
  }
}
