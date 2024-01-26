using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;

namespace Importer.BlockPattern.Equipment.HorizontalHeatExchanger
{
  namespace Lower
  {
    //"A!-HE-A1-1-G-S-OUT_-0"
    public class A1_1_OUT : PipingLayout
    {
      public A1_1_OUT( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Lower ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("A_1_OUT") ) ;
      }


      protected override void AddRule( Group group )
      {
        SetupBopEdge( group, bopEdgeIndex:2, flexEdgeIndex:1) ;
        SetupEndEdge( group, endEdgeIndex: 7 ) ;

        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 90);

        BlockPattern.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.5 ) ;
        BlockPattern.RuleList.AddRule( $"#{BopEdgeName()}.MinZ", $"#{BopEdgeName()}.MinZ - (#{BopEdgeName()}.GlobalMinZ - .BOP)" ) ;

        SetupDiameterRule( defaultDiameterNpsInchValue: 12, endEdgeConnectPoint: 1 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }

    //"A!-HE-A1-1-G-T-IN_-0"
    public class A_1_T_IN : PipingLayout
    {
      public A_1_T_IN( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Lower ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("A_1_T_IN") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupBopEdge( group, bopEdgeIndex:7, flexEdgeIndex:8) ;
        SetupEndEdge( group, endEdgeIndex: 0 ) ;

        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 270);

        BlockPattern.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.5 ) ;
        BlockPattern.RuleList.AddRule( $"#{BopEdgeName()}.MaxZ", $"#{BopEdgeName()}.MaxZ - (#{BopEdgeName()}.GlobalMaxZ - .BOP)" ) ;

        SetupDiameterRule( defaultDiameterNpsInchValue: 24, endEdgeConnectPoint: 1 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }
    
    
    public class B_1_OUT_L : PipingLayout
    {
      public B_1_OUT_L( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Lower ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("B_1_OUT(L)") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupBopEdge( group, bopEdgeIndex:2 , flexEdgeIndex:1) ;
        SetupEndEdge( group, endEdgeIndex: 2 ) ;

        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 0);

        BlockPattern.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.5 ) ;
        BlockPattern.RuleList.AddRule( $"#{BopEdgeName()}.MaxZ", $"#{BopEdgeName()}.MaxZ - (#{BopEdgeName()}.GlobalMaxZ - .BOP)" ) ;

        SetupDiameterRule( defaultDiameterNpsInchValue: 16, endEdgeConnectPoint: 1 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }
    
    public class B_1_IN : PipingLayout
    {
      public B_1_IN( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Lower ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("B_1_IN") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupBopEdge( group, bopEdgeIndex:0 , flexEdgeIndex:1) ;
        SetupEndEdge( group, endEdgeIndex: 0 ) ;

        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 90);

        BlockPattern.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.5 ) ;
        BlockPattern.RuleList.AddRule( $"#{BopEdgeName()}.MaxZ", $"#{BopEdgeName()}.MaxZ - (#{BopEdgeName()}.GlobalMaxZ - .BOP)" ) ;

        SetupDiameterRule( defaultDiameterNpsInchValue: 16, endEdgeConnectPoint: 0 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }

    public class B_1_T_IN : PipingLayout
    {
      public B_1_T_IN( Document doc, string idfFile, Chiyoda.CAD.Model.Equipment he,
        PipeInformation pipeInformation )
        : base( doc, idfFile, he )
      {
        InterchangeablePosition = EInterchangeablePosition.Lower ;
        PipeInfo = pipeInformation ;
      }

      protected override string GetIdfFilePath()
      {
        return Path.Combine( IdfFolder, IdfFileName("B_1_T_IN") ) ;
      }

      protected override void AddRule( Group group )
      {
        SetupBopEdge( group, bopEdgeIndex:0 , flexEdgeIndex:1) ;
        SetupEndEdge( group, endEdgeIndex: 0 ) ;

        CreateContinuousGroup( group ) ;
        SetupAngleRule(ContinuousGroupName(), 270);

        BlockPattern.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.5 ) ;
        BlockPattern.RuleList.AddRule( $"#{BopEdgeName()}.MaxZ", $"#{BopEdgeName()}.MaxZ - (#{BopEdgeName()}.GlobalMaxZ - .BOP)" ) ;

        SetupDiameterRule( defaultDiameterNpsInchValue: 16, endEdgeConnectPoint: 0 ) ;

        BlockPattern.RuleList.BindChangeEvents( true ) ;
      }
    }
  }
}