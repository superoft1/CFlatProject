using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;

public class ExportView : MonoBehaviour
{
  private bool isOnDown = false;

  private void LateUpdate()
  {
    if ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) || Input.GetMouseButtonDown( 2 ) ) {
      isOnDown = true;
    }
    else if ( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButtonUp( 1 ) || Input.GetMouseButtonUp( 2 ) ) {
      if ( isOnDown ) {
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

  public void MTOExport()
  {
    FileBrowser.SaveFilePanel("Save file Title",
                              "",
                              "",
                              "MTOReport",
                              new string[] { "csv", "CSV" },
                              "Save",
                              (bool canceled, string filePath) =>
    {
      if ( canceled ) {
        return;
      }

      var doc = DocumentCollection.Instance.Current;
      var serializer = new MTO.Serializer( doc );
      serializer.ExportData( filePath );

      Finish();
    } );
  }

  public void Cancel()
  {
    Hide();
  }

  void Hide()
  {
    this.gameObject.SetActive( false );
  }
}
