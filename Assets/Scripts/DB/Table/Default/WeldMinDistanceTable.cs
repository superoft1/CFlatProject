using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Default ;
using Chiyoda.DB.Entity.Standard ;

namespace Chiyoda.DB
{
  public class WeldMinDistanceTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public int NPSmm { get; internal set; }
      public string EndPrep { get; internal set; }
      public double Distance { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (
            from weldMinDist in _referenceDB.GetTable<WeldMinDistance>(context)
            join nps in _referenceDB.GetTable<STD_NPS>(context) on weldMinDist.NPS equals nps.ID
            join endPrep in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on weldMinDist.EndPrep equals endPrep.ID
            select new Record()
              {
                NPSmm = nps.mm,
                EndPrep = endPrep.Code,
                Distance = weldMinDist.Distance
              }
          ).Cast<RecordBase>().ToList() ;
      }
    }
    
    public double GetDistance( double NPSmm, string endPrep = "BW" )
    { 
      var hit = Records.Where( rec => rec.NPSmm == (int)NPSmm && rec.EndPrep == endPrep);
      if ( hit.Any() ) return hit.First().Distance ;
      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} and EndPrep = {endPrep} in DimensionOfReducerTable");
    }
  }
}