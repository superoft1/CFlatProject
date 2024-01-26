using System.IO ;
using UnityEngine;
using VtpAutoRouting ;

namespace Chiyoda.Test.Routing

{
  public class RoutingTestRunner
  {
    public static string TestConfigDirectory
    {
      get { return Path.Combine(Application.streamingAssetsPath, "VTP", "Test", "Routing"); }
    }

    public void RunAll()
    {
      foreach (var testConfigFilePath in System.IO.Directory.EnumerateFiles( TestConfigDirectory ))
      {
        //既存の要素を削除
        AutoRoutingMgr.Instance().DeleteAll();
        var testCase = new RoutingTestCase(testConfigFilePath);
        var success = testCase.Run();
        
        // 失敗したら抜ける
        if (!success) break;
      }
    }

    public void Setup( string testConfigFilePath )
    {
      //既存の要素を削除
      AutoRoutingMgr.Instance().DeleteAll();
      
      var testCase = new RoutingTestCase(testConfigFilePath);
      testCase.Setup();
    }

    public void Run( string testConfigFilePath )
    {
      //既存の要素を削除
      AutoRoutingMgr.Instance().DeleteAll();
      
      var testCase = new RoutingTestCase(testConfigFilePath);
      testCase.Run();
    }
  }
}