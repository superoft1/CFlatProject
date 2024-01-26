using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chiyoda.UI
{
  public static class MouseUtil
  {
    /// <summary>
    /// マウスポインタがスクロールビューの上にあるか返す
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseOnScrollView()
    {
      var pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
      var result = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointer, result);
      return result.Any(r => r.gameObject.CompareTag("ScrollView"));
    }

    /// <summary>
    /// マウスポインタがImageの上にあるか返す
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseOnImage()
    {
      var pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
      var result = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointer, result);
      return result.Any(r => r.gameObject.GetComponent<UnityEngine.UI.Image>() != null);
    }

    /// <summary>
    /// マウスポインタが画面内にあるか返す
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseInScreen()
    {
      return 0 <= Input.mousePosition.x && Input.mousePosition.x <= Screen.width &&
             0 <= Input.mousePosition.y && Input.mousePosition.y <= Screen.height;
    }

    /// <summary>
    /// マウスがコンテキストメニューをポップアップする範囲にあるか返す
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseInContextMenuPopupRect()
    {
      var pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
      var result = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointer, result);

      foreach (var r in result)
      {
        // HeaderPanelの上はダメ
        if (r.gameObject.name == "HeaderPanel") return false;

        // TreeViewの上ならOK
        if (r.gameObject.GetComponent<TreeView>() != null) return true;
        
        // TreeView以外のScrollViewの上はダメ
        if (r.gameObject.CompareTag("ScrollView")) return false;
      }

      // どれにも該当しなければOK
      return true;
    }

    /// <summary>
    /// マウスがコンテキストメニューの中にあるか返す
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseOnContextMenu()
    {
      var pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
      var result = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointer, result);
      return result.Any(r => r.gameObject.GetComponent<ContextMenu>() != null);
    }

    /// <summary>
    /// マウスがメインビューの上にあるか返す
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseOnMainView()
    {
      var pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
      var result = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointer, result);

      foreach (var r in result)
      {
        // HeaderPanelの上はダメ
        if (r.gameObject.name == "HeaderPanel") return false;

        // ScrollViewの上はダメ
        if (r.gameObject.CompareTag("ScrollView")) return false;
      }

      // どれにも該当しなければOK
      return true;
    }
  }
}
