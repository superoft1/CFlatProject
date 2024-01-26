using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class PipeThicknessTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public int NPSmm { get; internal set; }
      public string IdentificationNote { get; internal set; }
      public int? ScheduleNo { get; set; }
      public double WallThickness_mm { get; set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from pipethickness in _referenceDB.GetTable<STD_PipeThickness>(context)
           join nps in _referenceDB.GetTable<STD_NPS>(context) on pipethickness.NPS equals nps.ID
           select new Record()
           {
             NPSmm = nps.mm,
             IdentificationNote = pipethickness.IdentificationNote,
             ScheduleNo = pipethickness.Schedule_No,
             WallThickness_mm = pipethickness.WallThickness_mm
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(int NPSmm)
    {
      var hit = Records.Where(rec => rec.NPSmm == NPSmm);
      if (hit.Any()) {
        return hit.ToList();
      }

      throw new NoRecordFoundException($"No record found for NPS = {NPSmm} in PipeThickness");
    }
  }
}