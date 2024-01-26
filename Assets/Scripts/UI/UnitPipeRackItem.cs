using UnityEngine;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Model.Structure.Entities;
using UnityEngine.EventSystems;

namespace Chiyoda.UI
{
  /// <summary>
  /// Unitのパイプラック用の項目
  /// </summary>
  public class UnitPipeRackItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
  {
    [SerializeField] private double pipeRackRotation;
    [SerializeField] private int pipeRackFloorNumber = 2;
    [SerializeField] private int pipeRackIntervalNumber = 10;
    
    private Camera _mainCamera;
    private RectTransform _canvasRectTransform;
    private UnitElementList _elementList;
    private Document _document;

    private GameObject _draggingItemObject;
    private RectTransform _draggingItemObjectRectTransform;

    private PipeRackSingle _pipeRackSingle;

    private void Start()
    {
      _mainCamera = GameObject.Find("MainCamera")?.GetComponent<Camera>();
      _canvasRectTransform = GameObject.Find("Canvas")?.GetComponent<RectTransform>();
      _elementList = GameObject.Find("UnitElementList")?.GetComponent<UnitElementList>();
      _document = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
    }

    /// <summary>
    /// ドラッグ開始
    /// </summary>
    /// <param name="pointerEventData"></param>
    public void OnBeginDrag(PointerEventData pointerEventData)
    {
      var doc = DocumentCollection.Instance.Current;

      // パイプラックを作成
      _pipeRackSingle = (PipeRackSingle) StructureFactory.CreatePipeRack(doc, PipeRackFrameType.Single);
      _pipeRackSingle.FloorCount = pipeRackFloorNumber;
      _pipeRackSingle.IntervalCount = pipeRackIntervalNumber;
      _pipeRackSingle.Rotation = pipeRackRotation;
      _pipeRackSingle.Document.SelectElement(_pipeRackSingle);

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

        _pipeRackSingle.LocalCod = new LocalCodSys3d(newPos, _pipeRackSingle.LocalCod);
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
      
      _elementList.SetTarget(_document, _document.Structures);
      
      UnitSceneManager.SetSceneStatus(UnitSceneManager.SceneStatusType.Idle);
    }
  }
}