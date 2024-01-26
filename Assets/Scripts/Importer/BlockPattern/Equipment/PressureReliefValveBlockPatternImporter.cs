using System;
using System.Linq;
using Chiyoda;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Presenter;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment.PressureReliefValve;
using Importer.Equipment;
using UnityEngine;

namespace Importer.BlockPattern.Equipment
{
  public class PressureReliefValveBlockPatternImporter
  {
    [Flags]
    public enum PressureReliefValveType
    {
      None = 0,
      PRV_C1_V = 1 << 1,
      PRV_C1_H = 1 << 2,
      PRV_C1_U = 1 << 3,
    }

    /// <summary>
    /// </summary>
    /// <param name="onFinish"></param>
    public static void Import(Action<Edge> onFinish)
    {
      Debug.Log("Import");
      var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.PressureReliefValve);
      PressureReliefValveImport("PRV001", bp, onFinish);
    }

    /// <summary>
    /// BlockPatternタブからの import 呼び出し
    /// </summary>
    /// <param name="type"></param>
    /// <param name="onFinish"></param>
    public static void Import(PressureReliefValveType type, Action<Edge> onFinish)
    {
      Debug.Log("Import BlockPattern");
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      if (type.HasFlag(PressureReliefValveType.PRV_C1_V))
      {
        // new PressureReliefValvePRV_C1_V(curDoc).Create(onFinish);
        Debug.Log("PRV-C1-V");
      }

      if (type.HasFlag(PressureReliefValveType.PRV_C1_H))
      {
        new PressureReliefValvePRV_C1_H(curDoc).Create(onFinish);
        Debug.Log("PRV-C1-H");
      }

      if (type.HasFlag(PressureReliefValveType.PRV_C1_U))
      {
        // new PressureReliefValvePRV_C1_U(curDoc).Create(onFinish);
        Debug.Log("PRV-C1-U");
      }
    }

    /// <summary>
    /// csvファイルより Instruments をインポートする
    /// </summary>
    /// <param name="id"></param>
    /// <param name="bp"></param>
    /// <param name="onFinish"></param>
    /// <returns></returns>
    public static void PressureReliefValveImport(string id, Chiyoda.CAD.Topology.BlockPattern bp, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet();
      var instrumentTable = PipingPieceTableFactory.Create(bp.Type, dataSet);
      var(instrument, origin, rot) = instrumentTable.Generate(bp.Document, id, createNozzle : false);

      bp.LocalCod = new LocalCodSys3d(origin, rot, false);
      var leafEdge = bp.Document.CreateEntity<LeafEdge>();
      leafEdge.PipingPiece = instrument as PipingPiece;
      LeafEdgeCodSysUtils.LocalizePRVComponent(leafEdge, Vector3d.zero, Vector3d.down, Vector3d.left);
      bp.AddEdge(leafEdge);
      bp.Document.CreateHalfVerticesAndMakePairs(leafEdge);


      //暫定的に設定
      var vertices = leafEdge.Vertices.ToList();
      vertices[0].Flow = HalfVertex.FlowType.FromAnotherToThis;
      vertices[1].Flow = HalfVertex.FlowType.FromThisToAnother;

      onFinish?.Invoke(bp);
    }
  }
}