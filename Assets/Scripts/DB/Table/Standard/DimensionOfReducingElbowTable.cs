using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Standard ;

namespace Chiyoda.DB
{
  public class DimensionOfReducingElbowTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get ; internal set ; }
      public int NPS_S_mm { get ; internal set ; }
      public int NPS_L_mm { get ; internal set ; }
      public double Radius { get ; set ; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimReducingElbow in _referenceDB.GetTable<STD_DimensionOfReducingElbow>(context)
           join stan in _referenceDB.GetTable<STD_Standard>(context) on dimReducingElbow.Standard equals stan.ID
           join npsL in _referenceDB.GetTable<STD_NPS>(context) on dimReducingElbow.NPS_L equals npsL.ID
           join npsS in _referenceDB.GetTable<STD_NPS>(context) on dimReducingElbow.NPS_S equals npsS.ID
           select new Record()
           {
             Standard = stan.Name,
             NPS_L_mm = npsL.mm,
             NPS_S_mm = npsS.mm,
             Radius = dimReducingElbow.CenterToEnd
           }).Cast<RecordBase>().ToList();
      }
    }
    
    public Record GetOne(int NPS_L_mm, int NPS_S_mm, string standard = "ASME")
    {
      var hit = Records.Where(rec => rec.Standard == standard &&
                                    rec.NPS_L_mm == NPS_L_mm &&
                                    rec.NPS_S_mm == NPS_S_mm );
      if (hit.Any()) return hit.First();

      throw new NoRecordFoundException($"No record found for NPS_L = {NPS_L_mm} and NPS_S = {NPS_S_mm} in DimensionOfReducingElbow");
    }
  }

}