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
  public class FilterFD_B2_2_S : FilterBase<BlockPatternWithMirror>
  {
    private FilterSystemPipeUtility _suctionValveStopperA;    //  pump（含まず）～valve-A（含む）
    private FilterSystemPipeUtility _suctionValveStopperB;    //  valve(含まず）～末端
    private FilterSystemPipeUtility _dischargeValveStopperA;  //  pump（含まず）～Elb（含まず）
    private FilterSystemPipeUtility _dischargeValveStopperB;  //  elp（含まず）～Valve(含む） 

    private IUserDefinedRule _suctionValveStopperRuleA;       //  pump（含まず）～valve（含む）
    private IUserDefinedRule _suctionValveStopperRuleB;       //  valve(含まず）～末端
    private IUserDefinedRule _dischargeValveStopperRuleA;     //  pump（含まず）～Elb（含まず）
    private IUserDefinedRule _dischargeValveStopperRuleB;     //  elp（含まず）～Valve(含む） 

    FilterPipeLengthUpdater _minLengthUpdater;

    public FilterFD_B2_2_S( Document doc ) : base( doc, "FD-B2-2-S" )
    {
      Info = new SingleBlockPatternIndexInfoWithMirror
      {
        DischargeIndex = 0,
        SuctionIndex = 1  ,
        BasePumpIndex = 0,
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 16 },
#else
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 15 },
#endif
        },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePreValveAFlange,   4 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveA,            5 },  //  valve-A pump側本体
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePostValveAFlange,  6 },  //  joint側

          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePreValveBFlange,  12 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargeValveB,           13 },  //  valve-B joint側本体
          { SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType.DischargePostValveBFlange, 14 },  //  joint側
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 15 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 16 },
#else
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 15 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
#endif
        },
        SuctionAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionSpacer1,    12 },
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionSpacer2,    13 },
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionDistanceOrigin,14 },

          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPreValveAFlange, 11 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveA,          10 },  //  valve-A本体
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPostValveAFlange, 9 },  //  joint側

          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPreValveBFlange,  3 },  //  pump側
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionValveB,           2 },  //  valve-B本体
          { SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType.SuctionPostValveBFlange, 1 },  //  joint側

        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 15 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 0 },
#else
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, -1 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, -1 },
#endif
        },

        //
        //  Pipe index helper
        //
#if !DO_NOT_USE_FLEX_TO_MOVE_JOINTS
        SuctionFlexHelper = new int[,] { { 0, 13 } },
        DischargeFlexHelper = new int[,]{ { 2, 15 }   },     
#else
        SuctionFlexHelper = new int[,] { { 4, 13 } },
        DischargeFlexHelper = new int[,]{ { 2, 11 }   },     
#endif
        SuctionPipeIndexRange = new int[,]{ { 0, 13 }, },    //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,]{ { 2,15 }, },   //  all pipe

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
      
      FilterIndexingHelper.BuildIndexList(BpOwner, BaseBp, Info);
      FilterPipeLengthUpdater.AssortMinimumLengths(BaseBp, Info);

      _suctionValveStopperA　　  = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 15,10 );  //  pump（含まず）～valveA（含む）
      _suctionValveStopperB      = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex,  9, 2   );  //  valveA(含まず）～valveB（含む）
      _dischargeValveStopperA     = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 0, 5 ); //  pump（含まず）～valveA（含む）
      _dischargeValveStopperB     = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 6, 13);  // valveA（含まず）～ValveB(含む） 

      PostProcess();
      
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

      var cbp = BpOwner;
      #if ! INDEXING_NOW
      cbp.GetProperty( "DischargeDiameter" ).Value = DiameterFactory.FromNpsInch(Info.DischargeDiameterNPSInch).NpsMm;
      cbp.GetProperty( "SuctionDiameter" ).Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;
      cbp.GetProperty("DischargeFilterToJoint").Value = 1.0;
      cbp.GetProperty("SuctionFilterToJoint").Value = 1.0;
      cbp.GetProperty("DischargeFilterToValve").Value = 0.5;
      cbp.GetProperty("SuctionFilterToValve").Value = 0.5;

      //cbp.GetProperty("BOP").Value = 0.5;
      cbp.GetProperty("AccessSpace").Value = 2.0;
      cbp.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      cbp.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      cbp.GetProperty("AccessSpace").Value = 0.7;
      #endif
    }
    protected override void RemoveExtraEdges( Group group, string file )
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if ( file.Contains( "-DIS(A)" ) ) {
          removeEdgeList = group.EdgeList.Reverse().Take( 3 ).ToList() ;
        }
        else if ( file.Contains( "-SUC(A)" ) ) {
          removeEdgeList = group.EdgeList.Take( 3 ).ToList() ;
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
    /// <remarks>Suction 側は0に追加 Discharge側はNextOfDischargeEnd に追加するので注意</remarks>
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
#if true//! INDEXING_NOW
      IRule rule;
      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 1.2 ) ;

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
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValve", PropertyType.Length, 0.387);
      BaseBp.RegisterUserDefinedProperty("DischargeValveAToValveB", PropertyType.TemporaryValue, 0.0);
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToJoint", PropertyType.Length, 2.2 ) ;
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValve", PropertyType.Length, 0.387);
      BaseBp.RegisterUserDefinedProperty("SuctionValveAToValveB", PropertyType.TemporaryValue, 0.0);

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      BaseBp.RegisterUserDefinedProperty("DJ_MAX_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("DJ_POS_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("SJ_MIN_X", PropertyType.TemporaryValue, 0.387);
      BaseBp.RegisterUserDefinedProperty("SJ_POS_X", PropertyType.TemporaryValue, 0.387);

      BaseBp.RuleList.AddRule(".DJ_MAX_X",
        "#BasePump.MinX - .DVA_MinLen - .DVB_MinLen - .DVC_MinLen");
      BaseBp.RuleList.AddRule(".DJ_POS_X", $"Min(.DJ_MAX_X,(#BasePump.MinX - .DischargeFilterToJoint  +  {LengthFunc("#DischargeEndPipe.Diameter")}))");

      BaseBp.RuleList.AddRule("#DischargeEndPipe.Length", "(.DVB_POS_X-.DJ_POS_X)-(#DischargeValveBPipe.Length*0.5)-#DischargePostValveBFlangePipe.Length");

      BaseBp.RuleList.AddRule(".SJ_MIN_X",
        "#BasePump.MaxX + .SVA_MinLen + .SVB_MinLen + .SVC_MinLen");

      BaseBp.RuleList.AddRule(".SJ_POS_X", $"Max(.SJ_MIN_X,(#BasePump.MaxX + .SuctionFilterToJoint  -  {LengthFunc("#SuctionEndPipe.Diameter")}))");
      BaseBp.RuleList.AddRule("#SuctionEndPipe.Length", "(.SJ_POS_X-.SVB_POS_X)-(#SuctionValveBPipe.Length*0.5)-#SuctionPostValveBFlangePipe.Length");
#else
      BaseBp.RuleList.AddRule("#DischargeEnd.MinX", $"(#BasePump.MinX - .DischargeFilterToJoint  -  {LengthFunc("#DischargeEndPipe.Diameter")})");

      BaseBp.RuleList.AddRule("#SuctionEnd.MaxX", $"(#BasePump.MaxX + .SuctionFilterToJoint  -  {LengthFunc("#SuctionEndPipe.Diameter")})");
#endif
      //
      //  Filter to valve setting
      //
      //  Discharge側(X < Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("DVA_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVC_MinLen", PropertyType.TemporaryValue, 0.0);  // Valve-B(含まず)～末端のMinLength

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      rule = BaseBp.RuleList.AddRule(".DVC_MinLen", "#DischargeEndPipe.MinLength +(#DischargePostValveBFlangePipe.Length)");

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".DVA_MIN_X",
        ".DJ_POS_X +.DVC_MinLen+.DVB_MinLen+ (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");
#else
      rule = BaseBp.RuleList.AddRule(".DVC_MinLen", "#NextOfDischargeEndPipe.MinLength +(#DischargePostValveBFlangePipe.Length)");

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".DVA_MIN_X",
        "#DischargeEnd.MaxX+.DVC_MinLen+.DVB_MinLen+ (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");
#endif
      rule = BaseBp.RuleList.AddRule(".DVA_MAX_X",
        "#BasePump.MinX - .DVA_MinLen + (#DischargeValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule("#DischargeValveA.PosX", "Max(.DVA_MIN_X,MIN(.DVA_MAX_X,#BasePump.MinX-.DischargeFilterToValve ))");

      //  Discharge側(X < Filter) Valve B 位置の調整 
      BaseBp.RegisterUserDefinedProperty("DVB_MAX_X", PropertyType.TemporaryValue, 0.0);
      BaseBp.RegisterUserDefinedProperty("DVB_MIN_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("DVB_POS_X", PropertyType.TemporaryValue, 0.0);   

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      rule = BaseBp.RuleList.AddRule(".DVB_MIN_X",
        ".DJ_POS_X + .DVC_MinLen + (#DischargeValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");
#else
      rule = BaseBp.RuleList.AddRule(".DVB_MIN_X",
        "#DischargeEnd.MaxX + .DVC_MinLen + (#DischargeValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");
#endif
      rule = BaseBp.RuleList.AddRule(".DVB_MAX_X",
        "#DischargeValveA.MinX - .DVB_MinLen + (#DischargeValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".DVB_POS_X", "Max(.DVB_MIN_X,MIN(.DVB_MAX_X,#DischargeValveA.PosX-.DischargeValveAToValveB ))");
      rule = BaseBp.RuleList.AddRule("#DischargeValveB.PosX", ".DVB_POS_X");

      //  Suction側(X > Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("SVA_MAX_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("SVA_MIN_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("SVC_MinLen", PropertyType.TemporaryValue, 0.0);  

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      rule = BaseBp.RuleList.AddRule(".SVC_MinLen", "#SuctionEndPipe.MinLength +#SuctionPostValveBFlangePipe.Length");

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".SVA_MAX_X",
        ".SJ_POS_X-.SVC_MinLen-.SVB_MinLen-(#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
#else
      rule = BaseBp.RuleList.AddRule(".SVC_MinLen", "#NextOfSuctionEndPipe.MinLength +#SuctionPostValveBFlangePipe.Length");

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".SVA_MAX_X",
        "#SuctionEnd.MinX-.SVC_MinLen-.SVB_MinLen-(#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
#endif
      rule = BaseBp.RuleList.AddRule(".SVA_MIN_X",
        "#BasePump.MaxX + .SVA_MinLen - (#SuctionValveAPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");

      rule = BaseBp.RuleList.AddRule("#SuctionValveA.PosX", "Max(.SVA_MIN_X,MIN(.SVA_MAX_X,#BasePump.MaxX+.SuctionFilterToValve ))");

      //  Discharge側(X < Filter) Valve B 位置の調整 
      BaseBp.RegisterUserDefinedProperty("SVB_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("SVB_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("SVB_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)

#if DO_NOT_USE_FLEX_TO_MOVE_JOINTS
      rule = BaseBp.RuleList.AddRule(".SVB_MAX_X",
        ".SJ_POS_X - .SVC_MinLen - (#SuctionValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
#else
      rule = BaseBp.RuleList.AddRule(".SVB_MAX_X",
        "#SuctionEnd.MinX - .SVC_MinLen - (#SuctionValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");
#endif
      rule = BaseBp.RuleList.AddRule(".SVB_MIN_X",
        "#SuctionValveA.MaxX + .SVB_MinLen - (#SuctionValveBPipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SuctionEndPipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToJoint");

      rule = BaseBp.RuleList.AddRule(".SVB_POS_X", "Max(.SVB_MIN_X,MIN(.SVB_MAX_X,#SuctionValveA.PosX+.SuctionValveAToValveB ))");
      rule = BaseBp.RuleList.AddRule("#SuctionValveB.PosX", ".SVB_POS_X");

      //  Custom rules
      BaseBp.RegisterUserDefinedProperty("SVA_MinLen",   PropertyType.TemporaryValue, 0.0);  //  pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("SVB_MinLen",  PropertyType.TemporaryValue, 0.0);  //   valveA(含まず）～valveB（含む）
      BaseBp.RegisterUserDefinedProperty("DVA_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端
      BaseBp.RegisterUserDefinedProperty("DVB_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valve(含まず）～末端

      _suctionValveStopperRuleA    = new InjectorHookedRule(_suctionValveStopperA.CheckSystemMinimumLength,"SVA_MinLen");     //  pump（含まず）～valve（含む）
      _suctionValveStopperRuleB    = new InjectorHookedRule(_suctionValveStopperB.CheckSystemMinimumLength,"SVB_MinLen");    //  valve(含まず）～末端

      _dischargeValveStopperRuleA  = new InjectorHookedRule(_dischargeValveStopperA.CheckSystemMinimumLength,"DVA_MinLen"); //  pump（含まず）～Elb（含まず）
      _dischargeValveStopperRuleB  = new InjectorHookedRule(_dischargeValveStopperB.CheckSystemMinimumLength,"DVB_MinLen"); //  elp（含まず）～Valve(含む） 

      _minLengthUpdater = FilterPipeLengthUpdater.Create(BpOwner, BaseBp, Info);
      _minLengthUpdater.Activate(false);

      double dlevel = info.SuctionDiameterNPSInch;
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;

      var suctionProp = BaseBp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);
      suctionProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "SuctionEndPipe", 0 ) ) ) ;
      suctionProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateSuctionMinimumLengths) );
      suctionProp.AddUserDefinedRule(_suctionValveStopperRuleA);
      suctionProp.AddUserDefinedRule(_suctionValveStopperRuleB);

      dlevel = info.DischargeDiameterNPSInch;
      var dischargeProp = BaseBp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);
      dischargeProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "DischargeEndPipe", 1 ) ) ) ;
      dischargeProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateDischargeMinimumLengths) );
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRuleA);
      dischargeProp.AddUserDefinedRule(_dischargeValveStopperRuleB);

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
