using System ;
using System.IO;
using System.Linq ;

namespace VtpAutoRouting
{
  public class DirectoryUtility
  {
    public string AssetPath  { get; private set; }

    public string StreamingAssetPath  { get; private set; }

    public DirectoryUtility(string assetPath, string streamingAssetPath)
    {
      this.AssetPath = assetPath;
      this.StreamingAssetPath = streamingAssetPath;
    }

    public string RootPath   { get { return AssetPath.Substring(0, AssetPath.LastIndexOf('/') + 1); } }

    public string OutputPath { get { return Path.Combine(RootPath, "Outputs/Routing"); } }

    public string OutputPathTmp { get { return Path.Combine(OutputPath, "Tmp"); } }

    public string OutputPathResult { get { return Path.Combine(OutputPath, "Result"); } }


    public string XmlTempOutputDir(string RouteXmlFileName)
    {
      var routeXmlOutputPath = Path.Combine(OutputPathTmp, Path.GetFileNameWithoutExtension(RouteXmlFileName));
      if (!Directory.Exists(routeXmlOutputPath))
      {
        Directory.CreateDirectory(routeXmlOutputPath);
      }
      return routeXmlOutputPath;
    }

    internal string PathToRoutingResult(string fileName)
    {
      if (!Directory.Exists( OutputPathResult ))
      {
        Directory.CreateDirectory( OutputPathResult );
      }

      var invalidChars = Path.GetInvalidFileNameChars() ;
      var replaced = new string( fileName.Select( c => invalidChars.Contains( c ) ? '_' : c ).ToArray() ) ;
      
      //var replaced = fileName.Replace( "/", "_" ).Replace( " ", "_" ) ;
      return Path.Combine( OutputPathResult, replaced ) ;
    }

    public string InputPath { get { return Path.Combine(StreamingAssetPath, "VTP/Routing"); } }

    public string PathToChiyodalib{ get { return Path.Combine(RootPath, "ChiyodaLib/netcoreapp2.0/ChiyodaLib.dll"); } }

    public string TemplateFilePath { get { return Path.Combine(InputPath, "Templates/template.xml"); } }

    public string PointTemplateFilePath { get { return Path.Combine(InputPath, "Templates/cflat_dPoints.xml"); } }
  }
}
