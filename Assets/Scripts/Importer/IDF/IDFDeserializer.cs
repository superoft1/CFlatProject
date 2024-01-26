using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace IDF
{
  /// <summary>
  /// 単位オプション
  /// </summary>
  public enum UnitOption
  {
    mm,
    inch_16x
  }
  /// <summary>
  /// コンポーネント行を保存
  /// </summary>
  public class ComponentLines
  {
    public List<(string[] strs, int lineIndex)> Lines { get; } = new List<(string[] strs, int lineIndex)>();
  }

  // ストックナンバー行を保存
  public class StockNumberLines
  {
    public List<string> Lines { get; } = new List<string>();
  }

  public class IDFDeserializer
  {
    private Line Line { get; set; }
    private IGroup Group { get; set; }

    private Vector3d Standard { get; set; }

    private IDFImporterStacks Stacks { get; set; }

    private StockNumberLines StockNumberLines { get; set; }

    private string PipingSpecification { get; set; }

    private DebugLogger debugLogger { get; set; }

    public UnitOption UnitOption { get; set; }

    private string ConvertLineID(string id)
    {
      //FIXME: 連番が振られる規則がわからないので、しばらくはそのまま返す
      return id;
    }

    /// <summary>
    /// ファイルから必要な全行を取得
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerable<(string[] strs, int lineIndex)> AllLines(string path)
    {
      int lineIndex = 0 ;
      using ( var sr = new StreamReader( path, Encoding.Default ) ) {
        // header
        for ( int i = 0 ; i < 10 ; ++i, ++lineIndex ) {
          if ( sr.EndOfStream ) yield break;

          yield return (IDFStringUtility.Split( sr.ReadLine() ), lineIndex);
        }

        --lineIndex ;// この後のwhile文でlineIndexを数えやすいように一旦デクリメント
        // body
        string lastLine = null;
        bool overflowing = false;
        while ( !sr.EndOfStream ) {
          var line = sr.ReadLine();
          ++lineIndex ;

          if ( IDFRecordType.IsOverflowTextRecord( line ) ) {
            // 継続
            if ( !overflowing ) {
              overflowing = true;
              lastLine = lastLine.TrimEnd() + line.Substring(6);
            }
            else
            {
              lastLine += line.Substring(6);
            }
            continue;
          }

          if ( null != lastLine ) {
            // 継続ラインに対する処理
            if (overflowing)
            {
              // overflowingの場合はコメント文内にスペースが含まれているのでsplitが使えない
              yield return (new[] {lastLine.Substring(0, 6), lastLine.Substring(6).TrimEnd()}, lineIndex);
            }
            else
            {
              var list = IDFStringUtility.Split( lastLine );
              if ( 0 < list.Length ) yield return (list, lineIndex);
            }
            overflowing = false;
          }
          lastLine = line;
        }

        if ( null != lastLine ) {
          // 最後の継続ラインに対する処理
          var list = IDFStringUtility.Split( lastLine );
          if ( 0 < list.Length ) yield return (list, lineIndex);
        }
      }
    }

    private void SetUnitOption(List<string[]> dataBlock)
    {
      // Data Blockの41が単位に関する設定
      var s41 = dataBlock[2][12];
      if (s41[0] == '0' || s41[0] == '1')
      {
        UnitOption = UnitOption.inch_16x;
      }
      else if (s41[0] == '2')
      {
        UnitOption = UnitOption.mm;
      }
    }

    /// <summary>
    /// コンポーネント行を取得
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="allLines"></param>
    /// <returns></returns>
    private ComponentLines GetComponentLines(Chiyoda.Importer.GroupInfo groupInfo, IEnumerable<(string[] strs, int lineIndex)> allLines)
    {
      var componentLines = new ComponentLines();
      StockNumberLines = new StockNumberLines();
      var existStandard = false;
      Standard = Vector3d.zero;
      var dataBlock = new List<string[]>();
      foreach (var cols in allLines)
      {
        if (dataBlock.Count < 10)
        {
          dataBlock.Add(cols.strs);
          continue;
        }
        switch (IDFRecordType.GetPurposeType(cols.strs[0]))
        {
          case IDFRecordType.PurposeType.PipelineReference:
            (Line, Group) = groupInfo.GetLineAndGroup(ConvertLineID(cols.strs[1]));
            continue;
          case IDFRecordType.PurposeType.PipingSpecificationName:
            PipingSpecification = cols.strs[1];
            continue;
          case IDFRecordType.PurposeType.ItemCode:
            if ( cols.strs.Count() <= 1 ) {
              StockNumberLines.Lines.Add("");
            }
            else {
              StockNumberLines.Lines.Add(cols.strs[1]);
            }
            continue;
        }

        var fittingType = IDFRecordType.GetFittingType(cols.strs[0]);
        switch (fittingType)
        {
          case IDFRecordType.FittingType.Unknown:
            continue;
          case IDFRecordType.FittingType.Standard:
            Standard = new Vector3d(-double.Parse(cols.strs[1]), double.Parse(cols.strs[2]), double.Parse(cols.strs[3]));
            existStandard = true;
            continue;
        }

        if (!existStandard) continue;
        Line.ServiceClass = PipingSpecification;
        componentLines.Lines.Add(cols);
      }

      if (componentLines.Lines.Count < 2)
      {
        // IDF for This Line was Deleted !! と書かれていて内容がないIDFファイルが存在する
        return null;
      }

      SetUnitOption(dataBlock);
      return componentLines;
    }

    /// <summary>
    /// IDFの11番目の項目を7桁として見た時に上位2つが11であれば、他のIDFと重複している
    /// </summary>
    /// <returns>true: 重複 / false: 重複していない</returns>
    private static bool IsDuplicateComponent(string[] columns)
    {
      if(columns.Length <= 10) { return false; }

      if(!int.TryParse(columns[10],out var result))
      {
        return false;
      }
      var value = result.ToString("0000000");
      return value.Substring( 0, 2 ) == "11";
    }
    
    

    /// <summary>
    /// 指定されたIDFファイルの読み込み
    /// </summary>
    /// <param name="grpInfo"></param>
    /// <param name="path"></param>
    /// <param name="shouldCreateVertex"></param>
    /// <returns></returns>
    public IEnumerable<(LeafEdge le, int lineIndex)> ImportData(Chiyoda.Importer.GroupInfo grpInfo, string path, bool shouldCreateVertex = true)
    {
      var componentLines = GetComponentLines(grpInfo, AllLines(path));
      if (componentLines == null)
      {
        return null;
      }
      Stacks = new IDFImporterStacks(grpInfo.Document, Line, path, PipingSpecification, shouldCreateVertex );

      bool debug = false;
//      debug = true;

      debugLogger = new DebugLogger(Line, path, debug);

      var createdEdgeList = new List<(LeafEdge le, int lineIndex)>();

      using ( Chiyoda.CAD.Topology.Group.ContinuityIgnorer( Group ) ) {
        foreach (var columns in componentLines.Lines)
        {
          if (IsDuplicateComponent(columns.strs))
          {
            continue;
          }

          var edge = ImportData( grpInfo.Document, columns.strs, Group ) ;
          if (edge != null)
          {
            createdEdgeList.Add((edge, columns.lineIndex));
          }
          debugLogger?.WriteEntityOrder(columns.strs[0], edge);
        }
        Stacks.Build();
      }

      debugLogger?.WriteEntityEdge();

      // IDFのデータに問題があって、LeafEdgeオブジェクト作成後に除去している場合がある
      return createdEdgeList.Where(e => Group.EdgeList.Contains(e.le));
    }

    private LeafEdge ImportData(Document doc, string[] columns, IGroup group)
    {
      var fittingType = IDFRecordType.GetFittingType(columns[0]);
      var entityType = IDFEntityType.GetType(fittingType);
      var legType = IDFRecordType.GetLegType(columns[0]);

      LeafEdge createdEdge = null;
      if (entityType != EntityType.Type.NoEntity && legType == IDFRecordType.LegType.InLeg)
      {
        var importer = IDFEntityFactory.CreateEntityImporter(fittingType, legType, columns, Standard, UnitOption);
        importer.Document = doc;
        importer.Group = group;

        createdEdge = importer?.Import(doc);
        if ( fittingType == IDFRecordType.FittingType.PipeHanger ) {
//          importer.AddStockNumber(StockNumberLines.Lines);//TODO: StockNumberは未対応
//          Stacks.Add(importer);
          // サポートの場合はLeafEdgeを生成しない
          return null ;
        }
        if (createdEdge == null) return null;
        createdEdge.Line = Line;
        group.AddEdge( createdEdge );
        importer.AddStockNumber(StockNumberLines.Lines);
        Stacks.Add(importer);
      }
      else
      {
        Stacks.Update(fittingType, legType, columns);
      }

      return createdEdge;
    }
  }
}
