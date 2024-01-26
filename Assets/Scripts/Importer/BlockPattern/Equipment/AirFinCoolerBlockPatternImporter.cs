using System ;
using System.Collections.Generic;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.AirFinCooler;
using Importer.Equipment ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment
{
  public class AirFinCoolerBlockPatternImporter
  {
    public static void Import(Action<Edge> onFinish)
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      new AirFinCoolerBase(curDoc, "91-HFF66130").Create(onFinish);
      Debug.Log("91-HFF66130");
    }

    public static Chiyoda.CAD.Model.Equipment AirFinCoolerImport(string id, Chiyoda.CAD.Topology.BlockPattern bp, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet();
      var equipmentTable = PipingPieceTableFactory.Create(bp.Type, dataSet);
      var (equipment, origin, rot) = equipmentTable.Generate(bp.Document, id, createNozzle: true);
      BlockPatternFactory.CreateInstrumentEdgeVertex(bp, equipment as Chiyoda.CAD.Model.Equipment, origin, rot);

      bp.LocalCod = new LocalCodSys3d(Vector3d.zero, bp.LocalCod);

      foreach (var leafEdge in bp.GetAllLeafEdges())
      {
        AlignAllLeafEdges(bp, leafEdge);
      }

      bp.RuleList.BindChangeEvents(true);
      onFinish?.Invoke(bp);
      return equipment as Chiyoda.CAD.Model.Equipment;
    }

    public static void AlignAllLeafEdges(Chiyoda.CAD.Topology.BlockPattern baseBp, LeafEdge basePump)
    {
      var offset = -(Vector3) basePump.LocalCod.Origin ;
      baseBp.GetAllLeafEdges().ForEach( l => l.MoveLocalPos( offset ) ) ;
    }
  }
}
