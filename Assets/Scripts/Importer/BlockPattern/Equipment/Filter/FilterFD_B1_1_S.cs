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
  public class FilterFD_B1_1_S : FilterBase<BlockPatternArray>
  {

    Dictionary<string, double> _defaults; //  IDF設定から抜き出した情報を保存してある

    public FilterFD_B1_1_S( Document doc ) : base( doc, "FD-B1-1-S" )
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
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisElbowA0Elbow,   -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisElbowA1Pipe,    -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisElbowA2Elbow,   -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPipeC,          -1 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreValveAPipe,   3 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA0Flange,   4 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA1Valve,    5 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA2Flange,   6 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPostValveAPipe,  7 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreValveBPipe,  11 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB0Flange,  12 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB1Valve,   13 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveB2Flange,  14 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreTeePipe,     15 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee,            16 },
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
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostNozzlePipe,      26 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainFlangeA,         25 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainPipeB,           24 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreMainValveAPipe,   23 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA0Flange,   22 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve,    21 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA2Flange,   20 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostMainValveA,      19 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreMainValveBPipe,   15 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB0Flange,   14 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB1Valve,    13 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveB2Flange,   12 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainPipeF,           -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreTeePipe,          11 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucTee,                  0 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeA,          1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA0Elbow,   2 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA1Pipe,   -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA2Elbow,  -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeA,          3 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeB,          7 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeC,          8 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassValveA0Flange, -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassValveA1Valve,  -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassValveA2Flange, -1 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowB,         9 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeB,         10 },
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, -1 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, -1 },
        },

        //
        //  Pipe index helper
        //
        SuctionFlexHelper = new int[,] { { 24, 11 },{ 3, 8 } },
        DischargeFlexHelper = new int[,]{ { 2, 16 }   },      //  all discharge pipes
        SuctionPipeIndexRange = new int[,]{ { 10, 24 }, },    //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[,]{ { 2,16 }, },    //  all pipe

        SuctionDiameterNPSInch = 6,
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

      PostProcess();
      
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 設定反映
      
      var cbp = BpOwner;
#if !INDEXING_NOW
      cbp.GetProperty( "PipeDiameter" ).Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;

      cbp.GetProperty("DischargeFilterToBranch").Value = _defaults["DischargeFilterToBranch"];
      cbp.GetProperty("SuctionFilterToBranch").Value = _defaults["SuctionFilterToBranch"];
#endif
      
      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DisTee", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SucTee", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge)BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }
    
    protected override void ImportIdf(){
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinLength 停止
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
      var DisPreValveAPipe = GetLeafEdge(dischargeGroup, 3);
      var DisPostValveAPipe = GetLeafEdge(dischargeGroup, 7);
      var DisPreValveBPipe = GetLeafEdge(dischargeGroup, 11);

      var SucPostNozzlePipe = GetLeafEdge(suctionGroup, 26);
      var SucBypassPipeA = GetLeafEdge(suctionGroup, 3);
      var SucBranchPipeA = GetLeafEdge(suctionGroup, 1);
      var SucBranchPipeB = GetLeafEdge(suctionGroup, 10);
      var SucPreMainValveAPipe = GetLeafEdge(suctionGroup, 23);
      var SucPreMainValveBPipe = GetLeafEdge(suctionGroup, 15);
      var SucPostMainValveA = GetLeafEdge(suctionGroup, 19);

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

      ///<summary>
      /// LeafEdge を動かすためのルール設定
      ///</summary>
      ///<remarks>
      ///   【コンセプト（変更）】
      ///     実現可能な正しい座標をプロパティの設定範囲として算出、プロパティの値を直接
      ///   LeafEdgeに設定する
      ///</remarks>
    protected override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      _defaults = GetDefaultValues(Info);

#if !INDEXING_NOW

      //
      //  プロパティ、範囲設定に計算式が使える
      //
      string mainToBypassMinRule = "MinHorzDistanceOf(#SucTee,#SucBranchElbowA0Elbow)-#SucBranchPipeAPipe.Diameter";

      string dischargeFilterToBranchMinRule = "MinHorzDistanceOf(#DisPostNozzlePipe,#DisTee)+#DisPostNozzlePipePipe.Length*0.5+#SucBranchPipeBPipe.Diameter*0.5";
      string suctionFilterToBranchMinRule = "MinHorzDistanceOf(#SucPostNozzlePipe,#SucTee)+#SucPostNozzlePipePipe.Length*0.5+#SucBranchPipeAPipe.Diameter*0.5";      

      string dischargeFilterToValveMinRule = "MinHorzDistanceOf(#DisPostNozzlePipe,#DisValveA1Valve)+#DisPostNozzlePipePipe.MinLength*0.5";
      string dischargeFilterToValveMaxRule = ".DischargeFilterToBranch+#SucBranchPipeBPipe.Diameter * 0.5 - MinHorzDistanceOf(#DisTee,#DisValveA1Valve)";
      string suctionFilterToValveMinRule   = "MinHorzDistanceOf(#SucPostNozzlePipe,#SucMainValveA1Valve)+#SucPostNozzlePipePipe.MinLength*0.5";
      string suctionFilterToValveMaxRule   = ".SuctionFilterToBranch+#SucBranchPipeAPipe.Diameter * 0.5 - MinHorzDistanceOf(#SucTee,#SucMainValveA1Valve)";

      //
      //  プロパティ設定 上限もしくは下限を設定しないときは、null
      //
      BaseBp.RegisterUserDefinedProperty("MainToBypass", PropertyType.Length, _defaults["MainToBypass"],
        mainToBypassMinRule,null);  // 0.966

      BaseBp.RegisterUserDefinedProperty("DischargeFilterToBranch", PropertyType.Length, _defaults["DischargeFilterToBranch"]+1.0,
        dischargeFilterToBranchMinRule,null); // 3.039

      BaseBp.RegisterUserDefinedProperty("DischargeFilterToValve", PropertyType.Length, _defaults["DischargeFilterToValve"],
        dischargeFilterToValveMinRule,dischargeFilterToValveMaxRule);  // 1.387
      
      BaseBp.RegisterUserDefinedProperty("SuctionFilterToBranch", PropertyType.Length, _defaults["SuctionFilterToBranch"]+1.0,
        suctionFilterToBranchMinRule,null); // 3.039

      BaseBp.RegisterUserDefinedProperty("SuctionFilterToValve", PropertyType.Length, _defaults["SuctionFilterToValve"],
        suctionFilterToValveMinRule,suctionFilterToValveMaxRule);  // 1.387

      //
      //  Main to bypass
      //
      BaseBp.RegisterUserDefinedProperty("MTB_Len", PropertyType.TemporaryValue, 0.0);  // 0.962

      BaseBp.RuleList.AddRule(
        ".MTB_Len", "#SucPreTeePipe.MaxY+.MainToBypass-DiameterToElbow90Length(#SucPreTeePipePipe.Diameter)-(#SucPreTeePipePipe.Diameter*0.5)-#SucTee.MaxY");
      BaseBp.RuleList.AddRule(
        "#SucBranchPipeAPipe.Length", ".MTB_Len");
      BaseBp.RuleList.AddRule(
        "#SucBranchPipeBPipe.Length", ".MTB_Len");
      
      //
      //  Valve Tee の座標設定 (B -> Aの順序を変えるとうまくいかないので注意)
      //
      BaseBp.RuleList.AddRule("#DisTee.MaxX", "#BasePump.MinX - .DischargeFilterToBranch + (DiameterToTeeMainLength(#SucBranchPipeBPipe.Diameter,#SucBranchPipeBPipe.Diameter)-#SucBranchPipeBPipe.Diameter)*0.5");
      BaseBp.RuleList.AddRule("#SucTee.MinX", "#BasePump.MaxX + .SuctionFilterToBranch - (DiameterToTeeMainLength(#SucBranchPipeAPipe.Diameter,#SucBranchPipeAPipe.Diameter)-#SucBranchPipeAPipe.Diameter)*0.5");
      BaseBp.RuleList.AddRule("#SucMainValveB1Valve.PosX", "#BasePump.MaxX + .SuctionFilterToValve+MinHorzDistanceOf(#SucMainValveA1Valve,#SucMainValveB1Valve)");
      BaseBp.RuleList.AddRule("#SucMainValveA1Valve.PosX", "#BasePump.MaxX + .SuctionFilterToValve");
      BaseBp.RuleList.AddRule("#DisValveB1Valve.PosX", "#BasePump.MinX - .DischargeFilterToValve-MinHorzDistanceOf(#DisValveA1Valve,#DisValveB1Valve)");
      BaseBp.RuleList.AddRule("#DisValveA1Valve.PosX", "#BasePump.MinX - .DischargeFilterToValve");

      //
      //  Diameter設定兼 溶接間最短距離の再設定
      //
      double dlevel = info.SuctionDiameterNPSInch;
      var diameterMinNpsMm = DiameterRange.GetBlockPatternNpsMmRange().min;
      var diameterMaxNpsMm = DiameterRange.GetBlockPatternNpsMmRange().max;

      var diameterProp = BaseBp.RegisterUserDefinedProperty("PipeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(dlevel + ((dlevel< 6)?1.0:2.0)).NpsMm, diameterMinNpsMm, diameterMaxNpsMm);

      diameterProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "SucTeePipe", 1 ),("SucTeePipe",2),("SucBranchPipeAPipe",0)));

      diameterProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ( "DisTeePipe", 0 ),( "DisTeePipe", 1 ),( "DisTeePipe", 2 ) ) ) ;

      //
      //  Fixed 設定
      //
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

    #endif
    }
  }
}
