using System.Collections;
using UnityEngine;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Plotplan;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  /// <summary>
  /// Unit用のリストボックス 項目
  /// </summary>
  public class UnitListBoxItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
    ISubmitHandler, ISelectHandler
  {
    [SerializeField] private Toggle toggleButton;
    [SerializeField] private Text label;
    [SerializeField] private Image image;

    private Camera _mainCamera;
    private CameraOperator _cameraOperator;
    private RectTransform _canvasRectTransform;

    private LeafUnit _draggingLeafUnit;
    private GameObject _draggingItemObject;
    private RectTransform _draggingItemObjectRectTransform;
    private Body _body;
    private int _clickCount;

    public RectTransform RectTransform { get; private set; }

    public object Tag { get; set; }

    public bool IsOn
    {
      get => toggleButton.isOn;
      set => toggleButton.isOn = value;
    }

    public bool IsVisible
    {
      get => true;
      set { }
    }

    public string Text
    {
      get => label.text;
      set => label.text = value;
    }

    public Color TextColor
    {
      get => label.color;
      set => label.color = value;
    }

    public Texture2D Image
    {
      set
      {
        if (value == null) return;
        image.sprite = Sprite.Create(value, new Rect(0.0f, 0.0f, value.width, value.height), Vector2.zero);
      }
    }

    private void Start()
    {
      var mainCamera = GameObject.Find("MainCamera");
      if (mainCamera)
      {
        _mainCamera = mainCamera.GetComponent<Camera>();
        _cameraOperator = mainCamera.GetComponent<CameraOperator>();
      }

      _canvasRectTransform = GameObject.Find("Canvas")?.GetComponent<RectTransform>();

      RectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// クリック時の処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
      switch (eventData.clickCount)
      {
        case 1:
          _clickCount = 1;
          StartCoroutine(ClickEvent());
          break;

        case 2:
          _clickCount = 2;
          StartCoroutine(ClickEvent());
          break;
      }
    }

    /// <summary>
    /// Enterが押された時
    /// </summary>
    /// <param name="eventData"></param>
    public void OnSubmit(BaseEventData eventData)
    {
      _clickCount++;
      StartCoroutine(ClickEvent());
    }

    /// <summary>
    /// クリックイベント
    /// </summary>
    /// <returns></returns>
    private IEnumerator ClickEvent()
    {
      // 少し待ってから、シングルクリックかダブルクリックかを判定する
      yield return new WaitForSeconds(0.2f);

      var doc = DocumentCollection.Instance.Current;
      if (doc == null) yield break;

      switch (_clickCount)
      {
        // シングルクリック
        case 1:

          if (toggleButton.isOn)
          {
            Select(false);
          }
          else
          {
            doc.DeselectElement(doc.LastSelection);
          }

          break;

        // ダブルクリック
        case 2:
          if (_body == null) yield break;

          // bodyが非表示の時は拡大しない
          if (_body.gameObject.activeSelf == false) yield break;

          Select();

          // 選択した項目のbodyを拡大表示
          _cameraOperator.SetBoundary(CAD.Boundary.GetBounds(_body));
          break;
      }

      _clickCount = 0;
    }

    /// <summary>
    /// 項目を選択状態にする
    /// </summary>
    /// <param name="isToggleOn"></param>
    public void Select(bool isToggleOn = true)
    {
      var doc = DocumentCollection.Instance.Current;
      if (doc == null) return;

      doc.SelectElement((IElement) Tag);
      UnitListBox.Instance.CurrentItem = this;
      if (isToggleOn) toggleButton.isOn = true;
    }

    /// <summary>
    /// ドラッグ開始
    /// </summary>
    /// <param name="pointerEventData"></param>
    public void OnBeginDrag(PointerEventData pointerEventData)
    {
      IsOn = true;

      // メインビューに配置するbodyオブジェクトを作成する
      var listBoxItem = gameObject.GetComponent<UnitListBoxItem>();
      _draggingLeafUnit = (LeafUnit) listBoxItem.Tag;
      if (_draggingLeafUnit == null) return;

      _draggingLeafUnit.Document.SelectElement(_draggingLeafUnit);

      BodyMap.Instance.TryGetBody(_draggingLeafUnit, out var body);
      _body = (Body) body;
      _body.gameObject.SetActive(true);

      // ドラッグ中の一時オブジェクトを作成する
      _draggingItemObject = Instantiate(gameObject, transform.root);
      _draggingItemObject.name = "Dragging Object";
      _draggingItemObjectRectTransform = _draggingItemObject.GetComponent<RectTransform>();
      _draggingItemObjectRectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;

      UnitSceneManager.SetSceneStatus(UnitSceneManager.SceneStatusType.Dragging);
    }

    /// <summary>
    /// ドラッグ中 オブジェクトの位置を更新する
    /// </summary>
    /// <param name="pointerEventData"></param>
    public void OnDrag(PointerEventData pointerEventData)
    {
      if (MouseUtil.IsMouseOnMainView())
      {
        // メインビュー内だったら、bodyの位置を更新
        _draggingItemObject.gameObject.SetActive(false);

        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRectTransform, pointerEventData.position,
          _mainCamera, out var newPos);

        _draggingLeafUnit.LocalCod = new LocalCodSys3d(newPos, _draggingLeafUnit.LocalCod);
      }
      else
      {
        // メインビュー外だったら、一時オブジェクトの位置を更新
        _draggingItemObject.gameObject.SetActive(true);

        _draggingItemObjectRectTransform.position = pointerEventData.position;
      }
    }

    /// <summary>
    /// ドラッグ終了
    /// </summary>
    /// <param name="pointerEventData"></param>
    public void OnEndDrag(PointerEventData pointerEventData)
    {
      // 一時オブジェクトを削除
      Destroy(_draggingItemObject);

      if (MouseUtil.IsMouseOnMainView())
      {
        SetActive(false);
      }

      UnitSceneManager.SetSceneStatus(UnitSceneManager.SceneStatusType.Idle);
    }

    /// <summary>
    /// アクティブ設定
    /// </summary>
    /// <param name="value"></param>
    public void SetActive(bool value)
    {
      var color = image.color;
      image.color = new Color(color.r, color.g, color.b, value ? 1.0f : 0.3f);
    }

    /// <summary>
    /// 選択された時
    /// </summary>
    /// <param name="eventData"></param>
    public void OnSelect(BaseEventData eventData)
    {
      if (eventData.GetType() != typeof(AxisEventData)) return;
      
      UnitListBox.Instance.ScrollItem(this, false, ((AxisEventData) eventData).moveVector == Vector2.down);
    }
  }
}