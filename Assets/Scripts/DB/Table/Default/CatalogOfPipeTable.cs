using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class CatalogOfPipeTable : TableBaseCached 
  {
    public class Record : RecordOfCatalog
    {
      public int NPSmm { get; internal set; }
      public string EndPrep { get; internal set; }
      public string IdentificationNote { get; internal set; }
      public int? Schedule { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from catalog in _referenceDB.GetTable<CatalogOfPipe>(context)
           join standard in _referenceDB.GetTable<STD_Standard>(context) on catalog.Standard equals standard.ID
           join pipeThickness in _referenceDB.GetTable<STD_PipeThickness>(context) on catalog.PipeThickness equals pipeThickness.ID
           join nps in _referenceDB.GetTable<STD_NPS>(context) on pipeThickness.NPS equals nps.ID
           join endPrep in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on catalog.EndPrep equals endPrep.ID

           select new Record()
           {
             ID = catalog.ID,
             Standard = standard.Name,
             ShortCode = catalog.ShortCode,
             NPSmm = nps.mm,
             EndPrep = endPrep.Code,
             IdentificationNote = pipeThickness.IdentificationNote,
             Schedule = pipeThickness.Schedule_No,
             IdentCode = catalog.IdentCode
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(int NPSmm, string endPrep, string identificationNote, string standard = "ASME")
    {
      return Records.Where(rec => rec.NPSmm == NPSmm &&
                                  rec.EndPrep == endPrep &&
                                  rec.IdentificationNote == identificationNote &&
                                  rec.Standard == standard).ToList();
    }

    internal IList<Record> Get(IList<int> IDList)
    {
      return Records.Where(rec => IDList.Contains(rec.ID)).ToList() ;
    }
  }
}