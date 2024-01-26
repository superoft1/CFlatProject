using System ;
using UnityEngine;
using UnityEngine.Serialization ;
using UnityEngine.UI;

public class ModalDialog : MonoBehaviour
{
  [SerializeField]
  GameObject Title;

  [SerializeField]
  GameObject Messages;

  [SerializeField]
  GameObject ButtonText;

  private static int enabledCount;

  public EventHandler OKClickedHandler ;

  /// <summary>
  /// モーダルダイアログが開いているか
  /// </summary>
  public static bool IsOpened => enabledCount > 0;

  void Start()
  {
  }

  void OnEnable()
  {
    enabledCount++;
  }

  void OnDisable()
  {
    enabledCount--;
  }

  void Update()
  {        
    if(this.gameObject.activeSelf && 
      (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) )
    {
      Hide();
    }
  }

  void LateUpdate()
  {
  }

  void Hide()
  {
    OKClickedHandler?.Invoke(this,EventArgs.Empty) ; this.gameObject.SetActive(false);
  }

  public void Show(string title, string message, string buttonText = null)
  {
    Title.GetComponent<Text>().text = title;
    Messages.GetComponent<Text>().text = message;
    if(buttonText != null) ButtonText.GetComponent<Text>().text = buttonText;
    this.gameObject.SetActive(true);
  }

  public void Show()
  {
    this.gameObject.SetActive(true);
  }

  public void OKButton_Clicked()
  {
    Hide();
  }
}
