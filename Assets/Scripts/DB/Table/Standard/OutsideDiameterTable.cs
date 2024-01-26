using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class OutsideDiameterTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string Standard { get; internal set; }
      public int NPSmm { get; internal set; }
      public double Inchi { get; internal set; }
      public string InchiStr { get; internal set; }
      public double OutsideDiameter { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from od in _referenceDB.GetTable<STD_OutsideDiameter>(context)
           join nps in _referenceDB.GetTable<STD_NPS>(context) on od.NPS equals nps.ID
           join sta in _referenceDB.GetTable<STD_Standard>(context) on od.Standard equals sta.ID
           select new Record()
           {
             Standard = sta.Name,
             NPSmm = nps.mm,
             Inchi = nps.Inchi,
             InchiStr = nps.InchiDisp,
             OutsideDiameter = od.mm
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

      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} in OutsideDiameter");
    }

    /// <summary>
    /// 入力された外径に最も近い規格のNPSレコードを返す
    /// </summary>
    /// <param name="outsideDiameter"></param>
    /// <returns>NPSのレコード</returns>
    public Record GetClosestTo(double outsideDiameter, string standard = "ASME")
    {
      if (outsideDiameter >= 0.0)
      {
        return Records.Where(rec => rec.Standard == standard).OrderBy(rec => System.Math.Abs(outsideDiameter - rec.OutsideDiameter)).First();
      }
      throw new NoRecordFoundException($"No record found for OutsideDiameter = {outsideDiameter} and Standard = {standard} in OutsideDiameter");
    }


  }
}