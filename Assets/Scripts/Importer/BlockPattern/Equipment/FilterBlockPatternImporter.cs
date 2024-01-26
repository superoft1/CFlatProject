//#define FS_A1_1_S_FULL_FLEX

using System;
using System.Collections.Generic ;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Presenter;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment.Filter ;
using Importer.Equipment;

using UnityEngine;

namespace Importer.BlockPattern.Equipment
{
  public class FilterBlockPatternImporter
  {
    [Flags]
    public enum FilterType
    {
      None = 0,
      FS_A1_1_S = 1 << 1,
      FS_A1_1_G = 1 << 2,
      FS_A2_2_S = 1 << 3,
      FS_A2_2_G = 1 << 4,
      FD_B1_1_S = 1 << 5,
      FD_B1_1_G = 1 << 6,
      FD_B2_2_S = 1 << 7,
      FD_B2_2_G = 1 << 8,
    }

    public static void Import(FilterType type, Action<Edge> onFinish)
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      #if FS_A1_1_S_FULL_FLEX
      if (type.HasFlag(FilterType.FS_A1_1_S))
      {
        new FilterFS_A1_1_S_Temp(curDoc).Create(onFinish);
        Debug.Log("FS-A1-1-S-FULL-FLEX");
      }
      #else
      if (type.HasFlag(FilterType.FS_A1_1_S))
      {
        new FilterFS_A1_1_S(curDoc).Create(onFinish);
        Debug.Log("FS-A1-1-S");
      }
      #endif
      if (type.HasFlag(FilterType.FS_A1_1_G))
      {
        new FilterFS_A1_1_G(curDoc).Create(onFinish);
        Debug.Log("FS-A1-1-G");
      }
      if (type.HasFlag(FilterType.FS_A2_2_S))
      {
        new FilterFS_A2_2_S(curDoc).Create(onFinish);
        Debug.Log("FS-A2-2-S");
      }
      if (type.HasFlag(FilterType.FS_A2_2_G))
      {
        new FilterFS_A2_2_G(curDoc).Create(onFinish);
        Debug.Log("FS-A2-2-G");
      }
      if (type.HasFlag(FilterType.FD_B1_1_S))
      {
        new FilterFD_B1_1_S(curDoc).Create(onFinish);
        Debug.Log("FD-B1-1-S");
      }
      if (type.HasFlag(FilterType.FD_B1_1_G))
      {
        new FilterFD_B1_1_G(curDoc).Create(onFinish);
        Debug.Log("FD-B1-1-G");
      }
      if (type.HasFlag(FilterType.FD_B2_2_S))
      {
        new FilterFD_B2_2_S(curDoc).Create(onFinish);
        Debug.Log("FD-B2-2-S");
      }
      if (type.HasFlag(FilterType.FD_B2_2_G))
      {
        new FilterFD_B2_2_G(curDoc).Create(onFinish);
        Debug.Log("FD-B2-2-G");
      }
    }

    public static void FilterImport(string id, Chiyoda.CAD.Topology.BlockPattern bp, Action<Edge> onFinish = null)
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet();
      FilterTableImporter equipmentTable = (FilterTableImporter)PipingPieceTableFactory.Create(bp.Type, dataSet);
      var patternList = new List<string> { "", ".FIRST", ".SECOND" };

      foreach (var pattern in patternList)
      {
        if (equipmentTable.FindPattern(id + pattern) != null)
        {
          var (equip, origin, rot) = equipmentTable.Generate(bp.Document, id + pattern, createNozzle: true);
          BlockPatternFactory.CreateInstrumentEdgeVertex(bp, equip as Chiyoda.CAD.Model.Equipment, origin, rot);
        }
      }

      onFinish?.Invoke(bp);
    }
  }
}
