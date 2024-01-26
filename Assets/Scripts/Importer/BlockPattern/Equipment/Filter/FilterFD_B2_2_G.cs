///<summary>
/// Flex 新システム仕様確定までの暫定処理を条件コンパイルにて実装
///</summary>
//#define INDEXING_NOW  //  Index 設定作業中にIndexを必要とする処理を実行しないようにする
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
  public class FilterFD_B2_2_G : FilterBase<BlockPatternWithMirror>
  {
    private FilterSystemPipeUtility _suctionValveStopperPreA;    //  pump（含まず）～valve-A（含む）
    private FilterSystemPipeUtility _suctionValveStopperPreB;    //  valve-A（含まず）～valve-B（含む）
    private FilterSystemPipeUtility _suctionValveStopperPost;    //  valve-B(含まず）～末端
    private FilterSystemPipeUtility _dischargeValveStopperFirst; //  pump（含まず）～Elb（含まず）
    private FilterSystemPipeUtility _dischargeValveStopperPreA;  //  Elb(含まず）～Valve-A（含む）
    private FilterSystemPipeUtility _dischargeValveStopperPreB;  //  Valve-A（含まず）～Valve-B(含む） 
    private FilterSystemPipeUtility _dischargeValveStopperPost;  //  valve-B（含まず）から末端  

    //  Elbの長さ（そんなものはない）を取ろうとすると正しく取れないので、分割して対処
    private IUserDefinedRule _suctionValveStopperRulePreA;      //  pump（含まず）～valve-A（含む）
    private IUserDefinedRule _suctionValveStopperRulePreB;      //  valve-A（含まず）～valve-B（含む）
    private IUserDefinedRule _suctionValveStopperRulePost;      //  valve-B(含まず）～末端

    private IUserDefinedRule _dischargeValveStopperRuleFirst;   //  pump（含まず）～Elb（含まず）
    private IUserDefinedRule _dischargeValveStopperRulePreA;    //  Elb(含まず）～Valve-A（含む）
    private IUserDefinedRule _dischargeValveStopperRulePreB;    //  Valve-A（含まず）～Valve-B(含む）
    private IUserDefinedRule _dischargeValveStopperRulePost;    //  valve-B（含まず）から末端  

    FilterPipeLengthUpdater _minLengthUpdater;

    FilterSystemLengthUpdater _dischargeSystemLength; //  Discharge Valve-B ～ Joint までの系の長さを調整するオブジェクト
    FilterSystemLengthUpdater _suctionSystemLength;   //  Suction Valve-B ～ Joint までの系の長さを調整するオブジェクト


    public FilterFD_B2_2_G( Document doc ) : base( doc, "FD-B2-2-G" )
    {
      Info = new SingleBlockPatternIndexInfoWithMirror
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range( 0, 1 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
#if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 24 },
#else
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 23 },
#endif
        },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.PreDischargeBOPSpacer,     3 }, //  生き
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeBOPSpacer,        4 }, //  生き

          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePreValveAFlange,  11 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveA,           12 },  //  valve本体
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePostValveAFlange, 13 },  //  joint側


          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePreValveBFlange,  19 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveB,           20 },  //  valve-B joint側本体
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePostValveBFlange, 21 },  //  joint側
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
#if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 20 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 21 },
#else
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 20 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },

#endif
        },
        SuctionAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPreValveAFlange, 16 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveA,          15 },  //  valve-A本体
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPostValveAFlange,14 }, //  joint側

          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPreValveBFlange,  8 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveB,           7 },  //  valve-B本体
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPostValveBFlange, 6 },  //  joint側
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
#if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 23 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 0 },
#else
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 22 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
#endif
        },

        //
        //  Pipe index helper
        //
#if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
        SuctionFlexHelper = new int[,] { { 0, 18 } },
        DischargeFlexHelper = new int[,]{ { 2, 2 },{ 6, 23 }   },       //  all discharge pipes (ただし、垂直のPipe 一つを除く)
        SuctionPipeIndexRange = new int[,]{ { 0, 18 }, },    //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,]{ { 2,23 }, },   //  all pipe
#else
        SuctionFlexHelper = new int[,] { { 9, 18 } },
        DischargeFlexHelper = new int[,]{ { 2, 2 },{ 6, 18 }   },       //  all discharge pipes (ただし、垂直のPipe 一つを除く)
        SuctionPipeIndexRange = new int[,]{ { 0, 18 }, },    //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,]{ { 2,23 }, },   //  all pipe
#endif

        SuctionDiameterNPSInch = 10,
        DischargeDiameterNPSInch = 10,
      } ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      ImportIdfAndEquipment() ;
      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }
      FilterIndexingHelper.BuildIndexList(BpOwner, BaseBp, Info);
      FilterPipeLengthUpdater.AssortMinimumLengths(BaseBp, Info);

      _suctionValveStopperPreA    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 20,15 );    //  pump（含まず）～valve-A（含む）
      _suctionValveStopperPreB    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 14, 7 );    //  valve-A（含まず）～valve-B（含む）
      _suctionValveStopperPost    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 6,  0 );    //  valve-B(含まず）～末端
      _dischargeValveStopperFirst = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex,  0, 2 );   //  pump（含まず）～Elb（含まず）
      _dischargeValveStopperPreA  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex,  6,12 );  //  Elb(含まず）～Valve-A（含む）
      _dischargeValveStopperPreB  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 13,20 );  //  Valve-A（含まず）～Valve-B(含む） 
      _dischargeValveStopperPost  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 21,23 );  //  valve-B（含まず）から末端  

      
      PostProcess();

      _dischargeSystemLength?.Activate(true);
      _suctionSystemLength?.Activate(true);
      
      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge)BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }

    protected override void PostProcess()
    {
      base.PostProcess();
      #if ! INDEXING_NOW
      var cbp = BpOwner;
      cbp.GetProperty("AccessSpace").Value = 2.3;
      cbp.GetProperty( "DischargeDiameter" ).Value = DiameterFactory.FromNpsInch(Info.DischargeDiameterNPSInch).NpsMm;
      cbp.GetProperty( "SuctionDiameter" ).Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;
      cbp.GetProperty( "BOP").Value = 0.5;
      cbp.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      cbp.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      cbp.GetProperty( "DischargeFilterToJoint").Value = 2.25;
      cbp.GetProperty( "SuctionFilterToJoint").Value = 2.7;
      cbp.GetProperty( "DischargeFilterToValve").Value = 1.6;
      cbp.GetProperty( "SuctionFilterToValve").Value = 0.6;
      #endif
    }

    protected override void RemoveExtraEdges( Group group, string file )
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if (file.Contains("-DIS(A)")){
          removeEdgeList = group.EdgeList.Reverse().Take(2).ToList();
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

        #if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
        var newpipe = GetExtraPipe(group, file, Info);
        if (newpipe != null){ 
          group.AddEdge(newpipe);
        }
        #endif
      }
    }

    #if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
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

    /// <summary>
    /// IDFにノズル側にフランジ形状が潰れてしまっているので特別に追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected override LeafEdge GetNozzleSideFlange( Group group, string file )
    {
      return null ;
    }
    
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


    protected override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      IRule rule;
      #if ! INDEXING_NOW
      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 1.2 ) ;
#if false
      bp.RegisterUserDefinedProperty( "PipeMinLength", PropertyType.Length, 0.5 ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
#endif

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

      BaseBp.RegisterUserDefinedProperty("DischargeFilterToJoint", PropertyType.Length, 2.850 ) ;
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValve", PropertyType.Length, 0.5);
      BaseBp.RegisterUserDefinedProperty("DischargeValveAToValveB", PropertyType.TemporaryValue, 0.0);
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToJoint", PropertyType.Length, 2.2 ) ;
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValve", PropertyType.Length, 0.5);
      BaseBp.RegisterUserDefinedProperty("SuctionValveAToValveB", PropertyType.TemporaryValue, 0.0);

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS

      _dischargeSystemLength = new FilterSystemLengthUpdater(info.DischargeIndex,23,22);
      _suctionSystemLength = new FilterSystemLengthUpdater(info.SuctionIndex, 5, 1, 0);

      var prop1 = BaseBp.RegisterUserDefinedProperty( "DischargeSystemLengthTemp", PropertyType.TemporaryValue, 1.0) ;
      if (_dischargeSystemLength != null)
        prop1.AddUserDefinedRule(new GenericHookedRule(_dischargeSystemLength.UpdateSystemLength));
      var prop2 = BaseBp.RegisterUserDefinedProperty( "SuctionSystemLengthTemp", PropertyType.TemporaryValue, 1.0) ;
      if (_suctionSystemLength != null)
        prop2.AddUserDefinedRule(new GenericHookedRule(_suctionSystemLength.UpdateSystemLength));

      BaseBp.RegisterUserDefinedProperty("DJ_MAX_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("DJ_POS_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("SJ_MIN_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("SJ_POS_X", PropertyType.TemporaryValue, 0.387);

      BaseBp.RuleList.AddRule(".DJ_MAX_X", 
        "#BasePump.MinX - .DVA_MinLen - .DVB_MinLen - .DVF_MinLen");

      BaseBp.RuleList.AddRule(".DJ_POS_X", $"Min(.DJ_MAX_X,(#BasePump.MinX - .DischargeFilterToJoint  +  {LengthFunc("#DischargeEndPipe.Diameter")}))");
      BaseBp.RuleList.AddRule(".DischargeSystemLengthTemp", "(.DVB_POS_X-DebugLog(.DJ_POS_X))-(#DischargeValveBPipe.Length*0.5)-#DischargePostValveBFlangePipe.Length");

      BaseBp.RuleList.AddRule(".SJ_MIN_X",
        "#BasePump.MaxX + .SVA_MinLen + .SVB_MinLen + .SVF_MinLen");

      BaseBp.RuleList.AddRule(".SJ_POS_X", $"Max(.SJ_MIN_X ,(#BasePump.MaxX + .SuctionFilterToJoint  -  {LengthFunc("#SuctionEndPipe.Diameter")}))");
      BaseBp.RuleList.AddRule(".SuctionSystemLengthTemp", "(.SJ_POS_X-.SVB_POS_X)-(#SuctionValveBPipe.Length*0.5)-#SuctionPostValveBFlangePipe.Length");

#else
      BaseBp.RuleList.AddRule("#DischargeEnd.MinX", $"(#BasePump.MinX - .DischargeFilterToJoint  -  {LengthFunc("#DischargeEndPipe.Diameter")})");
      BaseBp.RuleList.AddRule("#SuctionEnd.MaxX", $"(#BasePump.MaxX + .SuctionFilterToJoint  -  {LengthFunc("#SuctionEndPipe.Diameter")})");
#endif


      
      BaseBp.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0 ) ;
      BaseBp.RuleList.AddRule("#DischargeBOPSpacerPipe.Length", 
        $"#PreDischargeBOPSpacer.MinZ - ((#BasePump.MinZ + .BOP)+(#DischargeEndPipe.Diameter*0.5)+(DiameterToElbow90Length(#DischargeEndPipe.Diameter)))");

      //
      //  Filter to valve setting
      //
      //  Discharge側(X < Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("DVA_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".DVA_MIN_X",
        ".DJ_POS_X +.DVF_MinLen + .DVB_MinLen + (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".DVA_MAX_X",
        "#BasePump.MinX - .DVA_MinLen + (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".DVA_POS_X", "Max(.DVA_MIN_X,MIN(.DVA_MAX_X,#BasePump.MinX-.DischargeFilterToValve ))");
      rule = BaseBp.RuleList.AddRule("#DischargeValveA.PosX", ".DVA_POS_X");

      //  Discharge側(X < Filter) Valve B 位置の調整 
      BaseBp.RegisterUserDefinedProperty("DVB_MAX_X", PropertyType.TemporaryValue, 0.0);
      BaseBp.RegisterUserDefinedProperty("DVB_MIN_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("DVB_POS_X", PropertyType.TemporaryValue, 0.0);   

      rule = BaseBp.RuleList.AddRule(".DVB_MIN_X",
        ".DJ_POS_X + .DVF_MinLen + (#DischargeValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".DVB_MAX_X",
        ".DVA_POS_X - (#DischargeValveAPipe.Length*0.5) - .DVB_MinLen + (#DischargeValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".DVB_POS_X", "Max(.DVB_MIN_X,MIN(.DVB_MAX_X,#DischargeValveA.PosX-.DischargeValveAToValveB ))");
      rule = BaseBp.RuleList.AddRule("#DischargeValveB.PosX", ".DVB_POS_X");


      //  Suction側(X > Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("SVA_MAX_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("SVA_MIN_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("SVA_POS_X", PropertyType.TemporaryValue, 0.0);   

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".SVA_MAX_X",
        ".SJ_POS_X-.SVF_MinLen-.SVB_MinLen-(#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".SVA_MIN_X",
        "#BasePump.MaxX + .SVA_MinLen - (#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".SVA_POS_X", "Max(.SVA_MIN_X,MIN(.SVA_MAX_X,#BasePump.MaxX+.SuctionFilterToValve ))");
      rule = BaseBp.RuleList.AddRule("#SuctionValveA.PosX", ".SVA_POS_X");

      //  Discharge側(X < Filter) Valve B 位置の調整 
      BaseBp.RegisterUserDefinedProperty("SVB_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("SVB_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("SVB_POS_X", PropertyType.TemporaryValue, 0.0);   

      rule = BaseBp.RuleList.AddRule(".SVB_MAX_X",
        ".SJ_POS_X - .SVF_MinLen - (#SuctionValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".SVB_MIN_X",
        ".SVA_POS_X+(#SuctionValveAPipe.Length*0.5) + .SVB_MinLen - (#SuctionValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".SVB_POS_X", "Max(.SVB_MIN_X,MIN(.SVB_MAX_X,#SuctionValveA.PosX+.SuctionValveAToValveB ))");
      rule = BaseBp.RuleList.AddRule("#SuctionValveB.PosX", ".SVB_POS_X");

      //  Custom rules
      BaseBp.RegisterUserDefinedProperty("SVA_MinLen",   PropertyType.TemporaryValue, 0.0);  //  pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("SVB_MinLen",  PropertyType.TemporaryValue, 0.0);  //   valveA(含まず）～valveB（含む）
      BaseBp.RegisterUserDefinedProperty("SVF_MinLen",  PropertyType.TemporaryValue, 0.0);  //   valveA(含まず）～valveB（含む）

      BaseBp.RegisterUserDefinedProperty("DVA1_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端
      BaseBp.RegisterUserDefinedProperty("DVA2_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端
      BaseBp.RegisterUserDefinedProperty("DVA_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端
      BaseBp.RegisterUserDefinedProperty("DVB_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端
      BaseBp.RegisterUserDefinedProperty("DVF_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端

      _suctionValveStopperRulePreA= new InjectorHookedRule(_suctionValveStopperPreA.CheckSystemMinimumLength,"SVA_MinLen");       //  pump（含まず）～valve-A（含む）
      _suctionValveStopperRulePreB= new InjectorHookedRule(_suctionValveStopperPreB.CheckSystemMinimumLength,"SVB_MinLen");       //  valve-A（含まず）～valve-B（含む）
      _suctionValveStopperRulePost= new InjectorHookedRule(_suctionValveStopperPost.CheckSystemMinimumLength,"SVF_MinLen");       //  valve-B(含まず）～末端

      _dischargeValveStopperRuleFirst= new InjectorHookedRule(_dischargeValveStopperFirst.CheckSystemMinimumLength,"DVA1_MinLen");    //  pump（含まず）～Elb（含まず）
      _dischargeValveStopperRulePreA = new InjectorHookedRule(_dischargeValveStopperPreA.CheckSystemMinimumLength, "DVA2_MinLen");    //  Elb(含まず）～Valve-A（含む）
      _dischargeValveStopperRulePreB = new InjectorHookedRule(_dischargeValveStopperPreB.CheckSystemMinimumLength, "DVB_MinLen");     //  Valve-A（含まず）～Valve-B(含む）
      _dischargeValveStopperRulePost = new InjectorHookedRule(_dischargeValveStopperPost.CheckSystemMinimumLength, "DVF_MinLen");     //  valve-B（含まず）から末端  

      BaseBp.RuleList.AddRule(".DVA_MinLen",".DVA1_MinLen+.DVA2_MinLen + (2.0 * DiameterToElbow90Length(#DischargeEndPipe.Diameter) )");
      _minLengthUpdater = FilterPipeLengthUpdater.Create(BpOwner, BaseBp, Info);
      _minLengthUpdater.Activate(false);

      double dlevel = info.SuctionDiameterNPSInch;
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;

      var suctionProp = BaseBp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);
      suctionProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "SuctionEndPipe", 0 ) ) ) ;
      suctionProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateSuctionMinimumLengths) );
      suctionProp.AddUserDefinedRule(_suctionValveStopperRulePreA);
      suctionProp.AddUserDefinedRule(_suctionValveStopperRulePreB);
      suctionProp.AddUserDefinedRule(_suctionValveStopperRulePost);

      dlevel = info.DischargeDiameterNPSInch;
      var dischargeProp = BaseBp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);
      dischargeProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "DischargeEndPipe", 1 ) ) ) ;
      dischargeProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateDischargeMinimumLengths) );
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRuleFirst);
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRulePreA);
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRulePreB);
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRulePost);

      if (info is SingleBlockPatternIndexInfoWithMirror infoDerived) {
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;

        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveA]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveB]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveA]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveB]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;
#if ! DO_NOT_USE_FLEX_TO_MOVE_JOINTS
        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(info.DischargeIndexTypeValue[SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;
#endif
      }
      #endif
    }
  }
}
