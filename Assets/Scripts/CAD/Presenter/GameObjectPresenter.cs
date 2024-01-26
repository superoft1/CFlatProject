using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Model.Structure.CommonEntities;
using Chiyoda.CAD.Topology;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Model.Electricals ;
using Chiyoda.CAD.Plotplan;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter : IEntityPresenter
  {
    private readonly GameObject _roots;
    private readonly TopologyObjectMap _topologyObjectMap = new TopologyObjectMap();
    private readonly BodyMap _bodyMap;

    private readonly Dictionary<Type, IEntityPresenter> _dicSubPresenter = new Dictionary<Type, IEntityPresenter>();

    private Document _document = null;

    private bool _reupdateSelectionState = false;
    
    public GameObjectPresenter( GameObject roots )
    {
      _roots = roots;
      _bodyMap = BodyMap.Instance;

      _dicSubPresenter.Add( typeof( Document ), new DocumentSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( Region ), new RegionSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( Edge ), new EdgeSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( PipingPiece ), new PipingPieceSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( HalfVertex ), new VertexSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( PlacementEntity ), new StructureSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( Point ), new ThroughPointSubPresenter(this));
      _dicSubPresenter.Add( typeof( NozzleArray ), new NozzleArraySubPresenter( this ) );
      _dicSubPresenter.Add( typeof( Nozzle ), new NozzleSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( Support ), new SupportSubPresenter( this ) );
      _dicSubPresenter.Add( typeof( Unit ), new UnitSubPresenter( this ) );
      _dicSubPresenter.Add(typeof(SubEquipment), new SubEquipmentSubPresenter(this));
      _dicSubPresenter.Add( typeof( Electricals ), new ElectricalSubPresenter(this));
    }

    private IEntityPresenter GetSubPresenter( IElement element )
    {
      IEntityPresenter presenter;
      var unknownTypes = new List<Type>();
      for ( var type = element.GetType() ; type != typeof( object ) ; type = type.BaseType ) {
        if ( _dicSubPresenter.TryGetValue( type, out presenter ) ) {
          RegisterAllUnknownTypes( unknownTypes, presenter );
          return presenter;
        }
        unknownTypes.Add( type );
      }
      RegisterAllUnknownTypes( unknownTypes, null );
      return null;
    }

    private void RegisterAllUnknownTypes( List<Type> unknownTypes, IEntityPresenter presenter )
    {
      foreach ( var type in unknownTypes ) {
        _dicSubPresenter.Add( type, presenter );
      }
    }

    public bool IsRaised( IElement element )
    {
      var presenter = GetSubPresenter( element );
      if ( null == presenter ) return true;

      return presenter.IsRaised( element );
    }
    public void Raise( IElement element )
    {
      if ( element is Document document ) {
        _document = document;
        _document.SelectionChanged += Document_SelectionChanged;
      }

      var presenter = GetSubPresenter( element );
      if ( null == presenter ) return;

      presenter.Raise( element );

      _reupdateSelectionState = true;
    }
    public void Update( IElement element )
    {
      var presenter = GetSubPresenter( element );
      if ( null == presenter ) return;

      presenter.Update( element );
    }
    public void TransformUpdate( IElement element )
    {
      var presenter = GetSubPresenter( element );
      if ( null == presenter ) return;

      presenter.TransformUpdate( element );
    }
    public void Destroy( IElement element )
    {
      if ( object.ReferenceEquals( element, _document ) ) {
        _document.DeselectAllElements();
        _document.SelectionChanged -= Document_SelectionChanged;
        _document = null;
      }
      else {
        _document?.DeselectElement( element );
      }

      var presenter = GetSubPresenter( element );
      if ( null == presenter ) return;

      presenter.Destroy( element );

      _reupdateSelectionState = true;
    }

    public void Processed()
    {
      if ( _reupdateSelectionState && _document.SelectedElements.Any() ) {
        UpdateSelectionState( _document.SelectedElements.ToArray(), true );
        _reupdateSelectionState = false;
      }
    }

    private void Document_SelectionChanged( object sender, ItemChangedEventArgs<IElement> args )
    {
      var removedItems = args.RemovedItems.Except( args.AddedItems ).ToArray();
      var addedItems = args.AddedItems.Except( args.RemovedItems ).ToArray();
      
      UpdateSelectionState( removedItems, false );
      UpdateSelectionState( addedItems, true );
    }

    private void UpdateSelectionState( IElement[] elements, bool selected )
    {
      foreach ( var element in elements )
      {
        // Document
        if ( object.ReferenceEquals( element, _document ) ) {
          Body.DocumentBody documentObject = _roots.GetComponent<DocumentBody>();
          if ( null != documentObject ) {
            documentObject.IsDrawBound = selected;
          }
          continue;
        }

        GameObject draggerOwner = UpdateSelectionState( element, selected, selected );
        if ( null != draggerOwner ) {
          if ( selected ) {
            ElementDragManager.Instance.UpdateElementDragger( element as Entity, draggerOwner );            
          }
          else {
            ElementDragManager.Instance.DestroyElementDragger( element as Entity );
          }
        }
        else {
          var highlightElements = SubstituteUpdateElements( element );
          if ( null != highlightElements ) {
            foreach ( var highlightElement in highlightElements ) {
              UpdateSelectionState( highlightElement, selected, false ); // elementに対応するGameObjectが存在しないため境界ボックスは表示できない
            }
          }
        }
      }
    }

    private GameObject UpdateSelectionState( IElement element, bool highlight, bool drawBound )
    {
      // Edge
      if ( element is Edge edge ) {
        var edgObject = _topologyObjectMap.GetEdgeObject( edge );
        if ( null != edgObject ) {
          edgObject.IsDrawBound = drawBound;
          UpdateMaterial( edgObject.transform.OfType<Transform>(), highlight );
          return edgObject.gameObject ;
        }
      }

      // Body
      _bodyMap.TryGetBody( element as Entity, out var body ) ;

      if ( body is Body.Body bodyObject ) {
        bodyObject.IsDrawBound = drawBound;
        UpdateMaterial( bodyObject, highlight );
        UpdateMaterial( bodyObject.transform.OfType<Transform>(), highlight );
        UpdateScale(element, bodyObject, highlight);
        return bodyObject.gameObject;
      }

      return null;
    }

    private void UpdateMaterial( IEnumerable<Transform> transforms, bool highlight )
    {
      foreach ( var transform in transforms )
      {
        Body.Body body = transform.GetComponent<Body.Body>();
        if ( null != body ) {
          UpdateMaterial( body, highlight );
        }
        else {
          UpdateMaterial( transform.transform.OfType<Transform>(), highlight );
        }
      }
    }

    private void UpdateMaterial( Body.Body body, bool highlight )
    {
      if ( body.IsHighlighted != highlight ) {
        body.IsHighlighted = highlight;
        BodyFactory.UpdateMaterial( body );
      }
    }

    private void UpdateScale(IElement element, Body.Body bodyObject, bool highlight)
    {
      var presenter = GetSubPresenter(element) as SubPresenter<HalfVertex>;
      presenter?.UpdateScale(bodyObject, highlight);
    }

    private IEnumerable<IElement> SubstituteUpdateElements( IElement element )
    {
      IEnumerable<IElement> highlightElements = null;
      switch ( element )
      {
        case Line line:
          highlightElements = line.LeafEdges;
          break;
        case HydraulicStream stream:
          highlightElements = stream.LeafEdges;
          break;
      }
      return highlightElements;
    }
    
    private abstract class SubPresenter<T> : IEntityPresenter
      where T : class, IElement
    {
      private readonly GameObjectPresenter _basePresenter;

      protected TopologyObjectMap TopologyObjectMap => _basePresenter._topologyObjectMap ;
      protected BodyMap BodyMap =>_basePresenter._bodyMap;

      protected GameObject RootGameObject => _basePresenter._roots ;
      protected GameObjectPresenter BasePresenter => _basePresenter ;

      protected SubPresenter( GameObjectPresenter basePresenter )
      {
        _basePresenter = basePresenter;
      }

      protected abstract bool IsRaised( T element );
      protected abstract void Raise( T element );
      protected abstract void Update( T element );
      protected abstract void TransformUpdate( T element );
      protected abstract void Destroy( T element );

      public virtual void UpdateScale(Body.Body bodyObject, bool highlight)
      {
      }

      bool IEntityPresenter.IsRaised( IElement element )
      {
        return IsRaised( element as T );
      }
      void IEntityPresenter.Raise( IElement element )
      {
        Raise( element as T );
      }
      void IEntityPresenter.Update( IElement element )
      {
        Update( element as T );
      }
      void IEntityPresenter.TransformUpdate( IElement element )
      {
        TransformUpdate( element as T );
      }
      void IEntityPresenter.Destroy( IElement element )
      {
        Destroy( element as T );
      }

      void IEntityPresenter.Processed()
      {
      }
    }
  }
}
