using System.IO;
using Routing ;
using UnityEngine;
using VtpAutoRouting.BridgeEntities ;

namespace Chiyoda.Test.Routing
{
  public class RoutingTestCase
  {
    private FieldDefinition FieldDefinition { get; set; }

    private string TestConfigFilePath { get; }
    
    public RoutingTestCase(string testConfigFilePath)
    {
      TestConfigFilePath = testConfigFilePath ;
    }
    
    public void Setup()
    {
      FieldDefinition = FieldDefinition.Load(TestConfigFilePath);
      Debug.Assert(FieldDefinition != null);
      FieldDefinition.Setup();
    }

    public bool Run()
    {
      Setup() ;
      
      StaticParameters.SetDiameterProvider( new PipePropertyProvider() );
      var fileNameWithoutExt = Path.GetFileNameWithoutExtension( TestConfigFilePath ) ;

      // 自動ルーティングを実行
      var direUtil = new VtpAutoRouting.DirectoryUtility( Application.dataPath, Application.streamingAssetsPath ) ;
      
      var (success, failed, calcTime) = VtpAutoRouting.API.CreateRoute( 
        VtpAutoRouting.AutoRoutingModelFactory.GetCurrentAllRacks(), FieldDefinition.GetRoutes(),
        VtpAutoRouting.ModelConverter.ToDebugFilePath( direUtil ) ) ; 
      var msg = $"success:{success}, failed:{failed}, {( calcTime.Milliseconds / 1000.0 ):F1} sec." ;
      if ( failed == 0 ) {
        Debug.Log( $"[TEST:AutoRouting] {fileNameWithoutExt} is finished without failed." + msg ) ;
        return true ;
      }

      Debug.LogError( $"[TEST:AutoRouting] {fileNameWithoutExt} is failed." + msg ) ;
      return false ;
    }
  }
}