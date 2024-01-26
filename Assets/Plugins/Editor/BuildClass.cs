using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildClass
{
  public static void Build()
  {
    // ビルド対象シーンリスト
    string[] sceneList = {
      "./Assets/Scenes/UI.unity"
    };

    var assetPath = Application.dataPath;
    var projectPath = assetPath.Substring(0, assetPath.LastIndexOf('/') + 1);
    var buildPath = System.IO.Path.Combine(projectPath, "bin","VTP.exe");
    var err = BuildPipeline.BuildPlayer(
      sceneList, 
      buildPath, 
      BuildTarget.StandaloneWindows64,
      BuildOptions.None);
  }
}