using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;

namespace Chiyoda.UI
{
  public class ColumnBlockPatternIcon : BlockPatternIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      ColumnBlockPatternImporter.Import( onFinish ) ;
    }
  }
}