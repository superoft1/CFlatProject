using System.Collections.Generic;
using System.IO;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

public abstract class StructureImporter
{
  public abstract void ImportData(string path, Document doc);

  public static void Import(string path, StructureType.Type type, Document doc)
  {
    var importer = StructureImporterFactory.CreateImporter(type);
    importer.ImportData(path, doc);
  }

  public static void ImportStructureList(string csvPath, System.Action onFinish)
  {
    var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

    foreach (var parsed in ImportUtil.ParseCsv( csvPath ) )
    {
      StructureType.Type sType = StructureType.Parse( parsed.type );
      if ( sType != StructureType.Type.unknown ) {
        Import( Path.Combine( ImportManager.StructuresPath(), parsed.values[0] ), sType, curDoc );
      }
    }
    onFinish?.Invoke();
  }


  protected virtual Vector3d Origin(string[] cells)
  {
    return new Vector3d(-double.Parse(cells[3]), double.Parse(cells[4]), double.Parse(cells[5])) / 1000d;
  }
}
