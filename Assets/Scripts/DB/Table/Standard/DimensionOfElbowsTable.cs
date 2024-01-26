using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class DimensionOfElbowsTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get; internal set; }
      public int NPSmm { get; internal set; }
      public string ElbowType { get; set; }
      public double CenterToEnd { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();    

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimElbows in _referenceDB.GetTable<STD_DimensionOfElbows>(context)
           join sta in _referenceDB.GetTable<STD_Standard>(context) on dimElbows.Standard equals sta.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on dimElbows.NPS equals nps.ID
           join typeElbow in _referenceDB.GetTable<STD_TypeOfElbow>(context) on dimElbows.ElbowType equals typeElbow.ID
           select new Record()
           {
             Standard = sta.Name,
             NPSmm = nps.mm,
             ElbowType = typeElbow.Name,
             CenterToEnd = dimElbows.CenterToEnd
           }).Cast<RecordBase>().ToList();
      }
    }

    public Record GetOne(int NPSmm, string elbowType = "90deg", string standard = "ASME")
    {
      var hit = Records.Where(rec => rec.Standard == standard &&
                                    rec.NPSmm == NPSmm &&
                                    rec.ElbowType == elbowType);
      if (hit.Any()) {
        return hit.First();
      }

      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} and ElbowType = {elbowType} in DimensionOfElbows");
    }

    public Record GetOne90(int NPSmm, double centerToEnd)
    {
      var targetNPS = Records.Where(rec => rec.NPSmm == NPSmm && 
                                         (rec.ElbowType == "90deg" || rec.ElbowType == "S90deg")).ToList();
      if(targetNPS.Any())
      {
        return targetNPS.OrderBy(rec => Math.Abs(centerToEnd - rec.CenterToEnd)).ThenBy(rec => rec.ElbowType).First();
      }
      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} in DimensionOfElbows");
    }
  }
}