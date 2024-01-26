using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  /// <summary>
  /// UIのサイズをドラッグで調整する
  /// </summary>
  public class SizeAdjuster : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
    IPointerExitHandler
  {
    public bool IsMouseOver => _isMouseOver;

    private static bool _isDragging;

    [SerializeField] private RectTransform targetRect;
    [SerializeField] private RectTransform temporaryRect;
    [SerializeField] private bool dragRightToSizeUp;
    [SerializeField] private GameObject sizeAdjusterCursor;
    private float _minWidth;
    private Vector2 _dragBeginPos;
    private Vector3 _selfBeginPos;
    private Vector2 _targetBeginSize;
    private bool _isMouseOver;

    private void Start()
    {
      // このサイズより小さくしない
      _minWidth = targetRect.sizeDelta.x;
    }

    public void OnBeginDrag(PointerEventData data)
    {
      // ドラッグ開始時の値を変数に保存しておく
      _dragBeginPos = data.position;
      _selfBeginPos = transform.localPosition;
      _targetBeginSize = targetRect.sizeDelta;

      // 処理負荷軽減のためドラッグ中はtargetRectのサイズを直接変更するのではなく別に半透明板を表示して動かす
      temporaryRect.gameObject.SetActive(true);

      // ドラッグ中はカーソルの見た目を変更する
      _isDragging = true;
      ChangeCursor();
    }

    public void OnDrag(PointerEventData data)
    {
      // ドラッグで移動した量
      var dragDiff = dragRightToSizeUp ? data.position.x - _dragBeginPos.x : _dragBeginPos.x - data.position.x;

      // _minWidth〜画面半分の範囲内におさめる
      var width = Mathf.Clamp(_targetBeginSize.x + dragDiff, _minWidth, Screen.width / 2f);

      // 実際に移動した量
      var moveDiff = dragRightToSizeUp ? width - _targetBeginSize.x : _targetBeginSize.x - width;

      // 半透明板のサイズ変更
      temporaryRect.sizeDelta = new Vector2(width, _targetBeginSize.y);

      // 自身の場所も更新
      transform.localPosition = new Vector3(_selfBeginPos.x + moveDiff, _selfBeginPos.y, _selfBeginPos.z);
    }

    public void OnEndDrag(PointerEventData data)
    {
      // 半透明板のサイズをtargetRectに反映して半透明板は非表示に戻す
      targetRect.sizeDelta = temporaryRect.sizeDelta;
      temporaryRect.gameObject.SetActive(false);

      // ドラッグが終了した場合、マウスが上になければカーソルの見た目を元に戻す
      _isDragging = false;

      if (!_isMouseOver)
      {
        ResetCursor();
      }
    }

    public void OnPointerEnter(PointerEventData data)
    {
      // マウスが上に乗ったらカーソルの見た目を変更する
      _isMouseOver = true;
      ChangeCursor();
    }

    public void OnPointerExit(PointerEventData data)
    {
      // マウスが外れた場合、ドラッグ中でなければカーソルの見た目を元に戻す
      _isMouseOver = false;

      if (!_isDragging)
      {
        ResetCursor();
      }
    }

    private void ChangeCursor()
    {
      // NOTE: Cursor.SetCursor()で見た目を変えた場合マウスカーソルの表示位置が微妙で、調整方法もわからなかったのでuGUIで表示する
      // OSのカーソルを非表示にしてsizeAdjusterCursorを表示
      Cursor.visible = false;
      sizeAdjusterCursor.SetActive(true);
    }

    private void ResetCursor()
    {
      // sizeAdjusterCursorを非表示にしてOSのカーソルを表示
      sizeAdjusterCursor.SetActive(false);
      Cursor.visible = true;
    }
  }
}
