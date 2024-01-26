#define USE_MINHORIZONTALDISTANCE_ON_DISCHARGE_LINE
//#define USE_LENGTH_UPDATER
using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text.RegularExpressions ;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.DB ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_V_H_S
{
  public class EndTopTypePumpET_B1_V_H_S : EndTopTypePumpBase<BlockPatternArray>
  {
    EndTopTypePumpLengthUpdater _lengthUpdater;
    EndTopTypePumpSystemLengthUpdater _systemLength;
    EndTopTypePumpPipeIndexKeeper _keeper;

    public EndTopTypePumpET_B1_V_H_S( Document doc ) : base( doc, "ET-B1-V-H-S" )
    {
      Info = new SingleBlockPatternIndexInfoVHS
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range( 2, 34 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 0 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 36 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart, 34 },  //  Discharge にFlex は使用しないが一応設定
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop,  3  },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin,2  },

        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 30 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,  7 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,   28 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 29 }, //  suction origin (reducer)
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 35 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
        },
        SuctionFlexHelper = new int[,]{ { 7, 28 }, },
        SuctionPipeIndexRange = new int[,]{ { 0,30  }, },
        DischargePipeIndexRange = new int[,]{ { 0,36 }, },
        
        DischargeFlexSystemPipeIndexRange = new int[]{ 34,3 },

        SuctionDiameterNPSInch = 3,
        DischargeDiameterNPSInch = 3,
      } ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      return EndTopTypePumpImport( onFinish ) ;
    }

    Chiyoda.CAD.Topology.BlockPattern EndTopTypePumpImport(
      Action<Edge> onFinish = null )
    {
      ImportIdfAndPump() ;

      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      if (Info is SingleBlockPatternIndexInfoVHS infoDerived) {
        if (EndTopTypePumpBase.INDEXING_NOW == true) {
          PostProcess();
        } else {
          EndTopTypePumpPipeIndexHelper.BuildIndexList(BpOwner, BaseBp, Info);
#if USE_LENGTH_UPDATER
          EndTopTypePumpLengthUpdater.AssortMinimumLengths(BaseBp, this.Info);
          _lengthUpdater = EndTopTypePumpLengthUpdater.Create(BpOwner, BaseBp, Info);
          _lengthUpdater.KeepDischargePipeOrderForLaterAdjustment();
#endif
          var dischargeGroup = GetGroup(Info.DischargeIndex);
          infoDerived.DischargeFlexSystemPipes
            = EndTopTypePumpPipeIndexHelper.ExtractGroupIndices(dischargeGroup, infoDerived.DischargeFlexSystemPipeIndexRange[0], infoDerived.DischargeFlexSystemPipeIndexRange[1]);

#if DEBUG_DUMP
            string dump = "discharge system index : ";
            foreach(var index in infoDerived.DischargeFlexSystemPipes){
              dump += $"{index} ";
            }
            UnityEngine.Debug.Log(dump);
#endif
          _keeper = new EndTopTypePumpPipeIndexKeeper(dischargeGroup, infoDerived.DischargeFlexSystemPipes);
          _systemLength = new EndTopTypePumpSystemLengthUpdater(infoDerived.DischargeIndex, infoDerived.DischargeFlexSystemPipes);
          _systemLength.Activate(true);

          PostProcess();

        }
        if (!EndTopTypePumpBase.INDEXING_NOW){
          BpOwner.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(Info.DischargeDiameterNPSInch).NpsMm;
          BpOwner.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;
        }

        // vertexにflowを設定
        // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
        BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
        BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

        onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;
      }
      return BaseBp ;
    }

    /// <summary>
    /// 基底クラスでグループが組み替えられると呼ばれる
    /// </summary>
    /// <returns></returns>
    internal override void OnGroupChanged()
    {
#if USE_LENGTH_UPDATER
      _lengthUpdater.AdjustDischargePipeIndexesAfterReconstructingGroups();
      _lengthUpdater.Activate(true);
      EndTopTypePumpLengthUpdater.AssortMinimumLengths(BaseBp, this.Info);
#else
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 反映
#endif
      if (Info is SingleBlockPatternIndexInfoVHS infoDerived){
        infoDerived.DischargeFlexSystemPipes = _keeper.ReassignIndices(infoDerived.DischargeFlexSystemPipes);
  #if DEBUG_DUMP
          dump = "reassigned discharge system index : ";
          foreach(var index in infoDerived.DischargeFlexSystemPipes){
            dump += $"{index} ";
          }
          UnityEngine.Debug.Log(dump);
  #endif
        if (_systemLength != null){
            _systemLength.ReplaceIndices(infoDerived.DischargeFlexSystemPipes);
          _systemLength.Activate(true);
        }
      }
    }
    protected override void ImportIdf(){
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinLength 停止
    }
    protected override void RemoveExtraEdges( Group group, string file )
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if ( file.Contains( "-DIS-A" ) ) {
          removeEdgeList = group.EdgeList.Reverse().Take( 2 ).ToList() ;
        }
        else if ( file.Contains( "-SUC-A" ) ) {
          removeEdgeList = group.EdgeList.Take( 2 ).ToList() ;
        }

        removeEdgeList?.ForEach( e => e.Unlink() ) ;
      }
    }

    /// <summary>
    /// IDFにノズル側にフランジ形状が潰れてしまっているので特別に追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected override LeafEdge GetNozzleSideFlange( Group group, string file )
    {
      // B1_V_H_S has no weld flange on the edge of the discharging sysytem.
      // IDFにノズル側にフランジ形状が潰れてしまっているので特別に追加する
      if ( ! file.Contains( "-DIS-A" ) ) {
        return null ;
      }

      var pipe = ( group.EdgeList.ElementAtOrDefault( 0 ) as LeafEdge )?.PipingPiece as Pipe ;
      if ( ! ( pipe?.Parent is LeafEdge pipeEdge ) ) {
        return null ;
      }

      var table = DB.Get<DimensionOfFlangeTable>();

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

        var height = ( (double) table.Get( (int)pipe.DiameterObj.NpsMm ).Height).Millimeters() ;
        var pipeDir = ( pipeNear - pipeFar ).normalized * height ;
        var leafEdge = Doc.CreateEntity<LeafEdge>() ;
        var flange = Doc.CreateEntity( EntityType.Type.WeldNeckFlange ) as WeldNeckFlange ;
        leafEdge.PipingPiece = flange ;
        Vector3d outside = pipeNear ;
        Vector3d weld = pipeNear + pipeDir ;
        IDFFlangeImporter.CalcShape( flange, weld, outside, pipe.Diameter ) ;
        Doc.CreateHalfVerticesAndMakePairs( leafEdge ) ;
        return leafEdge ;
      }
      return null ;
    }

    internal override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      if (!EndTopTypePumpBase.INDEXING_NOW) {

        BaseBp.RegisterUserDefinedProperty("AccessSpace", PropertyType.Length, 1.2);
        {
          var bpa = BpOwner;
          bpa.RegisterUserDefinedProperty("BlockCount", PropertyType.GeneralInteger, 1);
          bpa.RuleList.AddRule(".ArrayCount", ".BlockCount");
        }
        //  pipe基準のAccessSpace
        BaseBp.RuleList.AddRule(":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeEnd.MinX, #DischargeEnd.MaxX - #BasePump.MinX ) + .AccessSpace ");

        EndTopTypePumpBasicPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

        {
          var bp = BaseBp;
          IRule rule;
          bp.RegisterUserDefinedProperty( "HeaderInterval", PropertyType.Length, 0.5 ) ;
          bp.RegisterUserDefinedProperty( "SuctionDischargeDistance", PropertyType.Length, 0.33 ) ;

          //  Header interval
          bp.RegisterUserDefinedProperty( "SFlex_MinSysLength", PropertyType.TemporaryValue, 0.0 ) ;
          bp.RegisterUserDefinedProperty( "S_MinPos_Y", PropertyType.TemporaryValue, 0.0 ) ;
          bp.RegisterUserDefinedProperty( "DFlex_MinSysLength", PropertyType.TemporaryValue, 0.0 ) ;
          bp.RegisterUserDefinedProperty( "D_MinSysLength", PropertyType.TemporaryValue, 0.0 ) ;
          bp.RegisterUserDefinedProperty( "D_SysLen_Ofst", PropertyType.TemporaryValue, 0.0 ) ;
          bp.RegisterUserDefinedProperty( "D_MinPos_Y", PropertyType.TemporaryValue, 0.0 ) ;

          bp.RegisterUserDefinedProperty( "D_OSE_Len", PropertyType.TemporaryValue, 0.0 ) ;  //  length of opposite side edge.

          bp.RegisterUserDefinedProperty( "DHE_Pow2Len", PropertyType.TemporaryValue, 0.0 ) ;   //  pow2 of length of hypotenuse side edge.
          bp.RegisterUserDefinedProperty( "DOSE_Pow2Len", PropertyType.TemporaryValue, 0.0 ) ;  //  pow2 of length of opposite side edge.

          //  Suction System を最短長にしたときのSuctionEnd の座標を求める

          rule  = bp.RuleList.AddRule( ".SFlex_MinSysLength", " MinHorzDistanceOf(#SuctionSystemFlexStart,#SuctionSystemFlexStop) + (#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5" ) ;
          rule.AddTriggerSourcePropertyName("SuctionDiameter");
          bp.RuleList.AddRule( ".S_MinPos_Y", 
            ".SFlex_MinSysLength+DiameterToElbow90Length(#SuctionSystemFlexStartPipe.Diameter) + #SuctionSystemFlexOrigin.MaxY" ) ;
          /*
          rule = bp.RuleList.AddRule( ".S_MinPos_Y", 
            "MinHorzDistanceOf(#SuctionSystemFlexStart,#SuctionSystemFlexStop)+#SuctionSystemFlexStopPipe.MinLength*0.5+ #SuctionSystemFlexOrigin.MaxY" ) ;
          rule.AddTriggerSourcePropertyName("SuctionDiameter");
          */
          //  Discharge System を最短長にしたときの、DischargeEnd の座標を求める
          //  対辺（Opposite Side Edge の長さ）
          rule = bp.RuleList.AddRule( ".D_OSE_Len", 
            ".SuctionDischargeDistance + ((#DischargeSystemFlexStartPipe.Diameter+#SuctionSystemFlexStartPipe.Diameter)*0.5)" ) ;
          rule.AddTriggerSourcePropertyName("DischargeDiameter");

          bp.RuleList.AddRule(".D_SysLen_Ofst", "DiameterToElbow90Length(#DischargeSystemFlexStartPipe.Diameter)*2.0");

#if USE_MINHORIZONTALDISTANCE_ON_DISCHARGE_LINE
          //  DFlex_MinSysLength の設定は、DischargeDiameter のところへ移動
          rule = bp.RuleList.AddRule( ".D_MinSysLength", 
            "SystemMinHorzDistanceOf(#NextOfDischargeEnd,#DischargeSystemFlexOrigin)" ) ;
#else
          bp.RuleList.AddRule( ".D_MinSysLength",".D_SysLen_Ofst + .DFlex_MinSysLength" ) ;
#endif

          bp.RuleList.AddRule(".DHE_Pow2Len", ".D_MinSysLength*.D_MinSysLength"); //  斜辺(Hypotenuse Edge)長の二乗
          bp.RuleList.AddRule(".DOSE_Pow2Len", ".D_OSE_Len*.D_OSE_Len");
        
          bp.RuleList.AddRule( ".D_MinPos_Y", 
            "Sqrt(.DHE_Pow2Len-Min(.DOSE_Pow2Len,.DHE_Pow2Len))+#DischargeNozzle.PosY" ) ;  

          //  now we have both D_MinPos_Y and S_MinPos_Y so we can do it as other pumps do.
          bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準は0 Suction基準は1 
          bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
          bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
          bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
          bp.RegisterUserDefinedProperty("SYS_Short_Y", PropertyType.TemporaryValue, 0.0);   // Y座標不足分

          rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");

          //  Select origin position 
          rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, 1.0, 0.0)"); 

          //  基準座標設定
          rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))"); 
          rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.S_Base > 0, IF(.HeaderInterval >= 0, .S_MinPos_Y - .HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval), .D_MinPos_Y)");

          //  不足分を算出
          rule = bp.RuleList.AddRule(".SYS_Short_Y", "Max(0,Max(.S_MinPos_Y-.S_EndPos_Y,.D_MinPos_Y-.D_EndPos_Y))");

          //  LeafEdge に反映
          rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y+.SYS_Short_Y");

          bp.RegisterUserDefinedProperty("D_Result_Len", PropertyType.TemporaryValue, 0.0);   // 
          bp.RegisterUserDefinedProperty("D_Result_BSE", PropertyType.TemporaryValue, 0.0);   //  BaseSideEdgeLength of the Triangle 
          bp.RegisterUserDefinedProperty("D_Result_Angle", PropertyType.TemporaryValue, 0.0); // 

          bp.RuleList.AddRule(".D_Result_BSE", ".D_EndPos_Y+.SYS_Short_Y - #DischargeNozzle.PosY"); //  底辺(Base Side Edge)の長さを求める
          bp.RuleList.AddRule(".D_Result_Len", "Sqrt((.D_Result_BSE*.D_Result_BSE)+ .DOSE_Pow2Len)");

          bp.RuleList.AddRule(".D_Result_Angle", "IF (.D_OSE_Len > 0.0001,Rad2Deg(-Atan(.D_Result_BSE/.D_OSE_Len)),-90)");

          BaseBp.RegisterUserDefinedProperty( "DischargeDirection", 0, new Dictionary<string, double> { { "Right", 0 }, { "Left", 1 } } ) ;

          bp.RuleList.AddRule("#DischargeGroup.HorizontalRotationDegree","If( .DischargeDirection, 180 - .D_Result_Angle, .D_Result_Angle )");

          bp.RuleList.AddRule(".DischargeSystemLengthTemp", ".D_Result_Len - .D_SysLen_Ofst");

          var prop = BaseBp.RegisterUserDefinedProperty( "DischargeSystemLengthTemp", PropertyType.TemporaryValue, 4.0) ;
          if (info is SingleBlockPatternIndexInfoVHS infoDerived) {
            if (_systemLength != null)
              prop.AddUserDefinedRule(new GenericHookedRule(_systemLength.UpdateSystemLength));
          }

          var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
          var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
          if (edge != null){
            edge.PositionMode = PositionMode.FixedY ;
          }

          //  独自Diameter 設定
          var range = DiameterRange.GetBlockPatternNpsMmRange();
          double dinch = info.DischargeDiameterNPSInch;
          var suctionProp = bp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(dinch+((dinch<6)?1.0:2.0)).NpsMm, range.min, range.max);
          suctionProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));
#if  USE_LENGTH_UPDATER
          suctionProp.AddUserDefinedRule( new GenericHookedRule(_lengthUpdater.UpdateSuctionMinimumLengths) );
#endif
          //  Discharge Pipe については、径変更時、システムの最短長 MinHorzDistanceOf を更新する Rule を追加
          double sinch = info.SuctionDiameterNPSInch;
          var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(sinch+((sinch<6)?1.0:2.0)).NpsMm, range.min, range.max);
          dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1)));
#if  USE_LENGTH_UPDATER
          dischargeProp.AddUserDefinedRule( new GenericHookedRule(_lengthUpdater.UpdateDischargeMinimumLengths) );
#endif
#if ! USE_MINHORIZONTALDISTANCE_ON_DISCHARGE_LINE
          dischargeProp.AddUserDefinedRule( new InjectorHookedRule(_systemLength.CheckSystemMinimumLength,"DFlex_MinSysLength") );
#endif
        }
      }
    }

#if false
    protected override void NormalizeDischargeGroupAngle( Group dischargeGroup )
    {
      var angle = ModifyGroupAngle( dischargeGroup, 0 ) ;
      if ( null == angle ) return ;

      BpOwner.GetProperty( "DischargeAngle" ).Value = angle.Value ;
      dischargeGroup.ExtraHorizontalRotationDegree = BaseBp.GetProperty( "DischargeAngle" ).Value ;
    }
#else
    protected override void NormalizeDischargeGroupAngle( Group dischargeGroup )
    {
      var angle = ModifyGroupAngle( dischargeGroup, 0 ) ;
      if ( null == angle ) return ;

      var dischargeAngleEdge = BpOwner.GetProperty( "DischargeAngle" ) ;
      if ( null == dischargeAngleEdge ) return ;
      dischargeAngleEdge.Value = angle.Value ;
      dischargeGroup.ExtraHorizontalRotationDegree = BaseBp.GetProperty( "DischargeAngle" ).Value ;
    }
#endif
  }
}
