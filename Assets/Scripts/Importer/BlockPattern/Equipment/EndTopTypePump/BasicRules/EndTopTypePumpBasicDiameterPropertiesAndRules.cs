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
  class EndTopTypePumpBasicDiameterPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, EndTopTypePumpLengthUpdater updater, int suctionMinNpsMm, int suctionMaxNpsMm, int dischargeMinNpsMm, int dischargeMaxNpsMm)
    {
      double dinch = info.DischargeDiameterNPSInch;
      double sinch = info.SuctionDiameterNPSInch;
      var suctionProp = bp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(sinch).NpsMm, suctionMinNpsMm, suctionMaxNpsMm);
      suctionProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));
      if (updater != null)
        suctionProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateSuctionMinimumLengths) );

      var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(dinch).NpsMm, dischargeMinNpsMm, dischargeMaxNpsMm);
      dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1)));
      if (updater != null)
        dischargeProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateDischargeMinimumLengths) );
    }
  }

  class EndTopTypePumpErrorDiameterPropertiesAndRules
  {
    public static void SetPropertiesAndRules(CompositeBlockPattern bpa, Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info, EndTopTypePumpLengthUpdater updater, int suctionMinNpsMm, int suctionMaxNpsMm, int dischargeMinNpsMm, int dischargeMaxNpsMm)
    {
      double dinch = info.DischargeDiameterNPSInch;
      double sinch = info.SuctionDiameterNPSInch;
      var suctionProp = bp.RegisterUserDefinedProperty("SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(dinch).NpsMm, suctionMinNpsMm, suctionMaxNpsMm);
      suctionProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("SuctionEndPipe", 0)));
      if (updater != null)
        suctionProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateSuctionMinimumLengths) );

      var dischargeProp = bp.RegisterUserDefinedProperty("DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(sinch).NpsMm, dischargeMinNpsMm, dischargeMaxNpsMm);
      dischargeProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DischargeEndPipe", 1)));
      if (updater != null)
        dischargeProp.AddUserDefinedRule( new GenericHookedRule(updater.UpdateDischargeMinimumLengths) );
    }
  }

}

