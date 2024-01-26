using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.HorizontalHeatExchanger
{
  public class PipingLayoutFactory
  {
    private Document Doc { get ; }
    private string IdfFolderPath { get ; }
    private Chiyoda.CAD.Model.Equipment Equipment { get ; }
    
    public PipingLayoutFactory(Document doc, string idfFolderPath, Chiyoda.CAD.Model.Equipment equip)
    {
      Doc = doc ;
      IdfFolderPath = idfFolderPath ;
      Equipment = equip ;
    }

    public enum UpperLayout
    {
      A_1_IN,
      A_1_T_OUT,
      B_1_OUT_V,
      B_1_T_OUT,
      Empty,
    }

    public enum LowerLayout
    {
      A_1_OUT,
      A_1_T_IN,
      B_1_OUT_L,
      B_1_IN,
      B_1_T_IN,
      Empty,
    }

    public PipingLayout Create(UpperLayout upper)
    {
      switch ( upper ) {
        case UpperLayout.A_1_IN:
          return new Upper.A_1_IN( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 8, NozzleName = "ShellIn"
          } ) ;
        case UpperLayout.A_1_T_OUT:
          return new Upper.A_1_T_OUT( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 0, NozzleName = "TubeOut"
          } ) ;
        case UpperLayout.B_1_OUT_V:
          return new Upper.B_1_OUT_V( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 0, NozzleName = "ShellOut2"
          } ) ;
        case UpperLayout.B_1_T_OUT:
          return new Upper.B_1_T_OUT( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 0, NozzleName = "TubeOut"
          } ) ;
        default:
          break ;
      }

      return null ;
    }
    
    public PipingLayout Create(LowerLayout lower)
    {
      switch ( lower ) {
        case LowerLayout.A_1_OUT:
          return new Lower.A1_1_OUT( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 0, NozzleName = "ShellOut"
          } ) ;
        case LowerLayout.A_1_T_IN:
          return new Lower.A_1_T_IN( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 9, NozzleName = "TubeIn"
          } ) ;
        case LowerLayout.B_1_OUT_L:
          return new Lower.B_1_OUT_L( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 0, NozzleName = "ShellOut"
          } ) ;
        case LowerLayout.B_1_IN:
          return new Lower.B_1_IN( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 2, NozzleName = "ShellIn"
          } ) ;
        case LowerLayout.B_1_T_IN:
          return new Lower.B_1_T_IN( Doc, IdfFolderPath, Equipment, new PipingLayout.PipeInformation
          {
            BoarderEdgeIndex = 2, NozzleName = "TubeIn"
          } ) ;
        default:
          break ;
      }

      return null ;
    }

  }
  public abstract class PipingLayout
  {
    public class PipeInformation
    {
      public int BoarderEdgeIndex { get ; set ; }
      public string NozzleName { get ; set ; }
      public int BopEdgeIndex { get; set; }
      public int FlexEdgeIndex { get; set; }
      public int EndEdgeIndex { get; set; }
      public int EndEdgeConnectPoint { get; set; }
      public int AngleDefault { get; set; }
    }
    
    public EInterchangeablePosition InterchangeablePosition { get ; set ; }

    public string NozzleName => PipeInfo.NozzleName ;
    public string BlockName => Path.GetFileNameWithoutExtension( GetIdfFilePath() ) ;

    public enum EInterchangeablePosition
    {
      Upper,
      Lower
    }

    private Document Doc { get ; }
    protected PipeInformation PipeInfo { get ; set ; }
    protected string IdfFolder { get ; }
    protected static string IdfFileName(string basename)
    {
      basename = basename.ToString().Replace('_', '-');
      return "HE-" + basename.Substring(0, 3) + "/A!-HE-" + basename + "_-0.idf";
    }
    protected Chiyoda.CAD.Model.Equipment HeatExchanger { get ; }


    protected Chiyoda.CAD.Topology.BlockPattern BlockPattern { get ; private set ; }

    protected PipingLayout( Document doc, string idfFolder, Chiyoda.CAD.Model.Equipment he )
    {
      Doc = doc ;
      IdfFolder = idfFolder ;
      HeatExchanger = he ;
    }

    protected abstract string GetIdfFilePath() ;
    
    public Chiyoda.CAD.Topology.BlockPattern GetRuledBlockPattern()
    {
      if ( BlockPattern == null ) {
        CreateRuledBlockPattern() ;
      }
      return BlockPattern ;
    }

    private Group LoadIdfFile()
    {
      var grpInfo = new GroupInfo( Doc, BlockPattern, GetIdfFilePath() ) ;
      new IDFDeserializer().ImportData( grpInfo, GetIdfFilePath() ) ;
      var group = grpInfo.Line2Group.Values.ElementAt( 0 ) ;

#if true   
      var vtx0 = ChangeLocalOriginToConnectPoint( HeatExchanger, group, PipeInfo.BoarderEdgeIndex, boarderConnectPointNumber:0 ) ;
      BlockPattern.MoveLocalPos( HeatExchanger.FindConnectPoint( PipeInfo.NozzleName ).GlobalPoint ) ;
      var cpIndex = HeatExchanger.GetConnectPointIndex( PipeInfo.NozzleName ) ;
      if ( cpIndex.HasValue ) {
        var vtx1 = HeatExchanger.LeafEdge.GetVertex( cpIndex.Value ) ;
        if ( vtx1.Partner == null ) {
          vtx1.Partner = vtx0 ;
        }
      }
      else {
        throw new NullReferenceException();
      }
#endif      

      return group ;
    }
    
    // Groupのローカル座標系原点をConnectPointに変更
    private static HalfVertex ChangeLocalOriginToConnectPoint( Chiyoda.CAD.Model.Equipment equip, CompositeEdge group, int boarderEdgeIndex,
      int boarderConnectPointNumber )
    {
      if ( ! ( group?.EdgeList.ElementAtOrDefault( boarderEdgeIndex ) is LeafEdge le ) ) {
        throw new NullReferenceException();
      }
      var cp = le.PipingPiece.GetConnectPoint( boarderConnectPointNumber ) ;

      // group配下のすべての LeafEdge を、(groupの原点 - ConnectPoint)だけ移動
      var trans = group.GlobalCod.Origin - cp.GlobalPoint ;
      group.GetAllLeafEdges().ForEach( e => e.Translate( trans ) ) ;

      // 配管の流れ方向
      var dir = GroupDirectionInXyPlane(le, boarderConnectPointNumber) ;
      
      // 機器のY方向に配管方向を一旦揃える
      var equipY = equip.LeafEdge.GlobalCod.DirectionY ;
      var q = Quaternion.FromToRotation( (Vector3)dir, (Vector3)equipY ) ;
      group.LocalCod = new LocalCodSys3d(group.LocalCod.Origin, group.LocalCod.Rotation * q, false);
      
      return le.GetVertex( boarderConnectPointNumber ) ;
    }

    /// <summary>
    /// XY平面上での配管の流れ方向
    /// </summary>
    /// <param name="leafEdge"></param>
    /// <param name="boarderConnectPointNumber"></param>
    /// <returns></returns>
    private static Vector3d GroupDirectionInXyPlane( LeafEdge leafEdge, int boarderConnectPointNumber )
    {
      var results = new List<(PipingPiece, Vector3d)>(); 
      GroupDirectionImpl( leafEdge, boarderConnectPointNumber , results) ;
      var dir = results.Aggregate( Vector3d.zero, ( current, tuple ) => current + tuple.Item2 ) ;
      dir.z = 0 ;// xy平面で考える
      dir.Normalize();
      return dir ;
    }

    /// <summary>
    /// XY平面上での配管の流れ方向（詳細実装）
    /// </summary>
    /// <returns></returns>
    private static void GroupDirectionImpl(LeafEdge leafEdge, int connectPointNumber, List<(PipingPiece, Vector3d)> results)
    {
      Vector3d EdgeDirection(HalfVertex v) => v.GlobalPoint - leafEdge.GetVertex( connectPointNumber ).GlobalPoint ;
      
      foreach ( var v in leafEdge.Vertices
        .Where(v=>v.ConnectPointIndex != connectPointNumber) ) {

        var v2 = v.Partner;
        if ( v2 == null ) {
          results.Add( ( leafEdge.PipingPiece, EdgeDirection(v) ) ) ;
          return ;
        }

        if(v2.ConnectPointIndex >= 2){
          continue ;// 分岐は無視
        }

        results.Add((leafEdge.PipingPiece, EdgeDirection(v) ) ) ;
        GroupDirectionImpl(v2.LeafEdge, v2.ConnectPointIndex, results) ;
      }
    }


    private void CreateRuledBlockPattern()
    {
      BlockPattern = Doc.CreateEntity<Chiyoda.CAD.Topology.BlockPattern>() ;
      BlockPattern.Name = $"{PipeInfo.NozzleName}Block" ;
      BlockPattern.ObjectName = $"{PipeInfo.NozzleName}Block" ;
      
      var group = LoadIdfFile() ;
      
      group.Name = PipeInfo.NozzleName ;
      group.ObjectName = group.Name ;
      SetInterchangeableEdge( group ) ;

      AddRule(group);
    }

    private void SetInterchangeableEdge(IGroup group)
    {
      if ( ! ( group.EdgeList.ElementAtOrDefault( PipeInfo.BoarderEdgeIndex ) is LeafEdge interchangeable ) ) {
        throw new InvalidOperationException();
      }
      interchangeable.ObjectName = GetInterChangeableName() ;
    }

    public static string GetInterChangeableName()
    {
      return "InterChangeable" ;
    }

    protected static string ContinuousGroupName()
    {
      return $"ContinuousGroup" ;
    }

    public static string[] GetKeepProperties()
    {
      return new [] { "BOP", "Angle", "ContinuousGroup.HorizontalRotationDegree" } ;
    }

    protected void CreateContinuousGroup(IGroup group)
    {
      var continuousGroup =
        Group.CreateContinuousGroup( group.EdgeList.Select( e => (LeafEdge) e ).WithOlets().ToArray() ) ;
      continuousGroup.Name = ContinuousGroupName() ;
      continuousGroup.ObjectName = ContinuousGroupName() ;
      continuousGroup.ConnectionMaintenanceOrigin = HeatExchanger.LeafEdge ;
    }

    protected string BopEdgeName()
    {
      return $"{NozzleName}BOP" ;
    }

    protected void SetupBopEdge(IGroup group, int bopEdgeIndex, int flexEdgeIndex)
    {
      if ( ! ( @group.EdgeList.ElementAtOrDefault( bopEdgeIndex ) is LeafEdge bopEdge ) ) {
        throw new NullReferenceException();
      }
      bopEdge.ObjectName = BopEdgeName() ;
      bopEdge.PipingPiece.ObjectName = BopEdgeName() ;
      bopEdge.PositionMode = PositionMode.FixedZ ;
      bopEdge.ConnectionMaintenanceOrigin = HeatExchanger.LeafEdge ;

      SetupFlexEdge( group, flexEdgeIndex ) ;
    }

    private void SetupFlexEdge(IGroup group, int flexEdgeIndex)
    {
      if ( ! ( @group.EdgeList.ElementAtOrDefault( flexEdgeIndex ) is LeafEdge flexEdge ) ) {
        throw new NullReferenceException();
      }

      if ( ! ( flexEdge.PipingPiece is Pipe flex ) ) {
        throw new NullReferenceException();
      }
      flex.FlexRatio = 1 ;
      flexEdge.ConnectionMaintenanceOrigin = HeatExchanger.LeafEdge ;
    }

    private string EndEdgeName()
    {
      return "EndEdge" ;
    }

    protected void SetupEndEdge(IGroup group, int endEdgeIndex)
    {
      if ( ! ( @group.EdgeList.ElementAtOrDefault( endEdgeIndex ) is LeafEdge endEdge ) ) {
        throw new NullReferenceException();
      }
      endEdge.PipingPiece.ObjectName = EndEdgeName() ;
      endEdge.ConnectionMaintenanceOrigin = HeatExchanger.LeafEdge ; 
    }

    protected void SetupDiameterRule(int defaultDiameterNpsInchValue, int endEdgeConnectPoint)
    {
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;

      var prop = BlockPattern.RegisterUserDefinedProperty( "Diameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(defaultDiameterNpsInchValue).NpsMm , diameterMinNpsMm,  diameterMaxNpsMm) ;
      prop.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( EndEdgeName(), endEdgeConnectPoint ) ) ) ;
    }

    protected void SetupAngleRule(string groupName, int defaultAngle)
    {
      BlockPattern.RegisterUserDefinedProperty("Angle", PropertyType.Angle, defaultAngle, 0, 360, stepValue: 90);
      BlockPattern.RuleList.AddRule( $"#{groupName}.HorizontalRotationDegree", ".Angle" ) ;
    }
    
    protected virtual void AddRule(Group group)
    {
    }
  }
}
