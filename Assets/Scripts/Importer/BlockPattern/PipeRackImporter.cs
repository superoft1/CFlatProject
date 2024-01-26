using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeRackImporter : MonoBehaviour {

  public void PipeRackImportButtonClicked()
  {
    PipeRackImport("020-P-405A&B");
  }

  void PipeRackImport(string id)
  {
    //var folderPath = Path.Combine(XMLDirectoryPath(), "End-Top-Pump/" + id);
    //var fileList = new List<string>();
    //ImportManager.GetFiles(folderPath, ".xml", fileList);
  }

}
