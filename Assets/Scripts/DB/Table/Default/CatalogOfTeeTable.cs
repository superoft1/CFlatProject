using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using Chiyoda.DB.Entity.Default;
using Chiyoda.DB.Entity.Standard;

namespace Chiyoda.DB
{
  public class CatalogOfTeeTable : TableBaseCached 
  {
    public class Record : RecordOfCatalog
    {
      public int    NPS_H_mm            { get; internal set; }
      public int    NPS_B_mm            { get; internal set; }
      public string EndPrep             { get; internal set; }
      public string IdentificationNote  { get; internal set; }
      public int?   Schedule            { get; internal set; }
    }

    private IList<Record> Records => __cache.Cast<Record>().ToList();

    protected override IList<RecordBase> Query()
    {
      using (IDbConnection dbcon = _referenceDB.CreateDBConnection(Chiyoda.DB.DbSqLite.ConectDB_PDRef))
      using (var context = new DataContext(dbcon))
      {
        return
          (from cot in _referenceDB.GetTable<CatalogOfTee>(context)
           join sta in _referenceDB.GetTable<STD_Standard>(context) on cot.Standard equals sta.ID
           join ptn in _referenceDB.GetTable<STD_PipeThickness>(context) on cot.PipeThickness equals ptn.ID
           join nps_h in _referenceDB.GetTable<STD_NPS>(context) on ptn.NPS equals nps_h.ID
           join nps_b in _referenceDB.GetTable<STD_NPS>(context) on cot.NPS_B equals nps_b.ID
           join edp in _referenceDB.GetTable<STD_TypeOfEndPrep>(context) on cot.EndPrep equals edp.ID

           select new Record()
           {
             ShortCode = cot.ShortCode,
             Standard = sta.Name,
             IdentificationNote = ptn.IdentificationNote,
             NPS_H_mm = nps_h.mm,
             NPS_B_mm = nps_b.mm,
             Schedule = ptn.Schedule_No,
             EndPrep = edp.Code,
             IdentCode = cot.IdentCode
           }).Cast<RecordBase>().ToList();
      }
    }

    public IList<Record> Get(int NPS_H_mm, string endPrep, string identificationNote, string standard = "ASME")
    {
      var rtn = Records.Where(rec =>  rec.NPS_H_mm == NPS_H_mm &&
                                      rec.EndPrep == endPrep &&
                                      rec.IdentificationNote == identificationNote &&
                                      rec.Standard == standard).ToList();
      if (rtn.Any()) return rtn.ToList();
      throw new NoRecordFoundException($"No record found for NPS = {NPS_H_mm} and EndPrep = {endPrep} and IdentificationNotein = {identificationNote} In CatalogOfTee");
    }

  }
}