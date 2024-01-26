using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Plotplan;
using Chiyoda.CAD.Topology;
using Chiyoda.UI ;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  class PresenterManager : MonoBehaviour
  {
    [SerializeField]
    private GameObject roots = null;

    [SerializeField]
    private UI.DocumentTreeView treeView = null;

    [SerializeField]
    private UI.PropertyView propertyView = null;

    public static PresenterManager Instance { get; private set; }

    public TreeViewItemPresenter TreeViewItemPresenter { get; private set; }

    private readonly HashSet<IEntityPresenter> _presenters = new HashSet<IEntityPresenter>();
    private readonly PresentationCache _presentationCache = new PresentationCache();
    private readonly HashSet<IElement> _eventedElements = new HashSet<IElement>();

    private void Awake()
    {
      Instance = this;

      if ( null != roots ) {
        _presenters.Add( new GameObjectPresenter( roots ) );
      }

      if ( null != treeView ) {
        TreeViewItemPresenter = new TreeViewItemPresenter( treeView );
        _presenters.Add( TreeViewItemPresenter );
      }

      if ( null != propertyView ) {
        _presenters.Add( new PropertyViewPresenter( propertyView ) );
      }

      DocumentCollection.Instance.DocumentCreated += DocumentCollection_DocumentCreated ;
      DocumentCollection.Instance.DocumentClosed += DocumentCollection_DocumentClosed ;
      DocumentCollection.Instance.CreateNew() ;
    }

    private void OnDestroy()
    {
      Instance = null;
      TreeViewItemPresenter = null;
      _presenters.Clear();

      DocumentCollection.Instance.DocumentCreated -= DocumentCollection_DocumentCreated ;
      DocumentCollection.Instance.DocumentClosed -= DocumentCollection_DocumentClosed ;
    }

    private void DocumentCollection_DocumentCreated( object sender, DocumentEventArgs e )
    {
      Register( e.Document ) ;
    }

    private void DocumentCollection_DocumentClosed( object sender, DocumentEventArgs e )
    {
      Unregister( e.Document ) ;
    }

    public void ExecuteUpdate()
    {
      bool hasCache = _presentationCache.HasCache();

      foreach ( var item in _presentationCache.Flush() ) {
        switch ( item.PresentationType ) {
          case PresentationType.Raise: BodyRaise( item.Element ); break;
          case PresentationType.Update: BodyUpdate( item.Element ); break;
          case PresentationType.TransformUpdate: BodyTransformUpdate( item.Element ); break;
          case PresentationType.Destroy: BodyDestroy( item.Element ); break;
        }
      }

      if ( hasCache ) {
        Processed();
      }
    }

    private void Register( IElement element )
    {
      _presentationCache.AddRaise( element );
    }

    private void Unregister( IElement element )
    {
      _presentationCache.AddDestroy( element );
    }

    private void BodyRaise( IElement element )
    {
      foreach ( var presenter in _presenters ) {
        if ( ! presenter.IsRaised( element ) ) {
          presenter.Raise( element ) ;
        }
      }
      foreach ( var child in element.Children ) {
        Register( child );
      }

      if ( _eventedElements.Add( element ) ) {
        element.AfterNewlyValueChanged += Element_ValueChanged;
        element.AfterHistoricallyValueChanged += Element_ValueChanged;
        element.VisibilityChanged += Element_ValueChanged;
        element.AfterNewlyChildrenChanged += Element_ChildrenChanged;
        element.AfterHistoricallyChildrenChanged += Element_ChildrenChanged;

        if ( element is IRelocatable relocatable ){
          relocatable.LocalCodChanged += Element_LocalCodChanged;
        }
      }
      else {
        throw new InvalidOperationException() ;
      }
    }

    private void BodyUpdate( IElement element )
    {
      foreach ( var presenter in _presenters ) {
        presenter.Update( element );
      }
    }

    private void BodyTransformUpdate( IElement element )
    {
      foreach ( var presenter in _presenters ) {
        presenter.TransformUpdate( element ) ;
      }
    }

    private void BodyDestroy( IElement element )
    {
      foreach ( var presenter in _presenters ) {
        if ( presenter.IsRaised( element ) ) {
          presenter.Destroy( element );
        }
      }

      if ( _eventedElements.Remove( element ) ) {
        element.AfterNewlyValueChanged -= Element_ValueChanged;
        element.AfterHistoricallyValueChanged -= Element_ValueChanged;
        element.VisibilityChanged -= Element_ValueChanged;
        element.AfterNewlyChildrenChanged -= Element_ChildrenChanged;
        element.AfterHistoricallyChildrenChanged -= Element_ChildrenChanged;

        if ( element is IRelocatable relocatable ) {
          relocatable.LocalCodChanged -= Element_LocalCodChanged;
        }
      }
      else {
        throw new InvalidOperationException() ;
      }

      foreach ( var child in element.Children ) {
        Unregister( child );
      }
    }

    private void Processed()
    {
      foreach ( var presenter in _presenters ) {
        presenter.Processed();
      }
    }
    
    private void Element_ValueChanged( object sender, EventArgs e )
    {
      if ( sender is IElement element ) {
        _presentationCache.AddUpdate( element );
      }
    }

    private void Element_LocalCodChanged( object sender, EventArgs e )
    {
      if ( sender is IElement element ) {
        _presentationCache.AddTransformUpdate( element );
      }
    }

    private void Element_ChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      foreach ( var child in e.RemovedItems ) {
        _presentationCache.AddDestroy( child );
      }
      foreach ( var child in e.AddedItems ) {
        _presentationCache.AddRaise( child );
      }
    }
  }
}
