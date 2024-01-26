using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class STD_RatingClassTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public int    ID        { get; internal set; }
      public string Standard  { get; internal set; }
      public int    Rating    { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (
            from rate in _referenceDB.GetTable<STD_RatingClass>(context)
            join sta in _referenceDB.GetTable<STD_Standard>(context) on rate.Standard equals sta.ID

            select new Record()
              {
                ID = rate.ID,
                Standard = sta.Name,
                Rating = rate.Rating
              }
          ).OrderBy(rec=> rec.ID).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> GetRatingList(string standard = "ASME")
    {
      var hit = Records.Where(rec => rec.Standard == standard );
      if (hit.Any()) {
        return hit.ToList();
      }

      throw new NoRecordFoundException($"No record found for standard = {standard} in STD_RatingClass");
    }
  }
}