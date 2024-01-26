using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  [RequireComponent( typeof( VerticalLayoutGroup ) )]
  class TreeViewItemChildrenArea : MonoBehaviour, IList<TreeViewItem>
  {
    [SerializeField]
    private TreeViewItem treeViewItem;

    public TreeViewItem TreeViewItem { get { return treeViewItem; } }

    private int exceptCount = 0;

    public TreeView TreeView
    {
      get
      {
        var oa = this;
        while ( true ) {
          var tvitem = oa.treeViewItem;
          if ( null == tvitem ) break;

          oa = tvitem.OwnerArea;
          if ( null == oa ) return null;
        }

        for ( var t = transform.parent ; null != t ; t = t.parent ) {
          var tv = t.GetComponent<TreeView>();
          if ( null != tv ) return tv;
        }

        return null;
      }
    }

    public TreeViewItem this[int index]
    {
      get
      {
        if ( index < 0 || Count <= index ) throw new ArgumentOutOfRangeException();

        var trans = transform.OfType<Transform>()
                             .Where( child => null != child.GetComponent<TreeViewItem>() )
                             .ElementAt( index );
        return trans.GetComponent<TreeViewItem>();
      }
      set
      {
        if ( index < 0 || Count <= index ) throw new ArgumentOutOfRangeException();
        if ( null != value.transform.parent ) {
          throw new InvalidOperationException( "TreeViewItem already has parent." );
        }

        var trans = transform.OfType<Transform>()
                             .Select( ( child, i ) => new { value = child, index = i } )
                             .Where( tmp => null != tmp.value.GetComponent<TreeViewItem>() )
                             .ElementAt( index );

        trans.value.SetParent( null );

        value.transform.SetParent( transform );
        value.transform.SetSiblingIndex( trans.index );
      }
    }

    public int Count => transform.childCount - exceptCount;

    public bool IsReadOnly { get { return false; } }

    public void Add( TreeViewItem item )
    {
      if ( null != item.transform.parent ) {
        throw new InvalidOperationException( "TreeViewItem already has parent." );
      }
      item.transform.SetParent( transform );
      item.OnAdded( TreeView );

      AfterAdd();
    }

    public void Clear()
    {
      var tv = TreeView;

      foreach ( var tvitem in this.ToArray() ) {
        tvitem.OnRemoving( tv ) ;
        tvitem.transform.SetParent( null ) ;
      }

      exceptCount = transform.childCount;

      AfterRemove();
    }

    public bool Contains( TreeViewItem item )
    {
      return ( transform == item.transform.parent );
    }

    public void CopyTo( TreeViewItem[] array, int arrayIndex )
    {
      if ( null == array ) throw new ArgumentNullException( "array" );
      var items = transform.OfType<Transform>()
                           .Select( child => child.GetComponent<TreeViewItem>() )
                           .Where( tvitem => null != tvitem )
                           .ToArray();
      if ( arrayIndex < 0 || array.Length < arrayIndex + items.Length ) throw new ArgumentOutOfRangeException( "arrayIndex" );
      items.CopyTo( array, arrayIndex );
    }

    public IEnumerator<TreeViewItem> GetEnumerator()
    {
      if ( Count == 0 ) {
        yield break ;
      }

      foreach ( var tvitem in transform.OfType<Transform>().Select( child => child.GetComponent<TreeViewItem>() ).Where( tvitem => null != tvitem ) ) {
        yield return tvitem ;
      }
    }

    // 子も含めて全て取得
    public IEnumerable<TreeViewItem> GetAllDescendants()
    {
      foreach (var tv in this)
      {
        yield return tv;
        foreach (var tvChild in ((TreeViewItemChildrenArea) tv.Items).GetAllDescendants())
        {
          yield return tvChild;
        }
      }
    }

    // 選択中のアイテムをすべて取得
    public IEnumerable<TreeViewItem> GetSelectedAllDescendants()
    {
      foreach(var tv in this)
      {
        if (tv.IsOn)
        {
          yield return tv;
        }
        foreach(var tvChild in ((TreeViewItemChildrenArea)tv.Items).GetSelectedAllDescendants())
        {
          yield return tvChild;
        }
      }
    }

    // 指定した名前のアイテムをすべて取得
    public IEnumerable<TreeViewItem> GetDescendantsByNames(ICollection<string> itemNames)
    {
      foreach(var tv in this)
      {
        if (itemNames.Contains(tv.Text))
        {
          yield return tv;
        }
        foreach(var tvChild in ((TreeViewItemChildrenArea)tv.Items).GetDescendantsByNames(itemNames))
        {
          yield return tvChild;
        }
      }
    }

    public int IndexOf( TreeViewItem item )
    {
      if ( transform == item.transform.parent ) {
        var trans = transform.OfType<Transform>()
                             .Select( ( child, i ) => new { value = child, index = i } )
                             .SingleOrDefault( tmp => tmp.value == item.transform );
        return ( null != trans ) ? trans.index : -1;
      }
      return -1;
    }

    public void Insert( int index, TreeViewItem item )
    {
      if ( Count == index ) {
        Add( item );
        return;
      }

      if ( null != item.transform.parent ) {
        throw new InvalidOperationException( "TreeViewItem already has parent." );
      }

      var trans = transform.OfType<Transform>()
                           .Select( ( child, i ) => new { value = child, index = i } )
                           .Where( tmp => null != tmp.value.GetComponent<TreeViewItem>() )
                           .ElementAtOrDefault( index );
      if ( null != trans ) {
        index = trans.index;
      }

      item.transform.SetParent( transform );
      item.transform.SetSiblingIndex( index );
      item.OnAdded( TreeView );

      AfterAdd();
    }

    public bool Remove( TreeViewItem item )
    {
      if ( transform != item.transform.parent ) {
        return false;
      }

      item.OnRemoving( TreeView );
      item.transform.SetParent( null );

      AfterRemove();

      return true;
    }

    public void RemoveAt( int index )
    {
      Remove( this[index] );
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private void AfterAdd()
    {
      if ( null == TreeViewItem ) return;
      TreeViewItem.UpdateCanExpand();
    }

    private void AfterRemove()
    {
      if ( null == TreeViewItem ) return;
      TreeViewItem.UpdateCanExpand();
    }

    private void Awake()
    {
      exceptCount = null != treeViewItem ? transform.childCount : 0;
    }
  }
}
