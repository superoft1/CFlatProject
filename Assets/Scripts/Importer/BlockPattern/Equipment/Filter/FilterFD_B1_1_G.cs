//#define INDEXING_NOW
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
  public class FilterFD_B1_1_G : FilterBase<BlockPatternArray>
  {
    private FilterSystemPipeUtility _suctionValveStopperPreA;    //  pump本体（含まず）～valve-A（含む）
    private FilterSystemPipeUtility _suctionValveStopperPreB;    //  valve-A（含まず）～valve-B（含む）
    private FilterSystemPipeUtility _suctionValveStopperPost;    //  valve-B(含まず）～末端
    private FilterSystemPipeUtility _dischargeValveStopperFirst; //  pump本体（含まず）～Elb（含まず）
    private FilterSystemPipeUtility _dischargeValveStopperPreA;  //  Elb(含まず）～Valve-A（含む）
    private FilterSystemPipeUtility _dischargeValveStopperPreB;  //  Valve-A（含まず）～Valve-B(含む） 
    //private FilterSystemPipeUtility _dischargeValveStopperPost;  //  valve-B（含まず）から末端  

    //  Elbの長さ（そんなものはない）を取ろうとすると正しく取れないので、分割して対処
    private IUserDefinedRule _suctionValveStopperRulePreA;      //  pump本体（含まず）～valve-A（含む）
    private IUserDefinedRule _suctionValveStopperRulePreB;      //  valve-A（含まず）～valve-B（含む）
    private IUserDefinedRule _suctionValveStopperRulePost;      //  valve-B(含まず）～末端

    private IUserDefinedRule _dischargeValveStopperRuleFirst;   //  pump本体（含まず）～Elb（含まず）
    private IUserDefinedRule _dischargeValveStopperRulePreA;    //  Elb(含まず）～Valve-A（含む）
    private IUserDefinedRule _dischargeValveStopperRulePreB;    //  Valve-A（含まず）～Valve-B(含む）
    //private IUserDefinedRule _dischargeValveStopperRulePost;    //  valve-B（含まず）から末端  

    FilterPipeLengthUpdater _minLengthUpdater;

    Dictionary<string, double> _defaults;


    public FilterFD_B1_1_G( Document doc ) : base( doc, "FD-B1-1-G" )
    {
      Info = new SingleFilterPatternInfo
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength1, -1 }, // TODO MinLengthは一旦設定しない
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength2, -1 }, // TODO MinLengthは一旦設定しない
        },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleFilterPatternInfo.DischargeAdditionalIndexType, int>
        {
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPostNozzlePipe,  0 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisFlangeA,         1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPipeB,           2 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisElbowA0Elbow,    3 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisElbowA1Pipe,     4 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisElbowA2Elbow,    5 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPipeC,           6 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreValveAPipe,  10 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA0Flange,  11 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA1Valve,   12 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA2Flange,  13 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPostValveAPipe, 14 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreValveBPipe,  18 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB0Flange,  19 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB1Valve,   20 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB2Flange,  21 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreTeePipe,     22 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee,            23 },
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, -1 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, -1 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionMinLength1, -1 }, // TODO MinLengthは一旦設定しない
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionMinLength2, -1 }, // TODO MinLengthは一旦設定しない
        },
        SuctionAdditionalIndexTypeValue = new Dictionary<SingleFilterPatternInfo.SuctionAdditionalIndexType, int>
        {
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostNozzlePipe,      32 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainFlangeA,         31 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainPipeB,           30 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreMainValveAPipe,   29 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA0Flange,   28 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve,    27 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA2Flange,   26 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostMainValveA,      25 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreMainValveBPipe,   21 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB0Flange,   20 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB1Valve,    19 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB2Flange,   18 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainPipeF,           17 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreTeePipe,          13 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucTee,                  0 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeA,          1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA0Elbow,   2 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA1Pipe,    3 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA2Elbow,   4 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeA,          5 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeB,          6 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeC,         10 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassValveA0Flange,  7 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassValveA1Valve,   8 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassValveA2Flange,  9 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowB,        11 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeB,         12 },
        },
        NextOfIndexTypeValue = new Dictionary<SingleFilterPatternInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, -1 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, -1 },
        },
        //
        //  Pipe index helper
        //
        SuctionFlexHelper = new int[,] { { 12, 5 },{ 13, 30 } },
        DischargeFlexHelper = new int[,]{ { 2, 2 },{ 6, 23 }   },  //  all discharge pipes (ただし、垂直のPipe 一つを除く)
        SuctionPipeIndexRange = new int[,]{ { 12, 30 }, },        //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,]{ { 2,22 }, },        //  all pipe

        SuctionDiameterNPSInch = 10,
        DischargeDiameterNPSInch = -1,
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

      _suctionValveStopperPreA    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 32,27 );    //  pump（含まず）～valve-A（含む）
      _suctionValveStopperPreB    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 26,19 );    //  valve-A（含まず）～valve-B（含む）
      _suctionValveStopperPost    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 18,13 );    //  valve-B(含まず）～末端（Tee含まず）
      _dischargeValveStopperFirst = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex,  0, 2 );   //  pump（含まず）～Elb（含まず）
      _dischargeValveStopperPreA  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex,  6,12 );  //  Elb(含まず）～Valve-A（含む）
      _dischargeValveStopperPreB  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 13,20 );  //  Valve-A（含まず）～Valve-B(含む） 
      //_dischargeValveStopperPost  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 21,22 );  //  valve-B（含まず）から末端（Tee含まず）

      PostProcess();

      onFinish?.Invoke( (BlockEdge)BpOwner ?? BaseBp ) ;

      var cbp = BpOwner;
      #if !INDEXING_NOW
      cbp.GetProperty( "PipeDiameter" ).Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;

      cbp.GetProperty("DischargeFilterToBranch").Value = _defaults["DischargeFilterToBranch"];
      cbp.GetProperty("DischargeFilterToValve").Value = 1.6;
      cbp.GetProperty("SuctionFilterToBranch").Value = _defaults["SuctionFilterToBranch"];
      cbp.GetProperty("SuctionFilterToValve").Value = 0.5;
      #endif
      
      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DisTee", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SucTee", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;
      
      return BaseBp ;
    }

    protected override void RemoveExtraEdges( Group group, string file )
    {
    }

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
    
    //protected override bool SelectIdf( string idf )
    //{
    //  return true ;
    //}

    protected LeafEdge GetLeafEdge(IGroup group, int edgeIndex)
    {
      return GetEdge(group, edgeIndex) as LeafEdge;
    }

    protected Dictionary<string, double>GetDefaultValues(SingleBlockPatternIndexInfo info)
    {
      var dischargeGroup = GetGroup(info.DischargeIndex);
      var suctionGroup = GetGroup(info.SuctionIndex);

      Dictionary<string, double> values = new Dictionary<string, double>();

      var DisPostNozzlePipe = GetLeafEdge(dischargeGroup, 0);
      var DisPreValveAPipe = GetLeafEdge(dischargeGroup, 10);
      var DisPostValveAPipe = GetLeafEdge(dischargeGroup, 14);
      var DisPreValveBPipe = GetLeafEdge(dischargeGroup, 18);

      var SucPostNozzlePipe = GetLeafEdge(suctionGroup, 32);
      var SucBypassPipeA = GetLeafEdge(suctionGroup, 5);
      var SucBranchPipeA = GetLeafEdge(suctionGroup, 1);
      var SucBranchPipeB = GetLeafEdge(suctionGroup, 12);
      var SucPreMainValveAPipe = GetLeafEdge(suctionGroup, 29);
      var SucPreMainValveBPipe = GetLeafEdge(suctionGroup, 21);
      var SucPostMainValveA = GetLeafEdge(suctionGroup, 25);

      values.Add(
        "MainToBypass",
        SucBypassPipeA.GetProperty("MinY").Value - SucPostNozzlePipe.GetProperty("MaxY").Value
      );

      values.Add(
        "DischargeFilterToBranch",
        DisPostNozzlePipe.GetProperty("MaxX").Value - SucBranchPipeB.GetProperty("MaxX").Value
      );

      values.Add(
        "DischargeFilterToValve",
        DisPostNozzlePipe.GetProperty("maxX").Value - DisPreValveAPipe.GetProperty("MinX").Value
      );

      values.Add(
        "DischargeValveToValve",
        DisPostValveAPipe.GetProperty("MaxX").Value - DisPreValveBPipe.GetProperty("MinX").Value
      );

      values.Add(
        "SuctionFilterToBranch",
        SucBranchPipeA.GetProperty("MinX").Value - SucPostNozzlePipe.GetProperty("MinX").Value
      );

      values.Add(
        "SuctionFilterToValve",
        SucPreMainValveAPipe.GetProperty("MaxX").Value - SucPostNozzlePipe.GetProperty("MinX").Value
      );

      values.Add(
        "SuctionValveToValve",
        SucPreMainValveBPipe.GetProperty("MaxX").Value - SucPostMainValveA.GetProperty("MinX").Value
      );

      return values;
    }

    protected override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      _defaults = GetDefaultValues(Info);

      #if !INDEXING_NOW
      //
      // Properties
      //

      BaseBp.RegisterUserDefinedProperty("MainToBypass", PropertyType.Length, _defaults["MainToBypass"]);  // 0.966
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToBranch", PropertyType.Length, _defaults["DischargeFilterToBranch"]+1.0); // 3.039
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValve", PropertyType.Length, _defaults["DischargeFilterToValve"]);  // 1.387
      BaseBp.RegisterUserDefinedProperty("DischargeValveToValve", PropertyType.TemporaryValue, 0.01/*_defaults["DischargeValveToValve"]*/);   // 0.5
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToBranch", PropertyType.Length, _defaults["SuctionFilterToBranch"]+1.0); // 2.372
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValve", PropertyType.Length, _defaults["SuctionFilterToValve"]);  // 0.526
      BaseBp.RegisterUserDefinedProperty("SuctionValveToValve", PropertyType.TemporaryValue, 0.01/*_defaults["SuctionValveToValve"]*/);   // 0.344
      BaseBp.RegisterUserDefinedProperty("BOP", PropertyType.Length, 0.5);


#if true
      IRule rule;
      /*
      BaseBp.RegisterUserDefinedProperty("DischargeJointDistance", PropertyType.Length, 2.850 ) ;
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValveA", PropertyType.Length, 0.387);
      BaseBp.RegisterUserDefinedProperty("DischargeValveAToValveB", PropertyType.Length, 0.387);
      BaseBp.RegisterUserDefinedProperty("SuctionJointDistance", PropertyType.Length, 2.2 ) ;
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValveA", PropertyType.Length, 0.387);
      BaseBp.RegisterUserDefinedProperty("SuctionValveAToValveB", PropertyType.Length, 0.387);
      */

      BaseBp.RuleList.AddRule("#DisElbowA1PipePipe.Length", 
        $"DebugLog(#DisElbowA0Elbow.MinZ - ((#BasePump.MinZ + .BOP)+(#DisElbowA1PipePipe.Diameter*0.5)+(DiameterToElbow90Length(#DisElbowA1PipePipe.Diameter))))");
      BaseBp.RuleList.AddRule("#SucBranchElbowA1PipePipe.Length", 
        $"DebugLog(#SucBranchElbowA0Elbow.MinZ - ((#BasePump.MinZ + .BOP)+(#SucBranchElbowA1PipePipe.Diameter*0.5)+(DiameterToElbow90Length(#SucBranchElbowA1PipePipe.Diameter))))");

      BaseBp.RegisterUserDefinedProperty("MTB_Len", PropertyType.TemporaryValue, 0.0);  // 0.962

      BaseBp.RuleList.AddRule(
        ".MTB_Len", "#SucPreTeePipe.MaxY+.MainToBypass-DiameterToElbow90Length(#SucPreTeePipePipe.Diameter)-(#SucPreTeePipePipe.Diameter*0.5)-#SucTee.MaxY");
      BaseBp.RuleList.AddRule(
        "#SucBranchPipeAPipe.Length", ".MTB_Len");
      BaseBp.RuleList.AddRule(
        "#SucBranchPipeBPipe.Length", ".MTB_Len");

      BaseBp.RegisterUserDefinedProperty("DVC_MinLen", PropertyType.TemporaryValue, 0.0);  // Valve-B(含まず)～末端のMinLength
      rule = BaseBp.RuleList.AddRule(".DVC_MinLen", "#DisPreTeePipePipe.MinLength +(#DisValveB2FlangePipe.Length)");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");

      BaseBp.RegisterUserDefinedProperty("DT_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DT_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RuleList.AddRule(".DT_MAX_X", "#BasePump.MinX - (.DVA_MinLen+.DVB_MinLen+.DVC_MinLen)");
      
      BaseBp.RuleList.AddRule(".DT_POS_X", "#BasePump.MinX - .DischargeFilterToBranch  + ( ( DiameterToTeeMainLength(#DisPreTeePipePipe.Diameter,#SucBranchPipeBPipe.Diameter)-#DisPreTeePipePipe.Diameter ) * 0.5)");
      BaseBp.RuleList.AddRule("#DisTee.MaxX", "Min( .DT_MAX_X,.DT_POS_X)");

      BaseBp.RuleList.AddRule("#SucTee.MinX", $"#BasePump.MaxX + .SuctionFilterToBranch  -  ((DiameterToTeeMainLength(#SucPreTeePipePipe.Diameter,#SucBranchPipeAPipe.Diameter)-#SucPreTeePipePipe.Diameter)*0.5)");


      //
      //  Filter to valve setting
      //
      //  Discharge側(X < Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("DVA_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Valve-A のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Valve-A のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Valve-A のfar 死点(X)

      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".DVA_MIN_X",
        "#DisTee.MaxX+.DVC_MinLen+.DVB_MinLen+ (#DisValveA1ValvePipe.Length*0.5)+(.DischargeFilterToValve*0.0)");
      rule.AddTriggerSourcePropertyName("DisPreTeePipePipe","Diameter");


      rule = BaseBp.RuleList.AddRule(".DVA_MAX_X",
        "#BasePump.MinX - .DVA_MinLen + (#DisValveA1ValvePipe.Length*0.5)+(.DischargeFilterToValve*0.0)");
      rule.AddTriggerSourcePropertyName("DisPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToValve");

      rule = BaseBp.RuleList.AddRule(".DVA_POS_X", "Max(.DVA_MIN_X,MIN(.DVA_MAX_X,#BasePump.MinX-.DischargeFilterToValve ))");

      //rule = BaseBp.RuleList.AddRule("#DisValveA1Valve.PosX", ".DVA_MIN_X");

      //  Discharge側(X < Filter) Valve B 位置の調整 
      BaseBp.RegisterUserDefinedProperty("DVB_MAX_X", PropertyType.TemporaryValue, 0.0);  //  Discharge側 Valve-B のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVB_MIN_X", PropertyType.TemporaryValue, 0.0);  //  Discharge側 Valve-B のfar 死点(X) 

      rule = BaseBp.RuleList.AddRule(".DVB_MIN_X",
        "#DisTee.MaxX + .DVC_MinLen + (#DisValveB1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DisPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToBranch");

      rule = BaseBp.RuleList.AddRule(".DVB_MAX_X",
        ".DVA_POS_X- (#DisValveA1ValvePipe.Length*0.5) - .DVB_MinLen + (#DisValveB1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DisPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToBranch");

      //rule = BaseBp.RuleList.AddRule("#DisValveB1Valve.PosX", "Max(.DVB_MIN_X,MIN(.DVB_MAX_X,.DVA_POS_X-.DischargeValveToValve ))");
      //rule = BaseBp.RuleList.AddRule("#DisValveB1Valve.PosX", "DebugLog(.DVB_MAX_X)");
      rule = BaseBp.RuleList.AddRule("#DisValveB1Valve.PosX", "(.DVA_POS_X+0.0)- (#DisValveA1ValvePipe.Length*0.5) - .DVB_MinLen + (#DisValveB1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DischargeFilterToValve");

      rule = BaseBp.RuleList.AddRule("#DisValveA1Valve.PosX", ".DVA_POS_X");  //  Bを動かしてからAを動かす方が制約がないと仮定

      //  Suction側(X > Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("SVA_MAX_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("SVA_MIN_X", PropertyType.TemporaryValue, 0.0);   


      //  BとAとではAを優先する
      rule = BaseBp.RuleList.AddRule(".SVA_MAX_X",
        "#SucTee.MinX-.SVF_MinLen-.SVB_MinLen-(#SucMainValveA1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SucPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToBranch");

      rule = BaseBp.RuleList.AddRule(".SVA_MIN_X",
        "#BasePump.MaxX + .SVA_MinLen - (#SucMainValveA1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SucPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToBranch");

      rule = BaseBp.RuleList.AddRule("#SucMainValveA1Valve.PosX", "Max(.SVA_MIN_X,MIN(.SVA_MAX_X,#BasePump.MaxX+.SuctionFilterToValve ))");

      //  Discharge側(X < Filter) Valve B 位置の調整 
      BaseBp.RegisterUserDefinedProperty("SVB_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("SVB_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のfar 死点(X)

      rule = BaseBp.RuleList.AddRule(".SVB_MAX_X",
        "#SucTee.MinX - .SVF_MinLen - (#SucMainValveB1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SucPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToBranch");

      rule = BaseBp.RuleList.AddRule(".SVB_MIN_X",
        "#SucMainValveA1Valve.MaxX + .SVB_MinLen - (#SucMainValveB1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SucPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToBranch");

      rule = BaseBp.RuleList.AddRule("#SucMainValveB1Valve.PosX", "Max(.SVB_MIN_X,MIN(.SVB_MAX_X,#SucMainValveA1Valve.PosX+.SuctionValveToValve ))");     

      //  Custom rules
      BaseBp.RegisterUserDefinedProperty("SVA_MinLen",   PropertyType.TemporaryValue, 0.0);  //  pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("SVB_MinLen",  PropertyType.TemporaryValue, 0.0);  //   valveA(含まず）～valveB（含む）
      BaseBp.RegisterUserDefinedProperty("SVF_MinLen",  PropertyType.TemporaryValue, 0.0);  //   valveB(含まず）～末端(Teeなし)

      BaseBp.RegisterUserDefinedProperty("DVA1_MinLen",   PropertyType.TemporaryValue, 0.0);  //  部分 pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("DVA2_MinLen",   PropertyType.TemporaryValue, 0.0);  //  部分 pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("DVA_MinLen",   PropertyType.TemporaryValue, 0.0);  //  合成 pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("DVB_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valveA(含まず）～valveB（含む
      //BaseBp.RegisterUserDefinedProperty("DVF_MinLen",   PropertyType.TemporaryValue, 0.0);  //  valveB(含まず）～末端(Teeなし)

      _suctionValveStopperRulePreA= new InjectorHookedRule(_suctionValveStopperPreA.CheckSystemMinimumLength,"SVA_MinLen");       //  pump（含まず）～valve-A（含む）
      _suctionValveStopperRulePreB= new InjectorHookedRule(_suctionValveStopperPreB.CheckSystemMinimumLength,"SVB_MinLen");       //  valve-A（含まず）～valve-B（含む）
      _suctionValveStopperRulePost= new InjectorHookedRule(_suctionValveStopperPost.CheckSystemMinimumLength,"SVF_MinLen");       //  valve-B(含まず）～末端

      _dischargeValveStopperRuleFirst= new InjectorHookedRule(_dischargeValveStopperFirst.CheckSystemMinimumLength,"DVA1_MinLen");    //  pump（含まず）～Elb（含まず）
      _dischargeValveStopperRulePreA = new InjectorHookedRule(_dischargeValveStopperPreA.CheckSystemMinimumLength, "DVA2_MinLen");    //  Elb(含まず）～Valve-A（含む）
      _dischargeValveStopperRulePreB = new InjectorHookedRule(_dischargeValveStopperPreB.CheckSystemMinimumLength, "DVB_MinLen");     //  Valve-A（含まず）～Valve-B(含む）
      //_dischargeValveStopperRulePost = new InjectorHookedRule(_dischargeValveStopperPost.CheckSystemMinimumLength, "DVF_MinLen");     //  valve-B（含まず）から末端  

      BaseBp.RuleList.AddRule(".DVA_MinLen",".DVA1_MinLen+.DVA2_MinLen + (2.0 * DiameterToElbow90Length(#DisPreTeePipePipe.Diameter) )");
      _minLengthUpdater = FilterPipeLengthUpdater.Create(BpOwner, BaseBp, Info);
      _minLengthUpdater.Activate(false);

      double dlevel = info.SuctionDiameterNPSInch;
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;

      var diameterProp = BaseBp.RegisterUserDefinedProperty("PipeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);

      diameterProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "SucTeePipe", 1 ),("SucTeePipe",2),("SucBranchPipeAPipe",0)));
      diameterProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateSuctionMinimumLengths) );
      diameterProp.AddUserDefinedRule(_suctionValveStopperRulePreA);
      diameterProp.AddUserDefinedRule(_suctionValveStopperRulePreB);
      diameterProp.AddUserDefinedRule(_suctionValveStopperRulePost);

      diameterProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "DisTeePipe", 0 ),( "DisTeePipe", 1 ),( "DisTeePipe", 2 ) ) ) ;
      diameterProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateDischargeMinimumLengths) );
      diameterProp.AddUserDefinedRule(_dischargeValveStopperRuleFirst);
      diameterProp.AddUserDefinedRule(_dischargeValveStopperRulePreA);
      diameterProp.AddUserDefinedRule(_dischargeValveStopperRulePreB);
      //diameterProp.AddUserDefinedRule(_dischargeValveStopperRulePost);

      BaseBp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.TemporaryValue,0.0);
      BaseBp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.TemporaryValue,0.0);
      BaseBp.RuleList.AddRule(".SuctionDiameter", ".PipeDiameter");
      BaseBp.RuleList.AddRule(".DischargeDiameter", ".PipeDiameter");

      if (info is SingleFilterPatternInfo infoDerived) {
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;

        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA1Valve]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB1Valve]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB1Valve]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleFilterPatternInfo.SuctionAdditionalIndexType.SucTee]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;
      }

#else
      var range = DiameterRange.GetBlockPatternNpsMmRange();
      double dlevel = (double)info.SuctionDiameterNPSInch;
      var diameterProp = BaseBp.RegisterUserDefinedProperty("PipeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, range.min, range.max);

      //
      // Rules
      //

      string Depth1(string objectName) {
        return $"(#{objectName}.MaxY - #{objectName}.MinY)";
      }

      string Width1(string objectName) {
        return $"(#{objectName}.MaxX - #{objectName}.MinX)";
      }

      string RightWidth(string objectName1, string objectName2) {
        return $"(#{objectName2}.MaxX - #{objectName1}.MaxX)";
      }

      string LeftWidth(string objectName1, string objectName2) {
        return $"(#{objectName2}.MinX - #{objectName1}.MinX)";
      }

      // MainToBypass

      BaseBp.RuleList.AddRule(
        "#SucBranchPipeAPipe.Length",
        $@"
        .MainToBypass
        - ({Depth1("SucBranchElbowA0Elbow")} - #SucBypassPipeAPipe.Diameter)
        - ({Depth1("SucTee")} - #SucPreMainValveAPipePipe.Diameter / 2)
        "
      );

      BaseBp.RuleList.AddRule("#SucBranchPipeBPipe.Length", "#SucBranchPipeAPipe.Length");

      // DischargeFilterToBranch

      BaseBp.RuleList.AddRule(
        "#DisPreTeePipePipe.Length",
        $@"
        .DischargeFilterToBranch
        - {RightWidth("DisPreTeePipe", "DisPostNozzlePipe")}
        - ({Width1("DisTee")} - #SucBranchPipeBPipe.Diameter) / 2
        "
      );

      // SuctionFilterToBranch

      BaseBp.RuleList.AddRule(
        "#SucPreTeePipePipe.Length",
        $@"
        .SuctionFilterToBranch
        - {LeftWidth("SucPostNozzlePipe", "SucPreTeePipe")}
        - ({Width1("SucTee")} - #SucBranchPipeAPipe.Diameter) / 2
        "
      );

      // DischargeFilterToValve

      BaseBp.RuleList.AddRule(
        "#DisPreValveAPipePipe.Length",
        $".DischargeFilterToValve - {RightWidth("DisPreValveAPipe", "DisPostNozzlePipe")}"
      ).AddTriggerSourcePropertyName("DisPreTeePipePipe", "Length");

      // DischargeValveToValve

      BaseBp.RuleList.AddRule(
        "#DisPreValveBPipePipe.Length",
        ".DischargeValveToValve - #DisPostValveAPipePipe.Length"
      ).AddTriggerSourcePropertyName("DisPreTeePipePipe", "Length");

      // SuctionFilterToValve

      BaseBp.RuleList.AddRule(
        "#SucPreMainValveAPipePipe.Length",
        $".SuctionFilterToValve - {LeftWidth("SucPostNozzlePipe", "SucPreMainValveAPipe")}"
      ).AddTriggerSourcePropertyName("SucPreTeePipePipe", "Length");

      // SuctionValveToValve

      BaseBp.RuleList.AddRule(
        "#SucPreMainValveBPipePipe.Length",
        ".SuctionValveToValve - #SucPostMainValveAPipe.Length"
      ).AddTriggerSourcePropertyName("SucPreTeePipePipe", "Length");

      // SuctionDiameter == DischargeDiameter (EDIT)

      diameterProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(
        ("SucPostNozzlePipe", 1),
        ("SucPostNozzlePipe", 0),
        ("DisPostNozzlePipe", 0),
        ("DisPostNozzlePipe", 1),
        ("SucBypassPipeA", 1),
        ("SucBypassPipeB", 0)
      ));
      
      // diameterProp.AddUserDefinedRule(new AllComponentDiameterLevelRule(("SucPostNozzlePipe", 1)));
      // var dischargeProp = BaseBp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterLevel, Doc.DiameterLevel.GetLevelFromNPSInch(info.DischargeDiameterNPSInch));
      // dischargeProp.AddUserDefinedRule(new AllComponentDiameterLevelRule(("DisPostNozzlePipe", 1)));
#endif

      #endif

    }
  }
}
