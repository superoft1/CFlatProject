using System;
using System.Linq;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Plotplan;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  public class UnitSceneManager : MonoBehaviour
  {
    [SerializeField] private CameraOperator cameraOperator;
    [SerializeField] private UnitListBox unitListBox;
    [SerializeField] private Vector2 areaSize = new Vector2(5000, 5000);
    [SerializeField] private double gridInterval = 100;
    [SerializeField] private float cameraDistance = 5000;
    [SerializeField] private float cameraMaxLog = 300;

    private static UnitSceneManager Instance { get; set; }

    public enum SceneStatusType
    {
      Idle,
      Dragging
    }

    private SceneStatusType SceneStatus { get; set; }

    private void Awake()
    {
      Instance = this;
    }

    private void Start()
    {
      // エリアサイズの調整
      var doc = DocumentCollection.Instance.Current;
      doc.Area.EWWidth = areaSize.x;
      doc.Area.NSWidth = areaSize.y;
      doc.Area.GridInterval = gridInterval;

      // カメラ周りの設定
      cameraOperator.CameraDistance = cameraDistance;
      cameraOperator.MaxLog = cameraMaxLog;
      cameraOperator.SetBoundary(DocumentCollection.Instance.Current.Region.GetGlobalBounds());

      SceneStatus = SceneStatusType.Idle;

      // リストボックスにデフォルトでいくつか用意
      var cu = doc.CreateEntity<CompositeUnit>();
//    doc.Units.Add(cu);
      for (var i = 0; i < 4; i++)
      {
        var train = doc.CreateEntity<LeafUnit>();
        train.UnitType = UnitType.Type.LNGTrain;
        train.AreaSize = UnitContentManager.GetSize(UnitType.Type.LNGTrain);
        cu.AddUnit(train);
        doc.Units.Add(train);
      }

      foreach (UnitType.Type type in Enum.GetValues(typeof(UnitType.Type)))
      {
        var lu = doc.CreateEntity<LeafUnit>();
        lu.UnitType = type;
        lu.AreaSize = UnitContentManager.GetSize(type);
        doc.Units.Add(lu);
      }

      unitListBox.Create(doc.Units);
    }

    private void Update()
    {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
        UnsetCursor(false);
      }
      else if (Input.GetMouseButtonDown(0))
      {
        UnsetCursor();
      }
      else if (Input.GetKeyDown(KeyCode.Backspace))
      {
        HideCurrentBody();
      }
    }

    /// <summary>
    /// カーソルを外す
    /// </summary>
    private void UnsetCursor(bool mainViewCheck = true)
    {
      if (SceneStatus != SceneStatusType.Idle) return;
      if (ElementDragManager.Instance.IsDragging) return;
      if (mainViewCheck && !MouseUtil.IsMouseOnMainView()) return;

      var doc = DocumentCollection.Instance.Current;
      if (doc?.LastSelection == null) return;

      if (UnitListBox.Instance.ItemMap.ContainsKey(doc.LastSelection))
      {
        var item = UnitListBox.Instance.ItemMap[doc.LastSelection];
        item.IsOn = false;
      }

      doc.DeselectElement(doc.LastSelection);
    }

    /// <summary>
    /// 現在選択されているbodyを非表示
    /// </summary>
    private static void HideCurrentBody()
    {
      // InputFieldで入力中だったら削除を無効化
      if (EventSystem.current.currentSelectedGameObject != null)
        if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
          return;

      // 現在選択されているUnitを取得
      var doc = DocumentCollection.Instance.Current;
      if (!(doc.SelectedElements.FirstOrDefault() is LeafUnit leafUnit)) return;

      // 現在選択されているUnitをドキュメントから削除（これを行わなかったらPickableが表示されなくなる）
      doc.DeselectElement(leafUnit);

      // BodyMapで紐付いているbodyを非表示
      BodyMap.Instance.TryGetBody(leafUnit, out var body);
      ((Body) body)?.gameObject.SetActive(false);

      // リストボックスの項目を有効な状態にする
      var item = UnitListBox.Instance.ItemMap[leafUnit];
      if (item == null) return;
      
      item.SetActive(true);
    }

    /// <summary>
    /// 現在のシーンステータスを設定する
    /// </summary>
    /// <param name="type"></param>
    public static void SetSceneStatus(SceneStatusType type)
    {
      if (Instance == null) return;

      Instance.SceneStatus = type;
    }
  }
}