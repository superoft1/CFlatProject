using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Chiyoda;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Chiyoda.Importer;
using IDF;
using UnityEngine;
using Group = Chiyoda.CAD.Topology.Group;

namespace Importer.BlockPattern.Equipment.PressureReliefValve
{
  public class PressureReliefValvePRV_C1_H : PressureReliefValveBase<BlockPatternArray>
  {
    public PressureReliefValvePRV_C1_H(Document doc) : base(doc, "PRV-C1-H")
    {
      /* fixformat ignore:start */
      Info = new SingleBlockPatternIndexInfo
      {
        InletIndex = 0,
        OutletIndex = 1,
        BasePumpIndex = 0,
        InletIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.InletIndexType, int>
        {
           { SingleBlockPatternIndexInfo.InletIndexType.Inlet01Pipe, 1 },
        },
        OutletIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.OutletIndexType, int>
        {
           { SingleBlockPatternIndexInfo.OutletIndexType.Outlet03Pipe, 3 },
        }
      };
      /* fixformat ignore:end */
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="onFinish"></param>
    /// <returns></returns>
    public Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish)
    {
      ImportIdfAndEquipment();

      foreach (var edge in BaseBp.EdgeList.Where( e => !(e is LeafEdge)))
      {
        edge.LocalCod = LocalCodSys3d.Identity;
      }

      PostProcess();

      onFinish?.Invoke((BlockEdge)BpOwner ?? BaseBp);

      return BaseBp;
    }

    /// <summary>
    /// RemoveExtraEdges
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    protected override void RemoveExtraEdges(Group group, string file) { }

    /// <summary>
    /// SetPropertyAndRule
    /// </summary>
    /// <param name="info"></param>
    protected override void SetPropertyAndRule(SingleBlockPatternIndexInfo info) { }
  }
}