using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class DimensionOfCapsTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get; internal set; }
      public int NPSmm { get; internal set; }
      public double Length { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimCaps in _referenceDB.GetTable<STD_DimensionOfCaps>(context)
           join sta in _referenceDB.GetTable<STD_Standard>(context) on dimCaps.Standard equals sta.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on dimCaps.NPS equals nps.ID
           select new Record()
           {
             Standard = sta.Name,
             NPSmm = nps.mm,
             Length = dimCaps.Length
           }).Cast<RecordBase>().ToList();
      }
    }

    public Record GetOne(int NPSmm, string standard = "ASME")
    {
      var hit = Records.Where(rec => rec.Standard == standard &&
                                    rec.NPSmm == NPSmm);
      if (hit.Any()) {
        return hit.First();
      }

      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} in DimensionOfCaps");
    }
  }
}