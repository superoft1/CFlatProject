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
  public class FilterFS_A1_1_G : FilterBase<BlockPatternArray>
  {
    private FilterSystemPipeUtility _suctionValveStopperPreA;    //  pump本体（含まず）～valve-A（含む）
    private FilterSystemPipeUtility _suctionValveStopperPost;    //  valve-A(含まず）～末端
    private FilterSystemPipeUtility _dischargeValveStopperFirst; //  pump本体（含まず）～Elb（含まず）
    private FilterSystemPipeUtility _dischargeValveStopperPreA;  //  Elb(含まず）～Valve-A（含む）
    //private FilterSystemPipeUtility _dischargeValveStopperPost;  //  valve-A（含まず）から末端  

    //  Elbの長さ（そんなものはない）を取ろうとすると正しく取れないので、分割して対処
    private IUserDefinedRule _suctionValveStopperRulePreA;      //  pump本体（含まず）～valve-A（含む）
    private IUserDefinedRule _suctionValveStopperRulePost;      //  valve-A(含まず）～末端
    private IUserDefinedRule _dischargeValveStopperRuleFirst;   //  pump本体（含まず）～Elb（含まず）
    private IUserDefinedRule _dischargeValveStopperRulePreA;    //  Elb(含まず）～Valve-A（含む）
    //private IUserDefinedRule _dischargeValveStopperRulePost;    //  valve-A（含まず）から末端  

    FilterPipeLengthUpdater _minLengthUpdater;

    Dictionary<string, double> _defaults;

    public FilterFS_A1_1_G( Document doc ) : base( doc, "FS-A1-1-G" )
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
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPostValveAPipe, -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreValveBPipe,  -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB0Flange,  -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB1Valve,   -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB2Flange,  -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreTeePipe,     14 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee,            15 },
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
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostNozzlePipe,      24 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainFlangeA,         23 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainPipeB,           22 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreMainValveAPipe,   21 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA0Flange,   20 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve,    19 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA2Flange,   18 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostMainValveA,      17 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreMainValveBPipe,   -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB0Flange,   -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB1Valve,    -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB2Flange,   -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainPipeF,           -1 },
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
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, -1 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, -1 },
        },
        //
        //  Pipe index helper
        //
        SuctionFlexHelper = new int[,] { { 12, 5 }, { 13, 22 } },
        DischargeFlexHelper = new int[,] { { 2, 2 }, { 6, 15 } },  //  all discharge pipes (ただし、垂直のPipe 一つを除く)
        SuctionPipeIndexRange = new int[,] { { 12, 22 } },         //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,] { { 2, 14 } },        //  all pipe

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

      _suctionValveStopperPreA    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 24, 19 );     //  pump（含まず）～valve-A（含む）
      _suctionValveStopperPost    = new FilterSystemPipeUtility(BaseBp, Info.SuctionIndex, 18, 13 );     //  valve-A(含まず）～末端（Tee含まず）
      _dischargeValveStopperFirst = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 0, 2 );     //  pump（含まず）～Elb（含まず）
      _dischargeValveStopperPreA  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 6, 12 );    //  Elb(含まず）～Valve-A（含む）
      //_dischargeValveStopperPost  = new FilterSystemPipeUtility(BaseBp, Info.DischargeIndex, 12, 14 ); //  valve-A（含まず）から末端（Tee含まず）

      PostProcess();
      
      var cbp = BpOwner;
      cbp.GetProperty("PipeDiameter").Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;
      
      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DisTee", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SucTee", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge)BpOwner ?? BaseBp ) ;
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
    
    protected LeafEdge GetLeafEdge(IGroup group, int edgeIndex)
    {
      return GetEdge(group, edgeIndex) as LeafEdge;
    }

    protected Dictionary<string, double> GetDefaultValues(SingleBlockPatternIndexInfo info)
    {
      var dischargeGroup = GetGroup(info.DischargeIndex);
      var suctionGroup = GetGroup(info.SuctionIndex);

      Dictionary<string, double> values = new Dictionary<string, double>();

      var BasePump = BaseBp.EquipmentEdges.ElementAtOrDefault(info.BasePumpIndex);

      var DisPostNozzlePipe = GetLeafEdge(dischargeGroup, 0);
      var DisPreValveAPipe = GetLeafEdge(dischargeGroup, 10);

      var SucPostNozzlePipe = GetLeafEdge(suctionGroup, 24);
      var SucBypassPipeA = GetLeafEdge(suctionGroup, 5);
      var SucBranchPipeA = GetLeafEdge(suctionGroup, 1);
      var SucBranchPipeB = GetLeafEdge(suctionGroup, 12);
      var SucPreMainValveAPipe = GetLeafEdge(suctionGroup, 21);

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
        DisPostNozzlePipe.GetProperty("MaxX").Value - DisPreValveAPipe.GetProperty("MinX").Value
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
        "BOP",
        DisPreValveAPipe.GetProperty("MinZ").Value - BasePump.GetProperty("MinZ").Value
      );

      return values;
    }

    protected override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      _defaults = GetDefaultValues(Info);

      //
      // UI Properties
      //

      BaseBp.RegisterUserDefinedProperty("MainToBypass", PropertyType.Length, _defaults["MainToBypass"]);  // 0.966
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToBranch", PropertyType.Length, _defaults["DischargeFilterToBranch"]); // 2.161
      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValve", PropertyType.Length, _defaults["DischargeFilterToValve"]); // 1.387
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToBranch", PropertyType.Length, _defaults["SuctionFilterToBranch"]); // 1.494
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValve", PropertyType.Length, _defaults["SuctionFilterToValve"]);  // 0.526

      IRule rule;

      //
      // .MTB_Len       branch pipe 長さ
      //
      // .DT_POS_X      Discharge側 Tee のnear 死点(X)
      // .DT_MAX_X      Discharge側 Tee のnear 死点(X)
      //
      // .DVA_MinLen    合成 pump（含まず）～valveA（含む）
      // .DVC_MinLen    Valve-A(含まず)～末端のMinLength
      //
      // .DVA_MAX_X     Discharge側 Valve-A のnear 死点(X)
      // .DVA_MIN_X     Discharge側 Valve-A のfar 死点(X)
      // .DVA_POS_X     Discharge側 Valve-A のfar 死点(X)
      // 
      // .SVA_MinLen    pump（含まず）～valveA（含む）
      // .SVF_MinLen    valveA(含まず）～末端(Teeなし)
      //
      // .SVA_MAX_X     Suction側 Valve-A のnear 死点(X)
      // .SVA_MIN_X     Suction側 Valve-A のfar 死点(X)
      //

      //
      // MainToBypass
      //

      BaseBp.RegisterUserDefinedProperty("MTB_Len", PropertyType.TemporaryValue, 0.0);  // 0.962
      BaseBp.RuleList.AddRule(
        ".MTB_Len",
        "#SucPreTeePipe.MaxY + .MainToBypass - DiameterToElbow90Length(#SucPreTeePipePipe.Diameter) - (#SucPreTeePipePipe.Diameter*0.5) - #SucTee.MaxY"
      );
      BaseBp.RuleList.AddRule("#SucBranchPipeAPipe.Length", ".MTB_Len");
      BaseBp.RuleList.AddRule("#SucBranchPipeBPipe.Length", ".MTB_Len");

      //
      // DischargeFilterToBranch
      //

      BaseBp.RegisterUserDefinedProperty("DVC_MinLen", PropertyType.TemporaryValue, 0.0);  // Valve-A(含まず)～末端のMinLength

      rule = BaseBp.RuleList.AddRule(".DVC_MinLen", "#DisPreTeePipePipe.MinLength + (#DisValveA2FlangePipe.Length)");
      rule.AddTriggerSourcePropertyName("DischargeDiameter");

      BaseBp.RegisterUserDefinedProperty("DT_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DT_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Tee のnear 死点(X)

      BaseBp.RuleList.AddRule(".DT_MAX_X", "#BasePump.MinX - (.DVA_MinLen + .DVC_MinLen)");
      
      BaseBp.RuleList.AddRule(
        ".DT_POS_X",
        "#BasePump.MinX - .DischargeFilterToBranch + ((DiameterToTeeMainLength(#DisPreTeePipePipe.Diameter, #SucBranchPipeBPipe.Diameter) - #DisPreTeePipePipe.Diameter ) * 0.5)");

      BaseBp.RuleList.AddRule("#DisTee.MaxX", "Min(.DT_MAX_X, .DT_POS_X)");

      //
      // SuctionFilterToBranch
      //

      BaseBp.RuleList.AddRule(
        "#SucTee.MinX",
        $"#BasePump.MaxX + .SuctionFilterToBranch - ((DiameterToTeeMainLength(#SucPreTeePipePipe.Diameter, #SucBranchPipeAPipe.Diameter) - #SucPreTeePipePipe.Diameter)*0.5)");

      //
      //  Filter to valve setting
      //

      //  Discharge側(X < Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("DVA_MAX_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Valve-A のnear 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_MIN_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Valve-A のfar 死点(X)
      BaseBp.RegisterUserDefinedProperty("DVA_POS_X", PropertyType.TemporaryValue, 0.0);   //  Discharge側 Valve-A のfar 死点(X)

      rule = BaseBp.RuleList.AddRule(".DVA_MIN_X", "#DisTee.MaxX + .DVC_MinLen + (#DisValveA1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DisPreTeePipePipe", "Diameter");

      rule = BaseBp.RuleList.AddRule(".DVA_MAX_X", "#BasePump.MinX - .DVA_MinLen + (#DisValveA1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("DisPreTeePipePipe", "Diameter");
      rule.AddTriggerSourcePropertyName("DischargeFilterToValve");

      rule = BaseBp.RuleList.AddRule(".DVA_POS_X", "Max(.DVA_MIN_X, Min(.DVA_MAX_X, #BasePump.MinX - .DischargeFilterToValve))");
      rule = BaseBp.RuleList.AddRule("#DisValveA1Valve.PosX", ".DVA_POS_X");

      //  Suction側(X > Filter) Valve A 位置の調整      
      BaseBp.RegisterUserDefinedProperty("SVA_MAX_X", PropertyType.TemporaryValue, 0.0);   
      BaseBp.RegisterUserDefinedProperty("SVA_MIN_X", PropertyType.TemporaryValue, 0.0);   

      rule = BaseBp.RuleList.AddRule(".SVA_MAX_X",
        "#SucTee.MinX - .SVF_MinLen - (#SucMainValveA1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SucPreTeePipePipe", "Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToBranch");

      rule = BaseBp.RuleList.AddRule(".SVA_MIN_X",
        "#BasePump.MaxX + .SVA_MinLen - (#SucMainValveA1ValvePipe.Length*0.5)");
      rule.AddTriggerSourcePropertyName("SucPreTeePipePipe","Diameter");
      rule.AddTriggerSourcePropertyName("SuctionFilterToBranch");

      rule = BaseBp.RuleList.AddRule("#SucMainValveA1Valve.PosX", "Max(.SVA_MIN_X,MIN(.SVA_MAX_X,#BasePump.MaxX+.SuctionFilterToValve ))");

      //  Custom rules
      BaseBp.RegisterUserDefinedProperty("SVA_MinLen", PropertyType.TemporaryValue, 0.0);   // pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("SVF_MinLen", PropertyType.TemporaryValue, 0.0);   // valveA(含まず）～末端(Teeなし)

      BaseBp.RegisterUserDefinedProperty("DVA1_MinLen", PropertyType.TemporaryValue, 0.0);  // 部分 pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("DVA2_MinLen", PropertyType.TemporaryValue, 0.0);  // 部分 pump（含まず）～valveA（含む）
      BaseBp.RegisterUserDefinedProperty("DVA_MinLen", PropertyType.TemporaryValue, 0.0);   // 合成 pump（含まず）～valveA（含む）
      //BaseBp.RegisterUserDefinedProperty("DVF_MinLen", PropertyType.TemporaryValue, 0.0); // valveA(含まず）～末端(Teeなし)

      _dischargeValveStopperFirst.CheckSystemMinimumLength(BaseBp, null, "DVA1_MinLen");
      _dischargeValveStopperPreA.CheckSystemMinimumLength(BaseBp, null, "DVA2_MinLen");
      _suctionValveStopperPreA.CheckSystemMinimumLength(BaseBp, null, "SVA_MinLen");
      _suctionValveStopperPost.CheckSystemMinimumLength(BaseBp, null, "SVF_MinLen");

      _suctionValveStopperRulePreA = new InjectorHookedRule(_suctionValveStopperPreA.CheckSystemMinimumLength, "SVA_MinLen");        // pump（含まず）～valve-A（含む）
      _suctionValveStopperRulePost = new InjectorHookedRule(_suctionValveStopperPost.CheckSystemMinimumLength, "SVF_MinLen");        // valve-A(含まず）～末端

      _dischargeValveStopperRuleFirst = new InjectorHookedRule(_dischargeValveStopperFirst.CheckSystemMinimumLength, "DVA1_MinLen"); // pump（含まず）～Elb（含まず）
      _dischargeValveStopperRulePreA = new InjectorHookedRule(_dischargeValveStopperPreA.CheckSystemMinimumLength, "DVA2_MinLen");   // Elb(含まず）～Valve-A（含む）
      //_dischargeValveStopperRulePost = new InjectorHookedRule(_dischargeValveStopperPost.CheckSystemMinimumLength, "DVF_MinLen");  // valve-A（含まず）から末端  

      BaseBp.RuleList.AddRule(".DVA_MinLen", ".DVA1_MinLen + .DVA2_MinLen + (2.0*DiameterToElbow90Length(#DisPreTeePipePipe.Diameter))");
      _minLengthUpdater = FilterPipeLengthUpdater.Create(BpOwner, BaseBp, Info);
      _minLengthUpdater.Activate(false);

      //
      // BOP
      //

      BaseBp.RegisterUserDefinedProperty("BOP", PropertyType.Length, _defaults["BOP"]);  // 0.5

      BaseBp.RegisterUserDefinedProperty("NozzleHeight", PropertyType.TemporaryValue, 0.0);
      BaseBp.RuleList.AddRule(".NozzleHeight", "#DisPostNozzlePipe.PosZ - #BasePump.MinZ");
      BaseBp.RuleList.AddRule(
        "#DisElbowA1PipePipe.Length",
        "Max(.NozzleHeight - (.BOP + DiameterToElbow90Length(#DisPreTeePipePipe.Diameter)*2 + #DisPreTeePipePipe.Diameter*0.5), #DisElbowA1PipePipe.MinLength)");
      BaseBp.RuleList.AddRule("#SucBranchElbowA1PipePipe.Length", "#DisElbowA1PipePipe.Length");
      
      //
      // PipeDiameter
      //

      double dlevel = info.SuctionDiameterNPSInch;
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;

      var diameterProp = BaseBp.RegisterUserDefinedProperty("PipeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);

      diameterProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "SucTeePipe", 1 ),("SucTeePipe",2),("SucBranchPipeAPipe",0)));
      diameterProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateSuctionMinimumLengths) );
      diameterProp.AddUserDefinedRule(_suctionValveStopperRulePreA);
      diameterProp.AddUserDefinedRule(_suctionValveStopperRulePost);

      diameterProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "DisTeePipe", 0 ),( "DisTeePipe", 1 ),( "DisTeePipe", 2 ) ) ) ;
      diameterProp.AddUserDefinedRule( new GenericHookedRule( _minLengthUpdater.UpdateDischargeMinimumLengths) );
      diameterProp.AddUserDefinedRule(_dischargeValveStopperRuleFirst);
      diameterProp.AddUserDefinedRule(_dischargeValveStopperRulePreA);
      //diameterProp.AddUserDefinedRule(_dischargeValveStopperRulePost);

      BaseBp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.TemporaryValue, 0.0);
      BaseBp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.TemporaryValue, 0.0);
      BaseBp.RuleList.AddRule(".SuctionDiameter", ".PipeDiameter");
      BaseBp.RuleList.AddRule(".DischargeDiameter", ".PipeDiameter");

      if (info is SingleFilterPatternInfo infoDerived) {
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;

        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA1Valve]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;

        edge = suctionGroup?.EdgeList.ElementAtOrDefault(infoDerived.SuctionAdditionalIndexTypeValue[SingleFilterPatternInfo.SuctionAdditionalIndexType.SucTee]) as LeafEdge;
        if (edge != null)
          edge.PositionMode = PositionMode.FixedX;
      }
    }
  }
}
