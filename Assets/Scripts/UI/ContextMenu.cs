using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  /// <summary>
  /// TreeViewまたはMainView上で右クリックした時に表示されるコンテキストメニュー
  /// </summary>
  public class ContextMenu : MonoBehaviour
  {
    [SerializeField] private Button selectionVisibleButton;
    [SerializeField] private Text selectionVisibleText;
    [SerializeField] private Button nonSelectionVisibleButton;
    [SerializeField] private Text nonSelectionVisibleText;

    // コンテキストメニュー表示開始
    private void OnEnable()
    {
      var selectedItems = DocumentTreeView.Instance().GetSelectedAllDescendants();
      if ( !selectedItems.Any() ) {
        gameObject.SetActive( false ); // 未選択
        return;
      }

      // マウスの位置にポップアップ表示（はみ出ないよう補正）
      var halfSize = GetComponent<RectTransform>().sizeDelta / 2f;
      var x = Mathf.Clamp(Input.mousePosition.x, halfSize.x, Screen.width - halfSize.x);
      var y = Mathf.Clamp(Input.mousePosition.y, halfSize.y, Screen.height - halfSize.y);
      transform.position = new Vector3(x, y, 0f);

      // メニューの初期化
      if (selectedItems.All(item => item.CheckState == CheckState.Unchecked))
      {
        selectionVisibleText.text = "Show Selected";
        selectionVisibleButton.onClick = new Button.ButtonClickedEvent();
        selectionVisibleButton.onClick.AddListener(ShowSelection);
      }
      else
      {
        selectionVisibleText.text = "Hide Selected";
        selectionVisibleButton.onClick = new Button.ButtonClickedEvent();
        selectionVisibleButton.onClick.AddListener(HideSelection);
      }

      nonSelectionVisibleText.text = "Show Only";
      nonSelectionVisibleButton.onClick = new Button.ButtonClickedEvent();
      nonSelectionVisibleButton.onClick.AddListener( ShowSelectedOnly );
    }
    
    // 選択しているアイテムをすべて表示
    private void ShowSelection()
    {
      var treeView = DocumentTreeView.Instance();

      var selectedItems = treeView.GetSelectedAllDescendants();
      foreach ( var item in selectedItems ) {
        treeView.ChangeItemVisibility( item, true );
      }

      gameObject.SetActive( false );
    }

    // 選択しているアイテムをすべて非表示
    private void HideSelection()
    {
      var treeView = DocumentTreeView.Instance();

      var selectedItems = treeView.GetSelectedAllDescendants();
      foreach ( var item in selectedItems ) {
        treeView.ChangeItemVisibility( item, false );
      }

      gameObject.SetActive( false );
    }

    // 選択しているアイテムのみを表示
    private void ShowSelectedOnly()
    {
      var treeView = DocumentTreeView.Instance();

      var rootItem = treeView.TreeView.Items.Single( item => null == item.Parent );
      treeView.ChangeItemVisibility( rootItem, false );

      ShowSelection();

      gameObject.SetActive( false );
    }
  }
}
