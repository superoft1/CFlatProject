///<summary>
/// Flex 新システム仕様確定までの暫定処理を条件コンパイルにて実装
///</summary>
//#define INDEXING_NOW  //  Index 設定作業中にIndexを必要とする処理を止める
#define DO_NOT_USE_FLEX_TO_MOVE_JOINTS  //  ジョイントの移動にFlex を使用しない


using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.DB ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.Filter
{
  public class FilterFS_A2_2_S : FilterBase<BlockPatternWithMirror>
  {
    private FilterSystemPipeUtility _suctionValveStopper;
    private FilterSystemPipeUtility _dischargeValveStopper;
    private IUserDefinedRule _dischargeValveStopperRule;  //  
    private IUserDefinedRule _suctionValveStopperRule;   

    public FilterFS_A2_2_S( Document doc ) : base( doc, "FS-A2-2-S" )
    {
      Info = new SingleBlockPatternIndexInfoWithMirror
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 8 },
#else
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 7 },
#endif
        },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeDistanceOrigin, 1 },  //  nozzle side flange
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeSpacer1,     2 },
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeSpacer2,     3 },
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePreValveAFlange,  4 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveA,   5 },          //  valve本体
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePostValveAFlange,  6 }, //  joint側

        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 7 },      //  Nozzle pipe
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd,    8 },      //  追加されたパイプ
#else
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 7 },      //  Nozzle pipe
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd,    0 },      //  Suction End
#endif
        },
        SuctionAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionSpacer1,     4 },
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionSpacer2,     5 },
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionDistanceOrigin,6 },
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPreValveAFlange, 3 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveA, 2 },           //  valve本体
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPostValveAFlange, 1}, //  joint側
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 7 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
#else
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, -1 }, //  DischargePostValveAFlangeと同一
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, -1 },
#endif
        },
        //
        //  Pipe index helper
        //
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
        SuctionFlexHelper = new int[,] { { 0, 5 } },
        DischargeFlexHelper = new int[,]{ { 2, 7 } },       //  all discharge pipes
#else
        SuctionFlexHelper = new int[,] { { 4, 5 } },
        DischargeFlexHelper = new int[,]{ { 2, 3 } },       //  all discharge pipes
#endif
        SuctionPipeIndexRange = new int[,]{ { 0, 5 }, },    //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,]{ { 2,7 }, },   //  all pipe

        SuctionDiameterNPSInch = 6,
        DischargeDiameterNPSInch = 6,
      } ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      ImportIdfAndEquipment() ;
      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      #if !INDEXING_NOW
      FilterIndexingHelper.BuildIndexList(BpOwner, BaseBp, Info);
      FilterPipeLengthUpdater.AssortMinimumLengths(BaseBp, Info);

      _dischargeValveStopper = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 0, 5); //  Pump(含まない)～Valve(含む)の長さを取得
      _suctionValveStopper = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 7, 2);   //  Pump(含まない)～Valve(含む)の長さを取得
      #endif

      PostProcess();

      var cbp = BpOwner;
      #if !INDEXING_NOW
      cbp.GetProperty("AccessSpace").Value = 0.5;
      cbp.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch((double)Info.DischargeDiameterNPSInch).NpsMm;
      cbp.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch((double)Info.SuctionDiameterNPSInch).NpsMm;
      cbp.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      cbp.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      cbp.GetProperty("DischargeFilterToJoint").Value = 0.8;
      cbp.GetProperty("DischargeFilterToValve").Value = 0.5;
      cbp.GetProperty("SuctionFilterToJoint").Value = 1.2;
      cbp.GetProperty("SuctionFilterToValve").Value = 0.5;
      
      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;
      
      #endif
      onFinish?.Invoke( (BlockEdge)BpOwner ?? BaseBp ) ;
      return BaseBp ;
    }



    protected override void RemoveExtraEdges( Group group, string file )
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if (file.Contains("-DIS(A)")){
          removeEdgeList = group.EdgeList.Reverse().Take(3).ToList();
        }
        else if (file.Contains("-DIS(B)")){
          removeEdgeList = group.EdgeList.Reverse().Take(3).ToList();
        }
        else if (file.Contains("-SUC(A)"))
        {
          removeEdgeList = group.EdgeList.Take(3).ToList();
        }
        else if (file.Contains("-SUC(B)"))
        {
          removeEdgeList = group.EdgeList.Take(2).ToList();
        }
        removeEdgeList?.ForEach( e => e.Unlink() ) ;
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
        var newpipe = GetExtraPipe(group, file, Info);
        if (newpipe != null){ 
          group.AddEdge(newpipe);
        }
#endif
      }
    }

    #if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
    /// <summary>
    /// IDFにない外側のパイプをDis/Suc一つづつ追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected LeafEdge GetExtraPipe( Group group, string file, SingleBlockPatternIndexInfo info )
    {
      if ( file.Contains( "-SUC" ) ) {

        var pipe = ( group.EdgeList.ElementAtOrDefault( 0 ) as LeafEdge )?.PipingPiece as Pipe ;
        if ( ! ( pipe?.Parent is LeafEdge pipeEdge ) ) {
          return null ;
        }

        //var table = DB.Get<DimensionOfFlangeTable>();

        foreach ( var vertex in pipeEdge.Vertices ) {
          if ( vertex.Partner != null ) continue ;
          Vector3d pipeFar, pipeNear ;
          if ( vertex.ConnectPointIndex == 0 ) {
            pipeNear = pipeEdge.GetVertex( 0 ).ConnectPoint.GlobalPoint ;
            pipeFar = pipeEdge.GetVertex( 1 ).ConnectPoint.GlobalPoint ;
          }
          else {
            pipeNear = pipeEdge.GetVertex( 1 ).ConnectPoint.GlobalPoint ;
            pipeFar = pipeEdge.GetVertex( 0 ).ConnectPoint.GlobalPoint ;
          }

          var pipeDir = ( pipeNear - pipeFar ).normalized * 1.0 ;
          var leafEdge = Doc.CreateEntity<LeafEdge>() ;
          var newpipe = Doc.CreateEntity( EntityType.Type.Pipe ) as Pipe ;
          leafEdge.PipingPiece = newpipe ;
          Vector3d outside = pipeNear ;
          Vector3d weld = pipeNear + pipeDir ;
          {
            var origin = (weld + outside)  *   0.5;
            var direction = outside - weld; // weld - outside;
            LeafEdgeCodSysUtils.LocalizeStraightComponent((LeafEdge)newpipe.Parent, origin, direction);
            newpipe.Diameter = pipe.Diameter;
            newpipe.Length = 1.0;
          }
          leafEdge.CreateAllHalfVertices() ;

          //  connect new  edge 
          HalfVertex vertTarget, vertToConnect = null;
          double minMagnitude = Double.MaxValue;
          if (vertex.ConnectPointIndex == 0) {
            vertTarget = pipeEdge.GetVertex(0);
          }else{
            vertTarget = pipeEdge.GetVertex(1);
          }
          foreach(var vert in leafEdge.Vertices){
            var magnitude = (vertTarget.GlobalPoint - vert.GlobalPoint).magnitude;
            if (magnitude < minMagnitude){
              minMagnitude = magnitude;
              vertToConnect = vert;
            }
          }
          if (vertToConnect != null && vertTarget != null){ 
            vertTarget.Partner = null;
            vertTarget.Partner = vertToConnect;
          }
          newpipe.Length = 0.0;
          return leafEdge ;
        }
      }
      else if ( file.Contains( "-DIS" )){
        var pipe = ( group.EdgeList.ElementAtOrDefault( info.NextOfIndexTypeValue[SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd] ) as LeafEdge )?.PipingPiece as Pipe ;
        if ( ! ( pipe?.Parent is LeafEdge pipeEdge )) {
          return null ;
        }

        //var table = DB.Get<DimensionOfFlangeTable>();

        foreach ( var vertex in pipeEdge.Vertices ) {
          if ( vertex.Partner != null ) continue ;
          Vector3d pipeFar, pipeNear ;
          if ( vertex.ConnectPointIndex == 0 ) {
            pipeNear = pipeEdge.GetVertex( 0 ).ConnectPoint.GlobalPoint ;
            pipeFar = pipeEdge.GetVertex( 1 ).ConnectPoint.GlobalPoint ;
          }
          else {
            pipeNear = pipeEdge.GetVertex( 1 ).ConnectPoint.GlobalPoint ;
            pipeFar = pipeEdge.GetVertex( 0 ).ConnectPoint.GlobalPoint ;
          }

          var pipeDir = ( pipeNear - pipeFar ).normalized * 1.0 ;
          var leafEdge = Doc.CreateEntity<LeafEdge>() ;
          var newpipe = Doc.CreateEntity( EntityType.Type.Pipe ) as Pipe ;
          leafEdge.PipingPiece = newpipe ;
          Vector3d outside = pipeNear ;
          Vector3d weld = pipeNear + pipeDir ;
          
          {
            var origin = (weld + outside)  *   0.5;
            var direction = outside - weld;//weld - outside;

            LeafEdgeCodSysUtils.LocalizeStraightComponent((LeafEdge)newpipe.Parent, origin, direction);

            newpipe.Diameter = pipe.Diameter;
            newpipe.Length = 1.0; // 0.999999761581137;
          }
          leafEdge.CreateAllHalfVertices() ;

          //  connect new  edge 
          HalfVertex vertTarget, vertToConnect = null;
          double minMagnitude = Double.MaxValue;
          if (vertex.ConnectPointIndex == 0) {
            vertTarget = pipeEdge.GetVertex(0);
          }else{
            vertTarget = pipeEdge.GetVertex(1);
          }
          foreach(var vert in leafEdge.Vertices){
            var magnitude = (vertTarget.GlobalPoint - vert.GlobalPoint).magnitude;
            if (magnitude < minMagnitude){
              minMagnitude = magnitude;
              vertToConnect = vert;
            }
          }
          if (vertToConnect != null && vertTarget != null){ 
            vertTarget.Partner = null;
            vertTarget.Partner = vertToConnect;
          }

          newpipe.Length = 0.0;
          newpipe.Diameter = pipe.Diameter;
          return leafEdge ;
        }
      }
      return null ;
    }
#endif
    protected override bool SelectIdf( string idf )
    {
      if ( ! idf.Contains( base.PatternName ) ) {
        return false ;
      }
      if ( idf.Contains( "DIS(A)" )||idf.Contains( "SUC(A)" )){
        return true ;
      }
      return false ;
    }


    /*
    protected override void SetEdgeNames( SingleBlockPatternIndexInfo info ){
      base.SetEdgeNames(info);
      var suctionGroup = GetGroup( info.SuctionIndex ) ;
      var edge = GetEdge(suctionGroup, info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]);
      if (edge != null){
        edge.PositionMode = PositionMode.FixedX ;
      }
      var dischargeGroup = GetGroup( info.DischargeIndex ) ;
      edge = GetEdge(dischargeGroup, info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]);
      if (edge != null){
        edge.PositionMode = PositionMode.FixedX ;
      }
    }
    */

    protected override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
    #if !INDEXING_NOW
      IRule rule;

      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 1.2 ) ;
#if false
      bp.RegisterUserDefinedProperty( "PipeMinLength", PropertyType.Length, 0.5 ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
#endif

      //{
      //  var bpa = BpOwner ;
      //  bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
      //  bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      //}
      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetY", ".AccessSpace +(#BasePump.MaxY - #BasePump.MinY)+ 2.0*#BasePump.PosY" ) ;
      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetX", "0.0" )
          .AddTriggerSourcePropertyName( "AccessSpace" ) ;

      BaseBp.RegisterUserDefinedProperty( "SuctionJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "SuctionEnd" ) ) ;
      BaseBp.RegisterUserDefinedProperty( "DischargeJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "DischargeEnd" ) ) ;

      string LengthFunc( string diameter )
      {
        return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeMainLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )" ;
      }

      BaseBp.RegisterUserDefinedProperty( "DischargeFilterToJoint", PropertyType.Length, 2.850 ) ;
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValve", PropertyType.Length, 0.387);
      BaseBp.RegisterUserDefinedProperty( "SuctionFilterToJoint", PropertyType.Length, 2.2 ) ;
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValve", PropertyType.Length, 0.387);

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      BaseBp.RegisterUserDefinedProperty("DJ_MAX_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("DJ_POS_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("SJ_MIN_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("SJ_POS_X", PropertyType.TemporaryValue, 0.387);

      BaseBp.RuleList.AddRule(".DJ_MAX_X", 
        "#BasePump.MinX - .DV_MinLen - #DischargePostValveAFlangePipe.Length - #DischargeEndPipe.MinLength");
      BaseBp.RuleList.AddRule(".DJ_POS_X", $"Min(.DJ_MAX_X,(#BasePump.MinX - .DischargeFilterToJoint  +  {LengthFunc("#DischargeEndPipe.Diameter")}))");

      BaseBp.RuleList.AddRule("#DischargeEndPipe.Length", "(.DV_POS_X-.DJ_POS_X)-(#DischargeValveAPipe.Length*0.5)-#DischargePostValveAFlangePipe.Length");
      BaseBp.RuleList.AddRule(".SJ_MIN_X", 
        "#BasePump.MaxX + .SV_MinLen + #SuctionPostValveAFlangePipe.Length + #SuctionEndPipe.MinLength");
      BaseBp.RuleList.AddRule(".SJ_POS_X", $"Max(.SJ_MIN_X ,(#BasePump.MaxX + .SuctionFilterToJoint  -  {LengthFunc("#SuctionEndPipe.Diameter")}))");
      BaseBp.RuleList.AddRule("#SuctionEndPipe.Length", "(.SJ_POS_X-.SV_POS_X)-(#SuctionValveAPipe.Length*0.5)-#SuctionPostValveAFlangePipe.Length");

#else
      BaseBp.RuleList.AddRule("#DischargeEnd.MaxX", $"(#BasePump.MinX - .DischargeFilterToJoint  -  {LengthFunc("#DischargeEndPipe.Diameter")})");
      BaseBp.RuleList.AddRule("#SuctionEnd.MinX", $"(#BasePump.MaxX + .SuctionFilterToJoint  -  {LengthFunc("#SuctionEndPipe.Diameter")})");
#endif

      //  Discharge側(X < Filter) Valve 位置の調整      
      BaseBp.RegisterUserDefinedProperty("DV_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DV_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("DV_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      rule = BaseBp.RuleList.AddRule(".DV_MIN_X",
        ".DJ_POS_X+#DischargeEndPipe.MinLength+#DischargePostValveAFlangePipe.Length + (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");
#else
      rule = BaseBp.RuleList.AddRule(".DV_MIN_X",
        "#DischargeEnd.MaxX+#NextOfDischargeEndPipe.MinLength+#DischargePostValveAFlangePipe.Length + (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");
#endif
      rule = BaseBp.RuleList.AddRule(".DV_MAX_X", "#BasePump.MinX-.DV_MinLen+(#DischargePostValveAFlangePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".DV_POS_X", "Max(.DV_MIN_X,MIN(.DV_MAX_X,#BasePump.MinX-.DischargeFilterToValve ))");      
      rule = BaseBp.RuleList.AddRule("#DischargeValveA.PosX", ".DV_POS_X");      
  
      //  Suction側(X > Filter) Valve 位置の調整
      BaseBp.RegisterUserDefinedProperty("SV_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Suction側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("SV_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Suction側 Tee のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("SV_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)
#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      rule = BaseBp.RuleList.AddRule(".SV_MAX_X", ".SJ_POS_X- #SuctionEndPipe.MinLength-#SuctionPostValveAFlangePipe.Length-(#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
#else
      rule = BaseBp.RuleList.AddRule(".SV_MAX_X", "#SuctionEnd.MinX - #NextOfSuctionEndPipe.MinLength-#SuctionPostValveAFlangePipe.Length-(#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
#endif
      rule = BaseBp.RuleList.AddRule(".SV_MIN_X", "#BasePump.MaxX + .SV_MinLen - (#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
      rule = BaseBp.RuleList.AddRule(".SV_POS_X", "Max(.SV_MIN_X,MIN(.SV_MAX_X,#BasePump.MaxX+.SuctionFilterToValve ))");
      rule = BaseBp.RuleList.AddRule("#SuctionValveA.PosX", ".SV_POS_X");

      //  Custom rules
      BaseBp.RegisterUserDefinedProperty("SV_MinLen", PropertyType.TemporaryValue, 0.0);  //  BasePump - valve 間最短距離
      BaseBp.RegisterUserDefinedProperty("DV_MinLen", PropertyType.TemporaryValue, 0.0);  //  BasePump - valve 間最短距離
      _suctionValveStopperRule = new InjectorHookedRule(_suctionValveStopper.CheckSystemMinimumLength,"SV_MinLen");
      _dischargeValveStopperRule = new InjectorHookedRule(_dischargeValveStopper.CheckSystemMinimumLength,"DV_MinLen");

      var minLengthUpdater = FilterPipeLengthUpdater.Create(BpOwner, BaseBp, Info);

      double dlevel = info.SuctionDiameterNPSInch;
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;
      
      BaseBp.RegisterUserDefinedProperty("CopyDiameter",   PropertyType.TemporaryValue, 0.0);  //  pump（含まず）～valve（含む）
      rule = BaseBp.RuleList.AddRule(".CopyDiameter", "DebugLog(.SuctionDiameter)");

      var suctionProp = BaseBp.RegisterUserDefinedProperty( "SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch( (dlevel>=8)?6.0:8.0 ).NpsMm,
        diameterMinNpsMm, diameterMaxNpsMm ) ;

      suctionProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "SuctionEndPipe", 0 ) ) ) ;
      suctionProp.AddUserDefinedRule( new GenericHookedRule( minLengthUpdater.UpdateSuctionMinimumLengths) );
      suctionProp.AddUserDefinedRule(_suctionValveStopperRule);

      dlevel = info.DischargeDiameterNPSInch;
      var dischargeProp = BaseBp.RegisterUserDefinedProperty( "DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch( (dlevel>=8)?6.0:8.0 ).NpsMm,
        diameterMinNpsMm, diameterMaxNpsMm ) ;
      dischargeProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "DischargeEndPipe", 1 ) ) ) ;
      dischargeProp.AddUserDefinedRule( new GenericHookedRule( minLengthUpdater.UpdateDischargeMinimumLengths) );
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRule);

      if (info is SingleBlockPatternIndexInfoWithMirror infoDerived) {
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;

        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveA]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

 
        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveA]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
         edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault( info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;
#endif
      }
#endif
    }

  }
}
