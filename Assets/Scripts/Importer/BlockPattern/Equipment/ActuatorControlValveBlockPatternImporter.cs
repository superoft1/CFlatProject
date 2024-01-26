using System;
using System.Linq;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Presenter;
using Chiyoda.CAD.Topology;
using Importer.Equipment;
using UnityEngine;
using Importer.BlockPattern.Equipment.ControlValve;

namespace Importer.BlockPattern.Equipment
{
    public class ActuatorControlValveBlockPatternImporter
    {
        [Flags]
        public enum ControlValveType
        {
            None = 0,
            ACV_A1_TEST = 1 << 1,
            ACV_SELECTABLE = 1 << 2,
            SV_N1 = 1 << 3,
            DV_N2 = 1 << 4,
        }
        static Chiyoda.CAD.Topology.BlockPattern _dragging;

        public static void Import(Action<Edge> onFinish)
        {
            Debug.Log("--Import finish");
            var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.ActuatorControlValve);
            _dragging = ActuatorControlValveImport("ACV001", bp, onFinish);
        }

        public static void Import(ControlValveType type, Action<Edge> onFinish)
        {
            var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

            if (type.HasFlag(ControlValveType.ACV_A1_TEST))
            {
                Debug.Log("--ACV_A1_TEST");
                var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.ActuatorControlValve);
                _dragging = ActuatorControlValveImport("ACV001", bp, onFinish);
            }

            if (type.HasFlag(ControlValveType.ACV_SELECTABLE))
            {
                Debug.Log("--CV_SELECTABLE");
                new ControlValveSelectable(curDoc).Create(onFinish);
            }

            if (type.HasFlag(ControlValveType.SV_N1))
            {
                Debug.Log("--SV_N1_TEST");
                new ControlValveSV_N1(curDoc).Create(onFinish);
            }

            if (type.HasFlag(ControlValveType.DV_N2))
            {
                Debug.Log("--DV_N2_TEST");
                new ControlValveDV_N2(curDoc).Create(onFinish);
            }

            if (_dragging != null)
            {
                curDoc.RemoveEdge(_dragging);
                _dragging = null;
            }
        }


        public static Chiyoda.CAD.Topology.BlockPattern ActuatorControlValveImport(string id, Chiyoda.CAD.Topology.BlockPattern bp, Action<Edge> onFinish = null)
        {
            Debug.Log("--step 1");
            var dataSet = PipingPieceDataSet.GetPipingPieceDataSet();
            var instrumentTable = PipingPieceTableFactory.Create(bp.Type, dataSet);
            var (instrument, origin, rot) = instrumentTable.Generate(bp.Document, id, createNozzle: false);
            Debug.Log("--step 2");
            var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

            bp.LocalCod = new LocalCodSys3d(origin, rot, false);
            var leafEdge = bp.Document.CreateEntity<LeafEdge>();
            leafEdge.PipingPiece = instrument as PipingPiece;
            LeafEdgeCodSysUtils.LocalizePRVComponent(leafEdge, Vector3d.zero, Vector3d.down, Vector3d.left);
            bp.AddEdge(leafEdge);
            curDoc.CreateHalfVerticesAndMakePairs(leafEdge);
            Debug.Log("--step 3");

            var vertices = leafEdge.Vertices.ToList();
            vertices[0].Flow = HalfVertex.FlowType.FromAnotherToThis;
            vertices[1].Flow = HalfVertex.FlowType.FromThisToAnother;
            Debug.Log("--step 4");
            onFinish?.Invoke(bp);
            return bp;
        }
    }
}