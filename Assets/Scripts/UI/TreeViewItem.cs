using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  /// <summary>
  /// マウスボタンの種類。
  /// </summary>
  enum MouseButtonType
  {
    /// <summary>左ボタン。</summary>
    Left = PointerEventData.InputButton.Left,
    /// <summary>中ボタン。</summary>
    Middle = PointerEventData.InputButton.Middle,
    /// <summary>右ボタン。</summary>
    Right = PointerEventData.InputButton.Right,
  }

  /// <summary>
  /// マウスイベント クラス。
  /// </summary>
  class MouseButtonEventArgs : EventArgs
  {
    /// <summary>
    /// 関連ボタン。
    /// </summary>
    public MouseButtonType ButtonType { get; private set; }

    public MouseButtonEventArgs( MouseButtonType type )
    {
      ButtonType = type;
    }
  }

  /// <summary>
  /// チェック状態。
  /// </summary>
  enum CheckState
  {
    /// <summary>チェックなし。</summary>
    Unchecked,
    /// <summary>チェック不定。</summary>
    Indeterminate,
    /// <summary>チェックあり。</summary>
    Checked,
  }

  interface ITreeViewSource
  {
    object Value { get; }
  }

  /// <summary>
  /// ツリービュー要素クラス。
  /// </summary>
  class TreeViewItem : MonoBehaviour
  {
    /// <summary>
    /// ツリービュー要素をクリックした際のイベント。
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> Click;

    /// <summary>
    /// ツリービュー要素をダブルクリックした際のイベント。
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> DoubleClick;

    /// <summary>
    /// ツリービュー要素のチェック状態が変化した際のイベント。
    /// </summary>
    public event EventHandler CheckBoxClick;

    /// <summary>
    /// ツリービュー要素の開閉が変わった際のイベント。
    /// </summary>
    public event EventHandler ExpandedChanged;

    [SerializeField]
    private TreeViewItemCheckBox checkBox;

    [SerializeField]
    private TreeViewItemExpander expander;

    [SerializeField]
    private TreeViewItemSelection selection;

    [SerializeField]
    private Text text;

    [SerializeField]
    private CheckState checkState = CheckState.Checked;

    [SerializeField]
    private TreeViewItemChildrenArea childrenArea;

    private int _currentIndent;

    /// <summary>
    /// チェック状態。
    /// </summary>
    public CheckState CheckState
    {
      get { return checkState; }
      set
      {
        if ( checkState != value ) {
          checkState = value;
          checkBox.SetState( value );
        }
      }
    }

    public bool CanCollapse
    {
      get { return expander.CanCollapse; }
      set { expander.CanCollapse = value; }
    }

    /// <summary>
    /// ツリービュー
    /// </summary>
    public TreeView TreeView
    {
      get
      {
        var owner = OwnerArea;
        if ( null == owner ) return null;

        return owner.TreeView;
      }
    }

    /// <summary>
    /// 親要素
    /// </summary>
    public TreeViewItem Parent
    {
      get
      {
        var owner = OwnerArea;
        if ( null == owner ) return null;

        return owner.TreeViewItem;
      }
    }

    /// <summary>
    /// TreeViewを保持する ICollection&lt;TreeViewItem&gt;
    /// </summary>
    public ICollection<TreeViewItem> OwnerItems
    {
      get { return OwnerArea; }
    }

    /// <summary>
    /// TreeViewItemを保持するエリア
    /// </summary>
    public TreeViewItemChildrenArea OwnerArea
    {
      get
      {
        if ( null == transform.parent ) {
          return null;
        }
        return transform.parent.GetComponent<TreeViewItemChildrenArea>();
      }
    }

    /// <summary>
    /// 子要素
    /// </summary>
    public ICollection<TreeViewItem> Items
    {
      get { return childrenArea; }
    }

    /// <summary>
    /// 要素と関連づけられるオブジェクト。
    /// </summary>
    public object Tag { get; set; }

    public bool IsExpanded
    {
      get { return expander.IsOn; }
      set
      {
        expander.IsOn = value;
      }
    }

    public string Text
    {
      get { return text.text; }
      set
      {
        text.text = value;
        gameObject.name = "TreeViewItem (" + value + ")";
      }
    }

    public Color TextColor
    {
      get { return text.color; }
      set
      {
        text.color = value;
      }
    }

    public bool IsOn
    {
      get { return selection.IsOn; }
    }

    private void Start()
    {
      checkBox.Click += CheckBox_Click;
      selection.Click += Selection_Click;
      selection.DoubleClick += Selection_DoubleClick;
    }

    private void OnDestroy()
    {
      checkBox.Click -= CheckBox_Click;
      selection.Click -= Selection_Click;
      selection.DoubleClick -= Selection_DoubleClick;
    }

    private void CheckBox_Click( object sender, MouseButtonEventArgs e )
    {
      if ( MouseButtonType.Left != e.ButtonType ) {
        return;
      }
      
      OnCheckBoxClick( EventArgs.Empty );
    }

    private void Selection_Click( object sender, MouseButtonEventArgs e )
    {
      OnClick( e );
    }

    private void Selection_DoubleClick( object sender, MouseButtonEventArgs e )
    {
      OnDoubleClick( e );
    }


    protected virtual void OnClick( MouseButtonEventArgs e )
    {
      Click?.Invoke( this, e );
    }

    protected virtual void OnDoubleClick( MouseButtonEventArgs e )
    {
      DoubleClick?.Invoke( this, e );
    }

    protected virtual void OnCheckBoxClick( EventArgs e )
    {
      CheckBoxClick?.Invoke( this, e );
    }

    internal void OnExpandedChanged()
    {
      ExpandedChanged?.Invoke( this, EventArgs.Empty );

      var isExpanded = IsExpanded;
      var canCollapse = CanCollapse;

      foreach ( var tvitem in childrenArea ) {
        tvitem.gameObject.SetActive( isExpanded );

        if ( ( !isExpanded || !canCollapse ) && tvitem.IsExpanded ) {
          tvitem.IsExpanded = false;
        }
      }
    }

    internal void OnAdded( TreeView tv )
    {
      SetIndent( _currentIndent );

      tv.RegisterEvents( this );

      foreach ( var tvitem in childrenArea ) {
        tvitem.OnAdded( tv );
      }
    }

    internal void OnRemoving( TreeView tv )
    {
      if ( null == tv ) return;

      foreach ( var tvitem in childrenArea ) {
        tvitem.OnRemoving( tv );
      }

      tv.UnregisterEvents( this );
    }

    internal void SetIndent( int indent )
    {
      if ( _currentIndent == indent ) return;

      _currentIndent = indent;

      var hlg = this.childrenArea.GetComponent<HorizontalLayoutGroup>();
      hlg.padding.left = indent;

      foreach ( var tvitem in childrenArea ) {
        tvitem.SetIndent( indent );
      }
    }

    public void UpdateCanExpand( bool forceCanExpand )
    {
      expander.ExpandVisible = forceCanExpand;
    }

    public void UpdateCanExpand()
    {
      UpdateCanExpand( 0 != childrenArea.Count );
    }

    public void SetSelected( bool selected )
    {
      if ( selected ) {
        ShowIntoView();
      }

      if ( selected != selection.IsOn ) {
        selection.IsOn = selected;
      }
    }

    private void ShowIntoView()
    {
      for ( var tvitem = this.Parent ; null != tvitem ; tvitem = tvitem.Parent ) {
        if ( !tvitem.IsExpanded ) {
          tvitem.IsExpanded = true;
        }
      }

      Canvas.ForceUpdateCanvases();

      var scrollRect = TreeView.GetComponent<ScrollRect>();
      var scrollArea = scrollRect.GetComponent<RectTransform>().rect;
      var contentPanel = scrollRect.content;

      var scrollAreaYMin = scrollArea.yMin;
      var scrollAreaYMax = scrollArea.yMax;
      if ( scrollRect.horizontal ) {
        var scrollHeight = scrollRect.horizontalScrollbar.GetComponent<RectTransform>().rect.height + scrollRect.horizontalScrollbarSpacing;
        scrollAreaYMin += scrollHeight;
      }

      var myArea = selection.GetComponent<RectTransform>().rect;
      var currentYMin = scrollRect.transform.InverseTransformPoint( selection.transform.position ).y + myArea.yMin;
      var currentYMax = currentYMin + myArea.height;
      if ( currentYMin < scrollAreaYMin ) {
        var pos = contentPanel.anchoredPosition;
        pos.y += scrollAreaYMin - currentYMin;
        contentPanel.anchoredPosition = pos;
      }
      else if ( currentYMax > scrollAreaYMax ) {
        var pos = contentPanel.anchoredPosition;
        pos.y += scrollAreaYMax - currentYMax;
        contentPanel.anchoredPosition = pos;
      }
    }
  }
}
