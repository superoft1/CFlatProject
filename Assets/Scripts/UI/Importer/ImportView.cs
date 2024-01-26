using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportView : MonoBehaviour {

  bool isOnDown = false;

  private void LateUpdate()
  {
    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
    {
      isOnDown = true;
    }
    else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
    {
      if (isOnDown)
      {
        Finish();
      }
      isOnDown = false;
    }
  }

  void Finish()
  {
    isOnDown = false;
    Hide();
  }

  public void IDFImport()
  {
    ImportManager.Instance().IDFImportButtonClicked(() => {
      Finish();
    });
  }

  public void IDFImportDirectory()
  {
    ImportManager.Instance().IDFDirectoryImportButtonClicked(() => {
      Finish();
    });
  }

  public void XMLImport()
  {
    ImportManager.Instance().XMLImportButtonClicked(() => {
      Finish();
    });
  }

  public void XMLImportDirectory()
  {
    ImportManager.Instance().XMLDirectoryImportButtonClicked(() => {
      Finish();
    });
  }

  public void CAESAR2Import()
  {

  }

  public void CAESAR2ImportDirectory()
  {

  }

  public void LineListCSVImport()
  {
    ImportManager.Instance().LineListCSVImportButtonClicked(() => {
      Finish();
    });
  }

  public void Cancel()
  {
    Hide();
  }

  void Hide()
  {
    this.gameObject.SetActive(false);
  }

}
