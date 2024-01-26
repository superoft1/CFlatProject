using System ;
using System.Collections.Generic ;
using System.ComponentModel;
using System.IO ;
using System.Linq ;
using System.Text.RegularExpressions ;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.ControlValve
{
  public class ControlValveBase
  {
    public Document Doc { get; }
    public Chiyoda.CAD.Topology.BlockPattern BaseBp { get; }
    public SingleBlockPatternIndexInfo Info { get; internal set; }

    protected string IdfFolderPath { get; }
    protected string ControlValveShapeName { get; }
    protected string[] ShapeNames { get; }
    /// <summary>
    /// コンストラクタ　仮・直接生成可能に
    /// </summary>
    /// <param name="doc">Document</param>
    /// <param name="cvShapeName">シェイプ名</param>
    public ControlValveBase(Document doc, string cvSubFolder, params string[] cvShapeNames) {
      Doc = doc;
      //  isBlockPatternArrayChild = false にして、この段階ではDocument に登録しない（あとで切り替えるため）
      BaseBp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.ControlValve, isBlockPatternArrayChild: true);

      ShapeNames = cvShapeNames;
      IdfFolderPath = Path.Combine(ImportManager.IDFBlockPatternDirectoryPath(), cvSubFolder);
    }

    private IEnumerable<string> IdfFiles(string shapename)
    {
      var fileList = new List<string>();
      ImportManager.GetFiles(IdfFolderPath, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" }, fileList);
      foreach (var file in fileList.Where((id) => SelectIdf(shapename, id))) {
        yield return file;
      }
    }

    /// <summary>
    /// 読み込み用の仮メソッド
    /// </summary>
    public virtual Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish) {
      ImportIdf();
      foreach (var edge in BaseBp.NonEquipmentEdges) {
        edge.LocalCod = LocalCodSys3d.Identity;
      }

      PostProcess();

      Doc.AddEdge(BaseBp);
      //Doc.RemoveEdge(BaseBp);
      //onFinish?.Invoke( (BlockEdge) BaseBp ) ;
      return BaseBp;
    }

    /// <summary>
    /// 読み込んだBlockPattern を Document に登録する
    /// </summary>
    protected virtual void PostProcess()
    {
      SetBlockPatternInfo(Info);

      AlignAllLeafEdges(BaseBp, Info);

      //Doc.AddEdge( (BlockEdge) BaseBp ) ;

      //Doc.CreateHalfVerticesAndMakePairs(BaseBp);

      //BaseBp.Document.MaintainEdgePlacement() ;
      //var bpa = BpOwner ;
      //bpa.Name = "EndTopPumpBlocks" + $"({PumpShapeName})";

    }

    /// <summary>
    /// 基準点の位置を(0,0,0)に合わせる
    /// </summary>
    /// <param name="bp"></param>
    /// <param name="info"></param>
    protected static void AlignAllLeafEdges(Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info) {
      if (info.OriginLeafEdgeIndices != null) {
        int gid = 0;
        foreach (var edge in bp.EdgeList) {
          if (!(edge is Chiyoda.CAD.Topology.BlockPattern bpa)) continue;
          int num = 0;
          Vector3d sum = new Vector3d(0, 0, 0);
          var mainGroup = bpa.NonEquipmentEdges.ElementAtOrDefault(info.MainGroupIndices[gid]) as IGroup;
          foreach (var index in info.OriginLeafEdgeIndices[gid]) {
            if (!(mainGroup.EdgeList.ElementAt(index) is LeafEdge le)) continue;
            sum += le.LocalCod.Origin;
            num++;
          }
          if (num > 0) {
            sum *= 1.0 / num;
            var offset = -sum;
            bpa.GetAllLeafEdges().ForEach(l => l.MoveLocalPos(offset));
          }
          ++gid;
        }
      }
    }

    /// <summary>
    /// IDFから情報を読み込みつつ、BaseBp にデータを構築していく
    /// </summary>
    /// <remarks>
    /// このメソッドに入出力がなく、ここだけ見ても想像がつきにくいが、
    /// ImportDatameメソッドでgrpInfoにぶら下げたBaseBpにデータを構築しているIdf読み込みはここで完了する
    /// </remarks>
    protected virtual void ImportIdf()
    {
      foreach (var shapename in ShapeNames) {

        var bp = BlockPatternFactory.CreateBlockPattern(BlockPatternType.Type.ControlValve, isBlockPatternArrayChild: true);

        foreach (var file in IdfFiles(shapename)) {
          var grpInfo = new GroupInfo(Doc, bp, file, appendDirectlyToGroup: false);
          new IDFDeserializer().ImportData(grpInfo, file); //  IDFのデータは、IDFDeserializer のプロパティ Groupに読み込まれる。Edgeの読み込まれたGroupは、GetComponentLines ---> GetLineAndGroup でBaseBpに追加されるセットされる
          var group = grpInfo.Line2Group.Values.ElementAt(0);
          RemoveExtraEdges(group, file);
        }
        BaseBp.AddEdge(bp);
      }
    }
    protected virtual bool SelectIdf(string shapename, string idf)
    {
      if (!idf.Contains(shapename)) {
        return false;
      }
      return true;
    }

    protected virtual void RemoveExtraEdges(Group group, string file)
    {
      //throw new NotImplementedException() ;
    }

    internal IGroup GetGroup(int groupIndex)
    {
      return BaseBp.NonEquipmentEdges.ElementAtOrDefault(groupIndex) as IGroup;
    }

    internal LeafEdge GetEdge(IGroup group, int edgeIndex)
    {
      if (edgeIndex < 0) {
        return null;
      }
      return group?.EdgeList.ElementAtOrDefault(edgeIndex) as LeafEdge;
    }

    internal static void SetEdgeNameStatic(LeafEdge edge, string objectName) {
      var pattern = @"([^0-9]+)([0-9]?)$";
      edge.ObjectName = objectName;
      edge.PipingPiece.ObjectName = Regex.Replace(objectName, pattern, "$1Pipe$2");
    }

    internal void SetEdgeName(LeafEdge edge, string objectName)
    {
      ControlValveBase.SetEdgeNameStatic(edge, objectName);
    }

    internal LeafEdge GetEquipmentEdge(Chiyoda.CAD.Topology.BlockPattern bp, int equipmentIndex)
    {
      return bp.EquipmentEdges.ElementAtOrDefault(equipmentIndex);
    }

    /// <summary>
    /// LeafEdge にアクセスするための名前を登録する
    /// </summary>
    /// <param name="info"></param>
    internal virtual void SetEdgeNames(SingleBlockPatternIndexInfo info)
    {
    }

    public static Chiyoda.CAD.Topology.BlockPattern[] ExtractChildBlockPatterns(Chiyoda.CAD.Topology.BlockPattern bpowner){
      var bpl = new List<Chiyoda.CAD.Topology.BlockPattern>();
      foreach(var edge in bpowner.NonEquipmentEdges){
        if (edge is Chiyoda.CAD.Topology.BlockPattern bp)
          bpl.Add(bp);
      }
      return bpl.ToArray();
    }
    protected virtual void SetBlockPatternInfo(SingleBlockPatternIndexInfo info)
    {
      SetPropertyAndRule(info);
      //HorizontalPumpImporter.AlignAllLeafEdges( BaseBp, basePump ) ;
    }

    protected virtual void SetPropertyAndRule(SingleBlockPatternIndexInfo info) {

    }
  }
  public class ValveSelectorRule : IUserDefinedRule
  {
    Chiyoda.CAD.Topology.BlockPattern _bpowner;
    Chiyoda.CAD.Topology.BlockPattern[] _bpa;
    public ValveSelectorRule(SingleBlockPatternIndexInfo info,Chiyoda.CAD.Topology.BlockPattern bpowner,params Chiyoda.CAD.Topology.BlockPattern[] bpa){
      _bpowner = bpowner;
      _bpa = bpa;
    }
    public void Run(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      ValveSelectItems.Items item = (ValveSelectItems.Items)property.Value;
      //string name = Enum.GetName(typeof(ValveSelectItems.Items), item);
      string name = item.ToString();
      string descstr = null;
      var fi = typeof(ValveSelectItems.Items).GetField(item.ToString());
      if (fi != null){ 
        var desc = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        descstr = desc.Select(e => e.Description).FirstOrDefault();
      }
      Chiyoda.CAD.Topology.BlockPattern[] bpa = ControlValveBase.ExtractChildBlockPatterns(_bpowner);
      foreach (var edge in bpa) {
        _bpowner.RemoveEdge(edge);
      }
      foreach (var bp in _bpa){
        foreach (var group in bp.NonEquipmentEdges) { 
          if (group.Name.Contains(descstr ?? name)){
            _bpowner.AddEdge(bp);
            return;
          }
        }
      }

    }
  }

}