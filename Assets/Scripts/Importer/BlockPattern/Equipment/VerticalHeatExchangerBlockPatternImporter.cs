using System ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Presenter ;
using Chiyoda.CAD.Topology ;
using Importer.Equipment ;

namespace Importer.BlockPattern.Equipment
{
  public class VerticalHeatExchangerBlockPatternImporter
  {
    public static void Import(Action<Edge> onFinish)
    {
      VerticalHeatExchangerImport("24-E9301", onFinish);
    }

    static void VerticalHeatExchangerImport(string id, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.VerticalHeatExchanger );
      var instrumentTable = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
      var (instrument, origin, rot) = instrumentTable.Generate( bp.Document, id, createNozzle: true ) ;
      BlockPatternFactory.CreateInstrumentEdgeVertex( bp, instrument as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
      onFinish?.Invoke(bp);
    }
  }
}
