using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Plotplan;
using UnityEngine;

namespace Chiyoda.UI
{
  /// <summary>
  /// Unit用 リストボックスに項目を追加するダイアログ
  /// </summary>
  public class UnitAddDialog : ModalDialog
  {
    [SerializeField] private GameObject unitAddDialogItemPrefab;
    [SerializeField] private GameObject content;

    private void Start()
    {
      foreach (Transform child in content.transform)
      {
        Destroy(child.gameObject);
      }

      // ダイアログ内の項目を追加
      foreach (UnitType.Type type in Enum.GetValues(typeof(UnitType.Type)))
      {
        var obj = Instantiate(unitAddDialogItemPrefab, content.transform);
        var item = obj.GetComponent<UnitAddDialogItem>();

        item.title.text = type.ToString();

        item.button.onClick.AddListener(() =>
        {
          var doc = DocumentCollection.Instance.Current;
          var lu = doc.CreateEntity<LeafUnit>();
          lu.UnitType = type;
          lu.AreaSize = UnitContentManager.GetSize(type);
          doc.Units.Add(lu);

          UnitListBox.Instance.CreateItem(lu, autoScroll: true);
        });

        var tex = UnitContentManager.GetTexture(type);
        if (tex == null) continue;

        item.image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
      }
    }
  }
}