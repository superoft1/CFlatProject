﻿using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Chiyoda.CAD.Model ;
using UnityEngine ;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

namespace Chiyoda.UI
{
  class ElementList : MonoBehaviour
  {
    private const int KEY_REPEAT_START_FRAME = 20 ;
    private const int KEY_REPEAT_INTERVAL_FRAME = 2 ;

    private bool _started = false ;
    private ListBoxItem _baseItem = null ;
    private int keyCountUp = 0 ;
    private int keyCountDown = 0 ;

    private readonly Dictionary<IElement, ListBoxItem> _itemMap = new Dictionary<IElement, ListBoxItem>() ;

    public ListBox ListBox ;
    public CameraOperator CameraOperator ;
    public ListBoxItemPresenter Presenter ;

    public IElement Target { get ; private set ; }

    public ICollection TargetCollection { get ; private set ; }

    public void SetTarget<T>( IElement elm, ICollection<T> collection )
    {
      if ( null == collection ) {
        SetTarget( elm, null ) ;
      }
      else {
        SetTarget( elm, new Common.CollectionProxy<T>( collection ) ) ;
      }
    }

    public void SetTarget( IElement elm, ICollection collection )
    {
      if ( ( elm == null ) != ( collection == null ) ) throw new ArgumentException( $"`{nameof( elm )}' and `{nameof( collection )}' must be both null or both nonnull." ) ;
      
      if ( ReferenceEquals( Target, elm ) && ProxiedReferenceEqual( TargetCollection, collection ) ) return ;

      RemoveEventHandlers( Target, TargetCollection ) ;

      Target = elm ;
      TargetCollection = collection ;
      ListBox.Items.Clear() ;
      _itemMap.Clear() ;
      
      AddEventHandlers( Target, TargetCollection ) ;

      if ( null != collection ) {
        foreach ( var e in collection.OfType<IElement>() ) {
          var item = CreateListBoxItem( e ) ;
          if ( null != item ) {
            _itemMap.Add( e, item ) ;
            ListBox.Items.Add( item ) ;
          }
        }
      }
    }

    private bool ProxiedReferenceEqual( ICollection col1, ICollection col2 )
    {
      var obj1 = ( col1 is Common.ICollectionProxy proxy1 ) ? proxy1.BaseCollection : col1 ;
      var obj2 = ( col2 is Common.ICollectionProxy proxy2 ) ? proxy2.BaseCollection : col2 ;
      return ReferenceEquals( obj1, obj2 ) ;
    }

    private void AddEventHandlers( IElement target, ICollection collection )
    {
      if ( target == null ) return ;
      
      target.AfterNewlyChildrenChanged += Target_ChildrenChanged;
      target.AfterHistoricallyChildrenChanged += Target_ChildrenChanged;

      foreach ( var e in collection.OfType<IElement>() ) {
        e.AfterNewlyValueChanged += Element_ValueChanged;
        e.AfterHistoricallyValueChanged += Element_ValueChanged ;
      }
    }

    private void RemoveEventHandlers( IElement target, ICollection collection )
    {
      if ( target == null ) return ;

      target.AfterNewlyChildrenChanged -= Target_ChildrenChanged;
      target.AfterHistoricallyChildrenChanged -= Target_ChildrenChanged;

      foreach ( var e in collection.OfType<IElement>() ) {
        e.AfterNewlyValueChanged -= Element_ValueChanged;
        e.AfterHistoricallyValueChanged -= Element_ValueChanged ;
      }
    }

    private void Target_ChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      // 不要アイテムの削除
      List<IElement> removeList = null ;
      e.RemovedItems.ForEach( elm =>
      {
        if ( _itemMap.TryGetValue( elm, out var item ) ) {
          ListBox.Items.Remove( item ) ;

          if ( null == removeList ) removeList = new List<IElement>() ;
          removeList.Add( elm ) ;
        }
      } );
      if ( null != removeList ) {
        foreach ( var elm in removeList ) {
          _itemMap.Remove( elm ) ;
        }
      }

      // 必要アイテムの追加 (AddedItemsがTargetCollection麾下である保証はないため、TargetCollectionを追う)
      TargetCollection.OfType<IElement>().ForEach( ( elm, i ) =>
      {
        if ( _itemMap.TryGetValue( elm, out var item ) ) {
          ListBox.Items.MoveOrder( item, i ) ;
        }
        else {
          var newItem = CreateListBoxItem( elm ) ;
          if ( null != newItem ) {
            _itemMap.Add( elm, newItem ) ;
            ListBox.Items.Insert( i, newItem ) ;
          }
        }
      } );
    }

    private void Element_ValueChanged( object sender, EventArgs e )
    {
      var elm = (IElement) sender ;
      if ( _itemMap.TryGetValue( elm, out var item ) ) {
        UpdateListBoxItem( elm, item ) ;
      }
    }

    private ListBoxItem CreateListBoxItem( IElement elm )
    {
      if ( null == Presenter ) return null ;
      return Presenter.CreateItem( elm ) ;
    }

    private void UpdateListBoxItem( IElement elm, ListBoxItem item )
    {
      if ( null == Presenter ) return ;
      Presenter.UpdateItem( elm, item ) ;
    }


    private void Start()
    {
      ListBox.Items.Clear() ;

      ListBox.ListBoxItemDoubleClick += ListBox_ListBoxItemDoubleClick ;
      ListBox.ListBoxItemClick += ListBox_ListBoxItemClick ;

      if ( null != DocumentCollection.Instance.Current ) {
        CameraOperator.SetBoundary( DocumentCollection.Instance.Current.Region.GetGlobalBounds() ) ;
      }

      _started = true ;
    }

    private void OnDestroy()
    {
      _started = false ;

      ListBox.ListBoxItemDoubleClick -= ListBox_ListBoxItemDoubleClick ;
      ListBox.ListBoxItemClick -= ListBox_ListBoxItemClick ;
    }

    private void Update()
    {
      bool focus = EventSystem.current.currentSelectedGameObject != null ;
      bool up = Input.GetKey( KeyCode.UpArrow ) ;
      bool down = Input.GetKey( KeyCode.DownArrow ) ;
      bool left = Input.GetKey( KeyCode.LeftArrow ) ;
      bool right = Input.GetKey( KeyCode.RightArrow ) ;
      bool upRepeat = CheckKeyRepeat( ! focus && up && ! down && ! left && ! right, ref keyCountUp ) ;
      bool downRepeat = CheckKeyRepeat( ! focus && ! up && down && ! left && ! right, ref keyCountDown ) ;

      if ( upRepeat ) {
        SelectPrevItem() ;
      }
      else if ( downRepeat ) {
        SelectNextItem() ;
      }
      else if ( Input.GetKeyDown( KeyCode.Escape ) ) {
        if ( null != Target && null != TargetCollection ) {
          foreach ( var obj in TargetCollection ) {
            if ( obj is IElement elm ) {
              Target.Document.DeselectElement( elm ) ;
            }
          }
        }
      }
    }

    private void SelectPrevItem()
    {
      if ( null == _baseItem ) return ;

      var index = ListBox.Items.IndexOf( _baseItem ) ;
      if ( 0 < index ) {
        _baseItem.IsOn = false ;
        _baseItem = ListBox.Items[ index - 1 ] ;
        _baseItem.IsOn = true ;
      }
    }

    private void SelectNextItem()
    {
      if ( null == _baseItem ) return ;

      var index = ListBox.Items.IndexOf( _baseItem ) ;
      if ( index < ListBox.Items.Count - 1 ) {
        _baseItem.IsOn = false ;
        _baseItem = ListBox.Items[ index + 1 ] ;
        _baseItem.IsOn = true ;
      }
    }

    private void ListBox_ListBoxItemClick( object sender, ListBoxItemMouseButtonEventArgs e )
    {
      if ( MouseButtonType.Left == e.ButtonType ) {
        var clickItem = e.ListBoxItem ;
        if ( clickItem.GetListBoxItemSource() is IElement element ) {
          if ( InputKeyModifier.IsShiftDown ) {
            if ( _baseItem == null || ! _baseItem.gameObject.activeInHierarchy || _baseItem == clickItem ) {
              // 唯一の選択
              element.Document.SelectElement( element ) ;
              _baseItem = clickItem ;
            }
            else {
              // 選択するべきアイテムのリストを作成
              var selectionList = new List<IElement>() ;
              int n = 0 ;
              foreach ( var item in ListBox.Items ) {
                // _baseItemとclickItemの間にあるitemをリストアップする
                // n=0なら_baseItemまたはclickItemが見つかる前なのでリストに追加しない
                // n=1なら_baseItemとclickItemの間にあるitemなのでリストに追加する
                // n=2なら_baseItemとclickItemが両方見つかった状態なのでリストに追加してループを抜ける
                if ( item == _baseItem || item == clickItem ) n++ ;
                if ( n == 0 ) continue ;
                selectionList.Add( item.GetListBoxItemSource() as IElement ) ;
                if ( n == 2 ) break ;
              }

              // 初めに選択したitemを先頭にする
              if ( selectionList.FirstOrDefault() != _baseItem.GetListBoxItemSource() ) {
                selectionList.Reverse() ;
              }

              // リストに格納されたアイテムを選択状態にする
              element.Document.SelectElements( selectionList ) ;
            }
          }
          else if ( InputKeyModifier.IsCtrlOrCmdDown ) {
            // 追加
            element.Document.SelectElement( element, true ) ;
            _baseItem = clickItem ;
          }
          else {
            // 唯一の選択
            element.Document.SelectElement( element ) ;
            _baseItem = clickItem ;
          }
        }
      }
    }

    private void ListBox_ListBoxItemDoubleClick( object sender, ListBoxItemMouseButtonEventArgs e )
    {
      if ( MouseButtonType.Left == e.ButtonType ) {
        Fit( e.ListBoxItem.GetListBoxItemSource() ) ;
      }
    }

    public void Fit( object obj )
    {
      if ( null != CameraOperator ) {
        CameraOperator.SetBoundary( CAD.Boundary.GetBounds( obj ) ) ;
      }
    }


    internal IEnumerable<T> GetSelectedItem<T>()
    {
      return ListBox.Items.Where( item => item.IsOn && item.GetListBoxItemSource() is T ).Select( item => (T) item.GetListBoxItemSource() ) ;
    }

    // キーリピートチェック
    private bool CheckKeyRepeat( bool pressed, ref int pressedCount )
    {
      if ( pressed ) {
        pressedCount++ ;

        if ( pressedCount == 1 ) {
          // キーを押した瞬間
          return true ;
        }

        if ( pressedCount == KEY_REPEAT_START_FRAME ) {
          // キーを押してから一定フレーム経過後、リピート開始
          return true ;
        }

        if ( pressedCount == KEY_REPEAT_START_FRAME + KEY_REPEAT_INTERVAL_FRAME ) {
          // キーを押しっぱなしにした場合、一定フレーム間隔でキーリピート
          pressedCount = KEY_REPEAT_START_FRAME ;
          return true ;
        }
      }
      else {
        pressedCount = 0 ;
      }

      return false ;
    }
  }
}