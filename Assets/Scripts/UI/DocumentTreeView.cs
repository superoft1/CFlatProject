using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Chiyoda.CAD.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  class DocumentTreeView : MonoBehaviour
  {
    private const int KEY_REPEAT_START_FRAME = 20;
    private const int KEY_REPEAT_INTERVAL_FRAME = 2;

    private IElement _target;
    private bool _started = false;
    private bool _isFitting = false;
    private TreeViewItem _baseItem = null;
    private int keyCountUp = 0;
    private int keyCountDown = 0;
    private int keyCountLeft = 0;
    private int keyCountRight = 0;

    public TreeView TreeView;
    public CameraOperator CameraOperator;

    public IElement Target
    {
      get { return _target; }
      set
      {
        if ( object.ReferenceEquals( _target, value ) ) return;

        _target = value;
      }
    }

    public static DocumentTreeView Instance()
    {
      return instance;
    }

    static DocumentTreeView instance = null;

    private void Awake()
    {
      if ( instance == null )
      {
        instance = this;
      }
    }

    private void Start()
    {
      TreeView.Items.Clear();

      TreeView.TreeViewItemDoubleClick += TreeView_TreeViewItemDoubleClick;
      TreeView.TreeViewItemClick += TreeView_TreeViewItemClick;
      TreeView.TreeViewItemCheckBoxClick += TreeView_TreeViewItemCheckBoxClick;

      _started = true;
    }

    private void OnDestroy()
    {
      _started = false;

      TreeView.TreeViewItemDoubleClick -= TreeView_TreeViewItemDoubleClick;
      TreeView.TreeViewItemClick -= TreeView_TreeViewItemClick;
      TreeView.TreeViewItemCheckBoxClick -= TreeView_TreeViewItemCheckBoxClick;
    }

    private void Update()
    {
      bool focus = EventSystem.current.currentSelectedGameObject != null;
      bool up = Input.GetKey(KeyCode.UpArrow);
      bool down = Input.GetKey(KeyCode.DownArrow);
      bool left = Input.GetKey(KeyCode.LeftArrow);
      bool right = Input.GetKey(KeyCode.RightArrow);
      bool upRepeat = CheckKeyRepeat(!focus && up && !down && !left && !right, ref keyCountUp);
      bool downRepeat = CheckKeyRepeat(!focus && !up && down && !left && !right, ref keyCountDown);
      bool leftRepeat = CheckKeyRepeat(!focus && !up && !down && left && !right, ref keyCountLeft);
      bool rightRepeat = CheckKeyRepeat(!focus && !up && !down && !left && right, ref keyCountRight);

      if (upRepeat)
      {
        SelectPrevItem();
      }
      else if (downRepeat)
      {
        SelectNextItem();
      }
      else if (leftRepeat)
      {
        CollapseSelectedItem();
      }
      else if (rightRepeat)
      {
        ExpandSelectedItem();
      }
      else if (Input.GetKeyDown(KeyCode.Escape))
      {
        if ( null != _target ) {
          _target.Document.DeselectAllElements();
        }
      }

      if ( _isFitting ) {
        _isFitting = false;
        FitAll();
      }
    }

    public void Maintain()
    {
      if ( null != _target ) {
        _target.Document.MaintainEdgePlacement();
      }
    }

    public void PostFitAll()
    {
      _isFitting = true;
    }

    public void FitAll()
    {
      if ( null != CameraOperator && null != TreeView ) {
        CameraOperator.SetBoundary( CAD.Boundary.GetBounds( _target ) );
      }
    }

    private void TreeView_TreeViewItemCheckBoxClick( object sender, TreeViewItemEventArgs e )
    {
      var visible = ( CheckState.Unchecked == e.TreeViewItem.CheckState );
      ChangeItemVisibility( e.TreeViewItem, visible );

      var selectedItems = GetSelectedAllDescendants();
      if ( selectedItems.Contains( e.TreeViewItem ) )
      {
        // 選択中のアイテムがあり、かつこのアイテムが含まれているのであれば、選択中すべてのアイテムの表示状態を更新する
        foreach ( var item in selectedItems.Where( item => item != e.TreeViewItem ) ) {
          ChangeItemVisibility( item, visible );
        }
      }
    }

    internal void ChangeItemVisibility( TreeViewItem item, bool visible )
    {
      var elm = item.GetTreeViewItemSource() as IElement;
      if ( null == elm ) return;

      switch ( elm )
      {
        case Line line:
          foreach ( var leafEdge in line.LeafEdges ) {
            ChangeItemVisibility( leafEdge, visible );
          }
          break;
        case HydraulicStream stream:
          foreach ( var leafEdge in stream.LeafEdges ) {
            ChangeItemVisibility( leafEdge, visible );
          }
          break;
      }

      ChangeItemVisibility( elm, visible );

      if ( null != _target ) {
        _target.Document.HistoryCommit();
      }
    }

    private void ChangeItemVisibility( IElement elm, bool visible )
    {
      if ( elm.IsVisible == visible ) return;

      // 子要素へ表示状態を反映
      SetAllDescendantsVisibility( elm, visible );

      elm.IsVisible = visible;

      // 親要素へ表示状態を反映
      if ( visible ) {
        SetAllParentToVisible( elm ); // 非表示→表示の場合は親を全て表示へ
      }
      else {
        SetAllParentToUnvisible( elm ); // 表示→非表示の場合は子が全て非表示の親を非表示へ
      }
    }

    private void SetAllParentToVisible( IElement elm )
    {
      IElement parent = elm.Parent;
      if ( null != parent && !parent.IsVisible ) {
        parent.IsVisible = true;
        SetAllParentToVisible( parent );
      }
    }

    private void SetAllParentToUnvisible( IElement elm )
    {
      IElement parent = elm.Parent;
      if ( null != parent && parent.IsVisible ) {
        if ( ( parent is LeafEdge ) ||
             ( (  ( parent is IGroup                group     ) && group.EdgeList    .All( child => !child.IsVisible ) ) &&
               ( !( parent is ISupportParentElement supported ) || supported.Supports.All( child => !child.IsVisible ) ) ) ) {
          parent.IsVisible = false;
          SetAllParentToUnvisible( parent );
        }
      }
    }

    private void SetAllDescendantsVisibility( IElement elm, bool visible )
    {
      foreach ( var child in elm.Children ) {
        if ( child.IsVisible != visible ) {
          SetAllDescendantsVisibility( child, visible );
          child.IsVisible = visible;
        }
      }
    }
    
    private void TreeView_TreeViewItemClick( object sender, TreeViewItemMouseButtonEventArgs e )
    {
      if ( MouseButtonType.Left == e.ButtonType ) {
        var clickItem = e.TreeViewItem;
        var element = clickItem.GetTreeViewItemSource() as IElement;
        if ( null != element ) {
          if ( InputKeyModifier.IsShiftDown )
          {
            if (_baseItem == null || !_baseItem.gameObject.activeInHierarchy || _baseItem.Parent != clickItem.Parent || _baseItem == clickItem)
            {
              // 唯一の選択
              element.Document.SelectElement(element);
              _baseItem = clickItem;
            }
            else
            {
              // 選択するべきアイテムのリストを作成
              var selectionList = new List<IElement>();
              int n = 0;
              foreach (var item in _baseItem.Parent.Items)
              {
                // _baseItemとclickItemの間にあるitemをリストアップする
                // n=0なら_baseItemまたはclickItemが見つかる前なのでリストに追加しない
                // n=1なら_baseItemとclickItemの間にあるitemなのでリストに追加する
                // n=2なら_baseItemとclickItemが両方見つかった状態なのでリストに追加してループを抜ける
                if (item == _baseItem || item == clickItem) n++;
                if (n == 0) continue;
                selectionList.Add(item.GetTreeViewItemSource() as IElement);
                if (n == 2) break;
              }

              // 初めに選択したitemを先頭にする
              if (selectionList.FirstOrDefault() != _baseItem.GetTreeViewItemSource()) {
                selectionList.Reverse();
              }

              // リストに格納されたアイテムを選択状態にする
              element.Document.SelectElements(selectionList);
            }
          }
          else if ( InputKeyModifier.IsCtrlOrCmdDown ) {
            // 追加
            element.Document.SelectElement(element, true);
            _baseItem = clickItem;
          }
          else {
            // 唯一の選択
            element.Document.SelectElement(element);
            _baseItem = clickItem;
          }
        }
      }
    }

    private void TreeView_TreeViewItemDoubleClick( object sender, TreeViewItemMouseButtonEventArgs e )
    {
      if ( MouseButtonType.Left == e.ButtonType ) {
        Fit(e.TreeViewItem.GetTreeViewItemSource());
      }
    }

    public void Fit(object obj)
    {
      if (null != CameraOperator)
      {
        CameraOperator.SetBoundary(CAD.Boundary.GetBounds(obj));
      }
    }

    
    internal IEnumerable<T> GetSelectedItem<T>()
    {
      return ((TreeViewItemChildrenArea)TreeView.Items).GetSelectedAllDescendants()
                                                 .Where(item => item.GetTreeViewItemSource() is T)
                                                 .Select(item => (T)item.GetTreeViewItemSource());
    }

    // 選択中のアイテムをすべて取得
    internal IEnumerable<TreeViewItem> GetSelectedAllDescendants()
    {
      return ((TreeViewItemChildrenArea)TreeView.Items).GetSelectedAllDescendants();
    }

    internal IEnumerable<TreeViewItem> GetDescendantsByNames(ICollection<string> itemNames)
    {
      return ((TreeViewItemChildrenArea)TreeView.Items).GetDescendantsByNames(itemNames);
    }

    internal IEnumerable<T> GetAllDescendants<T>()
    {
      return ((TreeViewItemChildrenArea) TreeView.Items).GetAllDescendants()
        .Where(item => item.GetTreeViewItemSource() is T)
        .Select(item => (T) item.GetTreeViewItemSource());
    }

    // 次のアイテムを選択
    private void SelectNextItem()
    {
      var lastSelection = DocumentCollection.Instance.Current?.LastSelection;
      if (lastSelection == null) return;

      bool found = false;
      IElement nextElement = null;
      TreeViewItem nextItem = null;

      foreach (var item in ((TreeViewItemChildrenArea) TreeView.Items).GetAllDescendants())
      {
        if (!item.gameObject.activeInHierarchy) continue;
        nextElement = item.GetTreeViewItemSource() as IElement;
        nextItem = item;
        if (found) break;
        if (nextElement == lastSelection) found = true;
      }

      if (nextElement != null)
      {
        nextElement.Document.SelectElement(nextElement);
        _baseItem = nextItem;
      }
    }

    // キーリピートチェック
    private bool CheckKeyRepeat(bool pressed, ref int pressedCount)
    {
      if (pressed)
      {
        pressedCount++;

        if (pressedCount == 1)
        {
          // キーを押した瞬間
          return true;
        }
        if (pressedCount == KEY_REPEAT_START_FRAME)
        {
          // キーを押してから一定フレーム経過後、リピート開始
          return true;
        }
        if (pressedCount == KEY_REPEAT_START_FRAME + KEY_REPEAT_INTERVAL_FRAME)
        {
          // キーを押しっぱなしにした場合、一定フレーム間隔でキーリピート
          pressedCount = KEY_REPEAT_START_FRAME;
          return true;
        }
      }
      else
      {
        pressedCount = 0;
      }

      return false;
    }
    
    // 前のアイテムを選択
    private void SelectPrevItem()
    {
      var lastSelection = DocumentCollection.Instance.Current?.LastSelection;
      if (lastSelection == null) return;

      IElement prevElement = null;
      TreeViewItem prevItem = null;

      foreach (var item in ((TreeViewItemChildrenArea) TreeView.Items).GetAllDescendants())
      {
        if (!item.gameObject.activeInHierarchy) continue;
        var e = item.GetTreeViewItemSource() as IElement;
        if (e == lastSelection) break;
        prevElement = e;
        prevItem = item;
      }

      if (prevElement != null)
      {
        prevElement.Document.SelectElement(prevElement);
        _baseItem = prevItem;
      }
    }

    // 選択中のアイテムを展開
    private void ExpandSelectedItem()
    {
      var selectItems = GetSelectedAllDescendants().ToList();
      if (!selectItems.Any())
      {
        return;
      }

      if (selectItems.Count > 1)
      {
        selectItems.ForEach(item => item.IsExpanded = true);
        return;
      }

      var selectItem = selectItems.First();
      if (!selectItem.IsExpanded)
      {
        selectItem.IsExpanded = true;
        return;
      }

      var childItem = selectItem.Items.FirstOrDefault();
      if (childItem != null)
      {
        var childElement = childItem.GetTreeViewItemSource() as IElement;
        childElement?.Document.SelectElement(childElement);
      }
    }

    // 選択中のアイテムを閉じる
    private void CollapseSelectedItem()
    {
      var selectItems = GetSelectedAllDescendants().ToList();
      if (!selectItems.Any())
      {
        return;
      }

      if (selectItems.Count > 1)
      {
        selectItems.ForEach(item => item.IsExpanded = false);
        return;
      }

      var selectItem = selectItems.First();
      if (selectItem.IsExpanded && selectItem.Items.Any())
      {
        selectItem.IsExpanded = false;
        return;
      }

      var parentItem = selectItem.Parent;
      if (parentItem != null)
      {
        var parentElement = parentItem.GetTreeViewItemSource() as IElement;
        parentElement?.Document.SelectElement(parentElement);
      }
    }
  }
}
