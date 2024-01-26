using System ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Presenter ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.HorizontalHeatExchanger;
using Importer.Equipment ;

namespace Importer.BlockPattern.Equipment
{
  public class KettleTypeHeatExchangerBlockPatternImporter
  {
    public static void Import(Action<Edge> onFinish)
    {
#if true      
      //KettleTypeHeatExchangerImport("95-HBC63122", onFinish);
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      new KettleTypeHeatExchangerBase(curDoc, "HE-C2-1-G").Create(onFinish);  // TODO EquipMentShapeName
#else
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      
      // TODO BlockPattern No.3 のパイプ配置確認のため、暫定的に Hor.HE の実装を利用して Kettle を表示
      {
        new HorizontalHeatExchangerBase(curDoc, "HE-C2-1-G").Create(onFinish);
      }
#endif
    }

    static void KettleTypeHeatExchangerImport(string id, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.KettleTypeHeatExchanger );
      var instrumentTable = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
      var (instrument, origin, rot) = instrumentTable.Generate( bp.Document, id, createNozzle: true ) ;
      BlockPatternFactory.CreateInstrumentEdgeVertex( bp, instrument as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
      onFinish?.Invoke(bp);
    }
  }
}
