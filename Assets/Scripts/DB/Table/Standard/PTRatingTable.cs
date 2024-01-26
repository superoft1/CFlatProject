using System.Collections.Generic ;
using System.Data ;
using System.Data.Linq ;
using System.Linq ;
using Chiyoda.DB.Entity.Standard ;

namespace Chiyoda.DB
{
  public class PTRatingTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string MaterialGroup { get; internal set; }
      public double Temperature { get; internal set; }
      public int Class { get; internal set; }
      public double WorkingPressure { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using ( IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef) )
      using ( var context = new DataContext( dbcon ) ) {
        return
          (from ptrating in _referenceDB.GetTable<STD_PTRating>( context )
           join rating in _referenceDB.GetTable<STD_RatingClass>(context) on ptrating.Class equals rating.ID
          select new Record()
          {
            MaterialGroup = ptrating.MaterialGroup,
            Temperature = ptrating.Temperature,
            Class = rating.Rating,
            WorkingPressure = ptrating.WorkingPressure,
          }).Cast<RecordBase>().ToList() ;
      }
    }

    public int GetNP(string materialGroup, double temperature, double pressure)
    {
      return Records
        .Where(rec => rec.MaterialGroup == materialGroup && 
                      rec.Temperature >= temperature && 
                      rec.WorkingPressure >= pressure)
        .Min(rec => rec.Class);
    }
  }
}