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

namespace Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_V_L
{
  public class EndTopTypePumpET_A1_V_V_L : EndTopTypePumpBase<BlockPatternArray>
  {
    public EndTopTypePumpET_A1_V_V_L( Document doc ) : base( doc, "ET-A1-V-V-L" )
    {
      Info = new SingleBlockPatternIndexInfo
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range( 3, 8 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, 21 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 28 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPSpacer, 11 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark1,    0 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark2,    2 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark3,   10 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,21 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop, 13 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin,   12 },
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 23 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,  7 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,   21 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 22 }, //  suction origin (reducer)
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 27 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
        },

        SuctionFlexHelper = new int[,]{ { 7, 21 }, },
        DischargeFlexHelper = new int[,]{ { 21, 13  },{ 1, 1 } },
        SuctionPipeIndexRange = new int[,]{ { 0,23  }, },
        DischargePipeIndexRange = new int[,]{ { 1,28 }, },

        SuctionDiameterNPSInch = 12,  //  10 inches
        DischargeDiameterNPSInch = 10,  //  10 inches
      } ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      return EndTopTypePumpImport( onFinish ) ;
    }

    Chiyoda.CAD.Topology.BlockPattern EndTopTypePumpImport(
      Action<Edge> onFinish = null )
    {
      var pump = ImportIdfAndPump() ;

      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      EndTopTypePumpPipeIndexHelper.BuildIndexList(BpOwner, BaseBp, Info);

      PostProcess() ;

      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }

    protected override void ImportIdf(){
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinLength 停止
    }

    /// <summary>
    /// 基底クラスでグループが組み替えられると呼ばれる
    /// </summary>
    /// <returns></returns>
    internal override void OnGroupChanged()
    {
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 反映
    }

    protected override void RemoveExtraEdges( Group group, string file )
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if ( file.Contains( "-DIS-A" ) ) {
          removeEdgeList = group.EdgeList.Reverse().Take( 3 ).ToList() ;
        }
        else if ( file.Contains( "-SUC-A" ) ) {
          removeEdgeList = group.EdgeList.Take( 3 ).ToList() ;
        }

        removeEdgeList?.ForEach( e => e.Unlink() ) ;
      }
    }


    /// <summary>
    /// IDFにノズル側にフランジ形状が潰れてしまっているものがあれば追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected override LeafEdge GetNozzleSideFlange( Group group, string file )
    {
      return null ;
    }


    internal override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 1.2 ) ;

      {
        var bpa = BpOwner ;
        bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
        bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      }
      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeSystemFlexStart.MinX, #DischargeSystemFlexStart.MaxX - #BasePump.MinX ) + .AccessSpace " ) ;

      EndTopTypePumpBasicPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      EndTopTypePumpUpDownBOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      EndTopTypePumpBasicHeaderIntervalPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      //  Discharge Direction 設定
      BaseBp.RegisterUserDefinedProperty("DischargeDirection", 0, new Dictionary<string, double> { { "Right", 0 }, { "Left", 1 } });
      BaseBp.RegisterUserDefinedProperty("DischargeAngle", PropertyType.Angle, 0, -60, 45, stepValue: 15);
      BaseBp.RuleList.AddRule("#DischargeGroup.HorizontalRotationDegree", "If( .DischargeDirection, 180 - .DischargeAngle, .DischargeAngle )");

      var range = DiameterRange.GetBlockPatternNpsMmRange();
      EndTopTypePumpBasicDiameterPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info, null, range.min, range.max, range.min, range.max);
    }
  }
}
