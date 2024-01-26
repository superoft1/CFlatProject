using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Plot ;

namespace Chiyoda.DB
{
  public enum EquipHazardGroup
  {
    Compressors,
    IntermediateHazardPump,
    HighHazardPump,
    HighHazardReactor,
    IntermediateHazardReactor,
    ModerateHazardReactor,
    Columns_Accumulators_Drums,
    RundownTanks,
    FiredHeaters_Incineraors_Oxidizers,
    AirCooledHeatExchanger,
    HeatExchanger,
    PipeRacks,
    EmergencyExchangers,
    UnitBlockValves,
    AnalyzerRooms
  }

  public class EquipHazardClearanceTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string EquipHazardGroup1 { get; internal set; }
      public string EquipHazardGroup2 { get; internal set; }
      public double Clearance_ft { get; internal set; }
      public double Clearance_m { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using ( IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_Plot) )
      using ( var context = new DataContext( dbcon ) ) {
        return
          (from equipClearance in _referenceDB.GetTable<EquipHazardClearance>( context )
          select new Record()
          {
            EquipHazardGroup1 = equipClearance.EquipHazardGroup1,
            EquipHazardGroup2 = equipClearance.EquipHazardGroup2,
            Clearance_ft = equipClearance.Clearance_ft,
            Clearance_m = equipClearance.Clearance_m,
          }).Cast<RecordBase>().ToList() ;
      }
    }

    // 単位はmに変換後のものを返します
    public double GetClearance( EquipHazardGroup equip1, EquipHazardGroup equip2 )
    {
      var equip1Str = equip1.ToString() ;
      var equip2Str = equip2.ToString() ;
      var hit = Records.Where( rec => 
        (rec.EquipHazardGroup1 == equip1Str && rec.EquipHazardGroup2 == equip2Str) ||
         rec.EquipHazardGroup1 == equip2Str && rec.EquipHazardGroup2 == equip1Str);
      if (hit.Any()) {
        return hit.First().Clearance_m ;
      }
      
      throw new NoRecordFoundException($"No record found for equip1 = {equip1} and equip2 = {equip2} in EquipHazardClearanceTable");
    }
  }
}