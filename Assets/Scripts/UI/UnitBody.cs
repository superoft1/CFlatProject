using Chiyoda.CAD.Plotplan;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chiyoda.UI
{
  public class UnitBody : MonoBehaviour
  {
    [SerializeField] private GameObject plane;

    private LeafUnit _leafUnit;
    
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    /// <summary>
    /// LeafUnitを元に初期化
    /// </summary>
    /// <param name="leafUnit"></param>
    public void Init(LeafUnit leafUnit)
    {
      _leafUnit = leafUnit;
      
      var model = UnitContentManager.GetModel(_leafUnit.UnitType);
      if (model)
      {
        // モデルが設定してあったら、こちらを優先して使用する
        plane.SetActive(false);
        
        Instantiate(model, Vector3.zero, Quaternion.identity, gameObject.transform);
      }
      else
      {
        // モデルが無かったら、テクスチャをPlaneに貼るだけ
        plane.SetActive(true);

        var texRenderer = plane.GetComponent<Renderer>();
        if (texRenderer == null) return;

        var tex = UnitContentManager.GetTexture(_leafUnit.UnitType);
        if (tex == null) return;

        texRenderer.material.SetTexture(MainTex, tex);

        transform.localScale = UnitContentManager.GetSize(_leafUnit.UnitType);
      }
    }

    public void OnPointClick(BaseEventData data)
    {
      _leafUnit.Document.SelectElement(_leafUnit);

      var item = UnitListBox.Instance.ItemMap[_leafUnit];
      if (item == null) return;
      
      item.IsOn = true;
      UnitListBox.Instance.ScrollItem(item);
    }
  }
}