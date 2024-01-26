using System.IO ;
using Chiyoda.Routing ;
using Routing ;

namespace VtpAutoRouting.BrdigeEntities
{
  internal class DebugPath : IDebugFilePath
  {
    private DirectoryUtility _util ;

    public DebugPath( DirectoryUtility util )
    {
      _util = util ;
    }

    public string PathToRoutingResult( string filename ) => _util.PathToRoutingResult( filename ) ;
    
    public void ClearResultPath()
    {
      var outputPathResult = _util.OutputPathResult ;
      if ( Directory.Exists( outputPathResult ) ) {
        Directory.Delete( outputPathResult, true ) ;
      }
      
      var outputPathTmp = _util.OutputPathTmp ;
      if ( Directory.Exists( outputPathTmp ) ) {
        Directory.Delete( outputPathTmp, true ) ;
      }
    }
  }
}