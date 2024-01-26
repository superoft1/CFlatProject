using System ;
using UnityEngine;
using UnityEngine.Serialization ;
using UnityEngine.UI;

public class ModelessDialog : MonoBehaviour
{
    public EventHandler OKClickedHandler ;
    public EventHandler CancelClickedHandler ;
    
    private static int enabledCount;
    public static bool IsOpened => enabledCount > 0;
    
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
    
    void Hide()
    { 
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void OKButton_Clicked()
    {
        OKClickedHandler?.Invoke(this,EventArgs.Empty) ;
        Hide();
    }

    public void CancelButton_Clicked()
    {
        CancelClickedHandler?.Invoke(this,EventArgs.Empty) ;
        Hide();
    }
}
