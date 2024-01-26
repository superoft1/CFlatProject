using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  class TopTopTypePumpBasicDiameterPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, TopTopTypePumpPipeLengthUpdater updater)
    {
      Set(bpa, bp, info,updater); //  TODO 名前が長くて不便なので、こちらに一本化する予定
    }
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, TopTopTypePumpPipeLengthUpdater updater){
      
      var suctionProp = bp.RegisterUserDefinedProperty( "SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.SuctionDiameterNPSInch).NpsMm,
        DiameterRange.GetBlockPatternNpsMmRange().min, DiameterRange.GetBlockPatternNpsMmRange().max ) ;
      suctionProp.AddUserDefinedRule( new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));

      if (updater != null)
        suctionProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateSuctionMinimumLengths) );
      
      var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.DischargeDiameterNPSInch).NpsMm, DiameterRange.GetBlockPatternNpsMmRange().min, DiameterRange.GetBlockPatternNpsMmRange().max );

      dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1)));
      if (updater != null)
        dischargeProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateDischargeMinimumLengths) );
    }
  }


  class TopTopTypePumpMinimumFlowDiameterPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, TopTopTypeMinimumFlowPipeLengthUpdater updater){

      //  Diameter
      var suctionProp = bp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.SuctionDiameterNPSInch).NpsMm,
        DiameterRange.GetBlockPatternNpsMmRange().min, DiameterRange.GetBlockPatternNpsMmRange().max);
      suctionProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));
      if ( updater != null ) {
        suctionProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateSuctionMinimumLengths ) ) ;
      }
      

      
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow)){
        return; //  error
      }
      var infoDerived = (SingleBlockPatternIndexInfoWithMinimumFlow)info;

      var miniFlowProp = bp.RegisterUserDefinedProperty("MiniFlowDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(infoDerived.MinimumFlowDiameterNPSInch).NpsMm, DiameterRange.GetBlockPatternNpsMmRange().min, DiameterRange.GetBlockPatternNpsMmRange().max);

      miniFlowProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("MinimumFlowEndPipe", 1)));
      if ( updater != null ) {
        miniFlowProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateMinimumFlowMinimumLengths ) ) ;
      }
      
      var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.DischargeDiameterNPSInch).NpsMm, DiameterRange.GetBlockPatternNpsMmRange().min, DiameterRange.GetBlockPatternNpsMmRange().max);

      dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1)));
      if ( updater != null ) {
        dischargeProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateDischargeMinimumLengths ) ) ;
      }

      // 径変更時にMiniflowのドロップダウンリストの値を変更
      dischargeProp.AddUserDefinedRule( new ChangeMinimumFlowDiameterRangeRule( bpa, "MiniFlowDiameter", "DischargeEndPipe" ) ) ;
    }
  }
}
