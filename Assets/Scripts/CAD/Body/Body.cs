using System.Linq;
using UnityEngine;
using Chiyoda.CAD.Model;
using UnityEngine.EventSystems;
using Chiyoda.UI;

namespace Chiyoda.CAD.Body
{
  public class Body : MonoBehaviour, IPointerClickHandler, IBody
  {
    public Entity Entity { get; set; }

    public GameObject MainObject { get; set; }

    private bool isHighlighted = false;
    public bool IsHighlighted
    {
      get { return isHighlighted; }
      set
      {
        if ( isHighlighted != value )
        {
          isHighlighted = value;
          SetupOutline( isHighlighted );
        }
      }
    }

    public bool IsDrawBound { get; set; } = false;

    private readonly Color _boundColor = new Color( 248f / 255f, 152f / 255f, 0f / 255f );

    private Outline _outline = null;
    
    private Camera _mainCamera;

    public Bounds? GetGlobalBounds()
    {
      var colliders = this.gameObject.GetComponentsInChildren<Collider>();
      return colliders?.Select( collider => collider.bounds ).UnionBounds();
    }

    public void RemoveFromView()
    {
      Destroy( this.gameObject );
      this.transform.parent = null;
    }

    public void OnPointerClick( PointerEventData e )
    {
      if ( PointerEventData.InputButton.Left == e.button ) {
        if ( 1 == e.clickCount ) {
          OnLeftClick();
        }
        else if ( 2 == e.clickCount ) {
          OnLeftDoubleClick();
        }
      }
    }

    private void OnLeftClick()
    {
      if ( UI.InputKeyModifier.IsCtrlOrCmdDown ) {
        // 追加
        Entity.Document.SelectPreferredElement( Entity, true );
      }
      else {
        // 唯一の選択
        Entity.Document.SelectPreferredElement( Entity );
      }
    }
    private void OnLeftDoubleClick()
    {
      DocumentTreeView.Instance()?.Fit( Entity.Document.SelectedElements );
    }

    private void Awake()
    {
      _mainCamera = null;
    }

    void Update()
    {
      if ( null != _outline && null != _mainCamera) {
        _outline.OutlineWidth = _mainCamera.orthographicSize / Screen.height;
      }
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
      else {
        return ( _mainCamera == Camera.current );
      }
    }

    private void OnRenderObject()
    {
      if ( !IsForMainCamera() ) return;

      if ( !IsDrawBound ) return;

      Bounds? bounds = Entity.GetGlobalBounds();
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

    private void SetupOutline( bool draw )
    {
      if ( draw )
      {
        if ( null == _outline )
        {
          _outline = gameObject.AddComponent<Outline>();
//          _outline.OutlineMode = Outline.Mode.OutlineAll;
          _outline.OutlineMode = Outline.Mode.OutlineVisible;
          _outline.OutlineWidth = null != _mainCamera ? _mainCamera.orthographicSize / Screen.height : 0.0075f;
          _outline.OutlineColor = _boundColor;
        }
        _outline.enabled = true;
      }
      else {
        if ( null != _outline ) {
          _outline.enabled = false;
        }
      }
    }
  }

}