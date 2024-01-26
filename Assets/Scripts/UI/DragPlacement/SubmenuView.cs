using UnityEngine ;

namespace Chiyoda.UI
{
  public class SubmenuView : MonoBehaviour
  {
    private bool _isDown;
    private RectTransform _rectTransform;

    public void Show()
    {
      gameObject.SetActive(true);
    }


    public void Hide()
    {
      gameObject.SetActive(false);
    }

    private void Start()
    {
      _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
      _isDown = false;
    }
    
    private void LateUpdate()
    {
      if (!_isDown)
      {
        // PipeRackViewがEnableの時にボタンを押し始めたかどうかフラグで持つ (これをしないとPipeRackViewを開いたときのクリックにも反応してしまう)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
          _isDown = true;
        }
      }
      else if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
      {
        // PipeRackアイコンをドラッグ中に右クリックした時などにPipeRackViewが閉じるとドラッグ処理も途中終了してしまうので、すべてのボタンが離されたらHideする
        Hide();
        _isDown = false;
      }
    }
  }
}