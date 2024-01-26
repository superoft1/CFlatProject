using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chiyoda;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Chiyoda.Importer;
using IDF;
using UnityEngine;

namespace Importer.BlockPattern.Equipment.AirFinCooler
{
  public class AirFinCoolerBase
  {
    protected enum Side
    {
      Forward,
      Back
    }

    protected enum Direction
    {
      Up,
      Down
    }

    private Document Doc { get; }
    private Chiyoda.CAD.Topology.BlockPattern BaseBp { get; }
    private Chiyoda.CAD.Model.AirFinCooler AirFinCooler { get; set; }
    private string IdfFolderPath { get; }
    private string EquipmentShapeName { get; }

    private string LowerName { get; }
    private string UpperName { get; }

    public AirFinCoolerBase(Document doc, string equipmentShapeName)
    {
      Doc = doc;
      EquipmentShapeName = equipmentShapeName;
      BaseBp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.AirFinCooler);
      IdfFolderPath = Path.Combine(ImportManager.IDFBlockPatternDirectoryPath(), "AirFinCooler/IDF/COMB-TYPE");

      LowerName = "CN-B2-DODD-IN";
      UpperName = "CN-B2-USDD-IN";
    }

    public Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish)
    {
      AirFinCooler = ImportEquipment();
      ImportIdf();

      PostProcess();

      onFinish?.Invoke(BaseBp);

      return BaseBp;
    }

    protected virtual bool SelectIdf(string idf, Direction dir)
    {
      switch (dir)
      {
        case Direction.Up:
          return idf.Contains(UpperName);
        case Direction.Down:
          return idf.Contains(LowerName);
        default:
          throw new ArgumentException();
      }
    }

    private IEnumerable<string> IdfFiles(Direction dir)
    {
      var fileList = new List<string>();
      ImportManager.GetFiles(IdfFolderPath, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" }, fileList);
      foreach (var file in fileList.Where(file => SelectIdf(file, dir)))
      {
        yield return file;
      }
    }

    private void ImportIdf(Chiyoda.CAD.Model.Nozzle.Type type)
    {
      var nozzleArray = AirFinCooler.Children.Where(c => c is AFCNozzleArray na && na.NozzleType == type).FirstOrDefault() as AFCNozzleArray;
      var place = nozzleArray.Placement;

      Direction dir;
      Side side;
      switch(place)
      {
        case Chiyoda.CAD.Model.AirFinCooler.PlacementPlane.DownBack:
          dir = Direction.Down;
          side = Side.Back;
          break;
        case Chiyoda.CAD.Model.AirFinCooler.PlacementPlane.DownForward:
          dir = Direction.Down;
          side = Side.Forward;
          break;
        case Chiyoda.CAD.Model.AirFinCooler.PlacementPlane.UpBack:
          dir = Direction.Up;
          side = Side.Back;
          break;
        case Chiyoda.CAD.Model.AirFinCooler.PlacementPlane.UpForward:
          dir = Direction.Up;
          side = Side.Forward;
          break;
        default:
          throw new ArgumentException();
      }

      var file = IdfFiles(dir).ElementAtOrDefault(0);
      if (file == null)
      {
        Debug.Log(@"No File.");
        return;
      }

      var grpInfo = new GroupInfo(Doc, BaseBp, file, false);
      new IDFDeserializer().ImportData(grpInfo, file);
      var group = grpInfo.Line2Group.Values.ElementAt(0);
      RemoveExtraEdges(group, file);

      // グループのリストを作っておく
      var groups = new List<Group>();
      groups.Add(group);

      // ConnectPoints
      var connectPointNumbers = nozzleArray.Nozzles.Select(nozzle => nozzleArray.ConnectPointIndex(nozzle));
      var connectPoints = connectPointNumbers.Select(cpNum => AirFinCooler.GetConnectPoint(cpNum));
      var count = connectPoints.Count();

      for (int i = 0; i < count - 1; ++i)
      {
        Group copy;
        using (var storage = new CopyObjectStorage())
        {
          copy = group.Clone(storage);
        }
        BaseBp.AddEdge(copy);
        groups.Add(copy);
      }

      for (int i = 0; i < count; ++i)
      {
        var eachConnectPoint = connectPoints.ElementAt(i);
        var eachGroup = groups[i];
        // グループ内の原点を端に合わせる
        var vertex0 = ChangeLocalOriginToConnectPoint(AirFinCooler, eachGroup, side, eachGroup.EdgeCount - 1, 0);
        eachGroup.MoveLocalPos(eachConnectPoint.GlobalPoint);
        var vertex1 = AirFinCooler.LeafEdge.GetVertex(connectPointNumbers.ElementAt(i));
        if (vertex1.Partner == null)
        {
          vertex1.Partner = vertex0;
        }
      }

      Debug.Log(groups);
    }

    private void ImportIdf()
    {
      ImportIdf(Nozzle.Type.Discharge);
      ImportIdf(Nozzle.Type.Suction);
    }

    // Groupのローカル座標系原点をConnectPointに変更
    private static HalfVertex ChangeLocalOriginToConnectPoint(Chiyoda.CAD.Model.Equipment equip, CompositeEdge group, Side side, int boarderEdgeIndex, int boarderConnectPointNumber)
    {
      if (!(group?.EdgeList.ElementAtOrDefault(boarderEdgeIndex) is LeafEdge le))
      {
        throw new NullReferenceException();
      }
      var cp = le.PipingPiece.GetConnectPoint(boarderConnectPointNumber);

      // group配下のすべての LeafEdge を、(groupの原点 - ConnectPoint)だけ移動
      var trans = group.GlobalCod.Origin - cp.GlobalPoint;
      group.GetAllLeafEdges().ForEach(e => e.Translate(trans));

      Quaternion rotation;
      switch (side)
      {
        case Side.Back:
          rotation = Quaternion.FromToRotation(Vector3.up, Vector3.left);
          break;
        case Side.Forward:
          rotation = Quaternion.FromToRotation(Vector3.up, Vector3.right);
          break;
        default:
          throw new ArgumentException();
      }
      group.LocalCod = new LocalCodSys3d(group.LocalCod.Origin, group.LocalCod.Rotation * rotation, false);

      return le.GetVertex(boarderConnectPointNumber);
    }

      protected void RemoveExtraEdges(Group group, string file)
    {
      /// TODO : ファイルに依る
      using (Group.ContinuityIgnorer(group))
      {
        List<Edge> removeEdgeList = null;
        if (file.Contains("CN-B2-DODD-IN"))
        {
          removeEdgeList = group.EdgeList.Take(3).ToList();
          removeEdgeList.AddRange(group.EdgeList.Skip(13));
        }
        else if (file.Contains("CN-B2-USDD-IN"))
        {
          removeEdgeList = group.EdgeList.Take(3).ToList();
          removeEdgeList.AddRange(group.EdgeList.Skip(18));
        }
        else
        {
          throw new NotImplementedException();
        }

        removeEdgeList?.ForEach(e => e.Unlink());
      }
    }

    /// <summary>
    /// AFCの読み込み
    /// </summary>
    /// <returns></returns>
    private Chiyoda.CAD.Model.AirFinCooler ImportEquipment()
    {
      return AirFinCoolerBlockPatternImporter.AirFinCoolerImport(EquipmentShapeName, BaseBp) as Chiyoda.CAD.Model.AirFinCooler;
    }

    protected virtual void PostProcess()
    {
      SetBlockPatternInfo();
      BaseBp.Document.MaintainEdgePlacement();
    }

    protected virtual void SetBlockPatternInfo()
    {
      /*
      BaseEquipment.LeafEdge.ObjectName = "Base";
      var factory = new PipingLayoutFactory(Doc, IdfFolderPath, BaseEquipment);

      var layoutsUpper = new List<PipingLayout>
      {
        factory.Create( PipingLayoutFactory.UpperLayout.A1_1_G_S_IN ),
        factory.Create( PipingLayoutFactory.UpperLayout.A1_1_G_T_OUT ),
      };

      var layoutsLower = new List<PipingLayout>
      {
        factory.Create( PipingLayoutFactory.LowerLayout.A1_1_G_S_OUT ),
        factory.Create( PipingLayoutFactory.LowerLayout.A1_1_G_T_IN ),
        factory.Create( PipingLayoutFactory.LowerLayout.A1_1_S_T_IN ),
      };

      List<(string name, string nozzle, Edge pattern)> upPipes
        = layoutsUpper.Select(l => (l.BlockName, l.NozzleName, (Edge)l.GetRuledBlockPattern())).ToList();
      upPipes.Add((string.Empty, string.Empty, null));

      List<(string name, string nozzle, Edge pattern)> lowPipes
        = layoutsLower.Select(l => (l.BlockName, l.NozzleName, (Edge)l.GetRuledBlockPattern())).ToList();
      lowPipes.Add((string.Empty, string.Empty, null));


      upPipes.Take(2).ForEach(p => BaseBp.AddEdge(p.pattern));
      lowPipes.Take(2).ForEach(p => BaseBp.AddEdge(p.pattern));

      // 配管の取り替えルール設定（上部）
      for (int i = 0; i < 2; ++i)
      {
        BaseBp.RegisterUserDefinedProperty(upPipes[i].nozzle, i, new Dictionary<string, double>
        {
          // TODO: 名称は適当なので要修正
          { "Pipe", 0 },
          { "Pipe+Flange", 1 },
          { "Empty", 2 }
        }).AddUserDefinedRule(new InterChangeablePipingRule(GetNozzle(BaseBp, upPipes[i].nozzle), upPipes));
      }

      // 配管の取り替えルール設定（下部）
      for (int i = 0; i < 2; ++i)
      {
        BaseBp.RegisterUserDefinedProperty(lowPipes[i].nozzle, i, new Dictionary<string, double>
        {
          // TODO: 名称は適当なので要修正
          { "Pipe", 0 },
          { "Pipe+Flange", 1 },
          { "Pipe+Flange2", 2 },
          { "Empty", 3 }
        }).AddUserDefinedRule(new InterChangeablePipingRule(GetNozzle(BaseBp, lowPipes[i].nozzle), lowPipes));
      }

      upPipes.Take(2).ForEach(p => p.pattern.RuleList.BindChangeEvents(true));
      lowPipes.Take(2).ForEach(p => p.pattern.RuleList.BindChangeEvents(true));
      BaseBp.RuleList.BindChangeEvents(true);
      */
    }
  }
}
