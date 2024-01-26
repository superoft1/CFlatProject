using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Manager
{
  class ElementDragManager : MonoBehaviour
  {
    #region Instance

    public static ElementDragManager Instance { get; private set; }

    private void Start()
    {
      Instance = this;
    }

    private void OnDestroy()
    {
      Instance = null;
    }

    #endregion

    [SerializeField]
    private GameObject pickableGizmoPrefab;

    private readonly Dictionary<Entity, PickableGizmo> _entity2Gizmo = new Dictionary<Entity, PickableGizmo>();
    private readonly HashSet<Document> _documents = new HashSet<Document>();
    private PickableGizmo _draggingGizmo = null;
    private Document _currentDoc = null;
    private Entity _firstEntity = null;

    public bool IsDragging => (null != _draggingGizmo);

    public Camera DraggerCamera { get; set; }

    public void UpdateElementDragger( Entity entity, GameObject go )
    {
      if ( !_entity2Gizmo.TryGetValue( entity, out var gizmo ) ) {
        CreateElementDragger( entity, go );
      }
      else {
        MaintainElementDraggerParent( gizmo, go );
      }
    }

    public void DestroyElementDragger( Entity entity )
    {
      if ( _entity2Gizmo.TryGetValue( entity, out var gizmo ) )
      {
        GameObject.Destroy( gizmo.gameObject );

        if ( _firstEntity == entity )
        {
          var firstEntity = _currentDoc.SelectedElements.FirstOrDefault() as Entity;
          if ( null != firstEntity && _entity2Gizmo.TryGetValue( firstEntity, out var firstGizmo ) )
          {
            firstGizmo.gameObject.SetActive( true );
            _firstEntity = firstEntity;
          }
          else {
            _firstEntity = null;
          }
        }
      }
    }

    private void MaintainElementDraggerParent( PickableGizmo gizmo, GameObject parentGameObject )
    {
      gizmo.transform.SetParent( parentGameObject.transform );
      gizmo.transform.localPosition = Vector3.zero;
      gizmo.transform.localRotation = Quaternion.identity;
    }

    private PickableGizmo CreateElementDragger( Entity entity, GameObject go )
    {
      // 最初のEntityのみGizmoを表示する
      if ( null == _firstEntity ) {
        _firstEntity = entity;
      }
      bool isVisible = ( _firstEntity == entity );

      var gizmo = GameObject.Instantiate( pickableGizmoPrefab ).GetComponent<PickableGizmo>();
      gizmo.gameObject.SetActive( isVisible );
      gizmo.Entity = entity;
      gizmo.Destroyed += Gizmo_Destroyed;
      MaintainElementDraggerParent( gizmo, go );

      _entity2Gizmo.Add( entity, gizmo );
      gizmo.DragStateChanged += Gizmo_DragStateChanged;

      var doc = entity.Document;
      if ( _documents.Add( doc ) ) {
        doc.SelectionChanged += Document_SelectionChanged;
        doc.Unload += Document_Unload;
        Document_SelectionChanged( doc, new ItemChangedEventArgs<IElement>( Array.Empty<IElement>(),
                                                                                  Array.Empty<IElement>() ) );
      }
      else {
        UpdateElementDraggerAxes( gizmo, doc.Draggability );
      }

      return gizmo;
    }

    private void Gizmo_Destroyed( object sender, EventArgs e )
    {
      var gizmo = (sender as PickableGizmo);
      gizmo.DragStateChanged -= Gizmo_DragStateChanged;
      if ( _draggingGizmo == gizmo ) _draggingGizmo = null;

      _entity2Gizmo.Remove( gizmo.Entity );
    }

    private void UpdateElementDraggerAxes( Document doc )
    {
      var draggability = doc.Draggability;

      foreach ( var pair in _entity2Gizmo ) {
        if ( pair.Key.Document == doc ) UpdateElementDraggerAxes( pair.Value, draggability );
      }
    }

    public void Drag( Vector3 direction )
    {
      if ( null == _draggingGizmo ) return;

      var rot = _draggingGizmo.transform.parent.rotation;
      if (_draggingGizmo.XDragging) direction = ToDragDirection(direction, rot * new Vector3(1, 0, 0));
      else if (_draggingGizmo.YDragging) direction = ToDragDirection(direction, rot * new Vector3(0, 1, 0));
      else if (_draggingGizmo.ZDragging) direction = ToDragDirection(direction, rot * new Vector3(0, 0, 1));
      
      foreach ( var (entity, gizmo) in _entity2Gizmo ) {
        switch ( entity ) {
          case IPlacement placement:
            placement.MoveLocalPos( GetLocalVector( gizmo.transform.parent, direction ) );
            break;

          case PipingPiece pp:
            (pp.Parent as Edge)?.MoveLocalPos( GetLocalVector( gizmo.transform.parent, direction ) );
            break;

          default:
            throw new InvalidOperationException();
        }
      }
    }

    private Vector3 ToDragDirection( Vector3 direction, Vector3 axis )
    {
      var axis2 = DraggerCamera.transform.InverseTransformVector( axis );
      var invScale = (axis2.x * axis2.x + axis2.y * axis2.y);
      if ( invScale < 1e-4 ) return Vector3.zero;

      return Vector3.Dot(direction, axis) / invScale * axis;
    }

    private static Vector3 GetLocalVector( Transform transform, Vector3 direction )
    {
      var parent = transform.parent;
      if ( null == parent ) return direction;
      else return parent.InverseTransformVector( direction );
    }

    private void UpdateElementDraggerAxes( PickableGizmo gizmo, Draggability draggability )
    {
      gizmo.XVisible = draggability.X;
      gizmo.YVisible = draggability.Y;
      gizmo.ZVisible = draggability.Z;
    }


    private void Gizmo_DragStateChanged( object sender, EventArgs e )
    {
      var gizmo = sender as PickableGizmo;
      if ( gizmo.Dragging ) {
        _draggingGizmo = gizmo;
      }
      else {
        if ( null != _draggingGizmo ) {
          _draggingGizmo.Entity.Document.HistoryCommit() ;
        }
        _draggingGizmo = null;
      }
    }

    private void Document_Unload( object sender, EventArgs e )
    {
      var doc = sender as Document;
      doc.SelectionChanged -= Document_SelectionChanged;
      doc.Unload -= Document_Unload;
      _documents.Remove( doc );
    }

    private void Document_SelectionChanged( object sender, ItemChangedEventArgs<IElement> args )
    {
      _currentDoc = sender as Document;
      UpdateElementDraggerAxes( _currentDoc );
    }
  }
}
