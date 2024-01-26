using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Chiyoda.UI
{
  /// <summary>
  /// フォーカスをタブ移動出来るようにするクラス
  /// </summary>
  public class FocusController : MonoBehaviour
  {
    [SerializeField] private ScrollRect _scrollRect;
    private List<Selectable> _focusList = new List<Selectable>();

    /// <summary>
    /// フォーカスリストの更新
    /// </summary>
    public void UpdateFocusList()
    {
      _focusList.Clear();

      foreach (var field in GetComponentsInChildren<Selectable>())
      {
        if (!(field is Scrollbar))
        {
          _focusList.Add(field);
        }
      }
    }

    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.Tab))
      {
        ChangeFocus(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
      }
    }
    
    /// <summary>
    /// フォーカス移動
    /// </summary>
    /// <param name="back">後ろに戻るか</param>
    private void ChangeFocus(bool back)
    {
      GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
      if (currentSelected == null) return;

      var activeFocusList = _focusList.FindAll(f => f != null && f.interactable);
      int currentIndex = activeFocusList.FindIndex(f => f.gameObject.Equals(currentSelected));
      if (currentIndex < 0) return;

      int nextIndex;
      if (back)
      {
        if (currentIndex == 0) nextIndex = activeFocusList.Count - 1;
        else nextIndex = currentIndex - 1;
      }
      else
      {
        if (currentIndex == activeFocusList.Count - 1) nextIndex = 0;
        else nextIndex = currentIndex + 1;
      }
      
      activeFocusList[nextIndex].Select();
      ShowIntoView(activeFocusList[nextIndex]);
    }

    // TreeViewItemのShowIntoViewを参考に作成
    private void ShowIntoView(Selectable selection)
    {
      var scrollArea = _scrollRect.GetComponent<RectTransform>().rect;
      var contentPanel = _scrollRect.content;

      var scrollAreaYMin = scrollArea.yMin;
      var scrollAreaYMax = scrollArea.yMax;
      if ( _scrollRect.horizontal ) {
        var scrollHeight = _scrollRect.horizontalScrollbar.GetComponent<RectTransform>().rect.height + _scrollRect.horizontalScrollbarSpacing;
        scrollAreaYMin += scrollHeight;
      }

      var myArea = selection.GetComponent<RectTransform>().rect;
      var currentYMin = _scrollRect.transform.InverseTransformPoint( selection.transform.position ).y + myArea.yMin;
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