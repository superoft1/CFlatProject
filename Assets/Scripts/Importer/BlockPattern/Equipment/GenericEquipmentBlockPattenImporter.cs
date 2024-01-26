using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Presenter ;
using Chiyoda.CAD.Topology ;
using Importer.Equipment ;

namespace Importer.BlockPattern.Equipment
{
  public class GenericEquipmentBlockPattenImporter
  {
    public static void Import(Action<Edge> onFinish)
    {
      GenericEquipmentImport("XXXXXX", onFinish); // TODO: 暫定実装でCSVがないためidは適当
    }

    static void GenericEquipmentImport(string id, Action<Edge> onFinish)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      foreach ( var key in new List<string> {"001", "002", "003", "004"} ) {
        var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.GenericEquipment );
        var instrumentTable = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
        var (instrument, origin, rot) = instrumentTable.Generate( bp.Document, key, createNozzle: true ) ;
        BlockPatternFactory.CreateInstrumentEdgeVertex( bp, instrument as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
      }

      onFinish?.Invoke(null);
    }
  }
}
