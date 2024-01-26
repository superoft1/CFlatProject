using UnityEngine ;
using UnityEngine.UI ;

namespace Chiyoda.UI.Dialog
{
  public class CodInputModalDialog : ModalDialog
  {
    [SerializeField]
    GameObject X;

    [SerializeField]
    GameObject Y;

    [SerializeField]
    GameObject Z;

    void Start()
    {
      var xField = X.GetComponent<InputField>();
      xField.ActivateInputField();
    }

    void Update()
    {
      if ( this.gameObject.activeSelf && ( Input.GetKeyDown( KeyCode.Tab ) ) ) {
        // フォーカスを移動
        var xField = X.GetComponent<InputField>();
        var yField = Y.GetComponent<InputField>();
        var zField = Z.GetComponent<InputField>();
        if ( xField.isFocused ) {
          yField.ActivateInputField();
        } 
        else if (yField.isFocused ) {
          zField.ActivateInputField();
        }
        else {
          xField.ActivateInputField();
        }
      }
    }

    // return true if success
    public bool GetCod(out Vector3 vector)
    {
      var xStr = X.GetComponent<InputField>()?.text;
      var yStr = Y.GetComponent<InputField>()?.text;
      var zStr = Z.GetComponent<InputField>()?.text;
      if (float.TryParse( xStr, out float x ) &&
          float.TryParse( yStr, out float y ) &&
          float.TryParse( zStr, out float z ) ) {
        vector = new Vector3(x, y, z);
        return true ;
      }
      vector = new Vector3();
      return false ;
    }
  }
}