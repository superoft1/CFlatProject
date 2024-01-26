using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using Microsoft.CSharp.RuntimeBinder ;
using UnityEngine ;

namespace Chiyoda.UI
{
  /// <summary>
  /// リストボックス要素のマウスイベント クラス。
  /// </summary>
  class ListBoxItemMouseButtonEventArgs : EventArgs
  {
    /// <summary>
    /// 関連ボタン。
    /// </summary>
    public MouseButtonType ButtonType { get ; }

    /// <summary>
    /// 発生リストボックス要素。
    /// </summary>
    public ListBoxItem ListBoxItem { get ; }

    public ListBoxItemMouseButtonEventArgs( ListBoxItem listBoxItem, MouseButtonType type )
    {
      ListBoxItem = listBoxItem ;
      ButtonType = type ;
    }
  }

  /// <summary>
  /// リストボックス要素のイベント クラス。
  /// </summary>
  class ListBoxItemEventArgs : EventArgs
  {
    /// <summary>
    /// 発生リストボックス要素。
    /// </summary>
    public ListBoxItem ListBoxItem { get ; }

    public ListBoxItemEventArgs( ListBoxItem listBoxItem )
    {
      ListBoxItem = listBoxItem ;
    }
  }

  class ListBox : MonoBehaviour
  {
    [SerializeField]
    private GameObject childrenArea ;

    private ListBoxItemList _listboxItemList ;

    public event EventHandler<ListBoxItemMouseButtonEventArgs> ListBoxItemClick ;
    public event EventHandler<ListBoxItemMouseButtonEventArgs> ListBoxItemDoubleClick ;

    public ListBoxItemList Items
    {
      get
      {
        if ( null == _listboxItemList ) _listboxItemList = new ListBoxItemList( childrenArea ) ;
        return _listboxItemList ;
      }
    }
  }

  class ListBoxItemList : IList<ListBoxItem>
  {
    private readonly GameObject _childrenArea ;

    public ListBoxItemList( GameObject childrenArea )
    {
      _childrenArea = childrenArea ;
    }

    public IEnumerator<ListBoxItem> GetEnumerator()
    {
      return _childrenArea.transform.OfType<Transform>().Select( t => t.GetComponent<ListBoxItem>() ).Where( item => null != item ).GetEnumerator() ;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator() ;
    }

    public void Add( ListBoxItem item )
    {
      if ( Contains( item ) ) return ;

      if ( null != item.transform.parent ) {
        throw new InvalidOperationException( "ListBoxItem already has a parent." ) ;
      }

      item.transform.SetParent( _childrenArea.transform ) ;
    }

    public void Clear()
    {
      Clear( true ) ;
    }

    public void Clear( bool destroyItems )
    {
      foreach ( var item in this.ToArray() ) {
        item.transform.SetParent( null ) ;
        if ( destroyItems ) {
          GameObject.Destroy( item ) ;
        }
      }
    }

    public bool Contains( ListBoxItem item )
    {
      return ( item.transform.parent == _childrenArea.transform ) ;
    }

    public void CopyTo( ListBoxItem[] array, int arrayIndex )
    {
      this.ToArray().CopyTo( array, arrayIndex ) ;
    }

    public bool Remove( ListBoxItem item )
    {
      return Remove( item, true ) ;
    }

    public bool Remove( ListBoxItem item, bool destoryItem )
    {
      if ( Contains( item ) ) {
        item.transform.SetParent( null ) ;
        if ( destoryItem ) {
          GameObject.Destroy( item ) ;
        }

        return true ;
      }

      return false ;
    }

    public int Count => _childrenArea.transform.childCount ;
    public bool IsReadOnly => false ;

    public int IndexOf( ListBoxItem item )
    {
      if ( Contains( item ) ) {
        return item.transform.GetSiblingIndex() ;
      }

      return -1 ;
    }

    public void Insert( int index, ListBoxItem item )
    {
      if ( Contains( item ) ) return ;

      if ( null != item.transform.parent ) {
        throw new InvalidOperationException( "ListBoxItem already has a parent." ) ;
      }
      
      item.transform.SetParent( _childrenArea.transform ) ;
      item.transform.SetSiblingIndex( index ) ;
    }

    public void RemoveAt( int index )
    {
      RemoveAt( index, true ) ;
    }

    public void RemoveAt( int index, bool destroyItem )
    {
      var child = _childrenArea.transform.GetChild( index )?.GetComponent<ListBoxItem>() ;
      if ( null == child ) return ;

      Remove( child, destroyItem ) ;
    }

    public ListBoxItem this[ int index ]
    {
      get => _childrenArea.transform.GetChild( index )?.GetComponent<ListBoxItem>() ;
      set => SetAt( index, value, true ) ;
    }

    public void SetAt( int index, ListBoxItem item, bool destroyItem )
    {
      var oldItem = this[ index ] ;
      if ( oldItem == item ) return ;

      if ( null != item.transform.parent ) {
        throw new InvalidOperationException( "ListBoxItem already has a parent." ) ;
      }

      Remove( oldItem, destroyItem ) ;
      Insert( index, item ) ;
    }

    public void MoveOrder( ListBoxItem item, int index )
    {
      if ( false == Contains( item ) ) return ;

      item.transform.SetSiblingIndex( index ) ;
    }
  }
}