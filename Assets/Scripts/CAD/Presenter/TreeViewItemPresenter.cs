using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Chiyoda.UI;
using Chiyoda.UI.PropertyUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  enum TreeViewMode
  {
    BasicView,
    LineView,
    StreamView,
  }

  class TreeViewItemPresenter : IEntityPresenter
  {
    private readonly DocumentTreeView _treeView;
    private TreeView.ITreeViewVisualizer _visualizer;

    private readonly Dictionary<TreeViewMode, TreeView.ITreeViewVisualizer> _visualizers;
    private readonly Dictionary<TreeViewMode, IPreferredSelector> _selectors;

    private TreeViewMode _mode;
    public TreeViewMode Mode
    {
      get => _mode;
      set
      {
        if ( _mode == value ) return;

        _mode = value;
        _visualizer = _visualizers[_mode];

        _treeView.TreeView.Items.Clear();
        _treeViewMap.Clear();

        RaiseRecursive( _treeView.Target );
      }
    }

    private readonly Dictionary<IElement, VirtualTreeViewItem> _treeViewMap = new Dictionary<IElement, VirtualTreeViewItem>();

    private bool _reupdateSelectionState = false;

    public TreeViewItemPresenter( DocumentTreeView treeView )
    {
      _treeView = treeView;

      _visualizers = new Dictionary<TreeViewMode, TreeView.ITreeViewVisualizer>
      {
        [TreeViewMode.BasicView] = new TreeView.ObjectHierarchyTreeViewVisualizer(),
        [TreeViewMode.LineView] = new TreeView.LineViewTreeViewVisualizer(),
        [TreeViewMode.StreamView] = new TreeView.HydraulicStreamViewTreeViewVisualizer(),
      };
      _visualizer = _visualizers[_mode];

      _selectors = new Dictionary<TreeViewMode, IPreferredSelector>
      {
        [TreeViewMode.BasicView] = new CompositeEdgeSelector(),
        [TreeViewMode.LineView] = new LineSelector(),
        [TreeViewMode.StreamView] = new StreamSelector(),
      };
    }

    public bool IsRaised( IElement element )
    {
      if ( element == null ) {
        return false ;
      }
      if ( false == _visualizer.WillVisualize( element ) ) return true;

      if ( element is Document ) {
        return (_treeView.Target == element);
      }
      else {
        return _treeViewMap.ContainsKey( element );
      }
    }

    public void Raise( IElement element )
    {
      var vtvitem = RaiseImpl( element ) ;

      if ( element is Document doc ) {
        doc.SelectionChanged += Document_SelectionChanged;
      }

      _reupdateSelectionState = true;
    }

    private VirtualTreeViewItem RaiseImpl( IElement element )
    {
      if ( false == _visualizer.WillVisualize( element ) ) return null;

      var vtvitem = new VirtualTreeViewItem( this, element );

      _treeViewMap.Add( element, vtvitem );

      if ( element is Document doc ) {
        doc.PreferredSelector = _selectors[_mode];
        var treeViewItem = vtvitem.TreeViewItem;
        treeViewItem.CanCollapse = false;
        _treeView.TreeView.Items.Add( treeViewItem );
        _treeView.Target = element;
        _treeView.PostFitAll();
      }

      return vtvitem ;
    }

    private void RaiseRecursive( IElement element )
    {
      var cache = new Queue<IElement>();
      cache.Enqueue( element );

      while ( cache.Any() ) {
        element = cache.Dequeue();

        var vtvitem = RaiseImpl( element );
        if ( null != vtvitem ) {
          UpdateImpl( vtvitem, element );
        }

        foreach ( var child in element.Children ) {
          cache.Enqueue( child );
        }
      }
    }

    public void Update( IElement element )
    {
      if ( false == _visualizer.WillVisualize( element ) ) {
        Destroy( element );
        return;
      }

      if ( !_treeViewMap.TryGetValue( element, out var vtvitem ) ) return;

      UpdateImpl( vtvitem, element ) ;
    }

    private void UpdateImpl( VirtualTreeViewItem vtvitem, IElement element )
    {
      VirtualTreeViewItem newParent = null;
      int itemPosition = -1;

      var parent = _visualizer.GetParentForTreeView( element );
      if ( null != parent ) {
        _treeViewMap.TryGetValue( parent, out newParent );

        itemPosition = _visualizer.GetChildrenForTreeView( parent ).Select( ( child, i ) => new { value = child, index = i } )
                                                                   .SingleOrDefault( pair => pair.value == element )
                                                                  ?.index ?? -1;
        if ( -1 == itemPosition ) {
          Debug.LogError( $"Cannot maintain tree hierarchy order when removing/adding {element.GetType().Name}." );
        }
      }

      var oldParent = vtvitem.SwapParent( newParent, itemPosition );

      if ( element.IsVisible ) {
        if ( !vtvitem.IsExpanded ) {
          vtvitem.CheckState = CheckState.Checked;
        }
        else {
          // 部分的に非表示状態の要素があればIndeterminateに設定
          vtvitem.CheckState = vtvitem.ChildrenStateIsAll( CheckState.Checked ) && element.Children.All( child => child.IsVisible ) ? CheckState.Checked : CheckState.Indeterminate;
        }
      }
      else {
        vtvitem.CheckState = CheckState.Unchecked;
      }

      if ( newParent != oldParent && null != oldParent ) {
        if ( CheckState.Checked != vtvitem.CheckState ) {
          UpdateIndeterminateAncestors( oldParent );
        }
      }

      if ( null != newParent ) {
        switch ( newParent.CheckState ) {
          case CheckState.Checked:
            if ( !element.IsVisible ) {
              // 親がCheckedかつ自身がChecked以外であれば、親をIndeterminateに設定
              UpdateCheckedAncestors( newParent );
            }
            break;
          case CheckState.Indeterminate:
            if ( element.IsVisible ) {
              // 親がIndeterminateかつそれらの子要素が全てCheckedであれば、親をCheckedに設定
              UpdateIndeterminateAncestors( newParent );
            }
            break;
        }
      }

      vtvitem.Text = _visualizer.GetNodeName( element );
      vtvitem.TextColor = element.HasError? new Color(248f / 255f, 152f / 255f, 0f / 255f) : Color.white;
    }

    private void UpdateCheckedAncestors( VirtualTreeViewItem treeViewItem )
    {
      for ( var tvItem = treeViewItem ; tvItem != null && CheckState.Checked == tvItem.CheckState ; tvItem = tvItem.Parent ) {
        tvItem.CheckState = CheckState.Indeterminate;
      }
    }

    private void UpdateIndeterminateAncestors( VirtualTreeViewItem treeViewItem )
    {
      for ( var tvItem = treeViewItem ; tvItem != null && tvItem.ChildrenStateIsAll( CheckState.Checked ) ; tvItem = tvItem.Parent ) {
        // 部分的に非表示状態の要素があればIndeterminateに設定
        tvItem.CheckState = tvItem.Element.Children.All( child => child.IsVisible ) ? CheckState.Checked : CheckState.Indeterminate;
      }
    }

    public void TransformUpdate( IElement element )
    {
      // 何もしない
    }

    public void Destroy( IElement element )
    {
      if ( !_treeViewMap.TryGetValue( element, out var vtvitem ) ) return;

      if ( element is Document doc ) {
        doc.SelectionChanged -= Document_SelectionChanged;
        doc.PreferredSelector = null;
        _treeView.Target = null;
      }

      var oldParentItem = vtvitem.Parent;
      var needUpdateAncestor = (null == oldParentItem && CheckState.Unchecked != vtvitem.CheckState);
      vtvitem.Destroy();
      _treeViewMap.Remove( element );

      if ( needUpdateAncestor ) {
        UpdateIndeterminateAncestors( oldParentItem );
      }

      _reupdateSelectionState = true;
    }

    public void Processed()
    {
      if ( _treeView.Target is Document doc ) {
        if ( _reupdateSelectionState && doc.SelectedElements.Any() ) {
          UpdateSelectionState( doc.SelectedElements.ToArray(), true );
          _reupdateSelectionState = false;
        }
      }
    }

    private void Document_SelectionChanged( object sender, ItemChangedEventArgs<IElement> args )
    {
      UpdateSelectionState( args.RemovedItems.Except( args.AddedItems ).ToArray(), false );
      UpdateSelectionState( args.AddedItems, true ); // 選択済み要素のツリーも再展開するためすべての選択要素とする
    }

    private void UpdateSelectionState( IElement[] elements, bool selected )
    {
      foreach ( var element in elements ) {
        if ( _treeViewMap.TryGetValue( element, out var vtvitem ) ) {
          vtvitem.SetSelected( selected );
        }
      }
    }

    private class VirtualTreeViewItem : ITreeViewSource
    {
      private TreeViewItemPresenter Presenter { get; }
      public IElement Element { get; }
      private TreeViewItem _item = null;
      private VirtualTreeViewItem _parent = null;
      private string _text = null;
      private Color _textColor = Color.white;
      private bool _selected = false;
      private CheckState _checkState = CheckState.Checked;
      private List<VirtualTreeViewItem> _unexpandedChildren = null;

      public VirtualTreeViewItem( TreeViewItemPresenter presenter, IElement elm )
      {
        Presenter = presenter;
        Element = elm;
      }

      public object Value => Element;

      public bool IsExpanded
      {
        get
        {
          if ( null == _item ) return false;
          return _item.IsExpanded;
        }
      }

      public string Text
      {
        get { return _text; }
        set
        {
          _text = value;
          if (null != _item) _item.Text = value;
        }
      }

      public Color TextColor
      {
        get { return _textColor; }
        set
        {
          _textColor = value;
          if (null != _item) _item.TextColor = value;
        }
      }

      public CheckState CheckState
      {
        get { return _checkState; }
        set
        {
          _checkState = value;
          if ( null != _item ) _item.CheckState = value;
        }
      }

      public void SetSelected( bool isSelected )
      {
        if ( null == _item && true == isSelected ) {
          // 要素を作っておかないと選択できない
          CreateAncestorItems();
        }

        _selected = isSelected;
        if ( null != _item ) {
          _item.SetSelected( isSelected );
        }
      }

      private void CreateAncestorItems()
      {
        if ( null != _item ) return;
        if ( null == Parent ) return;

        Parent.CreateAncestorItems();
        Parent.TreeViewItem.IsExpanded = true;
      }

      public VirtualTreeViewItem Parent
      {
        get => _parent;
//        set => SwapParent( value );
      }

      public VirtualTreeViewItem SwapParent( VirtualTreeViewItem newParent, int myPosition = -1 )
      {
        var oldParent = _parent;

        if ( newParent == _parent ) return oldParent;

        if ( null != _item ) {
          _item.OwnerItems.Remove( _item );
        }

        if ( null != _parent ) {
          if ( !_parent.IsExpanded ) {
            _parent.EraseUnexpandedChild( this );
          }
        }
        _parent = newParent;
        if ( null == _parent ) {
          Presenter._treeView.TreeView.Items.Add( TreeViewItem );
        }
        else {
          if ( null == _parent._item ) {
            if ( null != _item ) {
              DestroyItemsRecursive( _item );
              _item = null;
            }
            _parent.AddUnexpandedChild( this, myPosition );
          }
          else if ( false == _parent._item.IsExpanded && true == _parent._item.CanCollapse ) {
            _parent._item.UpdateCanExpand( true );
            _parent.AddUnexpandedChild( this, myPosition );
          }
          else {
            if ( -1 == myPosition ) {
              _parent._item.Items.Add( TreeViewItem );
            }
            else {
              ((IList<TreeViewItem>)_parent._item.Items).Insert( myPosition, TreeViewItem );
            }
          }
        }

        return oldParent;
      }

      public void Destroy()
      {
        _parent?.EraseUnexpandedChild( this );

        if ( null != _item ) {
          DestroyItemsRecursive( _item );
          _item = null;
        }
      }

      private void AddUnexpandedChild( VirtualTreeViewItem vtvitem, int position )
      {
        if ( null == _unexpandedChildren ) {
          _unexpandedChildren = new List<VirtualTreeViewItem>();
          if ( null != _item ) {
            _item.ExpandedChanged += TreeViewItem_ExpandedChanged;
          }
        }

        if ( -1 == position ) {
          _unexpandedChildren.Add( vtvitem );
        }
        else {
          _unexpandedChildren.Insert( position, vtvitem );
        }
      }

      private void EraseUnexpandedChild( VirtualTreeViewItem vtvitem )
      {
        if ( null == _unexpandedChildren ) return;

        _unexpandedChildren.Remove( vtvitem );
        if ( 0 == _unexpandedChildren.Count ) {
          if ( null != _item ) {
            _item.ExpandedChanged -= TreeViewItem_ExpandedChanged;
          }
          _unexpandedChildren = null;
        }
      }

      private void TreeViewItem_ExpandedChanged( object sender, EventArgs e )
      {
        var items = _item.Items;
        foreach ( var vtitem in _unexpandedChildren ) {
          items.Add( vtitem.TreeViewItem );
        }

        _item.ExpandedChanged -= TreeViewItem_ExpandedChanged;
        _unexpandedChildren = null;
      }

      private static void DestroyItemsRecursive( TreeViewItem item )
      {
        foreach ( var subitem in item.Items ) {
          DestroyItemsRecursive( subitem );
        }
        if ( item.Tag is VirtualTreeViewItem vtvitem ) {
          vtvitem._item = null;
        }
        GameObject.Destroy( item.gameObject );
      }

      public TreeViewItem TreeViewItem
      {
        get
        {
          if ( null == _item ) {
            _item = Presenter._treeView.TreeView.CreateTreeViewItem();
            _item.Tag = this;
            _item.Text = _text ?? string.Empty;
            _item.TextColor = _textColor;
            _item.SetSelected( _selected );
            _item.CheckState = _checkState;
            if ( null != _unexpandedChildren ) {
              _item.UpdateCanExpand( true );
              _item.ExpandedChanged += TreeViewItem_ExpandedChanged;
            }
          }
          return _item;
        }
      }

      internal bool ChildrenStateIsAll( CheckState state )
      {
        if ( null != _item ) {
          return _item.Items.All( child => child.CheckState == state );
        }
        else {
          return (_checkState == state);
        }
      }
    }

    private class CompositeEdgeSelector : IPreferredSelector
    {
      public IEnumerable<IElement> FindPreferredElements( IElement element )
      {
        switch ( element ) {
          case BlockEdge blockEdge:
            var blockEdges = new List<IElement> { blockEdge };
            while ( blockEdge.Parent is BlockEdge parentBlockEdge ) {
              blockEdges.Add( parentBlockEdge );
              blockEdge = parentBlockEdge;
            }
            blockEdges.Reverse();
            return blockEdges;
          case Route route:
            return new List<IElement> { route };
        }

        var parent = element.Parent;
        return null != parent ? FindPreferredElements( parent ) : Array.Empty<IElement>();
      }
    }

    private class LineSelector : IPreferredSelector
    {
      public IEnumerable<IElement> FindPreferredElements( IElement element )
      {
        switch ( element ) {
          case Line line:
            return new List<IElement> { line };
          case LeafEdge leafEdge:
            if ( null != leafEdge.Line ) {
              return new List<IElement> { leafEdge.Line };
            }
            break;
        }

        var parent = element.Parent;
        return null != parent ? FindPreferredElements( parent ) : Array.Empty<IElement>();
      }
    }

    private class StreamSelector : IPreferredSelector
    {
      public IEnumerable<IElement> FindPreferredElements( IElement element )
      {
        switch ( element ) {
          case HydraulicStream hydraulicStream:
            return new List<IElement> { hydraulicStream };
          case LeafEdge leafEdge:
            var hydraulicStreams = new HashSet<HydraulicStream>();
            foreach ( var halfVertex in leafEdge.Vertices ) {
              var stream = halfVertex.HydraulicStream;
              if ( null != stream ) {
                hydraulicStreams.Add( stream );
              }
            }
            if ( hydraulicStreams.Count == 1 ) {
              return hydraulicStreams;
            }
            break;
        }

        var parent = element.Parent;
        return null != parent ? FindPreferredElements( parent ) : Array.Empty<IElement>();
      }
    }
  }
}
