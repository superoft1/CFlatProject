using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Plotplan;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  /// <summary>
  /// Unit用のリストボックス
  /// </summary>
  public class UnitListBox : MonoBehaviour
  {
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject unitListBoxItemPrefab;
    [SerializeField] private GameObject unitCompositeGroupPrefab;
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private Scrollbar scrollbar;

    private RectTransform _parentRectTransform;
    
    public static UnitListBox Instance { get; private set; }

    private readonly Dictionary<CompositeUnit.Type, UnitCompositeGroup> _unitCompositeGroup =
      new Dictionary<CompositeUnit.Type, UnitCompositeGroup>();

    public Dictionary<IElement, UnitListBoxItem> ItemMap { get; } = new Dictionary<IElement, UnitListBoxItem>();

    public UnitListBoxItem CurrentItem { get; set; }

    private void Awake()
    {
      Instance = this;
      
      _parentRectTransform = parent.GetComponent<RectTransform>();
    }

    /// <summary>
    /// リストボックスを作成する
    /// </summary>
    /// <param name="collection"></param>
    public void Create(IEnumerable<Unit> collection)
    {
      ClearItem();

      foreach (var item in collection)
      {
        CreateItem(item);
      }
    }

    /// <summary>
    /// リストボックスの項目を追加する
    /// </summary>
    /// <param name="elm"></param>
    /// <param name="index"></param>
    /// <param name="autoScroll"></param>
    /// <returns></returns>
    public void CreateItem(IElement elm, int index = -1, bool autoScroll = false)
    {
      if (elm.GetType() != typeof(LeafUnit)) return;

      var compositeUnitType = UnitContentManager.GetCompositeUnitType(((LeafUnit) elm).UnitType);

      if (_unitCompositeGroup.ContainsKey(compositeUnitType) == false)
      {
        var obj = Instantiate(unitCompositeGroupPrefab, parent.transform);
        _unitCompositeGroup[compositeUnitType] = obj.GetComponentInChildren<UnitCompositeGroup>();
        _unitCompositeGroup[compositeUnitType].title.text = compositeUnitType.ToString();
      }

      var unitComposite = _unitCompositeGroup[compositeUnitType];

      var unitListBoxItem = Instantiate(unitListBoxItemPrefab, unitComposite.content.transform);
      var toggle = unitListBoxItem.GetComponent<Toggle>();
      toggle.group = toggleGroup;

      var listBoxItem = unitListBoxItem.GetComponent<UnitListBoxItem>();
      if (listBoxItem == null) return;

      UpdateItem(elm, listBoxItem);

      ItemMap.Add(elm, listBoxItem);

      if (index > -1)
      {
        listBoxItem.transform.SetSiblingIndex(index);
      }

      if (autoScroll) StartCoroutine(ScrollWaitProc(listBoxItem));
    }

    /// <summary>
    /// リストボックスが作られるのを少し待ってから、指定した項目へスクロール
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private IEnumerator ScrollWaitProc(UnitListBoxItem item)
    {
      for (var i = 0; i < 3; i++)
      {
        yield return null;
      }

      ScrollItem(item);
    }

    /// <summary>
    /// 指定した項目へスクロール
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isSelect"></param>
    /// <param name="isMoveVectorDown"></param>
    public void ScrollItem(UnitListBoxItem item, bool isSelect = true, bool isMoveVectorDown = true)
    {
      float y;
      if (isMoveVectorDown)
      {
        y = parent.transform.position.y - (item.transform.position.y - item.RectTransform.sizeDelta.y);
      }
      else
      {
        y = parent.transform.position.y - (item.transform.position.y + item.RectTransform.sizeDelta.y);
      }

      scrollbar.value = 1 - y / _parentRectTransform.sizeDelta.y;

      if (isSelect) item.Select();
    }

    /// <summary>
    /// リストボックスの項目とエレメント（ドキュメント）を紐付ける
    /// </summary>
    /// <param name="elm"></param>
    /// <param name="item"></param>
    public void UpdateItem(IElement elm, UnitListBoxItem item = null)
    {
      if (item == null)
      {
        ItemMap.TryGetValue(elm, out var newItem);
        if (newItem == null) return;

        item = newItem;
      }

      item.IsOn = elm.Document.IsSelected(elm);
      item.IsVisible = elm.IsVisible;
      item.Text = elm.Name;
      item.TextColor = elm.HasError ? new Color(248f / 255f, 152f / 255f, 0f / 255f) : Color.white;
      item.Tag = elm;

      if (elm.GetType() == typeof(LeafUnit))
      {
        item.Image = UnitContentManager.GetTexture(((LeafUnit) elm).UnitType);
      }
    }

    /// <summary>
    /// リストボックスの項目を削除
    /// </summary>
    /// <param name="elm"></param>
    /// <returns></returns>
    public void RemoveItem(IElement elm)
    {
      if (!ItemMap.TryGetValue(elm, out var item)) return;
      item.transform.SetParent(null);
      Destroy(item.gameObject);

      ItemMap.Remove(elm);

      BodyMap.Instance.Remove((Entity) elm);
    }

    /// <summary>
    /// リストボックスの項目を全てクリア
    /// </summary>
    public void ClearItem()
    {
      _unitCompositeGroup.Clear();
      ItemMap.Clear();
    }

    /// <summary>
    /// 現在選択している項目を削除
    /// </summary>
    [UsedImplicitly]
    public void OnRemoveCurrentItem()
    {
      if (CurrentItem == null) return;

      RemoveItem((IElement) CurrentItem.Tag);
    }
  }
}