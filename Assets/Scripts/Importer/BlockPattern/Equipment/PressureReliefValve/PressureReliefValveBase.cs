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
  public abstract class PressureReliefValveBase
  {
    public Document Doc { get; }
    public Chiyoda.CAD.Topology.BlockPattern BaseBp { get; }
    protected CompositeBlockPattern BpOwner { get; }
    public SingleBlockPatternIndexInfo Info { get; internal set; }

    protected string IdfFolderPath { get; }
    protected string PatternName { get; }

    /// <summary>
    /// コンストラクター
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="patternName"></param>
    protected PressureReliefValveBase(Document doc, string patternName, CompositeBlockPattern bpOwner)
    {
      Doc = doc;
      PatternName = patternName;
      BaseBp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.PressureReliefValve, isBlockPatternArrayChild : true);
      BpOwner = bpOwner;
      IdfFolderPath = Path.Combine(ImportManager.IDFBlockPatternDirectoryPath(), "PRVs/IDF");
    }

    /// <summary>
    /// IDF関連ファイルの列挙
    /// </summary>
    /// <returns></returns>
    private IEnumerable<string> IdfFiles()
    {
      var fileList = new List<string>();
      ImportManager.GetFiles(IdfFolderPath, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" }, fileList);

      foreach (var file in fileList.Where(SelectIdf))
      {
        yield return file;
      }
    }

    /// <summary>
    /// IDFと安全弁の読み込み
    /// </summary>
    protected void ImportIdfAndEquipment()
    {
      ImportIdf();

      // フランジを付ける場合にバーテックスを考慮する関係で、ポンプよりも先にIDFを読み込む必要がある
      ImportEquipment();
    }

    /// <summary>
    /// 安全弁の読み込み
    /// </summary>
    /// <returns></returns>
    private void ImportEquipment()
    {
      PressureReliefValveBlockPatternImporter.PressureReliefValveImport("PRV001", BaseBp);
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void PostProcess()
    {
      SetBlockPatternInfo(Info);

      Doc.AddEdge((BlockEdge)BpOwner ?? BaseBp);

      // BaseBp.Document.MaintainEdgePlacement();
      var bpa = BpOwner;
      bpa.Name = bpa.GetType().Name;
    }

    /// <summary>
    /// IDFファイルのフィルター
    /// </summary>
    /// <param name="idf"></param>
    /// <returns></returns>
    protected virtual bool SelectIdf(string idf)
    {
      if (!idf.Contains(PatternName))
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// IDFファイルをインポートする
    /// </summary>
    protected virtual void ImportIdf()
    {
      foreach (var file in IdfFiles())
      {
        var grpInfo = new GroupInfo(Doc, BaseBp, file, appendDirectlyToGroup : false);
        new IDFDeserializer().ImportData(grpInfo, file, true);
        var group = grpInfo.Line2Group.Values.ElementAt(0);
        RemoveExtraEdges(group, file);
      }
    }

    /// <summary>
    /// 余分な edge を削除する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    protected virtual void RemoveExtraEdges(Group group, string file)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="group"></param>
    /// <param name="edgeIndex"></param>
    /// <param name="flexRatio"></param>
    private void SetFlexRatio(IGroup group, int edgeIndex, double flexRatio)
    {
      if (edgeIndex < 0)
      {
        return;
      }
      var edge = GetEdge(group, edgeIndex);
      var flex = edge?.PipingPiece as Pipe;
      if (flex == null)
      {
        UnityEngine.Debug.Log($"{group.Name}:{edgeIndex} is not a pipe");
      }
      else
        flex.FlexRatio = flexRatio;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <returns></returns>
    protected IGroup GetGroup(int groupIndex)
    {
      return BaseBp.NonEquipmentEdges.ElementAtOrDefault(groupIndex)as IGroup;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="group"></param>
    /// <param name="edgeIndex"></param>
    /// <returns></returns>
    protected LeafEdge GetEdge(IGroup group, int edgeIndex)
    {
      if (edgeIndex < 0)
      {
        return null;
      }
      return group?.EdgeList.ElementAtOrDefault(edgeIndex)as LeafEdge;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="objectName"></param>
    internal static void SetEdgeNameStatic(LeafEdge edge, string objectName)
    {
      var pattern = @"([^0-9]+)([0-9]?)$";
      edge.ObjectName = objectName;
      edge.PipingPiece.ObjectName = Regex.Replace(objectName, pattern, "$1Pipe$2");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="objectName"></param>
    internal void SetEdgeName(LeafEdge edge, string objectName)
    {
      PressureReliefValveBase.SetEdgeNameStatic(edge, objectName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bp"></param>
    /// <param name="instrumentIndex"></param>
    /// <returns></returns>
    internal LeafEdge GetInstrumentEdge(Chiyoda.CAD.Topology.BlockPattern bp, int instrumentIndex)
    {
      return bp.EdgeList.OfType<LeafEdge>().Where(e => e.PipingPiece is Chiyoda.CAD.Model.PressureReliefValve).ElementAtOrDefault(instrumentIndex);
    }

    /// <summary>
    /// LeafEdge にアクセスするための名前を登録する
    /// </summary>
    /// <param name="info"></param>
    internal virtual void SetEdgeNames(SingleBlockPatternIndexInfo info)
    {
      var inletGroup = GetGroup(info.InletIndex);
      var outletGroup = GetGroup(info.OutletIndex);

      foreach (SingleBlockPatternIndexInfo.OutletIndexType value in Enum.GetValues(typeof(SingleBlockPatternIndexInfo.OutletIndexType)))
      {
        if (!info.OutletIndexTypeValue.TryGetValue(value, out var index))
        {
          continue;
        }
        var edge = GetEdge(outletGroup, index);
        if (edge == null)
        {
          continue;
        }
        SetEdgeName(edge, Enum.GetName(typeof(SingleBlockPatternIndexInfo.OutletIndexType), value));
        // if (value == SingleBlockPatternIndexInfo.OutletIndexType.DischargeBOP)
        // {
        //   edge.PositionMode = PositionMode.FixedZ;
        // }
      }

      foreach (SingleBlockPatternIndexInfo.InletIndexType value in Enum.GetValues(typeof(SingleBlockPatternIndexInfo.InletIndexType)))
      {
        if (!info.InletIndexTypeValue.TryGetValue(value, out var index))
        {
          continue;
        }
        var edge = GetEdge(inletGroup, index);
        if (edge == null)
        {
          continue;
        }
        SetEdgeName(edge, Enum.GetName(typeof(SingleBlockPatternIndexInfo.InletIndexType), value));
        // if (value == SingleBlockPatternIndexInfo.InletIndexType.SuctionEnd)
        // {
        //   edge.PositionMode = PositionMode.FixedY;
        // }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    protected virtual void SetBlockPatternInfo(SingleBlockPatternIndexInfo info)
    {
      var groupList = BaseBp.NonEquipmentEdges.ToList();
      var outletGroup = groupList[info.OutletIndex] as Group;
      outletGroup.Name = "OutletPipes";
      var inletGroup = groupList[info.InletIndex] as Group;
      inletGroup.Name = "InletPipes";

      SetPropertyAndRule(info);

      if (info.OutletFlexIndexList != null)
      {
        foreach (var flex in info.OutletFlexIndexList)
        {
          SetFlexRatio(outletGroup, flex, 1);
        }
      }

      if (info.InletFlexIndexList != null)
      {
        foreach (var flex in info.InletFlexIndexList)
        {
          SetFlexRatio(inletGroup, flex, 1);
        }
      }

      var basePump = GetInstrumentEdge(BaseBp, info.BasePumpIndex);
      basePump.ObjectName = "BasePump";
      basePump.ConnectionMaintenanceOrigin = basePump;

      foreach (var value in info.OutletIndexTypeValue.Values.Where(v => v >= 0))
      {
        var edge = GetEdge(outletGroup, value);
        if (edge == null)
        {
          UnityEngine.Debug.Log($"Index { value } not found!");
        }
        else
        {
          edge.ConnectionMaintenanceOrigin = basePump;
        }
      }

      foreach (var value in info.InletIndexTypeValue.Values.Where(v => v >= 0))
      {
        var edge = GetEdge(inletGroup, value);
        if (edge == null)
        {
          UnityEngine.Debug.Log($"Index { value } not found!");
        }
        else
        {
          edge.ConnectionMaintenanceOrigin = basePump;
        }
      }

      SetEdgeNames(info);

      //var edges1 = info.DischargeAngleGroupIndexList.Select(v => GetEdge(outletGroup, v));
      //var group1 = Group.CreateContinuousGroup(edges1.WithOlets().ToArray());
      //group1.Name = "DischargeGroup";
      //group1.ObjectName = "DischargeGroup";
      //group1.ConnectionMaintenanceOrigin = basePump;

      // HorizontalPumpImporter.AlignAllLeafEdges(BaseBp, basePump);

      BaseBp.RuleList.BindChangeEvents(true);
      if (null != BpOwner)
      {
        BpOwner.BaseBlockPattern = BaseBp;
        BpOwner.RuleList.BindChangeEvents(true);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    protected virtual void SetPropertyAndRule(SingleBlockPatternIndexInfo info) { }
  }

  public abstract class PressureReliefValveBase<T> : PressureReliefValveBase where T : CompositeBlockPattern
  {
    protected PressureReliefValveBase(Document doc, string patternName) : base(doc, patternName, doc.CreateEntity<T>()) { }
    protected new T BpOwner => (T)base.BpOwner;
  }
}