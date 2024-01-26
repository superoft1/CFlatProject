using UnityEngine;

namespace Chiyoda.UI
{
  /// <summary>
  /// uGUIのImageオブジェクトにこのスクリプトを付けるとマウスカーソルの位置に表示される
  /// </summary>
  public class MouseCursor : MonoBehaviour
  {
    private Transform _self;

    private void Start()
    {
      _self = transform;
    }

    private void LateUpdate()
    {
      _self.position = Input.mousePosition;
    }
  }
}
