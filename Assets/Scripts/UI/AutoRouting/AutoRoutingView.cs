using Chiyoda.CAD.Core ;
using Chiyoda.Test.Routing;
using VtpAutoRouting;
using UnityEngine;

public class AutoRoutingView : MonoBehaviour
{
  bool isOnDown = false;
  bool isOnCreate = false;

  private void LateUpdate()
  {
    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
    {
      isOnDown = true;
    } else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
    {
      if (isOnDown && !isOnCreate) {
        Finish();
      }
      isOnDown = false;
    }
  }

  void Finish()
  {
    isOnCreate = false;
    isOnDown = false;
    Hide();
  }

  public void Cancel()
  {
    Hide();
  }

  void Hide()
  {
    this.gameObject.SetActive(false);
  }

  public void Show()
  {
    this.gameObject.SetActive(true);
  }

  #region OnClick
  public void ImportAll()
  {
    AutoRoutingMgr.Instance().DeleteAll();
    AutoRoutingMgr.Instance().ImportAll() ;
  }

  public void Import()
  {
    FileBrowser.OpenFilePanel("Select AutoRouting Test Define JSON", "", new string[] { "json" }, "Open", (bool canceled, string filePath) =>
    {
      if (canceled) return;
      var testRunner = new RoutingTestRunner();
      testRunner.Setup(filePath);
    } );
  }
  public void ImportAndExecute()
  {
    FileBrowser.OpenFilePanel("Select AutoRouting Test Define JSON", "", new string[] { "json" }, "Open", (bool canceled, string filePath) =>
    {
      if (canceled) return;
      var testRunner = new RoutingTestRunner();
      testRunner.Run(filePath);
    } );
  }

  public void ExecuteAll()
  {
    AutoRoutingMgr.Instance().Execute();
  }
  public void SaveProFile()
  {
    AutoRoutingMgr.Instance().SaveProFile();
  }
  public void DeleteAllResult()
  {
    AutoRoutingMgr.Instance().DeleteResult();
  }

  public void UpdateEndPoints()
  {
    AutoRoutingMgr.Instance().UpdateEndPoints( DocumentCollection.Instance.Current.Routes) ;    
  }

  public void Test()
  {}

  #endregion
}
