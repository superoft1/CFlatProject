using System ;
using System.Linq ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Presenter ;
using Chiyoda.CAD.Topology ;
using Importer.Equipment ;
using UnityEngine ;
using Importer.BlockPattern.Equipment.ControlValve;

namespace Importer.BlockPattern.Equipment
{
  public class ControlValveBlockPatternImporter
  {
    [Flags]
    public enum ControlValveType
    {
      None = 0,
      CV_A1_TEST = 1 << 1,
      CV_SELECTABLE = 1 << 2,
      SV_N1 = 1 << 3,
      DV_N2 = 1 << 4,
    }
    static Chiyoda.CAD.Topology.BlockPattern _dragging;
    //Specialityタブ
    public static void Import(Action<Edge> onFinish)
    {
            _dragging = ControlValveImport("CV001", onFinish);
    }
    /// <summary>
    /// 遷移の初期化
    /// </summary>
    public static void Prepare(){
      //_dragging = null;
    }

    //BPタブ
    public static void Import(ControlValveType type, Action<Edge> onFinish)
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      if (type.HasFlag(ControlValveType.CV_A1_TEST)) {
        Debug.Log("--CV_A1_TEST");
        ControlValveImport("CV001", onFinish);
      }

      if (type.HasFlag(ControlValveType.CV_SELECTABLE)) {
        Debug.Log("--CV_SELECTABLE");
        new ControlValveSelectable(curDoc).Create(onFinish);
      }

      if (type.HasFlag(ControlValveType.SV_N1)) {
        Debug.Log("--SV_N1_TEST");
        new ControlValveSV_N1(curDoc).Create(onFinish);
      }

      if (type.HasFlag(ControlValveType.DV_N2)) {
        Debug.Log("--DV_N2_TEST");
        new ControlValveDV_N2(curDoc).Create(onFinish);
      }

      if (_dragging != null){
        curDoc.RemoveEdge(_dragging);
        _dragging = null;
      }
    }

    static Chiyoda.CAD.Topology.BlockPattern ControlValveImport(string id, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet();
      var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.ControlValve);
      var instrumentTable = PipingPieceTableFactory.Create(bp.Type, dataSet);
      var (instrument, origin, rot) = instrumentTable.Generate(bp.Document, id, createNozzle: true);

      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      bp.LocalCod = new LocalCodSys3d(origin, rot, false);
      var leafEdge = curDoc.CreateEntity<LeafEdge>();
      leafEdge.PipingPiece = instrument as PipingPiece;
      LeafEdgeCodSysUtils.LocalizeCVComponent(leafEdge, Vector3d.zero, Vector3d.right, Vector3d.forward);
      bp.AddEdge(leafEdge);
      curDoc.CreateHalfVerticesAndMakePairs(leafEdge);

      //暫定的に設定
      var vertices = leafEdge.Vertices.ToList();
      vertices[0].Flow = HalfVertex.FlowType.FromAnotherToThis;
      vertices[1].Flow = HalfVertex.FlowType.FromThisToAnother;

      onFinish?.Invoke(bp);
      return bp;
    }
  }
}