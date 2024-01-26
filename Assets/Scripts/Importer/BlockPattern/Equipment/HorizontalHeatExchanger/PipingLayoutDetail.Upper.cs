using System.Collections.Generic ;
using System.IO ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;

namespace Importer.BlockPattern.Equipment.HorizontalHeatExchanger
{
  namespace Upper
  {
    //A!-HE-A1-1-G-S-IN_-0
    public class A_1_IN : PipingLayout
    {
      public A_1_IN( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Upper ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("A_1_IN") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupEndEdge( group, endEdgeIndex: 0 ) ;

        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 270);

        SetupDiameterRule( defaultDiameterNpsInchValue: 16, endEdgeConnectPoint: 0 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }

    //"A!-HE-A1-1-G-T-OUT_-0"
    public class A_1_T_OUT : PipingLayout
    {
      public A_1_T_OUT( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Upper ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("A_1_T_OUT") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupEndEdge( group, endEdgeIndex: 5 ) ;
        
        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 90);

        SetupDiameterRule( defaultDiameterNpsInchValue: 24, endEdgeConnectPoint: 1 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }
    
    public class B_1_OUT_V : PipingLayout
    {
      public B_1_OUT_V( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Upper ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("B_1_OUT(V)") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupEndEdge( group, endEdgeIndex: 2 ) ;
        
        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 90);

        SetupDiameterRule( defaultDiameterNpsInchValue: 16, endEdgeConnectPoint: 1 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }
    
    public class B_1_T_OUT : PipingLayout
    {
      public B_1_T_OUT( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Upper ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("B_1_T_OUT") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupEndEdge( group, endEdgeIndex: 5 ) ;
        
        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 270);

        SetupDiameterRule( defaultDiameterNpsInchValue: 16, endEdgeConnectPoint: 1 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }
  }
}