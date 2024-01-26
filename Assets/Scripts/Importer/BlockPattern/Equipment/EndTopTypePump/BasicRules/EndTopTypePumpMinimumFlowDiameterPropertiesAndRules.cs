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

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  class EndTopTypePumpMinimumFlowDiameterPropertiesAndRules
  {
    public static void Set(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfoWithMinimumFlow info, EndTopTypeMinimumFlowLengthUpdater updater, int suctionMinNpsMm, int suctionMaxNpsMm, int dischargeMinNpsMm, int dischargeMaxNpsMm, int miniFlowMinNpsMm, int miniFlowMaxNpsMm)
    {
      var suctionProp = bp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.SuctionDiameterNPSInch).NpsMm, suctionMinNpsMm, suctionMaxNpsMm);
      suctionProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));
      if ( updater != null ) {
        suctionProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateSuctionMinimumLengths ) ) ;
      }
      var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.DischargeDiameterNPSInch).NpsMm, dischargeMinNpsMm, dischargeMaxNpsMm);

      dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1),("DischargeMinLength1",0),("DischargeMinLength1",1)));
      if (updater != null){
        dischargeProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateDischargeMinimumLengths) );
      }
      var minimumFlowProp = bp.RegisterUserDefinedProperty("MiniFlowDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.MinimumFlowDiameterNPSInch).NpsMm, miniFlowMinNpsMm, miniFlowMaxNpsMm);

      minimumFlowProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("MinimumFlowEndPipe", 1)));
      if ( updater != null ) {
        dischargeProp.AddUserDefinedRule( new GenericHookedRule( updater.UpdateMinimumFlowMinimumLengths ) ) ;
      }
      // 径変更時にMiniflowのドロップダウンリストの値を変更
      dischargeProp.AddUserDefinedRule( new ChangeMinimumFlowDiameterRangeRule( bpa, "MiniFlowDiameter", "DischargeEndPipe" ) ) ;

    }
  }
}

