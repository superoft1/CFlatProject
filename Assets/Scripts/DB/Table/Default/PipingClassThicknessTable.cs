using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class PipingClassThicknessTable : TableBaseCached 
  {
    public class Record : RecordBase
    {
      public string PipingClass { get; internal set; }
      public int NPSmm { get; internal set; }
      public string IdentificationNote { get; internal set; }
      public int? ScheduleNo { get; internal set; }
      public double Thickness { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from pipingClass in _referenceDB.GetTable<PipingClass>(context)
           join stock in _referenceDB.GetTable<PipingClassStock>(context) on pipingClass.ID equals stock.PipingClass
           where stock.ShortCode == "PIP"
           join catalogOfPipe in _referenceDB.GetTable<CatalogOfPipe>(context) on stock.Catalog equals catalogOfPipe.ID
           join pipeThickeness in _referenceDB.GetTable<STD_PipeThickness>(context) on catalogOfPipe.PipeThickness equals pipeThickeness.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on pipeThickeness.NPS equals nps.ID
           select new Record()
           {
             PipingClass = pipingClass.Name,
             NPSmm = nps.mm,
             IdentificationNote = pipeThickeness.IdentificationNote,
             ScheduleNo = pipeThickeness.Schedule_No,
             Thickness = pipeThickeness.WallThickness_mm
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(string pipingClass)
    {
      return Records.Where(rec => rec.PipingClass == pipingClass).ToList();
    }

    public Record GetOne(string pipingClass, int NPSmm)
    {
      var hit = Records.Where(rec => rec.PipingClass == pipingClass &&
                              rec.NPSmm == NPSmm );
      if (hit.Any()) return hit.First();

      throw new NoRecordFoundException($"No record found for PipingClass = {pipingClass} and NPS = {NPSmm} in PipingClassThickness");
    }
  }
}