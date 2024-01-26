using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class DimensionOfPipeShoeTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public int NPSmm { get; internal set; }
      public string ShoeType { get; set; }
      public double CenterToEnd { get; set; }
      public double Width { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from dimPipeShoe in _referenceDB.GetTable<DimensionOfPipeShoe>(context)
           join nps in _referenceDB.GetTable<STD_NPS>(context) on dimPipeShoe.NPS equals nps.ID
           select new Record()
           {
             NPSmm = nps.mm,
             ShoeType = dimPipeShoe.ShoeType,
             CenterToEnd = dimPipeShoe.CenterToEnd,
             Width = dimPipeShoe.Width
           }).Cast<RecordBase>().ToList();
      }
    }

    public Record GetOne(int NPSmm, string shoeType)
    {
      var hit = Records.Where(rec => rec.NPSmm == NPSmm &&
                                    rec.ShoeType == shoeType);
      if (hit.Any()) {
        return hit.First();
      }

      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} and ShoeType = {shoeType} in DimensionOfPipeShoe");
    }
  }
}