using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Presenter;
using Chiyoda.CAD.Topology;
using IDF;
using Importer.CSV.AutoRouting ;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ImportManager : MonoBehaviour
{
  [SerializeField]
  XMLDeserializer xmlDeserializer = null;

  [SerializeField]
  XMLFileAnalysis xMLFileAnalysis = null;

  [SerializeField]
  IDFFileAnalysis iDFFileAnalysis = null;

  [SerializeField]
  WrlDeserealizer wrlDeserealizer = null;
  
  public List<System.Action> onImportActionList = new List<System.Action>();

  public static ImportManager Instance()
  {
    return manager;
  }

  static ImportManager manager = null;

  void Awake()
  {
    if ( manager == null ) {
      manager = this ;
      if ( xmlDeserializer == null ) {// Unit Testから呼び出す対応
        xmlDeserializer = gameObject.AddComponent<XMLDeserializer>() ;
      }
    }
  }
  
  private void OnDestroy()
  {
    if( manager == this ) {
      manager = null ;
    }
  }

  public static string InstrumentsPath()
  {
    return Path.Combine(DataPath(), "Instruments");
  }

  public static string StructuresPath()
  {
    return Path.Combine(DataPath(), "Structures");
  }

  public static string XMLDirectoryPath()
  {
    return Path.Combine(DataPath(), "XML");
  }
  
  public static string IDFBlockPatternDirectoryPath()
  {
    return Path.Combine(DataPath(), "IDF/BlockPattern");
  }

  public static string RootPath()
  {
    var assetsPath = Application.dataPath;
    return assetsPath.Substring(0, assetsPath.LastIndexOf('/') + 1);
  }

  public static string DataPath()
  {
    return Path.Combine(Application.streamingAssetsPath, "VTP");
  }

  public void XMLImportButtonClicked(System.Action onFinish = null)
  {
    FileBrowser.OpenMultipleFilesPanel("Open multiple files Title", System.Environment.GetFolderPath(System.Environment.SpecialFolder.History), new string[] { "xml", "XML" }, "Open", (bool canceled, string[] filePathArray) =>
    {
      if (canceled)
      {
        return;
      }
      var doc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      XMLImport(doc, filePathArray);
      doc.HistoryCommit();

      if (onFinish != null)
      {
        onFinish();
      }
    });
  }

  public void IDFImportButtonClicked(System.Action onFinish = null)
  {
    FileBrowser.OpenMultipleFilesPanel("Open multiple files Title", "", new string[] { "idf", "IDF", "id0", "id1", "id2", "id3", "id4" }, "Open", (bool canceled, string[] filePathArray) =>
    {
      if (canceled)
      {
        return;
      }
      var idfDeserializer = new IDFDeserializer();
      var doc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      IDFImport(doc, filePathArray, idfDeserializer );
      doc.HistoryCommit();

      onFinish?.Invoke();
    } );
  }

  public void IDFDirectoryImportButtonClicked(System.Action onFinish = null)
  {
    FileBrowser.OpenFolderPanel("Open folder Title", System.Environment.GetFolderPath(System.Environment.SpecialFolder.History), null, (bool canceled, string folderPath) =>
    {
      if (canceled)
      {
        return;
      }
      var idfDeserializer = new IDFDeserializer();
      var fileList = new List<string>();
      GetFiles(folderPath, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" }, fileList);
      var doc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      IDFImport(doc, fileList.ToArray(), idfDeserializer );
      doc.HistoryCommit();

      onFinish?.Invoke();
    } );
  }

  public void VRMLImportButtonClicked()
  {
    FileBrowser.OpenMultipleFilesPanel("Open multiple files Title", System.Environment.GetFolderPath(System.Environment.SpecialFolder.History), new string[] { "wrl" }, "Open", (bool canceled, string[] filePathArray) =>
    {
      if (canceled)
      {
        return;
      }
      var curDoc = DocumentCollection.Instance.Current;
      if (null != curDoc)
      {
        DocumentCollection.Instance.Close(curDoc);
      }
      Document doc = DocumentCollection.Instance.CreateNew();
      foreach (var path in filePathArray)
      {
        wrlDeserealizer.ImportData(doc, path);
      }
      doc.HistoryCommit();
    });
  }

  public void XMLDirectoryImportButtonClicked(System.Action onFinish = null)
  {
    FileBrowser.OpenFolderPanel("Open folder Title", System.Environment.GetFolderPath(System.Environment.SpecialFolder.History), null, (bool canceled, string folderPath) =>
    {
      if (canceled)
      {
        return;
      }
      var fileList = new List<string>();
      GetFiles(folderPath, ".xml", fileList);
      var doc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      XMLImport(doc, fileList.ToArray());
      doc.HistoryCommit();

      if (onFinish != null)
      {
        onFinish();
      }
    });
  }

  public void XMLDirectoryAnalysisButtonClicked()
  {
    FileBrowser.OpenFolderPanel("Open folder Title", System.Environment.GetFolderPath(System.Environment.SpecialFolder.History), null, (bool canceled, string folderPath) =>
    {
      if (canceled)
      {
        return;
      }
      var fileList = new List<string>();
      GetFiles(folderPath, ".xml", fileList);

      foreach (var file in fileList)
      {
        XMLAnalysis(file);
      }
      xMLFileAnalysis.Output();
    });
  }
  public void IDFDirectoryAnalysisButtonClicked()
  {
    FileBrowser.OpenFolderPanel("Open folder Title", "", null, (bool canceled, string folderPath) =>
   {
     if (canceled)
     {
       return;
     }
     var fileList = new List<string>();
     GetFiles(folderPath, ".idf", fileList);

     iDFFileAnalysis = new IDFFileAnalysis();
     List<string> xmlFiles = new List<string>();
     System.IO.Directory.GetFiles(@"D:\work\CFlat\XML Data Sample", "*.xml", System.IO.SearchOption.AllDirectories)
       .ToList()
       .ForEach(s => xmlFiles.Add(Path.GetFileNameWithoutExtension(s)));
     xmlFiles.Sort();
     var processedPath = Application.dataPath + "/Outputs/IDFFileAnalyze_processed.csv";
     foreach (var file in fileList)
     {
       using (var writer = new StreamWriter((processedPath), true, new UTF8Encoding(true)))
       {
         writer.WriteLine(file);
       }
       int index = xmlFiles.BinarySearch(Path.GetFileNameWithoutExtension(file));
       iDFFileAnalysis.ImportData(folderPath, file, index >= 0);
     }
     iDFFileAnalysis.Output();
   });
  }
 
  public void IDFDirectoryComponentTablesButtonClicked()
  {
    FileBrowser.OpenFolderPanel("Open folder Title", "", null, (bool canceled, string folderPath) =>
   {
     if (canceled)
     {
       return;
     }
     var builder = new StringBuilder();
     var idfDeserializer = new IDFDeserializer();
     var fileList = new List<string>();
     GetIDFFiles(folderPath, fileList);
     var folderMap = new Dictionary<string, List<string>>();
     foreach (var file in fileList)
     {
       var folder = file.Split(Path.DirectorySeparatorChar).FirstOrDefault(s => s.Contains("検証")) ?? file.Split(Path.DirectorySeparatorChar)[0];
       if (folderMap.ContainsKey(folder))
       {
         folderMap[folder].Add(file);
       }
       else
       {
         folderMap.Add(folder, new List<string> { file });
       }
     }
     var curDoc = DocumentCollection.Instance.Current;
     DocumentCollection.Instance.Close(curDoc);
     Document doc = DocumentCollection.Instance.CreateNew();
     var processedPath = Application.dataPath + "/Outputs/IDFComponentTables_processed.csv";
     foreach (var files in folderMap)
     {
       builder.Append(IDFComponentTable.Header() + "\n");
       foreach (var file in files.Value)
       {
         using (var writer = new StreamWriter((processedPath), true, new UTF8Encoding(true)))
         {
           writer.WriteLine(file);
         }
         var grpInfo = new Chiyoda.Importer.GroupInfo(curDoc, doc, file, appendDirectlyToGroup:false) ;
         idfDeserializer.ImportData(grpInfo, file);
       }

       var path = Application.dataPath + "/Outputs/IDFComponentTables_" + files.Key + ".csv";
       if (System.IO.File.Exists(path))
       {
         System.IO.File.Delete(path);
       }
       using (var writer = new StreamWriter((path), true, new UTF8Encoding(true)))
       {
         writer.Write(builder.ToString());
       }

       builder.Length = 0;
     }
   });
  }

  public void LineListCSVImportButtonClicked(System.Action onFinish = null)
  {
    FileBrowser.OpenFilePanel("Open multiple files Title", 
                              "", 
                              new string[] { "csv", "CSV" }, "Open", 
                              (bool canceled, string filePath) => 
    {
      if (canceled)
      {
        return;
      }

      var doc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      LineListCSVImport(doc, filePath);
      doc.HistoryCommit();

      if (onFinish != null)
      {
        onFinish();
      }
    });
  }

  public static void GetIDFFiles(string rootPath, List<string> fileList)
  {
    GetFiles(rootPath, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" }, fileList);
  }

  public static void GetFiles(string path, List<string> extensions, List<string> fileList)
  {
    foreach (var ext in extensions)
    {
      GetFiles(path, ext, fileList);
    }
  }

  public static void GetFiles(string path, string extension, List<string> fileList)
  {
    if (System.IO.File.Exists(path))
    {
      if (System.IO.Path.GetExtension(path) == extension)
      {
        fileList.Add(path);
      }
    }
    else if (System.IO.Directory.Exists(path))
    {
      var dirs = System.IO.Directory.GetDirectories(path);
      foreach (var dir in dirs)
      {
        GetFiles(dir, extension, fileList);
      }
      var files = System.IO.Directory.GetFiles(path);
      foreach (var file in files)
      {
        GetFiles(file, extension, fileList);
      }
    }
  }

  public void BlockPattermXMLImport(BlockPattern bp, Chiyoda.CAD.Core.Document doc, string[] path)
  {
    xmlDeserializer.ImportData(bp, doc, path);
  }

  public void XMLImport(Chiyoda.CAD.Core.Document doc, string[] path)
  {
    foreach (var p in path)
    {
      xmlDeserializer.ImportData(doc, p);
    }
  }

  public void IDFImport(Chiyoda.CAD.Core.Document doc, string[] path, IDFDeserializer idfDeserializer, bool shouldCreateVertex = true )
  {
    //var sw = Stopwatch.StartNew();
    foreach (var p in path)
    {
      var grpInfo = new Chiyoda.Importer.GroupInfo(doc, doc, p, appendDirectlyToGroup:false) ;
      idfDeserializer.ImportData( grpInfo, p, shouldCreateVertex ) ;
    }
    //sw.Stop();
    //Debug.Log( $"IDF Imported　{sw.Elapsed.Hours}時間 {sw.Elapsed.Minutes}分 {sw.Elapsed.Seconds}秒 {sw.Elapsed.Milliseconds}ミリ秒" );
  }

  public void XMLImport(Chiyoda.CAD.Core.Document doc, string path)
  {
    xmlDeserializer.ImportData(doc, path);
  }

  void XMLAnalysis(string path)
  {
    xMLFileAnalysis.ImportData(path);
  }

  public void LineListCSVImport(Chiyoda.CAD.Core.Document doc, string path)
  {
    throw new NotImplementedException();
  }
}
