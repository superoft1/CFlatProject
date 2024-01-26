using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;

namespace Chiyoda.UI
{
  public class KettleTypeHeatExchangerBlockPatternIcon : BlockPatternIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      KettleTypeHeatExchangerBlockPatternImporter.Import( onFinish ) ;
    }
  }
}