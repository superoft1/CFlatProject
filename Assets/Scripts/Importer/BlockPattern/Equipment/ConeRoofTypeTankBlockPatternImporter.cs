using System ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Presenter ;
using Chiyoda.CAD.Topology ;
using Importer.Equipment ;

namespace Importer.BlockPattern.Equipment
{
  public class ConeRoofTypeTankBlockPatternImporter
  {
    public static void Import(Action<Edge> onFinish)
    {
      ConeRoofTypeTankImport("95-MBJ69820", onFinish);
    }

    static void ConeRoofTypeTankImport(string id, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.ConeRoofTypeTank );
      var instrumentTable = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
      var (instrument, origin, rot) = instrumentTable.Generate( bp.Document, id, createNozzle: true ) ;
      BlockPatternFactory.CreateInstrumentEdgeVertex( bp, instrument as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
      onFinish?.Invoke(bp);
    }
  }
}
