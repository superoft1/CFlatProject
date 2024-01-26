using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.UI
{
  /// <summary>
  /// ツリービュー要素のマウスイベント クラス。
  /// </summary>
  class TreeViewItemMouseButtonEventArgs : EventArgs
  {
    /// <summary>
    /// 関連ボタン。
    /// </summary>
    public MouseButtonType ButtonType { get; private set; }

    /// <summary>
    /// 発生ツリービュー要素。
    /// </summary>
    public TreeViewItem TreeViewItem { get; private set; }

    public TreeViewItemMouseButtonEventArgs( TreeViewItem tvitem, MouseButtonType type )
    {
      TreeViewItem = tvitem;
      ButtonType = type;
    }
  }

  /// <summary>
  /// ツリービュー要素のイベント クラス。
  /// </summary>
  class TreeViewItemEventArgs : EventArgs
  {
    /// <summary>
    /// 発生ツリービュー要素。
    /// </summary>
    public TreeViewItem TreeViewItem { get; private set; }

    public TreeViewItemEventArgs( TreeViewItem tvitem )
    {
      TreeViewItem = tvitem;
    }
  }

  /// <summary>
  /// ツリービュー クラス。
  /// </summary>
  class TreeView : MonoBehaviour
  {
    [SerializeField]
    private TreeViewItemChildrenArea childrenArea;

    [SerializeField]
    private int indent = 20;

    [SerializeField]
    public GameObject treeViewPrefab;

    private object _treeViewItemSource;

    public event EventHandler<TreeViewItemMouseButtonEventArgs> TreeViewItemClick;
    public event EventHandler<TreeViewItemMouseButtonEventArgs> TreeViewItemDoubleClick;
    public event EventHandler<TreeViewItemEventArgs> TreeViewItemCheckBoxClick;

    /// <summary>
    /// インデント
    /// </summary>
    public int Indent
    {
      get { return indent; }
      set
      {
        if ( indent == value ) return;

        indent = value;
        foreach ( var tvitem in childrenArea ) {
          tvitem.SetIndent( indent );
        }
      }
    }

    public IList<TreeViewItem> Items { get { return childrenArea; } }

    public TreeViewItem CreateTreeViewItem()
    {
      return Instantiate( treeViewPrefab ).GetComponent<TreeViewItem>();
    }

    internal void RegisterEvents( TreeViewItem tvitem )
    {
      tvitem.Click += TreeViewItem_Click;
      tvitem.DoubleClick += TreeViewItem_DoubleClick;
      tvitem.CheckBoxClick += TreeViewItem_CheckBoxClick;
    }

    internal void UnregisterEvents( TreeViewItem tvitem )
    {
      tvitem.Click -= TreeViewItem_Click;
      tvitem.DoubleClick -= TreeViewItem_DoubleClick;
      tvitem.CheckBoxClick -= TreeViewItem_CheckBoxClick;
    }

    private void TreeViewItem_Click( object sender, MouseButtonEventArgs e )
    {
      var tvitem = sender as TreeViewItem;
      if ( null == tvitem ) return;

      OnTreeViewItemClick( new TreeViewItemMouseButtonEventArgs( tvitem, e.ButtonType ) );
    }

    private void TreeViewItem_DoubleClick( object sender, MouseButtonEventArgs e )
    {
      var tvitem = sender as TreeViewItem;
      if ( null == tvitem ) return;

      OnTreeViewItemDoubleClick( new TreeViewItemMouseButtonEventArgs( tvitem, e.ButtonType ) );
    }

    private void TreeViewItem_CheckBoxClick( object sender, EventArgs e )
    {
      var tvitem = sender as TreeViewItem;
      if ( null == tvitem ) return;

      OnCheckBoxClick( new TreeViewItemEventArgs( tvitem ) );
    }

    protected virtual void OnTreeViewItemClick( TreeViewItemMouseButtonEventArgs e )
    {
      if ( null != TreeViewItemClick ) {
        TreeViewItemClick( this, e );
      }
    }

    protected virtual void OnTreeViewItemDoubleClick( TreeViewItemMouseButtonEventArgs e )
    {
      if ( null != TreeViewItemDoubleClick ) {
        TreeViewItemDoubleClick( this, e );
      }
    }

    protected virtual void OnCheckBoxClick( TreeViewItemEventArgs e )
    {
      if ( null != TreeViewItemCheckBoxClick ) {
        TreeViewItemCheckBoxClick( this, e );
      }
    }
  }
}
